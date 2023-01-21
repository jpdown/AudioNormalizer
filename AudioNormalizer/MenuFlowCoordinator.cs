using BeatSaberMarkupLanguage;
using AudioNormalizer.UI;
using HMUI;
using Zenject;

namespace AudioNormalizer
{
    internal class MenuFlowCoordinator : HMUI.FlowCoordinator
    {
        private MainFlowCoordinator _mainFlowCoordinator = null!;
        private MenuButtonView _menuButtonView = null!;

        [Inject]
        public void Construct(MainFlowCoordinator mainFlowCoordinator, MenuButtonView menuButtonView)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _menuButtonView = menuButtonView;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle("AudioNormalizer");
                showBackButton = true;

                ProvideInitialViewControllers(_menuButtonView);
            }
        }

        protected override void BackButtonWasPressed(ViewController viewController)
        {
            base.BackButtonWasPressed(viewController);
            _mainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}