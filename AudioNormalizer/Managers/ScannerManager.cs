using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AudioNormalizer.Models;
using IPA.Utilities;
using SiraUtil.Logging;
using SongCore;
using Zenject;

namespace AudioNormalizer.Managers
{
    internal class ScannerManager : IInitializable, IDisposable
    {
        private readonly string _ffmpegPath = Path.Combine(UnityGame.LibraryPath, "ffmpeg.exe");
        private readonly SiraLog _log;
        private readonly Config _config;

        private readonly int _maxTasks;
        private readonly Dictionary<string, Task<float?>> _scanningLevels;
        private ScansModel _results = null!; // This will be initialized in Initialize
        private bool _scanningAll;
        private List<CustomPreviewBeatmapLevel>? _unscannedLevels;

        private const float ClipThreshold = -1f; // Give 1 dB of headroom
        private readonly float _targetLoudness;
        private readonly float _preamp;
        
        public event Action<int>? ScanFinished; // Parameter is new count of scanned songs

        public ScannerManager(SiraLog log, Config config, AudioManagerSO audioManager)
        {
            _log = log;
            _config = config;
            _scanningLevels = new Dictionary<string, Task<float?>>();
            _maxTasks = Environment.ProcessorCount;

            // Gets the perceived loudness const from base game
            _targetLoudness = (float) (typeof(PerceivedLoudnessPerLevelModel)
                .GetField("kPerceivedLoudnessTarget", BindingFlags.Static | BindingFlags.NonPublic)
                ?.GetValue(null) ?? throw new MissingFieldException());

            // Gets the global offset applied to all music volume, effectively our preamp
            _preamp = audioManager.GetField<float, AudioManagerSO>("_musicVolumeOffset");
        }

        public void Dispose()
        {
            Loader.SongsLoadedEvent -= _findUnscannedSongs;
        }

        public void Initialize()
        {
            _results = ScansModel.Load();
            // Subscribe to songs loaded event
            Loader.SongsLoadedEvent += _findUnscannedSongs;

        }


        public int NumScannedSongs()
        {
            return (from level in Loader.CustomLevels.Values
                where _results.Contains(level.levelID)
                select level).Count();
        }

        public Scan? GetScan(string levelId)
        {
            var success = _results.TryGet(levelId, out var scan);
            return success ? scan : null;
        }

        public float? GetLoudness(string levelId)
        {
            var success = _results.TryGet(levelId, out var scan);
            if (!success) return null;

            var loudness = scan.Loudness;

            if (_config.ClipPrevention)
            {
                // Apply gain to peak
                var gainedPeak = scan.Peak + (_targetLoudness - scan.Loudness + _preamp);
                if (gainedPeak > ClipThreshold)
                {
                    // We need to adjust gain so that gained peak is at threshold
                    var adjustment = gainedPeak - ClipThreshold;
                    loudness += adjustment;
                    _log.Debug($"Adjusting loudness by {adjustment}");
                }
            } 
            
            return loudness;
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

        public async Task ScanSong(CustomPreviewBeatmapLevel level)
        {
            _scanningLevels.TryGetValue(level.levelID, out var scan);
            if (scan is { } currentScan)
            {
                await currentScan;
                return;
            }

            scan = _internalScanSong(level);
            _scanningLevels.Add(level.levelID, scan);
            await scan;
        }

        private async Task<float?> _internalScanSong(CustomPreviewBeatmapLevel level)
        {
            string? loudness = null;
            string? peak = null;

            var scanProcess = new Process();
            scanProcess.StartInfo.UseShellExecute = false;
            scanProcess.StartInfo.CreateNoWindow = true;
            scanProcess.StartInfo.FileName = _ffmpegPath;
            scanProcess.StartInfo.RedirectStandardError = true; // ffmpeg outputs to stderr
            scanProcess.StartInfo.Arguments =
                $"-nostats -hide_banner -i \"{level.songPreviewAudioClipPath}\" -map a:0 -filter ebur128=framelog=verbose:peak=true -f null -";

            scanProcess.ErrorDataReceived += (s, e) =>
            {
                if (e.Data == null) return;
                
                if (e.Data.StartsWith("    I:"))
                {
                    var split = e.Data.Split();
                    loudness = split[split.Length - 2]; // Second last entry
                } else if (e.Data.StartsWith("    Peak:"))
                {
                    var split = e.Data.Split();
                    peak = split[split.Length - 2]; // Second last entry
                }
            };

            _log.Debug($"About to start scan for song {level.levelID}");

            scanProcess.Start();
            scanProcess.BeginErrorReadLine();
            await Task.Run(() => scanProcess.WaitForExit());

            if (loudness == null || peak == null)
            {
                return null;
            }

            var parsedLoudness = float.Parse(loudness);
            var parsedPeak = float.Parse(peak);

            _setScan(level.levelID, parsedLoudness, parsedPeak);
            _scanningLevels.Remove(level.levelID);

            _log.Debug($"Finished scan, {level.levelID} - {parsedLoudness} peak {parsedPeak}");

            ScanFinished?.Invoke(_results.Count());
            _scanNextSong();
            return parsedLoudness;
        }

        private void _setScan(string levelId, float rg, float peak)
        {
            if (_results.Contains(levelId))
            {
                _results.Remove(levelId);
            }

            _results.Add(levelId, rg, peak);
            if (!_scanningAll)
            {
                _results.Save();
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
                _results.Save();
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
                where !_results.Contains(level.Value.levelID)
                select level.Value;
            _unscannedLevels = unscanned.ToList();

            if (_config.ScanOnSongsLoaded)
            {
                ScanAllSongs();
            }
        }
    }
}