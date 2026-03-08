using UnityEngine;

namespace Cavrnus.SpatialConnector.Avatars
{
	[RequireComponent(typeof(Animator))]
    public class CavrnusAvatarAnimation : MonoBehaviour
    {
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private float groundedRadius = 0.5f;
        [SerializeField] private float groundedOffset = -0.14f;

        [Space]
        [SerializeField] private float smoothingFactor = 2f;
        [SerializeField] private float acceleration = 2f;
        [SerializeField] private float deceleration = 5f;

        private float currentSpeed;
        private float velocity;
        private Vector3 movement;

        private Animator animator;
        private readonly int animIDSpeed = Animator.StringToHash("Speed");
        private readonly int animIDInAir = Animator.StringToHash("InAir");
        private readonly int animIDGrounded = Animator.StringToHash("Grounded");
        private readonly int animIDInputHorizontal = Animator.StringToHash("InputHorizontal");
        private readonly int animIDInputForward = Animator.StringToHash("InputForward");

        private void Awake()
        {
            animator = GetComponent<Animator>();
            previousPosition = transform.position;
        }

        private Vector3 previousPosition;
        private Vector3 movementDirection;
        private Vector3 smoothedMovementDirection;

        private void Update()
        {
            var currentPosition = transform.position;
            movementDirection = currentPosition - previousPosition;
            
            var speed = movementDirection.magnitude / Time.deltaTime;
            currentSpeed = Mathf.SmoothDamp(currentSpeed, speed, ref velocity, acceleration, Mathf.Infinity, Time.deltaTime * deceleration);
            
            // Interpolate the movement direction vector
            smoothedMovementDirection = Vector3.Slerp(smoothedMovementDirection, movementDirection, smoothingFactor * Time.deltaTime);

            animator.SetFloat(animIDInputHorizontal, smoothedMovementDirection.x);
            animator.SetFloat(animIDInputForward, smoothedMovementDirection.z);

            animator.SetFloat(animIDSpeed, currentSpeed);

            var isGrounded = IsGrounded();
            animator.SetBool(animIDGrounded, isGrounded);
            animator.SetBool(animIDInAir, !isGrounded);

            previousPosition = currentPosition;
        }

        private bool IsGrounded()
        {
            var spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
            return Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
        }

        private void OnDrawGizmosSelected()
        {
            var transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            var transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = IsGrounded() ? transparentGreen : transparentRed;

            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z), groundedRadius);
        }
    }
}