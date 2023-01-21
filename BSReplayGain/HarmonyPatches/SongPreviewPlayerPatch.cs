using System;
using SiraUtil.Affinity;
using UnityEngine;

namespace BSReplayGain.HarmonyPatches
{
    public class SongPreviewPlayerPatch : IAffinity
    {
        private GameplayCoreSceneSetupData _sceneSetupData;
        private PerceivedLoudnessPerLevelModel _perceivedLoudness;

        public SongPreviewPlayerPatch(GameplayCoreSceneSetupData sceneSetupData, PerceivedLoudnessPerLevelModel perceivedLoudness)
        {
            _sceneSetupData = sceneSetupData;
            _perceivedLoudness = perceivedLoudness;
        }
        
        [AffinityPrefix]
        [AffinityPatch(typeof(SongPreviewPlayer), nameof(SongPreviewPlayer.CrossfadeTo), argumentTypes: new[] { typeof(AudioClip), typeof(float), typeof(float), typeof(float), typeof(bool), typeof(Action) })]
        internal void OverrideMusicVolume(ref float musicVolume)
        {
            // When starting a song, the musicVolume is set based on the perceived loudness
            // However, the SongPreviewPlayer is later unloaded, setting volume to -4
            // When in a game scene, we do not want to set the music volume to default

            musicVolume =
                _perceivedLoudness.GetLoudnessCorrectionByLevelId(_sceneSetupData.previewBeatmapLevel.levelID);
        }
    }
}