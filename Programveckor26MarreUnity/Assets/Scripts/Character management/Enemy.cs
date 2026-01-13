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

    [Header("Combat Settings")]
    [SerializeField] protected bool usesAttackState = false; // Whether this enemy uses attack state or collision damage
    [SerializeField] protected float collisionDamageCooldown = 1f; // Cooldown for collision damage
    [SerializeField] protected float collisionKnockbackForce = 5f;

    [Header("Spawn Settings")]
    [SerializeField] protected float idleSpawnTime = 1f; // Time enemy is idle after spawning

    protected Character target;
    protected float nextAttackTime;
    protected float nextCollisionDamageTime;
    protected float spawnTimer;
    protected bool isSpawning = true;

    public enum EnemyState
    {
        Idle,
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

        // Initialize spawn timer
        spawnTimer = 0f;
        isSpawning = true;
        currentState = EnemyState.Idle;
        nextCollisionDamageTime = 0f;
    }

    protected override void HandleBehavior()
    {
        // Handle spawn idle state
        if (isSpawning)
        {
            HandleIdleSpawnState();
            return;
        }

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

        // Always look at target when active
        LookAtTarget();
    }

    /// <summary>
    /// Handle idle spawn state where enemy cannot move and is invulnerable
    /// </summary>
    protected virtual void HandleIdleSpawnState()
    {
        currentState = EnemyState.Idle;
        moveDirection = Vector2.zero; // Cannot move during spawn

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= idleSpawnTime)
        {
            isSpawning = false;
            OnSpawnComplete();
        }
    }

    /// <summary>
    /// Called when spawn idle period completes
    /// </summary>
    protected virtual void OnSpawnComplete()
    {
        Debug.Log($"{gameObject.name} spawn complete - now active");
    }

    /// <summary>
    /// Handle chase behavior (Bad Dream)
    /// </summary>
    protected virtual void HandleChaseState()
    {
        if (target != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);

            // Only use attack state if this enemy type uses it
            if (usesAttackState && distanceToTarget <= attackDistance)
            {
                // Attack state behavior
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
                // Chase behavior (for all enemies, including collision damage enemies)
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

    /// <summary>
    /// Handle collision damage for enemies that don't use attack state
    /// </summary>
    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        // Only deal collision damage if not using attack state
        if (!usesAttackState && !isSpawning && !isGoodDream)
        {
            Character target = collision.gameObject.GetComponent<Character>();

            // Check if it's the player (or another valid target) and cooldown is ready
            if (target != null && target != this && Time.time >= nextCollisionDamageTime)
            {
                // Calculate knockback direction (away from this enemy)
                Vector2 knockbackDir = (target.transform.position - transform.position).normalized;

                // Deal damage with knockback
                target.TakeDamage(damage, knockbackDir * collisionKnockbackForce);

                // Set next damage time
                nextCollisionDamageTime = Time.time + collisionDamageCooldown;

                Debug.Log($"{gameObject.name} dealt {damage} collision damage to {target.name}!");
            }
        }
    }

    //Get enemys to look at target
    protected virtual void LookAtTarget()
    {
        if (target == null) return;

        // Calculate direction vector from enemy to target
        Vector2 direction = target.transform.position - transform.position;

        // Calculate angle in degrees (atan2 returns radians, so we convert)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply rotation around Z-axis (for 2D top-down)
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Override TakeDamage to make enemy invulnerable during spawn
    /// </summary>
    public override void TakeDamage(float damageAmount, Vector2 knockbackDirection)
    {
        // Invulnerable during spawn idle state
        if (isSpawning)
        {
            Debug.Log($"{gameObject.name} is invulnerable during spawn!");
            return;
        }

        base.TakeDamage(damageAmount, knockbackDirection);
    }

    /// <summary>
    /// Backward compatibility for damage without knockback
    /// </summary>
    public override void TakeDamage(float damageAmount)
    {
        TakeDamage(damageAmount, Vector2.zero);
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
    public virtual void InitializeEnemy(float speed, float health, float dmg, float sze, AttackType attack, bool goodDream = false, float spawnIdleTime = 1f)
    {
        movementSpeed = speed;
        maxHealth = health;
        currentHealth = health;
        damage = dmg;
        size = sze;
        attackType = attack;
        isGoodDream = goodDream;
        idleSpawnTime = spawnIdleTime; // Customizable idle spawn time
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

    /// <summary>
    /// Check if enemy is currently in spawn idle state
    /// </summary>
    public bool IsSpawning()
    {
        return isSpawning;
    }

    /// <summary>
    /// Get current enemy state
    /// </summary>
    public EnemyState GetCurrentState()
    {
        return currentState;
    }

    /// <summary>
    /// Set whether this enemy uses attack state or collision damage
    /// </summary>
    public void SetUsesAttackState(bool value)
    {
        usesAttackState = value;
    }

    /// <summary>
    /// Set collision damage cooldown
    /// </summary>
    public void SetCollisionDamageCooldown(float cooldown)
    {
        collisionDamageCooldown = cooldown;
    }

    /// <summary>
    /// Set collision knockback force
    /// </summary>
    public void SetCollisionKnockbackForce(float force)
    {
        collisionKnockbackForce = force;
    }

    protected void OnDrawGizmosSelected()
    {
        // Visualize detection radius in editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Visualize attack distance (only if using attack state)
        if (usesAttackState)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackDistance);
        }

        // Visualize spawn state
        if (isSpawning)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }

        // Draw collision damage indicator
        if (!usesAttackState)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}