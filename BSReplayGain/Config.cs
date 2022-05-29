using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace BSReplayGain
{
    internal class Config
    {
        public virtual bool ScanOnSongsLoaded { get; set; } = false; // Must be 'virtual' if you want BSIPA to detect a value change and save the config automatically.
    }
}

