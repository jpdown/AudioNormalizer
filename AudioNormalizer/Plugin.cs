using System.IO;
using AudioNormalizer.Installers;
using IPA;
using IPAConfig = IPA.Config.Config;
using IPA.Config.Stores;
using IPA.Utilities;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace AudioNormalizer
{
    [Plugin(RuntimeOptions.DynamicInit), NoEnableDisable]
    public class Plugin
    {
        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            zenjector.UseLogger(logger);
            Directory.CreateDirectory(Path.Combine(UnityGame.UserDataPath, nameof(AudioNormalizer)));
            var config = IPAConfig
                .GetConfigFor(nameof(AudioNormalizer) + Path.DirectorySeparatorChar + nameof(AudioNormalizer))
                .Generated<Config>();
            zenjector.Install<CoreInstaller>(Location.App, config);
            zenjector.Install<MenuInstaller>(Location.Menu);
            zenjector.Install<GameInstaller>(Location.GameCore);
        }
    }
}