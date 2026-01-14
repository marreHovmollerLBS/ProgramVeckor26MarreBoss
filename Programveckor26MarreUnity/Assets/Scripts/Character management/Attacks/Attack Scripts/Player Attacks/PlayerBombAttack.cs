using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player's bomb attack - places a bomb that explodes after a timer
/// </summary>
public class PlayerBombAttack : MonoBehaviour
{
    [Header("Bomb Settings")]
    [SerializeField] private float damage = 30f;
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private float bombTimer = 2f;
    [SerializeField] private float attackCooldown = 5f;
    [SerializeField] private float knockbackForce = 20f;

    [Header("Visual Settings")]
    [SerializeField] private Color bombColor = Color.red;
    [SerializeField] private Color explosionColor = new Color(1f, 0.3f, 0f, 0.6f);
    [SerializeField] private float bombSize = 0.5f;
    [SerializeField] private bool showVisualEffect = true;

    [Header("Unlock Settings")]
    [SerializeField] private bool isUnlocked = false;

    private float lastAttackTime;
    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    /// <summary>
    /// Attempt to place a bomb
    /// </summary>
    public bool TryAttack()
    {
        if (!isUnlocked)
        {
            return false;
        }

        if (Time.time - lastAttackTime < attackCooldown)
        {
            return false;
        }

        PlaceBomb();
        lastAttackTime = Time.time;
        return true;
    }

    /// <summary>
    /// Place a bomb at the player's current position
    /// </summary>
    private void PlaceBomb()
    {
        GameObject bomb = new GameObject("PlayerBomb");
        bomb.transform.position = transform.position;

        // Add bomb component
        Bomb bombComponent = bomb.AddComponent<Bomb>();
        bombComponent.Initialize(damage, explosionRadius, bombTimer, knockbackForce, bombColor, explosionColor, bombSize, showVisualEffect);

        Debug.Log($"Bomb placed! Will explode in {bombTimer} seconds.");
    }

    /// <summary>
    /// Unlock this attack
    /// </summary>
    public void Unlock()
    {
        isUnlocked = true;
        Debug.Log("Bomb attack unlocked!");
    }

    /// <summary>
    /// Check if this attack is unlocked
    /// </summary>
    public bool IsUnlocked()
    {
        return isUnlocked;
    }

    /// <summary>
    /// Check if attack is ready
    /// </summary>
    public bool IsReady()
    {
        return isUnlocked && Time.time - lastAttackTime >= attackCooldown;
    }

    /// <summary>
    /// Get cooldown remaining
    /// </summary>
    public float GetCooldownRemaining()
    {
        return Mathf.Max(0, attackCooldown - (Time.time - lastAttackTime));
    }

    // Public setters for upgrades
    public void SetDamage(float newDamage) => damage = newDamage;
    public void SetExplosionRadius(float newRadius) => explosionRadius = newRadius;
    public void SetBombTimer(float newTimer) => bombTimer = newTimer;
    public void SetAttackCooldown(float newCooldown) => attackCooldown = newCooldown;
    public void SetKnockbackForce(float newForce) => knockbackForce = newForce;
}

/// <summary>
/// Bomb component that counts down and explodes
/// </summary>
public class Bomb : MonoBehaviour
{
    private float damage;
    private float explosionRadius;
    private float timer;
    private float knockbackForce;
    private Color bombColor;
    private Color explosionColor;
    private float bombSize;
    private bool showVisual;

    private SpriteRenderer spriteRenderer;
    private float originalTimer;

    public void Initialize(float dmg, float radius, float time, float knockback, Color bColor, Color eColor, float size, bool visual)
    {
        damage = dmg;
        explosionRadius = radius;
        timer = time;
        originalTimer = time;
        knockbackForce = knockback;
        bombColor = bColor;
        explosionColor = eColor;
        bombSize = size;
        showVisual = visual;

        if (showVisual)
        {
            CreateBombVisual();
        }
    }

    /// <summary>
    /// Create visual representation of the bomb
    /// </summary>
    private void CreateBombVisual()
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.color = bombColor;
        spriteRenderer.sortingOrder = 20;

        // Create circle sprite
        Texture2D texture = new Texture2D(64, 64);
        Vector2 center = new Vector2(32, 32);

        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                Vector2 point = new Vector2(x, y);
                float distance = Vector2.Distance(point, center);

                if (distance <= 32)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        texture.Apply();

        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        transform.localScale = Vector3.one * bombSize;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        // Pulse effect
        if (showVisual && spriteRenderer != null)
        {
            float pulse = Mathf.PingPong(Time.time * 5f, 0.3f);
            transform.localScale = Vector3.one * (bombSize + pulse);

            // Flash faster as timer runs out
            float flashSpeed = Mathf.Lerp(2f, 10f, 1f - (timer / originalTimer));
            float flash = Mathf.PingPong(Time.time * flashSpeed, 1f);
            spriteRenderer.color = Color.Lerp(bombColor, Color.white, flash);
        }

        // Explode when timer reaches zero
        if (timer <= 0)
        {
            Explode();
        }
    }

    /// <summary>
    /// Trigger the explosion
    /// </summary>
    private void Explode()
    {
        Debug.Log($"Bomb exploded at {transform.position}!");

        // Create explosion visual
        if (showVisual)
        {
            StartCoroutine(CreateExplosionEffect());
        }

        // Detect all enemies in explosion radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    // Calculate knockback direction (away from explosion center)
                    Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;

                    // Deal damage with knockback
                    enemy.TakeDamage(damage, knockbackDir * knockbackForce);

                    Debug.Log($"Bomb explosion hit {enemy.name} for {damage} damage!");
                }
            }
        }

        // Destroy the bomb object after explosion effect starts
        Destroy(gameObject, 0.5f);
    }

    /// <summary>
    /// Create explosion visual effect
    /// </summary>
    private IEnumerator CreateExplosionEffect()
    {
        GameObject explosion = new GameObject("Explosion");
        explosion.transform.position = transform.position;

        SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
        sr.color = explosionColor;
        sr.sortingOrder = 25;

        // Create explosion sprite
        Texture2D texture = new Texture2D(128, 128);
        Vector2 center = new Vector2(64, 64);

        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                Vector2 point = new Vector2(x, y);
                float distance = Vector2.Distance(point, center);

                if (distance <= 64)
                {
                    // Create gradient
                    float alpha = 1f - (distance / 64f);
                    Color pixelColor = explosionColor;
                    pixelColor.a = alpha * explosionColor.a;
                    texture.SetPixel(x, y, pixelColor);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        texture.Apply();

        sr.sprite = Sprite.Create(texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));

        // Animate explosion
        float duration = 0.4f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * (explosionRadius * 2f / 1.28f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            explosion.transform.localScale = Vector3.Lerp(startScale, endScale, t);

            // Fade out
            Color color = sr.color;
            color.a = explosionColor.a * (1f - t);
            sr.color = color;

            yield return null;
        }

        Destroy(explosion);
    }

    private void OnDrawGizmos()
    {
        // Show explosion radius
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        // Show timer countdown
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            float timerPercent = timer / originalTimer;
            Gizmos.DrawWireSphere(transform.position, explosionRadius * timerPercent);
        }
    }
}
