using UnityEngine;

/// <summary>
/// Projectile behavior for ranged attacks with knockback
/// </summary>
public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float damage;
    private float knockbackForce;
    private Character owner;
    private Rigidbody2D rb;

    public void Initialize(Vector2 dir, float spd, float dmg, Character own, float knockback = 3f)
    {
        direction = dir;
        speed = spd;
        damage = dmg;
        owner = own;
        knockbackForce = knockback;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }

        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<CircleCollider2D>().isTrigger = true;
        }

        // Destroy after 5 seconds
        Destroy(gameObject, 5f);
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only damage the player
        if (collision.CompareTag("Player"))
        {
            Character target = collision.GetComponent<Character>();
            if (target != null && target != owner)
            {
                // Apply knockback in the direction the projectile was traveling
                target.TakeDamage(damage, direction * knockbackForce);
                Destroy(gameObject);
            }
        }
    }
}