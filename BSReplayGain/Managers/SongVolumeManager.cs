using System;
using BSReplayGain.Models;
using UnityEngine;
using Zenject;
using SiraUtil.Logging;

namespace BSReplayGain.Managers
{
    internal class SongVolumeManager : IInitializable
    {
        private readonly GameplayCoreSceneSetupData _sceneSetupData;
        private readonly AudioTimeSyncController _audioTimeSyncController;
        private readonly SiraLog _log;
        private readonly ReplayGainManager _replayGainManager;

        public SongVolumeManager(
            GameplayCoreSceneSetupData sceneSetupData, 
            AudioTimeSyncController audioTimeSyncController, 
            SiraLog log,
            ReplayGainManager replayGainManager
            )
        {
            _sceneSetupData = sceneSetupData;
            _audioTimeSyncController = audioTimeSyncController;
            _log = log;
            _replayGainManager = replayGainManager;
        }

        public void Initialize()
        {
            var previewBeatmapLevel = _sceneSetupData.previewBeatmapLevel;
            if (!(previewBeatmapLevel is CustomPreviewBeatmapLevel customLevel)) return;
            var src = _audioTimeSyncController.GetComponent<AudioSource>();
            
            var replayGain = _replayGainManager.GetReplayGain(customLevel);
            _log.Info(replayGain == null);
            if (!(replayGain is { } rg)) {
                _log.Info("Starting Coroutine");
                SharedCoroutineStarter.instance.StartCoroutine(_replayGainManager.ScanSong(customLevel));
                return;
            }
            _log.Info($"Using gain {rg.Gain} peak {rg.Peak}");

            var scale = Math.Pow(10, rg.Gain / 20);
            scale = Math.Min(scale, 1 / rg.Peak); // Clipping prevention
            _log.Info("Setting volume to: " + scale);
            src.volume = (float)scale;
        }
    }
}
