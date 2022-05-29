using BSReplayGain.Installers;
using IPA;
using IPAConfig = IPA.Config.Config;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace BSReplayGain
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        [Init]
        public Plugin(IPAConfig conf, IPALogger logger, Zenjector zenjector)
        {
            zenjector.UseLogger(logger);
            var config = conf.Generated<Config>();
            zenjector.Install<ReplayGainCoreInstaller>(Location.App, config);
            zenjector.Install<ReplayGainMenuInstaller>(Location.Menu);
        }
    }
}