using BSReplayGain.Managers;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using SongCore;

namespace BSReplayGain.HarmonyPatches
{
    internal class PerceivedLoudnessPatch : IAffinity
    {
        private readonly SiraLog _log;
        private readonly ReplayGainManager _rgManager;

        public PerceivedLoudnessPatch(ReplayGainManager rgManager, SiraLog log)
        {
            _rgManager = rgManager;
            _log = log;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(PerceivedLoudnessPerLevelModel),
            nameof(PerceivedLoudnessPerLevelModel.GetLoudnessByLevelId))]
        internal void Postfix(string levelId, ref float __result)
        {
            var loudness = _rgManager.GetLoudness(levelId);
            if (loudness != null)
            {
                _log.Debug($"Has ReplayGain, returning {loudness} for {levelId}");
                __result = (float)loudness;
                return;
            }

            _log.Debug($"No ReplayGain, falling back to default for {levelId}");
            // Start scan for next time this song is encountered
            var level = Loader.GetLevelById(levelId);
            if (level is CustomPreviewBeatmapLevel customLevel)
            {
                _rgManager.QueueScanSong(customLevel);
            }
        }
    }
}