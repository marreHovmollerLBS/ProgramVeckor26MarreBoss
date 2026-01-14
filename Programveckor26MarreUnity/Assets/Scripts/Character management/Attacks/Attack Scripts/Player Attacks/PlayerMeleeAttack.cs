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
    [SerializeField] private float attackWidth = 0.8f; // Width of the rectangle (perpendicular to attack direction)
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private float attackCooldown = 0.1f;
    [SerializeField] private float knockbackForce = 8f;

    [Header("Visual Settings")]
    [SerializeField] private Sprite hitboxSprite; // Custom sprite for hitbox visual (leave null for default). For animations, assign the first frame or use Animator component.
    [SerializeField] private Color hitboxColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private bool showDebugHitbox = true;

    private Player player;
    private float lastAttackTime = -999f;
    private bool isAttacking = false;

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

        // Add BoxCollider2D
        BoxCollider2D hitbox = hitboxObj.AddComponent<BoxCollider2D>();
        hitbox.isTrigger = true;
        hitbox.size = new Vector2(attackRange, attackWidth);

        // Rotate hitbox to match attack direction
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        hitboxObj.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Optional: Add visual feedback
        if (showDebugHitbox)
        {
            SpriteRenderer sr = hitboxObj.AddComponent<SpriteRenderer>();
            sr.color = hitboxColor;

            // Use custom sprite if provided, otherwise create default
            if (hitboxSprite != null)
            {
                sr.sprite = hitboxSprite;

                // Scale sprite to match hitbox dimensions
                // Get the sprite's native size
                float spriteWidth = hitboxSprite.bounds.size.x;
                float spriteHeight = hitboxSprite.bounds.size.y;

                // Calculate scale factors to match hitbox dimensions
                float scaleX = attackRange / spriteWidth;
                float scaleY = attackWidth / spriteHeight;

                hitboxObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }
            else
            {
                // Use default square sprite
                sr.sprite = CreateSquareSprite();

                // Scale the default sprite to match hitbox dimensions
                hitboxObj.transform.localScale = new Vector3(attackRange, attackWidth, 1f);
            }

            sr.sortingOrder = 100;
        }

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

        // Wait for attack duration
        yield return new WaitForSeconds(attackDuration);

        // Destroy hitbox
        Destroy(hitboxObj);
        isAttacking = false;
    }

    /// <summary>
    /// Get the direction the player should attack based on mouse position
    /// </summary>
    private Vector2 GetAttackDirection()
    {
        // Get mouse position in world space
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // Calculate direction from player to mouse
        Vector2 direction = (mousePos - transform.position).normalized;

        return direction;
    }

    /// <summary>
    /// Create a simple square sprite for visual feedback
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
    public void SetHitboxSprite(Sprite sprite) => hitboxSprite = sprite;
    public void SetHitboxColor(Color color) => hitboxColor = color;
    public void SetShowVisual(bool show) => showDebugHitbox = show;
}