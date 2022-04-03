using System.Linq;
using BSReplayGain.HarmonyPatches;
using BSReplayGain.Managers;
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
        }
    }
}