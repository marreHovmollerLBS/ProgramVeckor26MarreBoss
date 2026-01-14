using UnityEngine;

/// <summary>
/// Area of Effect attack type with knockback
/// </summary>
[CreateAssetMenu(fileName = "AOEAttack", menuName = "Attack Types/AOE Attack")]
public class AOEAttack : AttackType
{
    [SerializeField] private float aoeRadius = 3f;
    [SerializeField] private float knockbackForce = 12f;

    protected override void PerformAttack(Character attacker, Character target)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(target.transform.position, aoeRadius);

        foreach (Collider2D hit in hits)
        {
            // Only damage the player
            if (hit.CompareTag("Player"))
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
        }

        Debug.Log($"{attacker.name} performed AOE attack with radius {aoeRadius}!");
    }
}