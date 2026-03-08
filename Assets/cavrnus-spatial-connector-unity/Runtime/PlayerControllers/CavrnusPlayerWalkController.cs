using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace Cavrnus.SpatialConnector.PlayerControllers
{
	public class CavrnusPlayerWalkController : MonoBehaviour
    {
        [Space]
        [SerializeField] private float walkSpeed = 7.5f;
        [SerializeField] private float runningSpeed = 11.5f;
        [SerializeField] private float lookSpeed = 2.0f;
        
        [Space]
        [SerializeField] private float gravity = -20.0f;

        [Space]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float lookXLimit = 45.0f;

        private CharacterController characterController;
        private bool grounded;
        private Vector3 moveDirection = Vector3.zero;
        
        private float rotationX = 0;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            var input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            var forward = transform.TransformDirection(Vector3.forward);
            var right = transform.TransformDirection(Vector3.right);

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var isRunning = SimpleCavrnusInput.IsKeyDown(Key.LeftShift);
#elif ENABLE_LEGACY_INPUT_MANAGER
			var isRunning = SimpleCavrnusInput.IsKeyDown(KeyCode.LeftShift);
#else
			var isRunning = false;
#endif

            var curSpeedHorizontal = (isRunning ? runningSpeed : walkSpeed) * input.x;
            var curSpeedForward = (isRunning ? runningSpeed : walkSpeed) * input.y;
            moveDirection = (forward * curSpeedForward) + (right * curSpeedHorizontal);

            if (!characterController.isGrounded)
                moveDirection.y += gravity * Time.deltaTime;

            characterController.Move(moveDirection * Time.deltaTime);

            if (SimpleCavrnusInput.IsMouseDown(SimpleCavrnusInput.MouseButton.Right))
            {
                rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
                rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
                playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
                transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
            }
        }
    }
}