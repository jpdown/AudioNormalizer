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
            var source = _audioTimeSyncController.GetComponent<AudioSource>();
            
            var replayGain = _replayGainManager.GetReplayGain(customLevel);
            if (!(replayGain is { } rg)) {
                _log.Debug("Starting Coroutine");
                SharedCoroutineStarter.instance.StartCoroutine(_replayGainManager.ScanSong(customLevel, source));
                return;
            }

            _log.Debug($"Using gain {rg.Gain} peak {rg.Peak}");
            _replayGainManager.SetVolume(rg, source);
        }
    }
}
