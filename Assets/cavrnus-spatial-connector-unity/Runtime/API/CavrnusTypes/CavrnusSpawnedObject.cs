using UnityEngine;

namespace Cavrnus.SpatialConnector.API
{
	/// <summary>
	/// Represents an instantiation of content noted in the Cavrnus Space's Journal. Provides easy access to the object's property container name and the instantiated object.
	/// </summary>
	public class CavrnusSpawnedObject
	{
		public string PropertiesContainerName;
		public GameObject SpawnedObjectInstance;

		internal string CreationOpId;

		internal CavrnusSpaceConnection spaceConnection;

		internal CavrnusSpawnedObject(string propsContainerName, GameObject ob, string creationOpId, CavrnusSpaceConnection spaceConn)
		{
			spaceConnection = spaceConn;
			PropertiesContainerName = propsContainerName;
			SpawnedObjectInstance = ob;
			CreationOpId = creationOpId;
		}
	}
}