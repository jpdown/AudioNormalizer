using System.Threading.Tasks;
using BSReplayGain.Managers;
using MultiplayerCore.Objects;
using SiraUtil.Affinity;
using SiraUtil.Logging;

namespace BSReplayGain.HarmonyPatches {
    public class MultiplayerDownloadPatch : IAffinity {
        private readonly ReplayGainManager _rgManager;
        private readonly SiraLog _log;

        public MultiplayerDownloadPatch(ReplayGainManager rgManager, SiraLog log) {
            _rgManager = rgManager;
            _log = log;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MpLevelLoader),
            nameof(MpLevelLoader.LoadLevel))]
        internal void Postfix(ILevelGameplaySetupData gameplaySetupData, float initialStartTime, 
            ref Task<BeatmapLevelsModel.GetBeatmapLevelResult> ____getBeatmapLevelResultTask) {
            _log.Debug("Overriding task");
            var sameTask = ____getBeatmapLevelResultTask;
            ____getBeatmapLevelResultTask = ScanReplayGain(sameTask);
        }

        private async Task<BeatmapLevelsModel.GetBeatmapLevelResult> ScanReplayGain(
            Task<BeatmapLevelsModel.GetBeatmapLevelResult> originalTask) {
            var result = await originalTask;
            if (!result.isError && result.beatmapLevel is CustomPreviewBeatmapLevel customLevel 
                                && !(_rgManager.GetReplayGain(customLevel.levelID) is { })) {
                _log.Debug("Scanning RG");
                await _rgManager.ScanSong(customLevel);
            }
            
            _log.Debug("Returning");
            return result;
        }
    }
}