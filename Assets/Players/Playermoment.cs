using UnityEngine;

/// <summary>
/// Simple 2D top-down movement using WASD or Arrow Keys.
/// Attach to your Player GameObject.
/// Requires a Rigidbody2D component on the same GameObject.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Disable gravity so the player doesn't fall
        rb.gravityScale = 0f;
    }

    private void Update()
    {
        // Read WASD or Arrow Key input
        float horizontal = Input.GetAxisRaw("Horizontal"); // A / D
        float vertical   = Input.GetAxisRaw("Vertical");   // W / S

        moveInput = new Vector2(horizontal, vertical).normalized;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }
}