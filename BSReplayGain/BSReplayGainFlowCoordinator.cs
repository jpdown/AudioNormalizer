using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;
using BSReplayGain.UI;

namespace BSReplayGain {
    public class BSReplayGainFlowCoordinator : FlowCoordinator {
        private MainFlowCoordinator _mainFlowCoordinator;
        private MenuButtonView _menuButtonView;

        [Inject]
        public void Construct(MainFlowCoordinator mainFlowCoordinator, MenuButtonView menuButtonView) {
            _mainFlowCoordinator = mainFlowCoordinator;
            _menuButtonView = menuButtonView;
        }
        
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (firstActivation) {
                SetTitle("BSReplayGain");
                showBackButton = true;
                
                ProvideInitialViewControllers(_menuButtonView);
            }
        }

        protected override void BackButtonWasPressed(ViewController topViewController) {
            base.BackButtonWasPressed(topViewController);
            _mainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}