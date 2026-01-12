using UnityEngine;

/// <summary>
/// Base Enemy class that all enemy types inherit from
/// </summary>
public abstract class Enemy : Character
{
    [Header("Enemy Settings")]
    [SerializeField] protected float detectionRadius = 8f;
    [SerializeField] protected float attackDistance = 1.5f;
    [SerializeField] protected LayerMask playerLayer;
    
    protected Character target;
    protected float nextAttackTime;
    
    protected enum EnemyState
    {
        Chase,
        Flee,
        Attack
    }
    
    protected EnemyState currentState;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Enemy specific initialization
        gameObject.tag = "Enemy";
        gameObject.layer = LayerMask.NameToLayer("Enemy");

        //Identify player
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Character>();
    }

    protected override void HandleBehavior()
    {
        if (isGoodDream)
        {
            // Good dream: Enemies flee from player
            HandleFleeState();
        }
        else
        {
            // Bad dream: Enemies chase player
            HandleChaseState();
        }
    }
    
    /// <summary>
    /// Handle chase behavior (Bad Dream)
    /// </summary>
    protected virtual void HandleChaseState()
    {
        if (target != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);
            
            if (distanceToTarget <= attackDistance)
            {
                // Attack
                currentState = EnemyState.Attack;
                moveDirection = Vector2.zero;
                
                if (Time.time >= nextAttackTime && attackType != null)
                {
                    Attack(target);
                    nextAttackTime = Time.time + attackType.AttackCooldown;
                }
            }
            else
            {
                // Chase
                currentState = EnemyState.Chase;
                Vector2 direction = (target.transform.position - transform.position).normalized;
                moveDirection = direction;
            }
        }
    }
    
    /// <summary>
    /// Handle flee behavior (Good Dream)
    /// </summary>
    protected virtual void HandleFleeState()
    {
        if (target != null)
        {
            currentState = EnemyState.Flee;
            // Flee away from player
            Vector2 direction = (transform.position - target.transform.position).normalized;
            moveDirection = direction;
        }
    }
    
    protected override void Die()
    {
        base.Die();
        Destroy(gameObject);
    }
    
    protected override void OnDreamStateChanged()
    {
        base.OnDreamStateChanged();
        Debug.Log($"{gameObject.name} dream state changed to: {(isGoodDream ? "Good Dream (Fleeing)" : "Bad Dream (Chasing)")}");
    }
    
    /// <summary>
    /// Public method to initialize enemy from external script
    /// </summary>
    public virtual void InitializeEnemy(float speed, float health, float dmg, float sze, AttackType attack, bool goodDream = false)
    {
        movementSpeed = speed;
        maxHealth = health;
        currentHealth = health;
        damage = dmg;
        size = sze;
        attackType = attack;
        isGoodDream = goodDream;
    }
    
    /// <summary>
    /// Set the enemy sprite
    /// </summary>
    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }
    
    protected void OnDrawGizmosSelected()
    {
        // Visualize detection radius in editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Visualize attack distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}
