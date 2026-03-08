using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace Cavrnus.SpatialConnector.PlayerControllers
{
	public class CavrnusPlayerFlyController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float lookSpeedX = 2f;
        [SerializeField] private float lookSpeedY = 2f;

        private bool isRotating = false;

        private void Update()
        {
            if (SimpleCavrnusInput.IsMouseDown(SimpleCavrnusInput.MouseButton.Right)) {
                isRotating = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (SimpleCavrnusInput.IsMouseUp(SimpleCavrnusInput.MouseButton.Right)) {
                isRotating = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (isRotating) {
                var rotationX = Input.GetAxis("Mouse X") * lookSpeedX;
                var rotationY = -Input.GetAxis("Mouse Y") * lookSpeedY;

                transform.Rotate(Vector3.up, rotationX, Space.World);
                transform.Rotate(Vector3.right, rotationY, Space.Self);
            }

            var translationX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
            var translationZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

            var translationY = 0f;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (SimpleCavrnusInput.IsKeyDown(Key.E))
#elif ENABLE_LEGACY_INPUT_MANAGER
	        if (SimpleCavrnusInput.IsKeyDown(KeyCode.E))
#else
			if (false)
#endif
	        {
		        translationY = moveSpeed * Time.deltaTime;
	        }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            else if (SimpleCavrnusInput.IsKeyDown(Key.Q))
#elif ENABLE_LEGACY_INPUT_MANAGER
	        else if (SimpleCavrnusInput.IsKeyDown(KeyCode.Q))
#else
			else if (false)
#endif
			{
				translationY = -moveSpeed * Time.deltaTime;
	        }

            transform.Translate(new Vector3(translationX, translationY, translationZ), Space.Self);
        }
    }
}