using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPA.Utilities;
using Newtonsoft.Json;
using SiraUtil.Logging;
using SongCore;
using Zenject;

namespace BSReplayGain.Managers
{
    internal class ReplayGainManager : IInitializable, IDisposable
    {
        private static readonly string ScanResultsDir = Path.Combine(UnityGame.UserDataPath, nameof(BSReplayGain));
        private static readonly string ScanResultsPath = Path.Combine(ScanResultsDir, "scans.json");
        private readonly string _ffmpegPath = Path.Combine(UnityGame.LibraryPath, "ffmpeg.exe");
        private readonly SiraLog _log;
        private readonly Config _config;

        private readonly int _maxTasks;
        private readonly Dictionary<string, Task<float?>> _scanningLevels;
        private Dictionary<string, float> _results;
        private bool _scanningAll;
        private List<CustomPreviewBeatmapLevel>? _unscannedLevels;

        public ReplayGainManager(SiraLog log, Config config)
        {
            _log = log;
            _config = config;
            _results = new Dictionary<string, float>();
            _scanningLevels = new Dictionary<string, Task<float?>>();
            _maxTasks = Environment.ProcessorCount;
        }

        public void Dispose()
        {
            Loader.SongsLoadedEvent -= _findUnscannedSongs;
        }

        public void Initialize()
        {
            if (!File.Exists(ScanResultsPath))
            {
                File.WriteAllText(ScanResultsPath, JsonConvert.SerializeObject(_results), Encoding.UTF8);
            }
            else
            {
                _results = JsonConvert.DeserializeObject<Dictionary<string, float>>
                    (File.ReadAllText(ScanResultsPath, Encoding.UTF8)) ?? _results;
            }

            // Subscribe to songs loaded event
            Loader.SongsLoadedEvent += _findUnscannedSongs;
        }

        public event Action<int> ScanFinished; // Parameter is new count of scanned songs

        public int NumScannedSongs()
        {
            return _results.Count;
        }

        public float? GetReplayGain(string levelId)
        {
            var success = _results.TryGetValue(levelId, out var rg);
            return success ? rg : (float?)null;
        }

        public void QueueScanSong(CustomPreviewBeatmapLevel level)
        {
            _scanningLevels.TryGetValue(level.levelID, out var scan);
            if (scan is { })
            {
                return;
            }

            _scanningLevels.Add(level.levelID, _internalScanSong(level));
        }

        public void ScanAllSongs()
        {
            _log.Info("About to scan all songs");
            if (_unscannedLevels is null)
            {
                return;
            }

            _scanningAll = true;
            for (var i = 0; i < _maxTasks && i < _unscannedLevels.Count; i++)
            {
                _scanNextSong();
            }
        }

        public async Task<float?> ScanSong(CustomPreviewBeatmapLevel level)
        {
            _scanningLevels.TryGetValue(level.levelID, out var scan);
            if (scan is { } currentScan)
            {
                return await currentScan;
            }

            scan = _internalScanSong(level);
            _scanningLevels.Add(level.levelID, scan);
            return await scan;
        }

        private async Task<float?> _internalScanSong(CustomPreviewBeatmapLevel level)
        {
            string? loudness = null;

            var scanProcess = new Process();
            scanProcess.StartInfo.UseShellExecute = false;
            scanProcess.StartInfo.CreateNoWindow = true;
            scanProcess.StartInfo.FileName = _ffmpegPath;
            scanProcess.StartInfo.RedirectStandardError = true; // ffmpeg outputs to stderr
            scanProcess.StartInfo.Arguments =
                $"-nostats -hide_banner -i \"{level.songPreviewAudioClipPath}\" -map a:0 -filter ebur128=framelog=verbose -f null -";

            scanProcess.ErrorDataReceived += (s, e) =>
            {
                if (!e.Data.StartsWith("    I:"))
                {
                    return;
                }

                var split = e.Data.Split();
                loudness = split[split.Length - 2]; // Second last entry
            };

            _log.Debug($"About to start scan for song {level.levelID}");

            scanProcess.Start();
            scanProcess.BeginErrorReadLine();
            await Task.Run(() => scanProcess.WaitForExit());

            if (loudness == null)
            {
                return null;
            }

            var parsedLoudness = float.Parse(loudness);

            _setReplayGain(level.levelID, parsedLoudness);
            _scanningLevels.Remove(level.levelID);

            _log.Debug($"Finished scan, {level.levelID} - {parsedLoudness}");

            ScanFinished.Invoke(_results.Count);
            _scanNextSong();
            return parsedLoudness;
        }

        private void _setReplayGain(string levelId, float rg)
        {
            if (_results.ContainsKey(levelId))
            {
                _results.Remove(levelId);
            }

            _results.Add(levelId, rg);
            if (!_scanningAll)
            {
                File.WriteAllText(ScanResultsPath, JsonConvert.SerializeObject(_results), Encoding.UTF8);
            }
        }

        private void _scanNextSong()
        {
            if (!_scanningAll)
            {
                return;
            }

            if (_unscannedLevels is null || _unscannedLevels.Count == 0)
            {
                _scanningAll = false;
                File.WriteAllText(ScanResultsPath, JsonConvert.SerializeObject(_results), Encoding.UTF8);
                return;
            }

            var level = _unscannedLevels.First();
            ScanSong(level);
            _unscannedLevels.Remove(level);
        }

        private void _findUnscannedSongs(Loader loader, ConcurrentDictionary<string, CustomPreviewBeatmapLevel> levels)
        {
            _log.Info("Finding unscanned songs");
            var unscanned = from level in levels
                where !_results.ContainsKey(level.Value.levelID)
                select level.Value;
            _unscannedLevels = unscanned.ToList();

            if (_config.ScanOnSongsLoaded)
            {
                ScanAllSongs();
            }
        }
    }
}