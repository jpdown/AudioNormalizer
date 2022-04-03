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
            return success ? rg : (RGScan?)null;
        }

        public IEnumerator ScanSong(CustomPreviewBeatmapLevel level, AudioSource? source) {
            var finished = false;
            string? peak = null, loudness = null;
            
            var scanProcess = new Process();
            scanProcess.StartInfo.UseShellExecute = false;
            scanProcess.StartInfo.CreateNoWindow = true;
            scanProcess.StartInfo.FileName = _ffmpegPath;
            scanProcess.StartInfo.RedirectStandardError = true; // ffmpeg outputs to stderr
            scanProcess.EnableRaisingEvents = true;
            scanProcess.StartInfo.Arguments =
                $"-nostats -hide_banner -i \"{level.songPreviewAudioClipPath}\" -map a:0 -filter replaygain,ebur128 -f null -";

            scanProcess.ErrorDataReceived += (s, e) => {
                if (e.Data.StartsWith("[Parsed_replaygain_0" ) && e.Data.Contains("track_peak")) {
                    _log.Info(e.Data);
                    peak = e.Data.Split().Last();
                    _log.Info("peak " + peak);
                }
                else if (e.Data.StartsWith("    I:")) {
                    _log.Info(e.Data);
                    var split = e.Data.Split();
                    loudness = split[split.Length - 2]; // Second last entry
                    _log.Info("loudness " + loudness);
                }
            };
            
            scanProcess.Exited += (obj, args) => finished = true;
            _log.Debug("About To Start Scan");

            scanProcess.Start();
            scanProcess.BeginErrorReadLine();
            
            _log.Info("Started Scan");
            yield return new WaitUntil(() => finished);

            if (peak == null || loudness == null) {
                yield break;
            }
            _log.Debug("After null check");
            
            var parsedPeak = float.Parse(peak);
            var parsedLoudness = float.Parse(loudness);
            var gain = _targetLUFS - parsedLoudness;

            var rg = new RGScan(gain, parsedPeak);
            _setReplayGain(level.levelID, rg);
            _log.Debug($"gain: {gain}, peak: {peak}");

            if (source is { } aSource) {
                SetVolume(rg, aSource);
            }
        }

        public void SetVolume(RGScan rg, AudioSource source) {
            var scale = Math.Pow(10, rg.Gain / 20);
            scale = Math.Min(scale, 1 / rg.Peak); // Clipping prevention
            _log.Info("Setting volume to: " + scale);
            source.volume = (float)scale;
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