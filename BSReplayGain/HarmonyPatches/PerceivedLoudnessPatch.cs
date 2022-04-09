using BSReplayGain.Managers;
using SiraUtil.Affinity;
using SiraUtil.Logging;

namespace BSReplayGain.HarmonyPatches {
    public class PerceivedLoudnessPatch : IAffinity {
        private readonly ReplayGainManager _rgManager;
        private readonly SiraLog _log;

        public PerceivedLoudnessPatch(ReplayGainManager rgManager, SiraLog log) {
            _rgManager = rgManager;
            _log = log;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(PerceivedLoudnessPerLevelModel),
            nameof(PerceivedLoudnessPerLevelModel.GetLoudnessByLevelId))]
        internal void Postfix(string levelId, ref float __result) {
            var replayGain = _rgManager.GetReplayGain(levelId);
            if (replayGain is { } loudness) {
                _log.Debug($"Has ReplayGain, returning {loudness} for {levelId}");
                __result = loudness;
                return;
            }
            _log.Debug($"No ReplayGain, falling back to default for {levelId}");
            // Start scan for next time this song is encountered
            var level = SongCore.Loader.GetLevelById(levelId);
            if (level is CustomPreviewBeatmapLevel customLevel) {
                _rgManager.QueueScanSong(customLevel);
            }
        }
    }
}