using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using Zenject;

namespace AudioNormalizer.Managers
{
    internal class MenuButtonManager : IInitializable, IDisposable
    {
        private readonly MenuFlowCoordinator _menuFlowCoordinator;
        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly MenuButton _menuButton;

        public MenuButtonManager(MainFlowCoordinator mainFlowCoordinator, MenuFlowCoordinator menuFlowCoordinator)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _menuFlowCoordinator = menuFlowCoordinator;
            _menuButton = new MenuButton("AudioNormalizer", ShowFlowCoordinator);
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
            _mainFlowCoordinator.PresentFlowCoordinator(_menuFlowCoordinator);
        }
    }
}