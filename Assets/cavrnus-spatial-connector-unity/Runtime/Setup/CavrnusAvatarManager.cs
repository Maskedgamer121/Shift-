using System.Collections.Generic;
using Cavrnus.Base.Settings;
using Cavrnus.SpatialConnector.Core;
using Cavrnus.SpatialConnector.API;
using Cavrnus.SpatialConnector.Properties.Sync;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Setup
{
	public class CavrnusAvatarManager 
	{
		private GameObject remoteAvatarPrefab;
		private bool showLocalUser;

	    private Dictionary<string, GameObject> avatarInstances = new Dictionary<string, GameObject>();

		public void Setup(GameObject remoteAvatarPrefab, bool showLocalUser)
	    {
			this.remoteAvatarPrefab = remoteAvatarPrefab;
			this.showLocalUser = showLocalUser;

			if (remoteAvatarPrefab == null)
		    {
			    Debug.LogWarning("No Avatar Prefab has been assigned. Shutting down CoPresence display system.");
				return;
		    }

			CavrnusFunctionLibrary.AwaitAnySpaceConnection(OnSpaceConnection);
		}

		private CavrnusSpaceConnection cavrnusSpaceConnection = null;

		private void OnSpaceConnection(CavrnusSpaceConnection obj)
		{
			cavrnusSpaceConnection = obj;
			cavrnusSpaceConnection.BindSpaceUsers((u) => CavrnusStatics.Scheduler.ExecInMainThreadAfterFrames(3, () => UserAdded(u)), UserRemoved);
		}

		//Instantiate avatars when we get a new user
		private void UserAdded(CavrnusUser user)
		{
			//This list contains the player.  But we don't wanna show their avatar via this system.
			if (user.IsLocalUser) {
				user.ContainerIdSetting.Bind(cid=>user.SpaceConnection.BeginTransientBoolPropertyUpdate(cid, "AvatarVis", showLocalUser));
				return;
			}

			var initialTransform = user.SpaceConnection.GetTransformPropertyValue(user.ContainerId, "Transform");
			// TODO MNG: This initial transform is not necessarily updated yet. If it hasn't, then transform will probably be at the origin, then pop
			// to the avatar's position on the next proper update. This can be solved by passing through the 'invalid' transform state (null, basically)
			// but that will effect other CSC usages of transforms, since it does not currently pass that information through.
			// With that, or with a 'await valid transform' binding, the pop can be fixed. Just wait for the transform to become valid before initializing the avatar.

			var avatar = Object.Instantiate(remoteAvatarPrefab, initialTransform.Position, Quaternion.Euler(initialTransform.EulerAngles));
            avatar.AddComponent<CavrnusUserFlag>().User = user;
            user.BindUserName(n =>
            {
	            if (avatar != null) 
		            avatar.name = $"{user.ContainerId} ({n}'s Avatar)";
            });
			
			user.SpaceConnection.DefineBoolPropertyDefaultValue(user.ContainerId, "AvatarVis", false);
			user.SpaceConnection.BindBoolPropertyValue(user.ContainerId, "AvatarVis", vis => {
				if (avatar != null)
					avatar.SetActive(vis);
			});
			
            CavrnusPropertyHelpers.ResetLiveHierarchyRootName(avatar, $"{user.ContainerId}");

            foreach (var sync in avatar.GetComponentsInChildren<CavrnusValueSync<bool>>())
				sync.SendMyChanges = false;
			foreach (var sync in avatar.GetComponentsInChildren<CavrnusValueSync<float>>())
				sync.SendMyChanges = false;
			foreach (var sync in avatar.GetComponentsInChildren<CavrnusValueSync<Color>>())
				sync.SendMyChanges = false;
			foreach (var sync in avatar.GetComponentsInChildren<CavrnusValueSync<Vector4>>())
				sync.SendMyChanges = false;
			foreach (var sync in avatar.GetComponentsInChildren<CavrnusValueSync<CavrnusTransformData>>())
				sync.SendMyChanges = false;
			foreach (var sync in avatar.GetComponentsInChildren<CavrnusValueSync<string>>())
				sync.SendMyChanges = false;

			avatarInstances[user.ContainerId] = avatar;
		}

		//Destroy them when we lose that user
		private void UserRemoved(CavrnusUser user)
		{
			if (avatarInstances.ContainsKey(user.ContainerId))
			{
				Object.Destroy(avatarInstances[user.ContainerId].gameObject);
				avatarInstances.Remove(user.ContainerId);
			}
		}
	}
}