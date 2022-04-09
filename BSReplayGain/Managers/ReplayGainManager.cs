using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using IPA.Utilities;
using Newtonsoft.Json;
using SiraUtil.Logging;
using Zenject;

namespace BSReplayGain.Managers {
    public class ReplayGainManager : IInitializable {
        private Dictionary<string, float> _results;
        private readonly Dictionary<string, Task<float?>> _scanningLevels;
        private readonly SiraLog _log;
        
        private static readonly string ScanResultsDir = $"{Environment.CurrentDirectory}/UserData/BSReplayGain/";
        private static readonly string ScanResultsPath = ScanResultsDir + "scans.json";
        private readonly string _ffmpegPath = Path.Combine(UnityGame.LibraryPath, "ffmpeg.exe");

        public ReplayGainManager(SiraLog log) {
            _log = log;
            _results = new Dictionary<string, float>();
            _scanningLevels = new Dictionary<string, Task<float?>>();
        }

        public void Initialize() {
            _log.Info("In RGManager");
            if (!Directory.Exists(ScanResultsDir)) {
                Directory.CreateDirectory(ScanResultsDir);
            }
            if (!File.Exists(ScanResultsPath)) {
                File.WriteAllText(ScanResultsPath, JsonConvert.SerializeObject(_results), Encoding.UTF8);
            }
            else
            {
                _results = JsonConvert.DeserializeObject<Dictionary<string, float>>
                    (File.ReadAllText(ScanResultsPath, Encoding.UTF8)) ?? _results;
            }
        }

        public float? GetReplayGain(string levelId) {
            var success = _results.TryGetValue(levelId, out var rg);
            return success ? rg : (float?)null;
        }

        public void QueueScanSong(CustomPreviewBeatmapLevel level) {
            _scanningLevels.TryGetValue(level.levelID, out var scan);
            if (scan is { }) {
                return;
            }
            
            _scanningLevels.Add(level.levelID, _internalScanSong(level));
        }

        public async Task<float?> ScanSong(CustomPreviewBeatmapLevel level) {
            _scanningLevels.TryGetValue(level.levelID, out var scan);
            if (scan is { } currentScan) {
                return await currentScan;
            }

            scan = _internalScanSong(level);
            _scanningLevels.Add(level.levelID, scan);
            return await scan;
        }

        private async Task<float?> _internalScanSong(CustomPreviewBeatmapLevel level) {
            string? loudness = null;
            
            var scanProcess = new Process();
            scanProcess.StartInfo.UseShellExecute = false;
            scanProcess.StartInfo.CreateNoWindow = true;
            scanProcess.StartInfo.FileName = _ffmpegPath;
            scanProcess.StartInfo.RedirectStandardError = true; // ffmpeg outputs to stderr
            scanProcess.StartInfo.Arguments =
                $"-nostats -hide_banner -i \"{level.songPreviewAudioClipPath}\" -map a:0 -filter ebur128=framelog=verbose -f null -";

            scanProcess.ErrorDataReceived += (s, e) => {
                if (!e.Data.StartsWith("    I:")) return;
                var split = e.Data.Split();
                loudness = split[split.Length - 2]; // Second last entry
            };

            _log.Debug($"About to start scan for song {level.levelID}");

            scanProcess.Start();
            scanProcess.BeginErrorReadLine();
            

            await Task.Run(() => scanProcess.WaitForExit());
            
            if (loudness == null) {
                return null;
            }
            
            var parsedLoudness = float.Parse(loudness);

            _setReplayGain(level.levelID, parsedLoudness);
            _scanningLevels.Remove(level.levelID);

            _log.Debug($"Finished scan, {level.levelID} - {parsedLoudness}");

            return parsedLoudness;
        }

        private void _setReplayGain(string levelId, float rg) {
            if (_results.ContainsKey(levelId)) {
                _results.Remove(levelId);
            }

            _results.Add(levelId, rg);
            File.WriteAllText(ScanResultsPath, JsonConvert.SerializeObject(_results), Encoding.UTF8);
        }
    }
}