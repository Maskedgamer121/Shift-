using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    [Header("Settings")]
    public float detectionRange = 10f;
    public float moveSpeed = 3f;
    public float stopDistance = 1f;
    public float attackRadius = 1.5f;
    public float attackDamage = 10f;
    public float attackSpeed = 1.5f;
    public float currentHealth = 100f;

    private GameObject player;
    private float nextAttackTime;

    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            return;
        }

        // 1. Calculate distance in 2D
        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance <= detectionRange)
        {
            // 2. Move towards player (2D logic)
            if (distance > stopDistance)
            {
                // Move position without rotating the object (prevents vanishing)
                transform.position = Vector2.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
            }

            // 3. Attack logic
            if (distance <= attackRadius && Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackSpeed;
            }
        }
    }

    void Attack()
    {
        Debug.Log("Enemy Attacking Player!");
        // Optional: Damage the player here
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    // Visualize ranges in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}