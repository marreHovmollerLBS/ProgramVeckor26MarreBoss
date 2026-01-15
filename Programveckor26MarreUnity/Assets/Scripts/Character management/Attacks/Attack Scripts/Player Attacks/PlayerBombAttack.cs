using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player's bomb attack - places a bomb that explodes after a timer
/// </summary>
public class PlayerBombAttack : MonoBehaviour
{
    [Header("Bomb Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float bombTimer = 2f;
    [SerializeField] private float attackCooldown = 5f;
    [SerializeField] private float knockbackForce = 20f;

    [Header("Visual Settings")]
    [SerializeField] private Color bombColor = Color.red;
    [SerializeField] private Color explosionColor = new Color(1f, 0.3f, 0f, 0.6f);
    [SerializeField] private float bombSize = 0.5f;
    [SerializeField] private bool showVisualEffect = true;

    [Header("Bomb Animation")]
    [SerializeField] private Sprite[] bombAnimationFrames;
    [SerializeField] private float bombAnimationFrameRate = 12f;

    [Header("Explosion Animation")]
    [SerializeField] private Sprite[] explosionAnimationFrames;
    [SerializeField] private float explosionAnimationFrameRate = 24f;

    [Header("Unlock Settings")]
    [SerializeField] private bool isUnlocked = true;

    private float lastAttackTime;
    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

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

    private void PlaceBomb()
    {
        GameObject bomb = new GameObject("PlayerBomb");
        bomb.transform.position = transform.position;

        Bomb bombComponent = bomb.AddComponent<Bomb>();
        bombComponent.Initialize(
            damage,
            explosionRadius,
            bombTimer,
            knockbackForce,
            bombColor,
            explosionColor,
            bombSize,
            showVisualEffect,
            bombAnimationFrames,
            bombAnimationFrameRate,
            explosionAnimationFrames,
            explosionAnimationFrameRate
        );

        Debug.Log($"Bomb placed! Will explode in {bombTimer} seconds.");
    }

    public void Unlock()
    {
        isUnlocked = true;
        Debug.Log("Bomb attack unlocked!");
    }

    public bool IsUnlocked()
    {
        return isUnlocked;
    }

    public bool IsReady()
    {
        return isUnlocked && Time.time - lastAttackTime >= attackCooldown;
    }

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
    public void SetBombAnimationFrames(Sprite[] frames) => bombAnimationFrames = frames;
    public void SetBombAnimationFrameRate(float rate) => bombAnimationFrameRate = rate;
    public void SetExplosionAnimationFrames(Sprite[] frames) => explosionAnimationFrames = frames;
    public void SetExplosionAnimationFrameRate(float rate) => explosionAnimationFrameRate = rate;
}

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
    private Sprite[] bombFrames;
    private float bombFrameRate;
    private Sprite[] explosionFrames;
    private float explosionFrameRate;

    private SpriteRenderer spriteRenderer;
    private float originalTimer;

    public void Initialize(
        float dmg,
        float radius,
        float time,
        float knockback,
        Color bColor,
        Color eColor,
        float size,
        bool visual,
        Sprite[] bFrames,
        float bFrameRate,
        Sprite[] eFrames,
        float eFrameRate)
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
        bombFrames = bFrames;
        bombFrameRate = bFrameRate;
        explosionFrames = eFrames;
        explosionFrameRate = eFrameRate;

        if (showVisual)
        {
            CreateBombVisual();
        }
    }

    private void CreateBombVisual()
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.color = bombColor;
        spriteRenderer.sortingOrder = 20;

        if (bombFrames != null && bombFrames.Length > 0)
        {
            spriteRenderer.sprite = bombFrames[0];

            BombAnimator animator = gameObject.AddComponent<BombAnimator>();
            animator.Initialize(bombFrames, bombFrameRate, true);
        }
        else
        {
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
        }

        // No aspect ratio correction needed - always 1:1
        transform.localScale = Vector3.one * bombSize;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (showVisual && spriteRenderer != null && (bombFrames == null || bombFrames.Length == 0))
        {
            float pulse = Mathf.PingPong(Time.time * 5f, 0.3f);
            transform.localScale = Vector3.one * (bombSize + pulse);

            float flashSpeed = Mathf.Lerp(2f, 10f, 1f - (timer / originalTimer));
            float flash = Mathf.PingPong(Time.time * flashSpeed, 1f);
            spriteRenderer.color = Color.Lerp(bombColor, Color.white, flash);
        }

        if (timer <= 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        Debug.Log($"Bomb exploded at {transform.position}!");

        if (showVisual)
        {
            CreateExplosionEffect();
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                    enemy.TakeDamage(damage, knockbackDir * knockbackForce);
                    Debug.Log($"Bomb explosion hit {enemy.name} for {damage} damage!");
                }
            }
        }

        Destroy(gameObject);
    }

    private void CreateExplosionEffect()
    {
        GameObject explosion = new GameObject("Explosion");
        explosion.transform.position = transform.position;

        SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
        sr.color = Color.white;
        sr.sortingOrder = 25;

        CircleCollider2D collider = explosion.AddComponent<CircleCollider2D>();
        collider.radius = 1f;
        collider.isTrigger = true;

        if (explosionFrames != null && explosionFrames.Length > 0)
        {
            sr.sprite = explosionFrames[0];

            BombAnimator animator = explosion.AddComponent<BombAnimator>();
            animator.Initialize(explosionFrames, explosionFrameRate, false);

            // No aspect ratio correction - always 1:1
            explosion.transform.localScale = Vector3.one * explosionRadius;

            Debug.Log($"Explosion created with {explosionFrames.Length} frames, radius: {explosionRadius}, scale: {explosion.transform.localScale}");
        }
        else
        {
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
                        texture.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }
            texture.Apply();

            sr.sprite = Sprite.Create(texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));

            // No aspect ratio correction - always 1:1
            explosion.transform.localScale = Vector3.one * explosionRadius;

            Destroy(explosion, 0.4f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            float timerPercent = timer / originalTimer;
            Gizmos.DrawWireSphere(transform.position, explosionRadius * timerPercent);
        }
    }
}

public class BombAnimator : MonoBehaviour
{
    private Sprite[] frames;
    private float frameRate;
    private bool loop;
    private int currentFrame = 0;
    private float timer = 0f;
    private SpriteRenderer spriteRenderer;
    private bool animationComplete = false;

    public void Initialize(Sprite[] animFrames, float fps, bool shouldLoop)
    {
        frames = animFrames;
        frameRate = fps;
        loop = shouldLoop;
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

            if (currentFrame >= frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    animationComplete = true;
                    Destroy(gameObject);
                    return;
                }
            }

            spriteRenderer.sprite = frames[currentFrame];
            timer = 0f;
        }
    }
}