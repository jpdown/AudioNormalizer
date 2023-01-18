using BSReplayGain.HarmonyPatches;
using Zenject;

namespace BSReplayGain.Installers
{
    public class ReplayGainGameInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<SongPreviewPlayerPatch>().AsSingle();
        } 
    }
}