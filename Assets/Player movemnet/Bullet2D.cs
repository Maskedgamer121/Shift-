using UnityEngine;

/// <summary>
/// Attach to your Bullet prefab.
/// Moves in a set direction and destroys itself after 5 seconds.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet2D : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f; // Disappears after 5 seconds

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void SetDirection(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;
        Destroy(gameObject);
    }
}