using System.Linq;
using AudioNormalizer.HarmonyPatches;
using AudioNormalizer.Managers;
using AudioNormalizer.UI;
using IPA.Loader;
using Zenject;

namespace AudioNormalizer.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            // Install patch if MultiplayerCore is installed
            if (PluginManager.EnabledPlugins.Any(x => x.Id == "MultiplayerCore"))
            {
                Container.BindInterfacesTo<MultiplayerDownloadPatch>().AsSingle();
            }

            Container.Bind<MenuButtonView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<MenuFlowCoordinator>()
                .FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
        }
    }
}