using UnityEngine;

/// <summary>
/// Melee attack type - direct damage with knockback
/// </summary>
[CreateAssetMenu(fileName = "MeleeAttack", menuName = "Attack Types/Melee Attack")]
public class MeleeAttack : AttackType
{
    [SerializeField] private float knockbackForce = 5f;

    protected override void PerformAttack(Character attacker, Character target)
    {
        float finalDamage = attacker.Damage * damageMultiplier;

        // Calculate knockback direction
        Vector2 knockbackDir = (target.transform.position - attacker.transform.position).normalized;

        target.TakeDamage(finalDamage, knockbackDir * knockbackForce);
        Debug.Log($"{attacker.name} performed melee attack on {target.name} for {finalDamage} damage!");
    }
}

/// <summary>
/// Ranged attack type - shoots projectiles
/// </summary>
[CreateAssetMenu(fileName = "RangedAttack", menuName = "Attack Types/Ranged Attack")]
public class RangedAttack : AttackType
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float knockbackForce = 3f;

    protected override void PerformAttack(Character attacker, Character target)
    {
        if (projectilePrefab != null)
        {
            Vector2 direction = (target.transform.position - attacker.transform.position).normalized;
            GameObject projectile = Instantiate(projectilePrefab, attacker.transform.position, Quaternion.identity);

            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript == null)
                projScript = projectile.AddComponent<Projectile>();

            projScript.Initialize(direction, projectileSpeed, attacker.Damage * damageMultiplier, attacker, knockbackForce);

            Debug.Log($"{attacker.name} fired projectile at {target.name}!");
        }
    }
}

/// <summary>
/// Area of Effect attack type with knockback
/// </summary>
[CreateAssetMenu(fileName = "AOEAttack", menuName = "Attack Types/AOE Attack")]
public class AOEAttack : AttackType
{
    [SerializeField] private float aoeRadius = 3f;
    [SerializeField] private float knockbackForce = 4f;

    protected override void PerformAttack(Character attacker, Character target)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(target.transform.position, aoeRadius);

        foreach (Collider2D hit in hits)
        {
            Character character = hit.GetComponent<Character>();
            if (character != null && character != attacker)
            {
                float finalDamage = attacker.Damage * damageMultiplier;

                // Calculate knockback direction from AOE center
                Vector2 knockbackDir = (character.transform.position - target.transform.position).normalized;

                character.TakeDamage(finalDamage, knockbackDir * knockbackForce);
            }
        }

        Debug.Log($"{attacker.name} performed AOE attack with radius {aoeRadius}!");
    }
}

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
        Character target = collision.GetComponent<Character>();
        if (target != null && target != owner)
        {
            // Apply knockback in the direction the projectile was traveling
            target.TakeDamage(damage, direction * knockbackForce);
            Destroy(gameObject);
        }
    }
}