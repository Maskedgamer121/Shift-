using UnityEngine;

public class Crasher : MonoBehaviour
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
    private PlayerHealth playerHealth;
    private float nextAttackTime;

    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerHealth = player.GetComponent<PlayerHealth>();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance <= detectionRange)
        {
            if (distance > stopDistance)
                transform.position = Vector2.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);

            if (distance <= attackRadius && Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackSpeed;
            }
        }
    }

    void Attack()
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
            Debug.Log("Enemy attacked player for " + attackDamage + " damage!");
            Die();
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log("Enemy took " + amount + " damage! HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Enemy Died!");
            Die();
        }
    }

    void Die()
    {
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, attackRadius);

        foreach (Collider2D collider in hitObjects)
        {
            if (collider.gameObject == gameObject) continue;

            if (collider.CompareTag("Player"))
            {
                PlayerHealth pHealth = collider.GetComponent<PlayerHealth>();
                if (pHealth != null)
                {
                    pHealth.TakeDamage(attackDamage);
                    Debug.Log("Player took " + attackDamage + " damage from enemy explosion!");
                }
            }
            else if (collider.CompareTag("Enemy"))
            {
                Crasher crasher = collider.GetComponent<Crasher>();
                if (crasher != null)
                {
                    crasher.TakeDamage(attackDamage);
                    Debug.Log("Enemy took " + attackDamage + " damage from explosion!");
                }
            }
        }

        Debug.Log("Enemy Exploded!");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}