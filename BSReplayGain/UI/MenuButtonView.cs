using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BSReplayGain.Managers;
using SiraUtil.Logging;
using SongCore;
using Zenject;

namespace BSReplayGain.UI
{
    [HotReload(RelativePathToLayout = @"./MenuButtonView.bsml")]
    [ViewDefinition("BSReplayGain.UI.MenuButtonView.bsml")]
    internal class MenuButtonView : BSMLAutomaticViewController
    {
        private SiraLog _log;
        private ReplayGainManager _replayGainManager;
        private Config _config;

        private string _scanStatus = "";

        [UIValue("scan-on-load")]
        private bool scanOnLoad
        {
            get => _config.ScanOnSongsLoaded;
            set => _config.ScanOnSongsLoaded = value;
        }

        [UIValue("scan-status")]
        public string ScanStatus
        {
            get => _scanStatus;
            set
            {
                if (_scanStatus == value)
                {
                    return;
                }

                _scanStatus = value;
                NotifyPropertyChanged();
            }
        }

        [Inject]
        public void Construct(SiraLog log, ReplayGainManager replayGainManager, Config config)
        {
            _log = log;
            _replayGainManager = replayGainManager;
            _replayGainManager.ScanFinished += _updateScanStatus;
            _config = config;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            ScanStatus =
                $"Songs Scanned: {_replayGainManager.NumScannedSongs()} / {Loader.CustomLevels.Count}";
            NotifyPropertyChanged();
        }

        [UIAction("yep-sure")]
        private void ScanAllClicked()
        {
            _replayGainManager.ScanAllSongs();
        }

        private void _updateScanStatus(int newScanned)
        {
            ScanStatus =
                $"Songs Scanned: {_replayGainManager.NumScannedSongs()} / {Loader.CustomLevels.Count}";
            NotifyPropertyChanged();
        }
    }
}