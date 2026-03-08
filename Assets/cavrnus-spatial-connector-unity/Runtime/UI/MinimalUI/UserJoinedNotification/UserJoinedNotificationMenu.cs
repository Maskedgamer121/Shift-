using Cavrnus.SpatialConnector.API;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
	public class UserJoinedNotificationMenu : MonoBehaviour
    {
        [SerializeField] private UserJoinedNotificationEntry userJoinEntryPrefab;
        
        private void Start()
        {
            CavrnusFunctionLibrary.AwaitAnySpaceConnection(sc => {
                sc.BindSpaceUsers(OnUserAdded, OnUserRemoved);
            });
        }
        
        private void OnUserAdded(CavrnusUser user)
        {
            if (user.IsLocalUser) 
                return;

            Instantiate(userJoinEntryPrefab, transform).Setup(user, true);
        }

        private void OnUserRemoved(CavrnusUser user)
        {
            if (user.IsLocalUser) 
                return;

            Instantiate(userJoinEntryPrefab, transform).Setup(user, false);
        }
    }
}