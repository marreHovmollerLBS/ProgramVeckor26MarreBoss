using UnityEngine;

/// <summary>
/// Melee attack type - direct damage
/// </summary>
[CreateAssetMenu(fileName = "MeleeAttack", menuName = "Attack Types/Melee Attack")]
public class MeleeAttack : AttackType
{
    protected override void PerformAttack(Character attacker, Character target)
    {
        float finalDamage = attacker.Damage * damageMultiplier;
        target.TakeDamage(finalDamage);
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
    
    protected override void PerformAttack(Character attacker, Character target)
    {
        if (projectilePrefab != null)
        {
            Vector2 direction = (target.transform.position - attacker.transform.position).normalized;
            GameObject projectile = Instantiate(projectilePrefab, attacker.transform.position, Quaternion.identity);
            
            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript == null)
                projScript = projectile.AddComponent<Projectile>();
                
            projScript.Initialize(direction, projectileSpeed, attacker.Damage * damageMultiplier, attacker);
            
            Debug.Log($"{attacker.name} fired projectile at {target.name}!");
        }
    }
}

/// <summary>
/// Area of Effect attack type
/// </summary>
[CreateAssetMenu(fileName = "AOEAttack", menuName = "Attack Types/AOE Attack")]
public class AOEAttack : AttackType
{
    [SerializeField] private float aoeRadius = 3f;
    
    protected override void PerformAttack(Character attacker, Character target)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(target.transform.position, aoeRadius);
        
        foreach (Collider2D hit in hits)
        {
            Character character = hit.GetComponent<Character>();
            if (character != null && character != attacker)
            {
                float finalDamage = attacker.Damage * damageMultiplier;
                character.TakeDamage(finalDamage);
            }
        }
        
        Debug.Log($"{attacker.name} performed AOE attack with radius {aoeRadius}!");
    }
}

/// <summary>
/// Projectile behavior for ranged attacks
/// </summary>
public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float damage;
    private Character owner;
    private Rigidbody2D rb;
    
    public void Initialize(Vector2 dir, float spd, float dmg, Character own)
    {
        direction = dir;
        speed = spd;
        damage = dmg;
        owner = own;
        
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
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
