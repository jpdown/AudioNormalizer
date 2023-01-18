using System.IO;
using BSReplayGain.Installers;
using IPA;
using IPAConfig = IPA.Config.Config;
using IPA.Config.Stores;
using IPA.Utilities;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace BSReplayGain
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            zenjector.UseLogger(logger);
            Directory.CreateDirectory(Path.Combine(UnityGame.UserDataPath, nameof(BSReplayGain)));
            var config = IPAConfig
                .GetConfigFor(nameof(BSReplayGain) + Path.DirectorySeparatorChar + nameof(BSReplayGain))
                .Generated<Config>();
            zenjector.Install<ReplayGainCoreInstaller>(Location.App, config);
            zenjector.Install<ReplayGainMenuInstaller>(Location.Menu);
            zenjector.Install<ReplayGainGameInstaller>(Location.GameCore);
        }
    }
}