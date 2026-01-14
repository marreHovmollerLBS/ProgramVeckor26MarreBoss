using UnityEngine;

/// <summary>
/// Ranged attack type - shoots projectiles
/// </summary>
[CreateAssetMenu(fileName = "RangedAttack", menuName = "Attack Types/Ranged Attack")]
public class RangedAttack : AttackType
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float knockbackForce = 8f;

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