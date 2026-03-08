using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Properties.Sync;
using UnityEngine;

namespace Cavrnus.SpatialConnector
{
	public class CavrnusSpawnedObjectFlag : MonoBehaviour
	{
		public CavrnusSpawnedObject SpawnedObject;

		public void Init(CavrnusSpawnedObject spawnedObject)
		{
			SpawnedObject = spawnedObject;

			foreach(var container in gameObject.GetComponentsInChildren<CavrnusPropertiesContainer>())
			{
                container.UniqueContainerName = container.UniqueContainerName.Insert(0, spawnedObject.PropertiesContainerName + "/");
            }
        }
	}
}