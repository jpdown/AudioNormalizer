using BSReplayGain.Managers;
using Zenject;

namespace BSReplayGain.Installers
{
    internal class ReplayGainGameInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<SongVolumeManager>().AsSingle();
        }
    }
}
