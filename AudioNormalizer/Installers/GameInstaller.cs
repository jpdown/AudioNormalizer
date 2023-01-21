using AudioNormalizer.HarmonyPatches;
using Zenject;

namespace AudioNormalizer.Installers
{
    public class GameInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<SongPreviewPlayerPatch>().AsSingle();
        } 
    }
}