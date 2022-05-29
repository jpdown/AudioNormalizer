using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using Zenject;

namespace BSReplayGain.Managers
{
    internal class MenuButtonManager : IInitializable, IDisposable
    {
        private readonly BSReplayGainFlowCoordinator _flowCoordinator;
        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly MenuButton _menuButton;

        public MenuButtonManager(MainFlowCoordinator mainFlowCoordinator, BSReplayGainFlowCoordinator flowCoordinator)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _flowCoordinator = flowCoordinator;
            _menuButton = new MenuButton("BSReplayGain", ShowFlowCoordinator);
        }

        public void Dispose()
        {
            MenuButtons.instance.UnregisterButton(_menuButton);
        }

        public void Initialize()
        {
            MenuButtons.instance.RegisterButton(_menuButton);
        }

        private void ShowFlowCoordinator()
        {
            _mainFlowCoordinator.PresentFlowCoordinator(_flowCoordinator);
        }
    }
}