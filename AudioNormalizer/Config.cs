﻿using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace AudioNormalizer
{
    internal class Config
    {
        public virtual bool ScanOnSongsLoaded { get; set; } // Must be 'virtual' if you want BSIPA to detect a value change and save the config automatically.
        public virtual bool ClipPrevention { get; set; } = true;
    }
}

