using UnityEngine;

/// <summary>
/// Abstract class for different attack types
/// </summary>
public abstract class AttackType : ScriptableObject
{
    [SerializeField] protected float attackRange = 1f;
    [SerializeField] protected float attackCooldown = 1f;
    [SerializeField] protected float damageMultiplier = 1f;
    
    protected float lastAttackTime;
    
    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;
    
    /// <summary>
    /// Execute the attack
    /// </summary>
    public virtual void ExecuteAttack(Character attacker, Character target)
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;
            
        if (Vector2.Distance(attacker.transform.position, target.transform.position) <= attackRange)
        {
            PerformAttack(attacker, target);
            lastAttackTime = Time.time;
        }
    }
    
    /// <summary>
    /// Perform the specific attack behavior
    /// </summary>
    protected abstract void PerformAttack(Character attacker, Character target);
}
