using Cavrnus.EngineConnector;
using Cavrnus.SpatialConnector.Setup;
using Cavrnus.SpatialConnector.UI;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Avatars
{
	public class CavrnusAvatarTag : MonoBehaviour
    {
        [SerializeField] private WidgetUserProfileImage profileImage;
        
        private Camera mainCam;

        private void Start()
		{
			mainCam = Camera.main;
			if (mainCam == null)
				Debug.LogWarning("Missing main cam in scene!");

            var userFlag = gameObject.GetComponentInAllParents<CavrnusUserFlag>();
            //Stop matching them up when the menu is destroyed
            
            userFlag.AwaitUser(user =>
            {
				profileImage.Setup(user);
            });
		}

		private void Update()
        {
            if (mainCam != null)
            {
                var dir = transform.position - mainCam.transform.position;
                dir.y = 0;
                transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            }
        }
    }
}