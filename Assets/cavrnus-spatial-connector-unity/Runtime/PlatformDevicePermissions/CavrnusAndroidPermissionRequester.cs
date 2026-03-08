using System;
using System.Collections;
using UnityEngine;
# if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

namespace Cavrnus.SpatialConnector.PlatformPermissions
{
    public class CavrnusAndroidPermissionRequester : MonoBehaviour
    {
# if PLATFORM_ANDROID
        /*
         * Android permissions suggest to ask the user a second time if they choose to deny
         * the first time. Source:
         * https://docs.unity3d.com/Manual/android-RequestingPermissions.html
         */
        private const int WIDTH = 500;
        private const int HEIGHT = 300;
        private const int BTN_WIDTH = 100;
        private const int BTN_HEIGHT = 100;

        private const string MESSAGE =
            "In order to use Audio/Video in this app, access to microphone and camera input from your device is needed";

        private Action androidSuccessfulPermissionAction;
        private Action androidUnsuccessfulPermissionAction;
        private bool onPopup = false;
        private bool windowOpen = false;
        private bool audioOnly = false;

        private void Start()
        {
            StartCoroutine(RequestPermissions());
        }

        /// <summary>
        /// (Android) Creates a window that tells the user why camera and microphone permissions
        /// are needed.
        /// </summary>
        /// <param name="windowID"></param>
        void DoWindow(int windowID)
        {
            GUI.Label(new Rect(10, 20, WIDTH, HEIGHT), MESSAGE);
            GUI.Button(new Rect(WIDTH - (BTN_WIDTH + 10), HEIGHT - BTN_HEIGHT, BTN_WIDTH, BTN_HEIGHT), "No");

            if (GUI.Button(new Rect(10, HEIGHT - BTN_HEIGHT, BTN_WIDTH, BTN_HEIGHT), "Yes"))
            {
                StartCoroutine(RequestPermissions());
            }
        }

        public void OnGUI()
        {
            if (windowOpen)
            {
                Rect rect = new Rect((Screen.width / 2) - (WIDTH / 2), (Screen.height / 2) - (HEIGHT / 2), WIDTH,
                    HEIGHT);
                GUI.ModalWindow(0, rect, DoWindow, "Permissions Request Dialog");
            }
        }

        // (Android) focus returns false if application is on dialog
        // idea source: https://forum.unity.com/threads/the-android-permission-class-needs-serious-improvements.705770/
        private void OnApplicationFocus(bool focus)
        {
            onPopup = !focus;
        }

        /// <summary>
        /// (Android) Checks if Android permissions are permitted
        /// </summary>
        /// <returns> True if both microphone and camera access is enabled, depending if its audio only </returns>
        private bool CanUseAndroidPermissions()
        {
            if (!audioOnly)
            {
                return Permission.HasUserAuthorizedPermission(Permission.Microphone)
                       && Permission.HasUserAuthorizedPermission(Permission.Camera);
            }
            else
            {
                return Permission.HasUserAuthorizedPermission(Permission.Microphone);
            }
        }

        /// <summary>
        /// (Android) Requests Android permission given the permission string
        /// </summary>
        /// <param name="permission"></param>
        private IEnumerator CheckForPermission(string permission)
        {
            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                onPopup = true;
                while (onPopup)
                {
                    Debug.Log("Requesting for the following permission " + permission);
                    Permission.RequestUserPermission(permission);
                    yield return null;
                }
            }
        }

        /// <summary>
        /// (Android) Fetches and checks Android permissions
        /// </summary>
        public IEnumerator GetAndroidPermissions()
        {
            if (CanUseAndroidPermissions() == false)
            {
                if (!audioOnly)
                {
                    Debug.Log("Requesting webcam");
                    yield return CheckForPermission(Permission.Camera);
                }

                Debug.Log("Requesting microphone");
                yield return CheckForPermission(Permission.Microphone);
            }
        }

        /// <summary>
        /// Debug camera detection after permissions are granted
        /// </summary>
        private IEnumerator DebugCameraAfterPermissions()
        {
            Debug.Log("=== Starting Camera Debug ===");

            // Wait longer for Android to fully process the permission grant
            yield return new WaitForSeconds(1.0f);

            Debug.Log("=== Camera Debug Info ===");
            Debug.Log($"Camera Permission: {Permission.HasUserAuthorizedPermission(Permission.Camera)}");
            Debug.Log($"Microphone Permission: {Permission.HasUserAuthorizedPermission(Permission.Microphone)}");

            // Only check Unity cameras if we have permission
            if (Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Debug.Log("Permission confirmed, checking Unity camera devices...");

                // Additional small delay before accessing WebCamTexture
                yield return new WaitForSeconds(0.5f);

                try
                {
                    var unityDevices = WebCamTexture.devices;
                    Debug.Log($"Unity found {unityDevices.Length} camera devices");

                    for (int i = 0; i < unityDevices.Length; i++)
                    {
                        Debug.Log($"Unity Camera {i}: {unityDevices[i].name}, Front: {unityDevices[i].isFrontFacing}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error accessing Unity camera devices: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("Camera permission not granted, skipping Unity camera check");
            }

            Debug.Log("=== Camera Debug Complete ===");
        }

        /// <summary>
        /// Handle successful permissions with proper timing
        /// </summary>
        private void OnPermissionsGranted()
        {
            Debug.Log("Permissions granted, starting camera initialization");
            StartCoroutine(InitializeCameraAfterPermissions());
        }

        /// <summary>
        /// Initialize camera after permissions with proper delays
        /// </summary>
        private IEnumerator InitializeCameraAfterPermissions()
        {
            // Wait a bit for Android to fully process the permission grant
            yield return new WaitForSeconds(0.5f);

            // Debug camera detection
            yield return DebugCameraAfterPermissions();

            // Call the original success action
            if (androidSuccessfulPermissionAction != null)
            {
                androidSuccessfulPermissionAction();
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// (Android) Overloaded RequestPermissions() method for Android to request a second time
        /// if the first request was unsuccessful
        /// </summary>
        /// <returns></returns>
        public IEnumerator RequestPermissions()
        {
            yield return GetAndroidPermissions();

            if (CanUseAndroidPermissions())
            {
                Debug.Log("User has granted permissions");
                OnPermissionsGranted();
            }
            else
            {
                Debug.Log("Unable to get permissions");
                if (androidUnsuccessfulPermissionAction != null)
                {
                    androidUnsuccessfulPermissionAction();
                }
            }
        }
#endif
    }
}