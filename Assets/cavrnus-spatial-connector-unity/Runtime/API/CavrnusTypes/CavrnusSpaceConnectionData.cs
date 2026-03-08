using System;
using System.Collections.Generic;
using Cavrnus.LiveRoomSystem;
using Cavrnus.SpatialConnector.Core;
using Cavrnus.SpatialConnector.Setup;
using Cavrnus.EngineConnector;
using UnityEngine;

namespace Cavrnus.SpatialConnector.API
{
	internal class CavrnusSpaceConnectionData : IDisposable
    {
        public readonly RoomSystem RoomSystem;
        public readonly CavrnusObjectCreationHandler CreationHandler;
        private readonly IDisposable timeUpdater;
        
        public CavrnusSpaceConnectionData(RoomSystem roomSystem, List<CavrnusSpatialConnector.CavrnusSpawnableObject> spawnableObjects, CavrnusSpaceConnection sc)
        {
            RoomSystem = roomSystem;
            CreationHandler = new CavrnusObjectCreationHandler(spawnableObjects, sc);
            timeUpdater = CavrnusStatics.Scheduler.ExecInMainThreadRepeatingEachFrame(() => RoomSystem.DateTimeProperties.Update(Time.realtimeSinceStartupAsDouble));
        }

        public void Dispose()
        {
            RoomSystem?.Shutdown();
            CreationHandler?.Dispose();
            timeUpdater?.Dispose();
        }
    }
}