using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player's melee attack - spawns a temporary hitbox in front of the player
/// </summary>
public class PlayerMeleeAttack : MonoBehaviour
{
    [Header("Melee Attack Settings")]
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private float attackRange = 2f;   // Length of the rectangle (forward direction)
    [SerializeField] private float attackWidth = 1f; // Width of the rectangle (perpendicular to attack direction)
    [SerializeField] private float attackCooldown = 0.2f;
    [SerializeField] private float knockbackForce = 7.5f;

    [Header("Visual Settings")]
    [SerializeField] private Sprite[] animationFrames; // Array of sprites for frame-by-frame animation
    [SerializeField] private float frameRate = 24f; // Frames per second for animation
    [SerializeField] private Color hitboxColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private bool showDebugHitbox = true;

    [Header("Attack Direction")]
    [SerializeField] private bool attackTowardsMouse = false; // If true, attack towards mouse. If false, attack in player's facing direction

    // Aspect ratio - set by CharacterManager
    private Vector2 spriteAspectRatio = new Vector2(1f, 1f);

    private Player player;
    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    private bool flipNextAttack = false;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    /// <summary>
    /// Attempt to perform melee attack
    /// </summary>
    public bool TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown || isAttacking)
        {
            return false;
        }

        StartCoroutine(PerformMeleeAttack());
        return true;
    }

    /// <summary>
    /// Execute the melee attack
    /// </summary>
    private IEnumerator PerformMeleeAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Get the direction the player is facing based on mouse position
        Vector2 attackDirection = GetAttackDirection();

        // Calculate hitbox position
        Vector2 hitboxCenter = (Vector2)transform.position + attackDirection * (attackRange / 2f);

        // Create the hitbox
        GameObject hitboxObj = new GameObject("MeleeHitbox");
        hitboxObj.transform.position = hitboxCenter;
        hitboxObj.layer = LayerMask.NameToLayer("Default");

        // Add BoxCollider2D - always 1x1 size initially
        BoxCollider2D hitbox = hitboxObj.AddComponent<BoxCollider2D>();
        hitbox.isTrigger = true;

        // Calculate aspect ratio correction
        Vector2 aspectCorrection = CalculateAspectCorrection();

        // Apply aspect ratio correction to collider size
        // If aspect ratio is (1, 2), collider size becomes (1/1, 1/2) = (1, 0.5)
        hitbox.size = new Vector2(1f / aspectCorrection.x, 1f / aspectCorrection.y);

        // Rotate hitbox to match attack direction
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        hitboxObj.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Scale the transform to match desired hitbox dimensions, including aspect ratio correction
        // If aspect ratio is (1, 2), transform scale is multiplied by (1, 2)
        Vector3 finalScale = new Vector3(
            attackRange * aspectCorrection.x,
            attackWidth * aspectCorrection.y,
            1f
        );
        hitboxObj.transform.localScale = finalScale;

        Debug.Log($"Hitbox created - Attack Range: {attackRange}, Attack Width: {attackWidth}, Aspect Ratio: {spriteAspectRatio}, Transform Scale: {finalScale}, Collider Size: {hitbox.size}");

        // Add visual feedback
        if (showDebugHitbox)
        {
            SpriteRenderer sr = hitboxObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 100;
            sr.color = hitboxColor;

            // Flip the sprite on Y axis if needed
            if (flipNextAttack)
            {
                sr.flipY = true;
            }

            // Add frame animator component
            if (animationFrames != null && animationFrames.Length > 0)
            {
                SimpleFrameAnimator animator = hitboxObj.AddComponent<SimpleFrameAnimator>();
                animator.Initialize(animationFrames, frameRate);

                Debug.Log($"Melee attack animation started: {animationFrames.Length} frames at {frameRate} fps, FlipY: {flipNextAttack}");
            }
            else
            {
                Debug.LogWarning("No animation frames assigned to melee attack!");
                // Still create a simple visual with first frame or placeholder
                sr.sprite = CreateSquareSprite();
            }
        }

        // Toggle flip for next attack
        flipNextAttack = !flipNextAttack;

        // Detect enemies in hitbox
        Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, new Vector2(attackRange, attackWidth), angle);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    // Calculate knockback direction
                    Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;

                    // Deal damage with knockback
                    enemy.TakeDamage(attackDamage, knockbackDir * knockbackForce);

                    Debug.Log($"Melee attack hit {enemy.name} for {attackDamage} damage!");
                }
            }
        }

        // The SimpleFrameAnimator will destroy the hitbox when animation completes
        // No need to manually destroy here

        isAttacking = false;
        yield break;
    }

    /// <summary>
    /// Calculate aspect ratio correction factor
    /// For example: if aspect ratio is (1, 2), returns (1, 2)
    /// This means the sprite is twice as tall as it is wide
    /// </summary>
    private Vector2 CalculateAspectCorrection()
    {
        // Prevent division by zero
        float x = Mathf.Max(spriteAspectRatio.x, 0.001f);
        float y = Mathf.Max(spriteAspectRatio.y, 0.001f);

        return new Vector2(x, y);
    }

    /// <summary>
    /// Get the direction the player should attack based on mouse position or player's facing direction
    /// </summary>
    private Vector2 GetAttackDirection()
    {
        if (attackTowardsMouse)
        {
            // Get mouse position in world space
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            // Calculate direction from player to mouse
            Vector2 direction = (mousePos - transform.position).normalized;
            return direction;
        }
        else
        {
            // Use player's current facing direction (rotation)
            float angle = transform.rotation.eulerAngles.z;
            Vector2 direction = new Vector2(
                Mathf.Sin(angle * Mathf.Deg2Rad),
                -Mathf.Cos(angle * Mathf.Deg2Rad)
            );
            return direction.normalized;
        }
    }

    /// <summary>
    /// Create a simple square sprite for visual feedback (fallback)
    /// </summary>
    private Sprite CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }

    /// <summary>
    /// Check if attack is on cooldown
    /// </summary>
    public bool IsOnCooldown()
    {
        return Time.time - lastAttackTime < attackCooldown;
    }

    /// <summary>
    /// Get cooldown progress (0 to 1)
    /// </summary>
    public float GetCooldownProgress()
    {
        return Mathf.Clamp01((Time.time - lastAttackTime) / attackCooldown);
    }

    // Setters for customization
    public void SetAttackDamage(float damage) => attackDamage = damage;
    public void SetAttackRange(float range) => attackRange = range;
    public void SetAttackWidth(float width) => attackWidth = width;
    public void SetKnockbackForce(float force) => knockbackForce = force;
    public void SetAttackCooldown(float cooldown) => attackCooldown = cooldown;
    public void SetHitboxColor(Color color) => hitboxColor = color;
    public void SetShowVisual(bool show) => showDebugHitbox = show;
    public void SetAnimationFrames(Sprite[] frames) => animationFrames = frames;
    public void SetFrameRate(float rate) => frameRate = rate;
    public void SetSpriteAspectRatio(Vector2 aspectRatio) => spriteAspectRatio = aspectRatio;
    public void SetAttackTowardsMouse(bool towardsMouse) => attackTowardsMouse = towardsMouse;
}

/// <summary>
/// Simple component to animate sprites frame-by-frame
/// </summary>
public class SimpleFrameAnimator : MonoBehaviour
{
    private Sprite[] frames;
    private float frameRate;
    private int currentFrame = 0;
    private float timer = 0f;
    private SpriteRenderer spriteRenderer;
    private bool animationComplete = false;

    public void Initialize(Sprite[] animFrames, float fps)
    {
        frames = animFrames;
        frameRate = fps;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (frames != null && frames.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = frames[0];
        }
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0 || spriteRenderer == null || animationComplete)
            return;

        timer += Time.deltaTime;
        float frameTime = 1f / frameRate;

        if (timer >= frameTime)
        {
            currentFrame++;

            // Check if animation is complete
            if (currentFrame >= frames.Length)
            {
                animationComplete = true;
                // Destroy the entire hitbox GameObject when animation completes
                Destroy(gameObject);
                return;
            }

            spriteRenderer.sprite = frames[currentFrame];
            timer = 0f;
        }
    }
}