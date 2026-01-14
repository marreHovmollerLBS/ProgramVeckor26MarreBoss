using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player's ground slam attack - AOE damage in a circle around the player
/// </summary>
public class PlayerGroundSlamAttack : MonoBehaviour
{
    [Header("Ground Slam Settings")]
    [SerializeField] private float slamDamage = 30f;
    [SerializeField] private float slamRadius = 3f;
    [SerializeField] private float slamCooldown = 3f;
    [SerializeField] private float knockbackForce = 15f;
    [SerializeField] private float slamDuration = 0.3f;

    [Header("Visual Settings")]
    [SerializeField] private Color slamColor = new Color(1f, 0.5f, 0f, 0.4f);
    [SerializeField] private bool showSlamEffect = true;

    [Header("Animation Settings")]
    [SerializeField] private float expandSpeed = 10f; // How fast the visual expands

    private Player player;
    private float lastSlamTime = -999f;
    private bool isUnlocked = false;
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

        // Wait for slam duration
        yield return new WaitForSeconds(slamDuration);

        // Destroy visual effect
        if (slamEffect != null)
        {
            Destroy(slamEffect);
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

        // Add sprite renderer
        SpriteRenderer sr = effectObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = slamColor;
        sr.sortingOrder = 10;

        // Start small and expand
        effectObj.transform.localScale = Vector3.zero;
        
        // Add expanding animation
        GroundSlamEffect effect = effectObj.AddComponent<GroundSlamEffect>();
        effect.Initialize(slamRadius, slamDuration, expandSpeed);

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

    private void OnDrawGizmosSelected()
    {
        // Draw slam radius
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, slamRadius);
    }
}

/// <summary>
/// Component to handle the expanding visual effect
/// </summary>
public class GroundSlamEffect : MonoBehaviour
{
    private float targetScale;
    private float duration;
    private float expandSpeed;
    private float elapsedTime;

    public void Initialize(float target, float dur, float speed)
    {
        targetScale = target * 2f; // Multiply by 2 for diameter
        duration = dur;
        expandSpeed = speed;
        elapsedTime = 0f;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        // Expand to target size
        float scale = Mathf.Lerp(0f, targetScale, elapsedTime * expandSpeed / duration);
        transform.localScale = new Vector3(scale, scale, 1f);

        // Fade out towards the end
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color color = sr.color;
            color.a = Mathf.Lerp(color.a, 0f, elapsedTime / duration);
            sr.color = color;
        }
    }
}
