using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet2D : MonoBehaviour
{
    private float damage = 25f;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void SetProperties(Vector2 direction, float speed, float lifetime, float bulletDamage)
    {
        rb.linearVelocity = direction.normalized * speed;
        damage = bulletDamage;
        Destroy(gameObject, lifetime);
    }

    // Keep old SetDirection for compatibility
    public void SetDirection(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * 20f;
        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void HandleHit(GameObject hit)
    {
        if (hit.CompareTag("Player")) return;

        if (hit.CompareTag("Enemy"))
        {
            EnemyScript enemy = hit.GetComponent<EnemyScript>();
            if (enemy != null)
                enemy.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}