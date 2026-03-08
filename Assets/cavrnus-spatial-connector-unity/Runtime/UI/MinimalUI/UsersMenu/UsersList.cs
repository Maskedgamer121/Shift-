using System.Collections.Generic;
using Cavrnus.SpatialConnector.API;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
	public class UsersList : MonoBehaviour
    {
        [SerializeField] private UsersListEntry entryPrefab;
        [SerializeField] private Transform container;

        private readonly Dictionary<string, UsersListEntry> menuInstances = new Dictionary<string, UsersListEntry>();

        private CavrnusSpaceConnection spaceConn;
        
        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                spaceConn = sc;
                spaceConn.BindSpaceUsers(UserAdded, UserRemoved);
            });
        }
        
        private void UserAdded(CavrnusUser user)
        {
            var go = Instantiate(entryPrefab, container);
            menuInstances[user.IsLocalUser ? "local" : user.ContainerId] = go;
            menuInstances[user.IsLocalUser ? "local" : user.ContainerId].Setup(user, MaximizedUserSelected);
        }

        private void MaximizedUserSelected(CavrnusUser user)
        {
            if (CavrnusMinimalUIMenuManager.Instance != null) {
                CavrnusMinimalUIMenuManager.Instance.MaximizedUserManager.LoadUser(user);
            }
        }

        private void UserRemoved(CavrnusUser user)
        {
            if (menuInstances.ContainsKey(user.IsLocalUser ? "local" : user.ContainerId)) {
                Destroy(menuInstances[user.IsLocalUser ? "local" : user.ContainerId].gameObject);
                menuInstances.Remove(user.IsLocalUser ? "local" : user.ContainerId);
            }
        }
    }
}