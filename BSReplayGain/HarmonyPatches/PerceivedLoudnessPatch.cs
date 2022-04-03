using BSReplayGain.Managers;
using BSReplayGain.Models;
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
            // TODO use peak value for clipping prevention
            var replayGain = _rgManager.GetReplayGain(levelId);
            if ((replayGain is { } rg)) {
                _log.Debug($"Has ReplayGain, returning {rg.Loudness} for {levelId}");
                __result = rg.Loudness;
            }
            _log.Debug($"No ReplayGain, falling back to default for {levelId}");
            // Start scan for next time this song is encountered
            var level = SongCore.Loader.GetLevelById(levelId);
            if (level is CustomPreviewBeatmapLevel customLevel) {
                SharedCoroutineStarter.instance.StartCoroutine(_rgManager.ScanSong(customLevel));
            }
        }
    }
}