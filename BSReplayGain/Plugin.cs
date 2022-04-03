using BSReplayGain.Installers;
using IPA;
using IPALogger = IPA.Logging.Logger;
using SiraUtil.Zenject;

namespace BSReplayGain
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            zenjector.UseLogger(logger);
            zenjector.Install<ReplayGainCoreInstaller>(Location.App);
            zenjector.Install<ReplayGainMenuInstaller>(Location.Menu);
        }
    }
}
