using System;
using UnityEngine;
using Zenject;
using SiraUtil.Logging;

namespace BSReplayGain.Managers
{
    internal class SongVolumeManager : IInitializable
    {
        private readonly GameplayCoreSceneSetupData _sceneSetupData;
        private readonly SiraLog _log;

        public SongVolumeManager(GameplayCoreSceneSetupData _sceneSetupData, SiraLog _log)
        {
            this._sceneSetupData = _sceneSetupData;
            this._log = _log;
        }

        public void Initialize()
        {
            _log.Info("In SongVolumeManager");
        }
    }
}
