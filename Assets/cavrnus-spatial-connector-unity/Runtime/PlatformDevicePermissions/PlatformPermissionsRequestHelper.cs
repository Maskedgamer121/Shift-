using System.Collections;
using Cavrnus.SpatialConnector.Core;
using UnityEngine;
# if PLATFORM_ANDROID
using UnityEngine.Android;
# endif 

namespace Cavrnus.SpatialConnector.PlatformPermissions
{
	public static class PlatformPermissionsRequestHelper
    {
        public static void RequestPermissions(bool disableVoice, bool disableVideo)
        {
            CavrnusStatics.Scheduler.ExecCoRoutine(DefaultPermissionsRoutine(disableVoice, disableVideo));

# if PLATFORM_ANDROID
            new GameObject("AndroidPermissionRequester").AddComponent<CavrnusAndroidPermissionRequester>();
#endif
        }

        private static IEnumerator DefaultPermissionsRoutine(bool disableAudio, bool disableVideo)
        {
            if (disableAudio && disableVideo)
            {
                Debug.Log("No permissions requested: Both audio and video are disabled");
                yield break;
            }

            UserAuthorization userAuthorization = 0;

            if (!disableAudio)
                userAuthorization |= UserAuthorization.Microphone;

            if (!disableVideo)
                userAuthorization |= UserAuthorization.WebCam;

            yield return Application.RequestUserAuthorization(userAuthorization);
        }
    }
}