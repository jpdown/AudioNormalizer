using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using Zenject;

namespace BSReplayGain.Managers {
    public class MenuButtonManager : IInitializable, IDisposable {
        private readonly MenuButton _menuButton;
        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly BSReplayGainFlowCoordinator _flowCoordinator;

        public MenuButtonManager(MainFlowCoordinator mainFlowCoordinator, BSReplayGainFlowCoordinator flowCoordinator) {
            _mainFlowCoordinator = mainFlowCoordinator;
            _flowCoordinator = flowCoordinator;
            _menuButton = new MenuButton("BSReplayGain", ShowFlowCoordinator);
        }

        public void Initialize() {
            MenuButtons.instance.RegisterButton(_menuButton);
        }

        public void Dispose() {
            MenuButtons.instance.UnregisterButton(_menuButton);
        }

        private void ShowFlowCoordinator() {
            _mainFlowCoordinator.PresentFlowCoordinator(_flowCoordinator);
        }
    }
}