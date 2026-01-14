using UnityEngine;

/// <summary>
/// Base Character class that all game characters inherit from
/// </summary>
public abstract class Character : MonoBehaviour
{
    [Header("Character Stats")]
    [SerializeField] protected float movementSpeed = 5f;
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float size = 0.5f;

    [Header("Attack Configuration")]
    [SerializeField] protected AttackType attackType;

    [Header("Knockback Settings")]
    [SerializeField] protected float knockbackForce = 7.5f;
    [SerializeField] protected float knockbackDuration = 0.1f; // How long knockback lasts

    protected bool isKnockedBack = false;

    [Header("Dream State")]
    [SerializeField] protected bool isGoodDream = true;

    [Header("Components")]
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected Collider2D col;

    // Movement direction
    protected Vector2 moveDirection;

    // Properties for external access
    public float MovementSpeed
    {
        get => movementSpeed;
        set => movementSpeed = value;
    }

    public float MaxHealth
    {
        get => maxHealth;
        set
        {
            maxHealth = value;
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;
        }
    }

    public float CurrentHealth
    {
        get => currentHealth;
        set => currentHealth = Mathf.Clamp(value, 0, maxHealth);
    }

    public float Damage
    {
        get => damage;
        set => damage = value;
    }

    public float Size
    {
        get => size;
        set => size = value;
    }

    public AttackType AttackType
    {
        get => attackType;
        set => attackType = value;
    }

    public bool IsGoodDream
    {
        get => isGoodDream;
        set
        {
            isGoodDream = value;
            OnDreamStateChanged();
        }
    }

    public Rigidbody2D Rigidbody => rb;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public Collider2D Collider => col;

    protected virtual void Awake()
    {
        InitializeComponents();
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Initialize or create necessary components
    /// </summary>
    protected virtual void InitializeComponents()
    {
        // Get or add Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // Configure for top-down 2D with knockback support
        rb.gravityScale = 0f; // 2D top-down
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearDamping = 0f; // No drag - important for knockback
        rb.angularDamping = 0f;

        // Get or add SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Get or add Collider2D (using CircleCollider2D as default)
        col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
    }

    protected virtual void Update()
    {
        HandleBehavior();
    }

    protected virtual void FixedUpdate()
    {
        Move();
    }

    /// <summary>
    /// Handle character-specific behavior (overridden by subclasses)
    /// </summary>
    protected abstract void HandleBehavior();

    /// <summary>
    /// Move the character
    /// </summary>
    protected virtual void Move()
    {
        // Don't move if being knocked back
        if (rb != null && !isKnockedBack)
        {
            rb.linearVelocity = moveDirection.normalized * movementSpeed;
        }
    }

    /// <summary>
    /// Take damage with knockback
    /// </summary>
    public virtual void TakeDamage(float damageAmount, Vector2 knockbackDirection)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Apply knockback
        if (rb != null && knockbackDirection != Vector2.zero)
        {
            StartCoroutine(ApplyKnockback(knockbackDirection));
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Coroutine to handle knockback with duration
    /// </summary>
    protected virtual System.Collections.IEnumerator ApplyKnockback(Vector2 direction)
    {
        isKnockedBack = true;

        // Apply the impulse force
        rb.linearVelocity = Vector2.zero; // Clear current velocity
        rb.AddForce(direction.normalized * knockbackForce, ForceMode2D.Impulse);

        // Wait for knockback duration
        yield return new WaitForSeconds(knockbackDuration);

        // Gradually slow down
        float elapsed = 0f;
        float slowdownTime = 0.1f;
        Vector2 startVelocity = rb.linearVelocity;

        while (elapsed < slowdownTime)
        {
            elapsed += Time.deltaTime;
            rb.linearVelocity = Vector2.Lerp(startVelocity, Vector2.zero, elapsed / slowdownTime);
            yield return null;
        }

        isKnockedBack = false;
    }

    /// <summary>
    /// Take damage without knockback (backward compatibility)
    /// </summary>
    public virtual void TakeDamage(float damageAmount)
    {
        TakeDamage(damageAmount, Vector2.zero);
    }

    /// <summary>
    /// Heal the character
    /// </summary>
    public virtual void Heal(float healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    /// <summary>
    /// Perform attack
    /// </summary>
    public virtual void Attack(Character target)
    {
        if (target != null && attackType != null)
        {
            attackType.ExecuteAttack(this, target);
        }
    }

    /// <summary>
    /// Called when the character dies
    /// </summary>
    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} has died!");
        // Override in subclasses for specific death behavior
    }

    /// <summary>
    /// Called when dream state changes
    /// </summary>
    protected virtual void OnDreamStateChanged()
    {
        // Override in subclasses for specific dream state behavior
    }
}