using BSReplayGain.HarmonyPatches;
using BSReplayGain.Managers;
using Zenject;

namespace BSReplayGain.Installers
{
    internal class ReplayGainCoreInstaller : Installer
    {
        private readonly Config _config;

        public ReplayGainCoreInstaller(Config config)
        {
            _config = config;
        }
        
        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();
            Container.BindInterfacesAndSelfTo<ReplayGainManager>().AsSingle();
            Container.BindInterfacesTo<PerceivedLoudnessPatch>().AsSingle();
        }
    }
}