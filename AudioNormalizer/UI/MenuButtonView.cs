using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using AudioNormalizer.Managers;
using SongCore;
using Zenject;

namespace AudioNormalizer.UI
{
    [HotReload(RelativePathToLayout = @"./MenuButtonView.bsml")]
    [ViewDefinition("AudioNormalizer.UI.MenuButtonView.bsml")]
    internal class MenuButtonView : BSMLAutomaticViewController
    {
        private ScannerManager _scannerManager = null!;
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
        public void Construct(ScannerManager scannerManager, Config config)
        {
            _scannerManager = scannerManager;
            _scannerManager.ScanFinished += _updateScanStatus;
            _config = config;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            ScanStatus =
                $"Songs Scanned: {_scannerManager.NumScannedSongs()} / {Loader.CustomLevels.Count}";
            NotifyPropertyChanged();
        }

        [UIAction("yep-sure")]
        private void ScanAllClicked()
        {
            _scannerManager.ScanAllSongs();
        }

        private void _updateScanStatus(int newScanned)
        {
            ScanStatus =
                $"Songs Scanned: {_scannerManager.NumScannedSongs()} / {Loader.CustomLevels.Count}";
            NotifyPropertyChanged();
        }
    }
}