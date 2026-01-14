using UnityEngine;

/// <summary>
/// Melee attack type - direct damage with knockback
/// </summary>
[CreateAssetMenu(fileName = "MeleeAttack", menuName = "Attack Types/Melee Attack")]
public class MeleeAttack : AttackType
{
    [SerializeField] private float knockbackForce = 10f;

    protected override void PerformAttack(Character attacker, Character target)
    {
        // Only damage if target is the player
        if (target != null && target.CompareTag("Player"))
        {
            float finalDamage = attacker.Damage * damageMultiplier;

            // Calculate knockback direction
            Vector2 knockbackDir = (target.transform.position - attacker.transform.position).normalized;

            target.TakeDamage(finalDamage, knockbackDir * knockbackForce);
            Debug.Log($"{attacker.name} performed melee attack on {target.name} for {finalDamage} damage!");
        }
    }
}