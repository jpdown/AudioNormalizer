using System.Linq;
using BSReplayGain.HarmonyPatches;
using BSReplayGain.Managers;
using BSReplayGain.UI;
using Zenject;

namespace BSReplayGain.Installers
{
    internal class ReplayGainMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            // Install patch if MultiplayerCore is installed
            if (IPA.Loader.PluginManager.EnabledPlugins.Any(x => x.Id == "MultiplayerCore")) {
                Container.BindInterfacesTo<MultiplayerDownloadPatch>().AsSingle();
            }

            Container.Bind<MenuButtonView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<BSReplayGainFlowCoordinator>()
                .FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
        }
    }
}