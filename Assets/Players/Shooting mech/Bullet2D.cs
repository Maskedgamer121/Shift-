using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet2D : MonoBehaviour
{
    private float damage = 25f;
    private float range = 10f;
    private Vector3 spawnPosition;
    private Rigidbody2D rb;
    private bool initialized = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void SetProperties(Vector2 direction, float speed, float lifetime, float bulletDamage, float bulletRange)
    {
        rb.linearVelocity = direction.normalized * speed;
        damage = bulletDamage;
        range = bulletRange;
        spawnPosition = transform.position;
        initialized = true;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!initialized) return;
        if (Vector3.Distance(transform.position, spawnPosition) >= range)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Bullet")) return;

        if (other.CompareTag("Enemy"))
        {
            EnemyScript enemy = other.GetComponent<EnemyScript>();
            if (enemy != null)
                enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Bullet")) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyScript enemy = collision.gameObject.GetComponent<EnemyScript>();
            if (enemy != null)
                enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}