using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player's ground slam attack - AOE damage in a circle around the player
/// </summary>
public class PlayerGroundSlamAttack : MonoBehaviour
{
    [Header("Ground Slam Settings")]
    [SerializeField] private float slamDamage = 8f;
    [SerializeField] private float slamRadius = 1.5f;
    [SerializeField] private float slamCooldown = 3f;
    [SerializeField] private float knockbackForce = 15f;
    [SerializeField] private float slamDuration = 0.3f;

    [Header("Visual Settings")]
    [SerializeField] private Color slamColor = new Color(1f, 0.5f, 0f, 0.4f);
    [SerializeField] private bool showSlamEffect = true;

    [Header("Slam Animation")]
    [SerializeField] private Sprite[] slamAnimationFrames;
    [SerializeField] private float slamAnimationFrameRate = 24f;

    [Header("Animation Settings")]
    [SerializeField] private float expandSpeed = 10f; // How fast the visual expands

    private Player player;
    private float lastSlamTime = -999f;
    private bool isUnlocked = true;
    private bool isSlaming = false;

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
        Debug.Log("Ground Slam Attack Unlocked!");
    }

    /// <summary>
    /// Attempt to perform ground slam
    /// </summary>
    public bool TryAttack()
    {
        if (!isUnlocked)
            return false;

        if (Time.time - lastSlamTime < slamCooldown || isSlaming)
            return false;

        StartCoroutine(PerformGroundSlam());
        return true;
    }

    /// <summary>
    /// Execute ground slam attack
    /// </summary>
    private IEnumerator PerformGroundSlam()
    {
        isSlaming = true;
        lastSlamTime = Time.time;

        Debug.Log("Ground Slam activated!");

        // Create visual effect
        GameObject slamEffect = null;
        if (showSlamEffect)
        {
            slamEffect = CreateSlamEffect();
        }

        // Detect all enemies in radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, slamRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    // Calculate knockback direction (away from player)
                    Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;

                    // Deal damage with knockback
                    enemy.TakeDamage(slamDamage, knockbackDir * knockbackForce);

                    Debug.Log($"Ground slam hit {enemy.name} for {slamDamage} damage!");
                }
            }
        }

        // Wait for slam duration (only if using fallback visual)
        if (slamAnimationFrames == null || slamAnimationFrames.Length == 0)
        {
            yield return new WaitForSeconds(slamDuration);

            // Destroy visual effect
            if (slamEffect != null)
            {
                Destroy(slamEffect);
            }
        }

        isSlaming = false;
    }

    /// <summary>
    /// Create visual effect for ground slam
    /// </summary>
    private GameObject CreateSlamEffect()
    {
        GameObject effectObj = new GameObject("GroundSlamEffect");
        effectObj.transform.position = transform.position;
        effectObj.layer = LayerMask.NameToLayer("Default");

        // Add sprite renderer with pure white color, alpha 1
        SpriteRenderer sr = effectObj.AddComponent<SpriteRenderer>();
        sr.color = Color.white; // Pure white with alpha 1
        sr.sortingOrder = 10;

        // Add circle collider - ALWAYS radius 1, scale is handled by transform
        CircleCollider2D collider = effectObj.AddComponent<CircleCollider2D>();
        collider.radius = 1f;
        collider.isTrigger = true;

        // If we have animation frames, use them
        if (slamAnimationFrames != null && slamAnimationFrames.Length > 0)
        {
            sr.sprite = slamAnimationFrames[0];

            // Add animator component that will destroy the object when done
            GroundSlamAnimator animator = effectObj.AddComponent<GroundSlamAnimator>();
            animator.Initialize(slamAnimationFrames, slamAnimationFrameRate);

            // No aspect ratio correction - always 1:1
            effectObj.transform.localScale = Vector3.one * slamRadius;

            Debug.Log($"Ground slam created with {slamAnimationFrames.Length} frames, radius: {slamRadius}, scale: {effectObj.transform.localScale}");
        }
        else
        {
            // Fallback to circle sprite at full size (no expansion)
            sr.sprite = CreateCircleSprite();

            // No aspect ratio correction - always 1:1
            effectObj.transform.localScale = Vector3.one * slamRadius;

            // Add simple timed destruction
            GroundSlamEffect effect = effectObj.AddComponent<GroundSlamEffect>();
            effect.Initialize(slamRadius, slamDuration, expandSpeed);
        }

        return effectObj;
    }

    /// <summary>
    /// Create a circle sprite
    /// </summary>
    private Sprite CreateCircleSprite()
    {
        int resolution = 64;
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
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), resolution / 2f);
    }

    /// <summary>
    /// Check if unlocked
    /// </summary>
    public bool IsUnlocked() => isUnlocked;

    /// <summary>
    /// Check if on cooldown
    /// </summary>
    public bool IsOnCooldown() => Time.time - lastSlamTime < slamCooldown;

    /// <summary>
    /// Get cooldown progress
    /// </summary>
    public float GetCooldownProgress() => Mathf.Clamp01((Time.time - lastSlamTime) / slamCooldown);

    // Setters
    public void SetDamage(float damage) => slamDamage = damage;
    public void SetRadius(float radius) => slamRadius = radius;
    public void SetKnockback(float knockback) => knockbackForce = knockback;
    public void SetSlamAnimationFrames(Sprite[] frames) => slamAnimationFrames = frames;
    public void SetSlamAnimationFrameRate(float rate) => slamAnimationFrameRate = rate;

    private void OnDrawGizmosSelected()
    {
        // Draw slam radius
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, slamRadius);
    }
}

/// <summary>
/// Component to handle the expanding visual effect (fallback when no animation)
/// </summary>
public class GroundSlamEffect : MonoBehaviour
{
    private float duration;
    private float elapsedTime;

    public void Initialize(float target, float dur, float speed)
    {
        duration = dur;
        elapsedTime = 0f;

        // Already scaled by parent, no need to scale here
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        // Destroy after duration
        if (elapsedTime >= duration)
        {
            Destroy(gameObject);
        }
    }
}

/// <summary>
/// Component to handle ground slam animation
/// </summary>
public class GroundSlamAnimator : MonoBehaviour
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
                // Destroy the GameObject when animation completes
                Destroy(gameObject);
                return;
            }

            spriteRenderer.sprite = frames[currentFrame];
            timer = 0f;
        }
    }
}