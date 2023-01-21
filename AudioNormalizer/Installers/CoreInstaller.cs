using AudioNormalizer.HarmonyPatches;
using AudioNormalizer.Managers;
using Zenject;

namespace AudioNormalizer.Installers
{
    internal class CoreInstaller : Installer
    {
        private readonly Config _config;

        public CoreInstaller(Config config)
        {
            _config = config;
        }
        
        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();
            Container.BindInterfacesAndSelfTo<ScannerManager>().AsSingle();
            Container.BindInterfacesTo<PerceivedLoudnessPatch>().AsSingle();
        }
    }
}