using UnityEngine;

/// <summary>
/// Default enemy that uses collision damage instead of attack state
/// </summary>
public class DefaultEnemy : Enemy
{
    protected override void Awake()
    {
        // Set collision damage mode BEFORE calling base.Awake()
        usesAttackState = false;
        collisionDamageCooldown = 1f;
        collisionKnockbackForce = 5f;

        base.Awake();

        gameObject.name = "Default Enemy";
    }

    protected override void InitializeComponents()
    {
        base.InitializeComponents();

        // Make sure the collider is NOT a trigger for collision damage
        if (col != null)
        {
            col.isTrigger = false;
        }
    }

    public override void InitializeEnemy(float speed, float health, float dmg, float sze, AttackType attack, bool goodDream = false, float spawnIdleTime = 1f)
    {
        base.InitializeEnemy(speed, health, dmg, sze, attack, goodDream, spawnIdleTime);
        gameObject.name = "Default Enemy";

        // Ensure collision damage is enabled
        usesAttackState = false;
    }
}

// Example of an enemy that DOES use attack state:
/// <summary>
/// Ranged enemy that uses attack state to shoot projectiles
/// </summary>
public class RangedEnemy : Enemy
{
    protected override void Awake()
    {
        // Set attack state mode BEFORE calling base.Awake()
        usesAttackState = true;
        attackDistance = 5f; // Ranged enemies attack from further away

        base.Awake();

        gameObject.name = "Ranged Enemy";
    }

    public override void InitializeEnemy(float speed, float health, float dmg, float sze, AttackType attack, bool goodDream = false, float spawnIdleTime = 1f)
    {
        base.InitializeEnemy(speed, health, dmg, sze, attack, goodDream, spawnIdleTime);
        gameObject.name = "Ranged Enemy";

        // Ensure attack state is enabled
        usesAttackState = true;
    }
}

/// <summary>
/// Tank enemy that uses attack state for powerful melee attacks
/// </summary>
public class TankEnemy : Enemy
{
    protected override void Awake()
    {
        // Set attack state mode BEFORE calling base.Awake()
        usesAttackState = true;
        attackDistance = 2f; // Melee range

        base.Awake();

        gameObject.name = "Tank Enemy";
    }

    public override void InitializeEnemy(float speed, float health, float dmg, float sze, AttackType attack, bool goodDream = false, float spawnIdleTime = 1f)
    {
        base.InitializeEnemy(speed, health, dmg, sze, attack, goodDream, spawnIdleTime);
        gameObject.name = "Tank Enemy";

        // Ensure attack state is enabled
        usesAttackState = true;
    }
}