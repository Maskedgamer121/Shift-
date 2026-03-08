using Cavrnus.Base.Settings;
using Cavrnus.SpatialConnector.Core;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Properties.Sync;
using Cavrnus.EngineConnector;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Setup
{
	public class CavrnusLocalUserFlag : CavrnusUserFlag
	{
	    private void Start()
	    {
			CavrnusFunctionLibrary.AwaitAnySpaceConnection(spaceConn =>
			{
				if (HelperFunctions.NullOrDestroyed(this))	return;

				spaceConn.AwaitLocalUser(localUser =>
				{
					if (HelperFunctions.NullOrDestroyed(this)) return;

					User = localUser;

					localUser.ContainerIdSetting.Bind(lucid =>
					{
						CavrnusPropertyHelpers.ResetLiveHierarchyRootName(gameObject, $"{lucid}");

						foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<CavrnusTransformData>>())
						{
							sync.Setup();
							//Force init user props
							if (sync.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName.StartsWith(lucid))
								sync.ForceBeginTransientUpdate();
						}

						foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<bool>>())
						{
							sync.Setup();
							//Force init user props
							if (sync.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName.StartsWith(lucid))
								sync.ForceBeginTransientUpdate();
						}

						foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<float>>())
						{
							sync.Setup();
							//Force init user props
							if (sync.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName.StartsWith(lucid))
								sync.ForceBeginTransientUpdate();
						}

						foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<Color>>())
						{
							sync.Setup();
							//Force init user props
							if (sync.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName.StartsWith(lucid))
								sync.ForceBeginTransientUpdate();
						}

						foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<Vector4>>())
						{
							sync.Setup();
							//Force init user props
							if (sync.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName.StartsWith(lucid))
								sync.ForceBeginTransientUpdate();
						}

						foreach (var sync in gameObject.GetComponentsInChildren<CavrnusValueSync<string>>())
						{
							sync.Setup();
							//Force init user props
							if (sync.GetComponent<CavrnusPropertiesContainer>().UniqueContainerName.StartsWith(lucid))
								sync.ForceBeginTransientUpdate();
						}
					});
				});				
			});
		}
	}
}