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

    [Header("Spawn Settings")]
    [SerializeField] protected float idleSpawnTime = 1f; // Time enemy is idle after spawning

    protected Character target;
    protected float nextAttackTime;
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

    /// <summary>
    /// Override TakeDamage to make enemy invulnerable during spawn
    /// </summary>
    public override void TakeDamage(float damageAmount)
    {
        // Invulnerable during spawn idle state
        if (isSpawning)
        {
            Debug.Log($"{gameObject.name} is invulnerable during spawn!");
            return;
        }

        base.TakeDamage(damageAmount);
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

    protected void OnDrawGizmosSelected()
    {
        // Visualize detection radius in editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Visualize attack distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        // Visualize spawn state
        if (isSpawning)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}