using System;
using System.Collections.Generic;
using System.Linq;
using Cavrnus.Base.Settings;
using Cavrnus.Comm.Comm.LocalTypes;
using Cavrnus.Comm.LiveJournal;
using Cavrnus.Comm.Prop;
using Cavrnus.SpatialConnector;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Setup;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Core
{
	public class CavrnusObjectCreationHandler : IDisposable
	{
		public Dictionary<string, Action<CavrnusSpawnedObject, GameObject>> SpawnCallbacks = new Dictionary<string, Action<CavrnusSpawnedObject, GameObject>>();

		private List<CavrnusSpatialConnector.CavrnusSpawnableObject> spawnablePrefabs;

        private Dictionary<string, GameObject> createdObjects = new Dictionary<string, GameObject>();

		private List<IDisposable> disp = new List<IDisposable>();

		private CavrnusSpaceConnection spaceConn;

		public CavrnusObjectCreationHandler(List<CavrnusSpatialConnector.CavrnusSpawnableObject> spawnablePrefabs, CavrnusSpaceConnection spaceConn)
		{
			disp.Add(spaceConn.CurrentSpaceConnection.Bind(sc => {
				if (sc == null)
					return;
				
				var creationHandler = spaceConn.CurrentSpaceConnection.Value.RoomSystem.LiveJournal.GetObjectCreationReciever();

				this.spaceConn = spaceConn;
				this.spawnablePrefabs = spawnablePrefabs;

				var visibleObjectOps = creationHandler.GetMultiEntryWatcher<PropertyId, OpCreateObjectLive>(op => op.ObjectContextPath).ActiveOps;

				//We have the delay to allow for the new object's properties to arrive.  This way there's less weird "pop-in" of values
				disp.Add(visibleObjectOps.BindAll(
					         (_, op) => CavrnusStatics.Scheduler.ExecInMainThreadAfterFrames(3, () => ObjectCreated(op)),
                              (_, op) => ObjectRemoved(op)));
			}));
		}

		private void ObjectCreated(OpInfo<OpCreateObjectLive> createOp)
		{
			//Since we waited, this should be here if they already set it
			var initialTransform = spaceConn.GetTransformPropertyValue(createOp.Op.ObjectContextPath.ToString(), "Transform");

			if (createOp.Op.ObjectType is ContentTypeWellKnownId cId) 
			{
				if (spawnablePrefabs.Any(sp => sp.UniqueId == cId.WellKnownId)) 
				{
					var prefab = spawnablePrefabs.FirstOrDefault(sp => sp.UniqueId == cId.WellKnownId)?.Object;
					
                    var ob = GameObject.Instantiate(prefab, initialTransform.Position, Quaternion.Euler(initialTransform.EulerAngles));
					createdObjects[createOp.Op.ObjectContextPath.ToString()] = ob.gameObject;
					ob.gameObject.name = $"{createOp.Op.ObjectContextPath.ToString()} ({prefab.name})";
					
					var spawnedObject = new CavrnusSpawnedObject(createOp.Op.ObjectContextPath.ToString(), ob, createOp.Id, spaceConn);
					ob.gameObject.AddComponent<CavrnusSpawnedObjectFlag>().Init(spawnedObject);
					
					CavrnusPropertyHelpers.ResetLiveHierarchyRootName(ob.gameObject, createOp.Op.ObjectContextPath.ToString());

					var key = createOp.Op.ObjectContextPath.ToString().TrimStart('/');
					if(SpawnCallbacks.ContainsKey(key))
					{
						SpawnCallbacks[key].Invoke(spawnedObject, ob);
						SpawnCallbacks.Remove(key);
					}
				}
				else {
					Debug.LogWarning(
						$"Could not find spawnable prefab with ID {cId.WellKnownId} in the \"Cavrnus Spatial Connector\"");
				}
			}
			else if (createOp.Op.ObjectType is ContentTypeUrl cUrl) {
				Debug.LogWarning($"ContentType URL coming soon...");
			}
			else if (createOp.Op.ObjectType is ContentTypeChatEntry chat) {
				
			}
			else {
				Debug.LogWarning($"ContentType {createOp.Op.ObjectType} is not currently supported by the Cavrnus SDK.");
			}
		}

		internal void ObjectRemoved(OpInfo<OpCreateObjectLive> createOp)
		{
			if (createdObjects.ContainsKey(createOp.Op.ObjectContextPath.ToString())) {
				GameObject.Destroy(createdObjects[createOp.Op.ObjectContextPath.ToString()]);
				createdObjects.Remove(createOp.Op.ObjectContextPath.ToString());
			}
		}
		
		public void Dispose()
		{
			disp.ForEach(d => d?.Dispose());
		}
	}
}