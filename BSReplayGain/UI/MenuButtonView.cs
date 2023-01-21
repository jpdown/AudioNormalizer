using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BSReplayGain.Managers;
using SongCore;
using Zenject;

namespace BSReplayGain.UI
{
    [HotReload(RelativePathToLayout = @"./MenuButtonView.bsml")]
    [ViewDefinition("BSReplayGain.UI.MenuButtonView.bsml")]
    internal class MenuButtonView : BSMLAutomaticViewController
    {
        private ReplayGainManager _replayGainManager = null!;
        private Config _config = null!;

        private string _scanStatus = "";

        [UIValue("scan-on-load")]
        private bool ScanOnLoad
        {
            get => _config.ScanOnSongsLoaded;
            set => _config.ScanOnSongsLoaded = value;
        }
        
        [UIValue("clipping-prevention")]
        private bool ClippingPrevention
        {
            get => _config.ClipPrevention;
            set => _config.ClipPrevention = value;
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
        public void Construct(ReplayGainManager replayGainManager, Config config)
        {
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