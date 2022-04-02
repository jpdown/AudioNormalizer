using System;
using UnityEngine;
using Zenject;
using SiraUtil.Logging;
using TagLib;
using static System.Double;

namespace BSReplayGain.Managers
{
    internal class SongVolumeManager : IInitializable
    {
        private readonly GameplayCoreSceneSetupData _sceneSetupData;
        private readonly AudioTimeSyncController _audioTimeSyncController;
        private readonly SiraLog _log;

        public SongVolumeManager(GameplayCoreSceneSetupData sceneSetupData, AudioTimeSyncController audioTimeSyncController, SiraLog log)
        {
            _sceneSetupData = sceneSetupData;
            _audioTimeSyncController = audioTimeSyncController;
            _log = log;
        }

        public void Initialize()
        {
            var previewBeatmapLevel = _sceneSetupData.previewBeatmapLevel;
            if (!(previewBeatmapLevel is CustomPreviewBeatmapLevel customLevel)) return;
            var src = _audioTimeSyncController.GetComponent<AudioSource>();
            
            var tfile = TagLib.File.Create(customLevel.songPreviewAudioClipPath, "taglib/ogg", ReadStyle.Average);
            var rg = tfile.Tag.ReplayGainTrackGain;
            var peak = tfile.Tag.ReplayGainTrackPeak;

            if (IsNaN(rg)) return;
            var scale = Math.Pow(10, rg / 20);
            scale = Math.Min(scale, 1 / peak); // Clipping prevention
            _log.Info("Setting volume to: " + scale);
            src.volume = (float)scale;
        }
    }
}
