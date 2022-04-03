using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BSReplayGain.Models;
using IPA.Utilities;
using Newtonsoft.Json;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace BSReplayGain.Managers {
    public class ReplayGainManager : IInitializable {
        private Dictionary<string, RGScan> _results;

        private SiraLog _log;
        
        private static readonly string ScanResultsDir = $"{Environment.CurrentDirectory}/UserData/BSReplayGain/";
        private static readonly string ScanResultsPath = ScanResultsDir + "scans.json";
        
        private readonly string _ffmpegPath = Path.Combine(UnityGame.LibraryPath, "ffmpeg.exe");
        private readonly float _targetLUFS = -18f; // 89 dB, ReplayGain 2.0

        public ReplayGainManager(SiraLog log) {
            _log = log;
            _results = new Dictionary<string, RGScan>();
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
                _results = JsonConvert.DeserializeObject<Dictionary<string, RGScan>>
                    (File.ReadAllText(ScanResultsPath, Encoding.UTF8)) ?? _results;
            }
        }

        public RGScan? GetReplayGain(CustomPreviewBeatmapLevel level) {
            var success = _results.TryGetValue(level.levelID, out var rg);
            return success ? rg : null;
        }

        public IEnumerator ScanSong(CustomPreviewBeatmapLevel level) {
            _log.Info("In Scan");
            var finished = false;
            var scanProcess = new Process();
            scanProcess.StartInfo.UseShellExecute = false;
            scanProcess.StartInfo.FileName = _ffmpegPath;
            scanProcess.StartInfo.RedirectStandardOutput = true;
            scanProcess.StartInfo.RedirectStandardError = true;
            scanProcess.StartInfo.Arguments =
                $"-nostats -hide_banner -i \"{level.songPreviewAudioClipPath}\" -map a:0 -filter replaygain,ebur128 -f null -";

            scanProcess.Exited += (obj, args) => finished = true;
            scanProcess.Start();
            _log.Info("Started Scan");
            scanProcess.WaitForExit(); // TODO Make this yield instead
            _log.Info("Parsing");
            
            // Parse peak and gain from output
            _log.Debug(scanProcess.StandardError.ReadToEnd());
            var reader = scanProcess.StandardOutput;
            string? peak = null, loudness = null;
            while (reader.ReadLine() is { } line) {
                _log.Debug(line);
                if (line.Contains("track_peak")) {
                    _log.Debug(line);
                    peak = line.Split().Last();
                    _log.Debug("peak " + peak);
                }
                else if (line.StartsWith("    I:")) {
                    _log.Debug(line);
                    loudness = line.Split()[line.Length - 2]; // Second last entry
                    _log.Debug("loudness " + loudness);
                    break;
                }
            }

            if (peak == null || loudness == null) {
                yield break;
            }
            
            var parsedPeak = float.Parse(peak);
            var parsedLoudness = float.Parse(loudness);
            var gain = _targetLUFS - parsedLoudness;
            
            _setReplayGain(level.levelID, new RGScan(gain, parsedPeak));
            _log.Debug($"gain: {gain}, peak: {peak}");
        }

        private void _setReplayGain(string levelId, RGScan rg) {
            if (_results.ContainsKey(levelId)) {
                _results.Remove(levelId);
            }

            _results.Add(levelId, rg);
            File.WriteAllText(ScanResultsPath, JsonConvert.SerializeObject(_results), Encoding.UTF8);
        }
    }
}