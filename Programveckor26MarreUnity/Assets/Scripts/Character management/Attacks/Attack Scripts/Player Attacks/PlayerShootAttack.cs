using UnityEngine;

/// <summary>
/// Handles player's shoot attack - fires a projectile that pierces through enemies
/// </summary>
public class PlayerShootAttack : MonoBehaviour
{
    [Header("Shoot Attack Settings")]
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileLifetime = 3f;
    [SerializeField] private float shootCooldown = 1f;
    [SerializeField] private float knockbackForce = 5f;

    [Header("Projectile Visual")]
    [SerializeField] private float projectileSize = 0.3f;
    [SerializeField] private Color projectileColor = Color.yellow;

    [Header("Projectile Sprite")]
    [SerializeField] private Sprite projectileSprite;

    [Header("Attack Direction")]
    [SerializeField] private bool attackTowardsMouse = false; // If true, attack towards mouse. If false, attack in player's facing direction

    private Player player;
    private float lastShootTime = -999f;
    private bool isUnlocked = true;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    /// <summary>
    /// Unlock this attack
    /// </summary>
    public void Unlock()
    {
        isUnlocked = true;
        Debug.Log("Shoot Attack Unlocked!");
    }

    /// <summary>
    /// Attempt to shoot
    /// </summary>
    public bool TryAttack()
    {
        if (!isUnlocked)
            return false;

        if (Time.time - lastShootTime < shootCooldown)
            return false;

        FireProjectile();
        return true;
    }

    /// <summary>
    /// Fire a projectile
    /// </summary>
    private void FireProjectile()
    {
        lastShootTime = Time.time;

        // Get shoot direction based on setting
        Vector2 direction = GetShootDirection();

        // Create projectile
        GameObject projectileObj = new GameObject("PlayerProjectile");
        projectileObj.transform.position = transform.position;
        projectileObj.layer = LayerMask.NameToLayer("Default");

        // Add visual
        SpriteRenderer sr = projectileObj.AddComponent<SpriteRenderer>();
        sr.color = projectileColor;
        sr.sortingOrder = 50;

        // Use provided sprite or create circle sprite
        if (projectileSprite != null)
        {
            sr.sprite = projectileSprite;
        }
        else
        {
            sr.sprite = CreateCircleSprite();
        }

        // No aspect ratio correction - always 1:1
        projectileObj.transform.localScale = Vector3.one * projectileSize;

        // Rotate projectile to face direction of travel
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectileObj.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Add collider
        CircleCollider2D collider = projectileObj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;

        // Add rigidbody
        Rigidbody2D rb = projectileObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = direction * projectileSpeed;

        // Add projectile behavior
        PlayerProjectile proj = projectileObj.AddComponent<PlayerProjectile>();
        proj.Initialize(projectileDamage, knockbackForce, projectileLifetime);

        Debug.Log($"Fired projectile in direction {direction}");
    }

    /// <summary>
    /// Get the direction the projectile should travel
    /// </summary>
    private Vector2 GetShootDirection()
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
    /// Create a circle sprite for the projectile
    /// </summary>
    private Sprite CreateCircleSprite()
    {
        int resolution = 32;
        Texture2D texture = new Texture2D(resolution, resolution);
        Color[] pixels = new Color[resolution * resolution];

        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float radius = resolution / 2f;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * resolution + x] = distance <= radius ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);
    }

    /// <summary>
    /// Check if unlocked
    /// </summary>
    public bool IsUnlocked() => isUnlocked;

    /// <summary>
    /// Check if on cooldown
    /// </summary>
    public bool IsOnCooldown() => Time.time - lastShootTime < shootCooldown;

    /// <summary>
    /// Get cooldown progress
    /// </summary>
    public float GetCooldownProgress() => Mathf.Clamp01((Time.time - lastShootTime) / shootCooldown);

    // Setters
    public void SetDamage(float damage) => projectileDamage = damage;
    public void SetSpeed(float speed) => projectileSpeed = speed;
    public void SetKnockback(float knockback) => knockbackForce = knockback;
    public void SetProjectileSprite(Sprite sprite) => projectileSprite = sprite;
    public void SetAttackTowardsMouse(bool towardsMouse) => attackTowardsMouse = towardsMouse;
}

/// <summary>
/// Behavior for the piercing projectile
/// </summary>
public class PlayerProjectile : MonoBehaviour
{
    private float damage;
    private float knockbackForce;
    private float lifetime;

    public void Initialize(float dmg, float knockback, float life)
    {
        damage = dmg;
        knockbackForce = knockback;
        lifetime = life;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only damage enemies
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Calculate knockback direction
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                Vector2 knockbackDir = rb.linearVelocity.normalized;

                // Deal damage with knockback
                enemy.TakeDamage(damage, knockbackDir * knockbackForce);

                Debug.Log($"Projectile hit {enemy.name} for {damage} damage!");

                // Projectile pierces through - doesn't get destroyed on hit
            }
        }
    }
}