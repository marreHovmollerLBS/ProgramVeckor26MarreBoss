using UnityEngine;
using System.Collections;

/// <summary>
/// Default enemy that uses collision damage instead of attack state
/// </summary>
public class DefaultEnemy : Enemy
{
    protected override void Awake()
    {
        // Set collision damage mode BEFORE calling base.Awake()
        usesAttackState = false;
        collisionDamageCooldown = 1f;
        collisionKnockbackForce = 5f;

        base.Awake();

        gameObject.name = "Default Enemy";
    }

    protected override void InitializeComponents()
    {
        base.InitializeComponents();

        // Make sure the collider is NOT a trigger for collision damage
        if (col != null)
        {
            col.isTrigger = false;
        }
    }

    public override void InitializeEnemy(float speed, float health, float dmg, float sze, AttackType attack, bool goodDream = false, float spawnIdleTime = 1f)
    {
        base.InitializeEnemy(speed, health, dmg, sze, attack, goodDream, spawnIdleTime);
        gameObject.name = "Default Enemy";

        // Ensure collision damage is enabled
        usesAttackState = false;
    }
}

/// <summary>
/// Heal enemy - weak but fast, always flees, heals player when killed
/// </summary>
public class HealEnemy : Enemy
{
    [Header("Heal Settings")]
    [SerializeField] private float healAmount = 25f;
    [SerializeField] private float healRadius = 2f;

    protected override void Awake()
    {
        // Heal enemy doesn't attack
        usesAttackState = false;
        collisionDamageCooldown = 1f;
        collisionKnockbackForce = 2f;

        base.Awake();

        gameObject.name = "Heal Enemy";
    }

    protected override void InitializeComponents()
    {
        base.InitializeComponents();

        // Make collider a trigger - doesn't deal damage
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    protected override void HandleBehavior()
    {
        // Handle spawn idle state
        if (isSpawning)
        {
            HandleIdleSpawnState();
            return;
        }

        // ALWAYS flees, regardless of dream state
        if (target != null)
        {
            currentState = EnemyState.Flee;
            Vector2 direction = (transform.position - target.transform.position).normalized;
            moveDirection = direction;
        }

        LookAtTarget();
    }

    protected override void Die()
    {
        // Heal nearby players when killed
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, healRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Character player = hit.GetComponent<Character>();
                if (player != null)
                {
                    player.Heal(healAmount);
                    Debug.Log($"Heal Enemy healed {player.name} for {healAmount} HP!");
                }
            }
        }
        if (PersistentPlayerManager.Instance != null)
        {
            PersistentPlayerManager.Instance.coins += 2;
        }

        base.Die();
    }

    public override void InitializeEnemy(float speed, float health, float dmg, float sze, AttackType attack, bool goodDream = false, float spawnIdleTime = 1f)
    {
        base.InitializeEnemy(speed, health, dmg, sze, attack, goodDream, spawnIdleTime);
        gameObject.name = "Heal Enemy";
        usesAttackState = false;
    }

    public void SetHealAmount(float amount)
    {
        healAmount = amount;
    }

    public void SetHealRadius(float radius)
    {
        healRadius = radius;
    }

    protected void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Draw heal radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, healRadius);
    }
}

/// <summary>
/// Ranged enemy that uses attack state to shoot projectiles
/// </summary>
public class RangedEnemy : Enemy
{
    protected override void Awake()
    {
        // Set attack state mode BEFORE calling base.Awake()
        usesAttackState = true;
        attackDistance = 5f; // Ranged enemies attack from further away

        base.Awake();

        gameObject.name = "Ranged Enemy";
    }

    public override void InitializeEnemy(float speed, float health, float dmg, float sze, AttackType attack, bool goodDream = false, float spawnIdleTime = 1f)
    {
        base.InitializeEnemy(speed, health, dmg, sze, attack, goodDream, spawnIdleTime);
        gameObject.name = "Ranged Enemy";

        // Ensure attack state is enabled
        usesAttackState = true;
    }
}

/// <summary>
/// Tank enemy that uses attack state for powerful melee attacks
/// </summary>
public class TankEnemy : Enemy
{
    protected override void Awake()
    {
        // Set attack state mode BEFORE calling base.Awake()
        usesAttackState = true;
        attackDistance = 2f; // Melee range

        base.Awake();

        gameObject.name = "Tank Enemy";
    }

    public override void InitializeEnemy(float speed, float health, float dmg, float sze, AttackType attack, bool goodDream = false, float spawnIdleTime = 1f)
    {
        base.InitializeEnemy(speed, health, dmg, sze, attack, goodDream, spawnIdleTime);
        gameObject.name = "Tank Enemy";

        // Ensure attack state is enabled
        usesAttackState = true;
    }
}

/// <summary>
/// BOSS: Evil Father - First boss, uses teleportation and memory attacks
/// </summary>
public class EvilFather : Enemy
{
    [Header("Evil Father Settings")]
    [SerializeField] private float teleportCooldown = 1f;
    [SerializeField] private float teleportRange = 3f;
    [SerializeField] private int shieldHealth = 50;
    [SerializeField] private float shieldRegenDelay = 10f;

    private float nextTeleportTime;
    private int currentShieldHealth;
    private float lastDamageTime;
    private bool isShieldActive = true;

    protected override void Awake()
    {
        usesAttackState = true;
        attackDistance = 3f;

        base.Awake();

        gameObject.name = "Evil Father";
        currentShieldHealth = shieldHealth;
    }

    protected override void InitializeComponents()
    {
        base.InitializeComponents();

        // Make sure the collider is NOT a trigger for attack state
        if (col != null)
        {
            col.isTrigger = false;
        }
    }

    protected override void HandleBehavior()
    {
        if (isSpawning)
        {
            HandleIdleSpawnState();
            return;
        }

        // Regenerate shield after delay
        if (!isShieldActive && Time.time - lastDamageTime > shieldRegenDelay)
        {
            isShieldActive = true;
            currentShieldHealth = shieldHealth;
            Debug.Log("Evil Father's shield regenerated!");
        }

        // Teleport ability
        if (Time.time >= nextTeleportTime && target != null)
        {
            PerformTeleport();
            nextTeleportTime = Time.time + teleportCooldown;
        }

        // Normal chase/attack behavior
        if (isGoodDream)
        {
            HandleFleeState();
        }
        else
        {
            HandleChaseState();
        }

        LookAtTarget();
    }

    private void PerformTeleport()
    {
        if (target == null) return;

        // Teleport behind or to the side of player
        Vector2 directionToPlayer = (target.transform.position - transform.position).normalized;
        Vector2 perpendicularDir = new Vector2(-directionToPlayer.y, directionToPlayer.x);

        // Random teleport: behind player or to the side
        Vector2 teleportOffset;
        if (Random.value > 0.5f)
        {
            // Behind player
            teleportOffset = -directionToPlayer * teleportRange;
        }
        else
        {
            // To the side
            teleportOffset = perpendicularDir * teleportRange * (Random.value > 0.5f ? 1f : -1f);
        }

        Vector3 newPosition = target.transform.position + (Vector3)teleportOffset;
        transform.position = newPosition;

        Debug.Log("Evil Father teleported!");
    }

    public override void TakeDamage(float damageAmount, Vector2 knockbackDirection)
    {
        if (isSpawning) return;

        // Shield absorbs damage first
        if (isShieldActive)
        {
            currentShieldHealth -= (int)damageAmount;
            lastDamageTime = Time.time;

            if (currentShieldHealth <= 0)
            {
                isShieldActive = false;
                float overflow = -currentShieldHealth;
                if (overflow > 0)
                {
                    base.TakeDamage(overflow, knockbackDirection);
                }
                Debug.Log("Evil Father's shield broken!");
            }
            else
            {
                Debug.Log($"Evil Father's shield absorbed damage! Shield: {currentShieldHealth}/{shieldHealth}");
            }
        }
        else
        {
            base.TakeDamage(damageAmount, knockbackDirection);
            lastDamageTime = Time.time;
        }
    }

    public override void InitializeEnemy(float speed, float health, float dmg, float sze, AttackType attack, bool goodDream = false, float spawnIdleTime = 1f)
    {
        base.InitializeEnemy(speed, health, dmg, sze, attack, goodDream, spawnIdleTime);
        gameObject.name = "Evil Father";
        usesAttackState = true;
        currentShieldHealth = shieldHealth;
    }
    public override void Die()
    {
        base.Die()
        if (PersistentPlayerManager.Instance != null)
        {
            PersistentPlayerManager.Instance.bossCoins += 1;
        }
    }
}

/// <summary>
/// BOSS: The Mare - Second boss, uses shadow clones and area denial
/// </summary>
public class TheMare : Enemy
{
    [Header("Mare Settings")]
    [SerializeField] private int numberOfClones = 2;
    [SerializeField] private float cloneSpawnCooldown = 15f;
    [SerializeField] private float shadowDashSpeed = 15f;
    [SerializeField] private float shadowDashCooldown = 4f;
    [SerializeField] private float shadowDashDuration = 0.5f;
    [SerializeField] private GameObject clonePrefab;

    [Header("Arena Bounds")]
    [SerializeField] private float arenaWidth = 15f; // Arena width
    [SerializeField] private float arenaHeight = 11f; // Arena height

    private float nextCloneSpawnTime;
    private float nextShadowDashTime;
    private bool isShadowDashing = false;
    private Vector2 dashDirection;

    protected override void Awake()
    {
        usesAttackState = true;
        attackDistance = 2f;

        base.Awake();

        gameObject.name = "The Mare";
    }

    protected override void HandleBehavior()
    {
        if (isSpawning)
        {
            HandleIdleSpawnState();
            return;
        }

        if (isShadowDashing)
        {
            // Continue dashing
            return;
        }

        // Spawn shadow clones
        if (Time.time >= nextCloneSpawnTime && !isGoodDream)
        {
            SpawnShadowClones();
            nextCloneSpawnTime = Time.time + cloneSpawnCooldown;
        }

        // Shadow dash ability
        if (Time.time >= nextShadowDashTime && target != null && !isGoodDream)
        {
            StartCoroutine(PerformShadowDash());
            nextShadowDashTime = Time.time + shadowDashCooldown;
        }

        // Normal behavior
        if (isGoodDream)
        {
            HandleFleeState();
        }
        else
        {
            HandleChaseState();
        }

        LookAtTarget();
    }

    private void SpawnShadowClones()
    {
        // Calculate arena bounds from configurable dimensions
        float arenaHalfWidth = arenaWidth / 2f;
        float arenaHalfHeight = arenaHeight / 2f;

        for (int i = 0; i < numberOfClones; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * 3f;
            Vector3 spawnPos = transform.position + (Vector3)randomOffset;

            // Clamp spawn position to arena bounds
            spawnPos.x = Mathf.Clamp(spawnPos.x, -arenaHalfWidth, arenaHalfWidth);
            spawnPos.y = Mathf.Clamp(spawnPos.y, -arenaHalfHeight, arenaHalfHeight);
            spawnPos.z = 0f;

            // Create a weaker clone enemy
            GameObject clone = new GameObject("Shadow Clone");
            clone.transform.position = spawnPos;
            clone.transform.localScale = Vector3.one * (size * 0.7f);

            DefaultEnemy cloneEnemy = clone.AddComponent<DefaultEnemy>();
            cloneEnemy.InitializeEnemy(
                speed: movementSpeed * 0.8f,
                health: maxHealth * 0.2f,
                damage * 0.5f,
                size * 0.7f,
                attackType,
                isGoodDream,
                0f
            );

            // Copy the sprite from the original Mare
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                cloneEnemy.SetSprite(spriteRenderer.sprite);
                // Make clones visually distinct with darker, semi-transparent appearance
                if (cloneEnemy.SpriteRenderer != null)
                {
                    cloneEnemy.SpriteRenderer.color = new Color(0.3f, 0.3f, 0.5f, 0.7f);
                }
            }

            // Auto-destroy clones after 10 seconds
            Destroy(clone, 10f);
        }

        Debug.Log($"The Mare spawned {numberOfClones} shadow clones!");
    }

    private IEnumerator PerformShadowDash()
    {
        if (target == null) yield break;

        isShadowDashing = true;
        moveDirection = Vector2.zero;

        // Calculate dash direction toward player
        dashDirection = (target.transform.position - transform.position).normalized;

        // Make partially transparent during dash
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
        }

        float dashTimer = 0f;
        while (dashTimer < shadowDashDuration)
        {
            rb.linearVelocity = dashDirection * shadowDashSpeed;
            dashTimer += Time.deltaTime;
            yield return null;
        }

        // Restore appearance
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }

        rb.linearVelocity = Vector2.zero;
        isShadowDashing = false;

        Debug.Log("The Mare performed shadow dash!");
    }

    protected override void Move()
    {
        if (isShadowDashing)
        {
            // Dash movement is handled in coroutine
            return;
        }
        base.Move();
    }

    public override void InitializeEnemy(float speed, float health, float dmg, float sze, AttackType attack, bool goodDream = false, float spawnIdleTime = 1f)
    {
        base.InitializeEnemy(speed, health, dmg, sze, attack, goodDream, spawnIdleTime);
        gameObject.name = "The Mare";
        usesAttackState = true;
    }
    public override void Die()
    {
        base.Die()
        if (PersistentPlayerManager.Instance != null)
        {
            PersistentPlayerManager.Instance.bossCoins += 1;
        }
    }
}

/// <summary>
/// BOSS: The Devil - Final boss, multi-phase with increasing difficulty
/// </summary>
public class TheDevil : Enemy
{
    [Header("Devil Settings")]
    [SerializeField] private float phase2HealthThreshold = 0.66f;
    [SerializeField] private float phase3HealthThreshold = 0.33f;
    [SerializeField] private float hellFireCooldown = 18f; // Long cooldown between volleys
    [SerializeField] private int hellFireWaves = 3; // Number of waves per volley
    [SerializeField] private float hellFireWaveDelay = 0.6f; // Delay between waves
    [SerializeField] private float hellFireRadius = 4f;
    [SerializeField] private int hellFireCount = 8;
    [SerializeField] private float hellFireSpeed = 7f;
    [SerializeField] private GameObject hellFirePrefab;
    [SerializeField] private Sprite hellFireSprite;
    [SerializeField] private float enrageDamageMultiplier = 1.5f;
    [SerializeField] private float enrageSpeedMultiplier = 1.3f;
    [SerializeField] private float summonCooldown = 15f;
    [SerializeField] private int minionCount = 2;
    [SerializeField] private float groundSlamCooldown = 7f;
    [SerializeField] private float groundSlamRadius = 1f;
    [SerializeField] private Sprite groundSlamWaveSprite; // Sprite for ground slam wave
    [SerializeField] private float meteorCooldown = 12f;
    [SerializeField] private int meteorCount = 5;
    [SerializeField] private Sprite meteorWarningSprite; // Sprite for meteor warning
    [SerializeField] private Sprite meteorImpactSprite; // Sprite for meteor impact

    [Header("Arena Bounds")]
    [SerializeField] private float arenaWidth = 15f; // Arena width
    [SerializeField] private float arenaHeight = 11f; // Arena height

    private int currentPhase = 1;
    private float nextHellFireTime;
    private float nextSummonTime;
    private float nextGroundSlamTime;
    private float nextMeteorTime;
    private bool isEnraged = false;
    private float baseSpeed;
    private float baseDamage;
    private bool hasEnteredPhase2 = false;
    private bool hasEnteredPhase3 = false;
    private bool isCastingHellfire = false; // Prevent overlapping hellfire casts

    protected override void Awake()
    {
        usesAttackState = true;
        attackDistance = 3f;

        base.Awake();

        gameObject.name = "The Devil";
        baseSpeed = movementSpeed;
        baseDamage = damage;
    }

    protected override void InitializeComponents()
    {
        base.InitializeComponents();

        // Make sure the collider is NOT a trigger for attack state
        if (col != null)
        {
            col.isTrigger = false;
        }
    }

    protected override void HandleBehavior()
    {
        if (isSpawning)
        {
            HandleIdleSpawnState();
            return;
        }

        // Check for phase transitions
        CheckPhaseTransition();

        if (!isGoodDream && target != null)
        {
            // Hellfire attack - all phases (multiple waves in quick succession)
            if (Time.time >= nextHellFireTime && !isCastingHellfire)
            {
                StartCoroutine(SpawnHellFireVolley());
                nextHellFireTime = Time.time + hellFireCooldown;
            }

            // Ground Slam - ALL PHASES (now available from start)
            if (Time.time >= nextGroundSlamTime)
            {
                PerformGroundSlam();
                nextGroundSlamTime = Time.time + groundSlamCooldown;
            }

            // Summon minions - Phase 2 and 3
            if (currentPhase >= 2 && Time.time >= nextSummonTime)
            {
                SummonMinions();
                nextSummonTime = Time.time + summonCooldown;
            }

            // Meteor Rain - Phase 3 only
            if (currentPhase >= 3 && Time.time >= nextMeteorTime)
            {
                StartCoroutine(MeteorRain());
                nextMeteorTime = Time.time + meteorCooldown;
            }
        }

        // Normal behavior
        if (isGoodDream)
        {
            HandleFleeState();
        }
        else
        {
            HandleChaseState();
        }

        LookAtTarget();
    }

    private void CheckPhaseTransition()
    {
        float healthPercent = currentHealth / maxHealth;

        if (currentPhase == 1 && healthPercent <= phase2HealthThreshold && !hasEnteredPhase2)
        {
            EnterPhase2();
            hasEnteredPhase2 = true;
        }
        else if (currentPhase == 2 && healthPercent <= phase3HealthThreshold && !hasEnteredPhase3)
        {
            EnterPhase3();
            hasEnteredPhase3 = true;
        }
    }

    private void EnterPhase2()
    {
        currentPhase = 2;

        // Increase stats
        movementSpeed = baseSpeed * 1.2f;
        damage = baseDamage * 1.2f;
        hellFireCooldown = 15f; // Slightly faster hellfire volleys
        hellFireWaves = 4; // More waves per volley
        hellFireCount = 12;
        summonCooldown = 10f;
        groundSlamCooldown = 6f; // Faster ground slams

        Debug.Log("The Devil entered Phase 2! More hellfire waves and faster attacks!");

        // Visual indicator - could make sprite darker/redder
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.7f, 0.7f);
        }

        // Heal a bit on phase transition to make fight longer
        Heal(maxHealth * 0.1f);
    }

    private void EnterPhase3()
    {
        currentPhase = 3;
        isEnraged = true;

        // Dramatically increase stats
        movementSpeed = baseSpeed * enrageSpeedMultiplier;
        damage = baseDamage * enrageDamageMultiplier;
        hellFireCooldown = 12f; // Even faster hellfire
        hellFireWaves = 5; // Maximum waves per volley
        hellFireCount = 16;
        summonCooldown = 8f;
        groundSlamCooldown = 4f; // Very fast ground slams
        meteorCooldown = 12f;

        Debug.Log("The Devil entered Phase 3 - ENRAGED! METEOR RAIN UNLOCKED! MAXIMUM HELLFIRE!");

        // Visual indicator - make sprite red
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.3f, 0.3f);
        }

        // Heal on phase transition
        Heal(maxHealth * 0.15f);
    }

    private System.Collections.IEnumerator SpawnHellFireVolley()
    {
        isCastingHellfire = true;

        // Shoot multiple waves in quick succession
        for (int wave = 0; wave < hellFireWaves; wave++)
        {
            SpawnHellFireWave();

            // Wait before next wave (except after last wave)
            if (wave < hellFireWaves - 1)
            {
                yield return new WaitForSeconds(hellFireWaveDelay);
            }
        }

        isCastingHellfire = false;
        Debug.Log($"The Devil unleashed {hellFireWaves} waves of hellfire!");
    }

    private void SpawnHellFireWave()
    {
        if (target == null) return;

        // Create a ring of hellfire around the boss
        float angleStep = 360f / hellFireCount;

        for (int i = 0; i < hellFireCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector3 spawnPos = transform.position + (Vector3)(direction * 1f);

            // Create hellfire projectile
            GameObject hellFire = new GameObject("HellFire");
            hellFire.transform.position = spawnPos;
            hellFire.transform.localScale = Vector3.one * 0.5f;

            // Add sprite renderer for visibility
            SpriteRenderer sr = hellFire.AddComponent<SpriteRenderer>();
            if (hellFireSprite != null)
            {
                sr.sprite = hellFireSprite;
            }
            else
            {
                // Create a simple colored circle if no sprite assigned
                sr.color = new Color(1f, 0.3f, 0f, 1f); // Orange-red color
            }

            // Add projectile component
            Projectile proj = hellFire.AddComponent<Projectile>();
            proj.Initialize(direction, hellFireSpeed, damage * 0.8f, this, 5f);

            Destroy(hellFire, 3f);
        }
    }

    private void SummonMinions()
    {
        if (target == null) return;

        // Calculate arena bounds from configurable dimensions
        float arenaHalfWidth = arenaWidth / 2f;
        float arenaHalfHeight = arenaHeight / 2f;

        for (int i = 0; i < minionCount; i++)
        {
            // Generate random offset but clamp to arena bounds
            Vector2 randomOffset = Random.insideUnitCircle * 4f;
            Vector3 spawnPos = transform.position + (Vector3)randomOffset;

            // Clamp spawn position to arena bounds
            spawnPos.x = Mathf.Clamp(spawnPos.x, -arenaHalfWidth, arenaHalfWidth);
            spawnPos.y = Mathf.Clamp(spawnPos.y, -arenaHalfHeight, arenaHalfHeight);
            spawnPos.z = 0f;

            // Create minion
            GameObject minion = new GameObject("Devil Minion");
            minion.transform.position = spawnPos;
            minion.transform.localScale = Vector3.one * (size * 0.5f);

            DefaultEnemy minionEnemy = minion.AddComponent<DefaultEnemy>();
            minionEnemy.InitializeEnemy(
                speed: movementSpeed * 0.7f,
                health: maxHealth * 0.15f,
                damage * 0.6f,
                size * 0.5f,
                attackType,
                isGoodDream,
                0f
            );

            // Copy sprite and make it look demonic
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                minionEnemy.SetSprite(spriteRenderer.sprite);
                if (minionEnemy.SpriteRenderer != null)
                {
                    minionEnemy.SpriteRenderer.color = new Color(0.5f, 0f, 0f, 0.8f); // Dark red
                }
            }

            // Auto-destroy after 15 seconds
            Destroy(minion, 15f);
        }

        Debug.Log($"The Devil summoned {minionCount} minions!");
    }

    private void PerformGroundSlam()
    {
        if (target == null) return;

        // AOE damage around the boss
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, groundSlamRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Character player = hit.GetComponent<Character>();
                if (player != null)
                {
                    Vector2 knockbackDir = (player.transform.position - transform.position).normalized;
                    float slamDamage = damage * 1.5f;
                    player.TakeDamage(slamDamage, knockbackDir * 15f);
                }
            }
        }

        // Visual effect - create expanding wave
        StartCoroutine(CreateGroundSlamWave());

        Debug.Log("The Devil performed a devastating ground slam!");
    }

    private System.Collections.IEnumerator CreateGroundSlamWave()
    {
        // Create visual wave effect
        GameObject wave = new GameObject("Ground Slam Wave");
        wave.transform.position = transform.position;

        SpriteRenderer waveRenderer = wave.AddComponent<SpriteRenderer>();

        // Use sprite if available, otherwise use colored visual
        if (groundSlamWaveSprite != null)
        {
            waveRenderer.sprite = groundSlamWaveSprite;
            waveRenderer.color = new Color(1f, 0f, 0f, 0.7f); // Bright red with good visibility
        }
        else
        {
            // Fallback: create visible colored indicator
            waveRenderer.color = new Color(1f, 0f, 0f, 0.7f); // Bright red
        }

        waveRenderer.sortingOrder = 10; // Make sure it's visible above ground

        // Add circle collider with standard radius 0.5, scale via transform
        CircleCollider2D waveCollider = wave.AddComponent<CircleCollider2D>();
        waveCollider.radius = 0.5f; // Always 0.5
        waveCollider.isTrigger = true;

        float elapsed = 0f;
        float duration = 0.5f;
        float startScale = 1f;
        float endScale = groundSlamRadius * 4f; // Scale transform to match visual size

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Scale the TRANSFORM, not the collider radius
            wave.transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, progress);

            Color color = waveRenderer.color;
            color.a = Mathf.Lerp(0.7f, 0f, progress); // Start more visible
            waveRenderer.color = color;

            yield return null;
        }

        Destroy(wave);
    }

    private System.Collections.IEnumerator MeteorRain()
    {
        if (target == null) yield break;

        Debug.Log("The Devil summons METEOR RAIN!");

        // Calculate arena bounds from configurable dimensions
        float arenaHalfWidth = arenaWidth / 2f;
        float arenaHalfHeight = arenaHeight / 2f;

        for (int i = 0; i < meteorCount; i++)
        {
            // Spawn meteors near player but within arena bounds
            Vector2 randomOffset = Random.insideUnitCircle * 8f;
            Vector3 spawnPos = target.transform.position + (Vector3)randomOffset;

            // Clamp spawn position to arena bounds (with small margin for meteor radius)
            float meteorMargin = 2f; // Keep meteors away from edges
            spawnPos.x = Mathf.Clamp(spawnPos.x, -arenaHalfWidth + meteorMargin, arenaHalfWidth - meteorMargin);
            spawnPos.y = Mathf.Clamp(spawnPos.y, -arenaHalfHeight + meteorMargin, arenaHalfHeight - meteorMargin);
            spawnPos.z = 0f;

            // Create warning indicator
            GameObject warning = new GameObject("Meteor Warning");
            warning.transform.position = spawnPos;
            warning.transform.localScale = Vector3.one;

            SpriteRenderer warningRenderer = warning.AddComponent<SpriteRenderer>();

            // Use sprite if available
            if (meteorWarningSprite != null)
            {
                warningRenderer.sprite = meteorWarningSprite;
                warningRenderer.color = new Color(1f, 0f, 0f, 0.6f); // Bright red warning
            }
            else
            {
                // Fallback: bright red circle
                warningRenderer.color = new Color(1f, 0f, 0f, 0.6f);
            }

            warningRenderer.sortingOrder = 10; // Make sure it's visible

            // Circle collider with standard radius 0.5, scale via transform
            CircleCollider2D warningCollider = warning.AddComponent<CircleCollider2D>();
            warningCollider.radius = 0.5f; // Always 0.5
            warningCollider.isTrigger = true;

            // Wait before meteor impact
            yield return new WaitForSeconds(1f);

            // Meteor impact - create damaging AOE (using Physics2D.OverlapCircle with correct world-space radius)
            float impactRadius = 1.5f; // 1.5 unit world-space radius
            Collider2D[] hits = Physics2D.OverlapCircleAll(spawnPos, impactRadius);
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    Character player = hit.GetComponent<Character>();
                    if (player != null)
                    {
                        Vector2 knockbackDir = (player.transform.position - (Vector3)spawnPos).normalized;
                        player.TakeDamage(damage * 1.2f, knockbackDir * 10f);
                    }
                }
            }

            // Visual meteor impact
            if (meteorImpactSprite != null)
            {
                warningRenderer.sprite = meteorImpactSprite;
            }
            warningRenderer.color = new Color(1f, 0.5f, 0f, 1f); // Bright orange impact

            yield return new WaitForSeconds(0.1f);

            Destroy(warning);

            // Short delay between meteors
            yield return new WaitForSeconds(0.3f);
        }
    }

    public override void InitializeEnemy(float speed, float health, float dmg, float sze, AttackType attack, bool goodDream = false, float spawnIdleTime = 1f)
    {
        base.InitializeEnemy(speed, health, dmg, sze, attack, goodDream, spawnIdleTime);
        gameObject.name = "The Devil";
        usesAttackState = true;
        baseSpeed = speed;
        baseDamage = dmg;
    }

    /// <summary>
    /// Set the sprite for hellfire projectiles
    /// </summary>
    public void SetHellFireSprite(Sprite sprite)
    {
        hellFireSprite = sprite;
    }

    /// <summary>
    /// Set the sprite for ground slam wave
    /// </summary>
    public void SetGroundSlamWaveSprite(Sprite sprite)
    {
        groundSlamWaveSprite = sprite;
    }

    /// <summary>
    /// Set the sprite for meteor warning
    /// </summary>
    public void SetMeteorWarningSprite(Sprite sprite)
    {
        meteorWarningSprite = sprite;
    }

    /// <summary>
    /// Set the sprite for meteor impact
    /// </summary>
    public void SetMeteorImpactSprite(Sprite sprite)
    {
        meteorImpactSprite = sprite;
    }

    protected override void Die()
    {
        Debug.Log("THE DEVIL HAS BEEN DEFEATED! VICTORY!");

        // Create massive victory explosion effect
        StartCoroutine(DeathExplosion());

        // Don't destroy immediately - let explosion play
        Destroy(gameObject, 2f);
    }

    private System.Collections.IEnumerator DeathExplosion()
    {
        // Multiple explosion waves
        for (int wave = 0; wave < 3; wave++)
        {
            int explosionCount = 12;
            float radius = 2f + wave * 2f;

            for (int i = 0; i < explosionCount; i++)
            {
                float angle = (360f / explosionCount * i) * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector3 explosionPos = transform.position + (Vector3)(direction * radius);

                GameObject explosion = new GameObject("Victory Explosion");
                explosion.transform.position = explosionPos;

                SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
                sr.color = new Color(1f, 0.8f, 0f, 1f);
                sr.sortingOrder = 10;

                Destroy(explosion, 0.5f);
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    protected void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Draw ground slam radius (available in all phases)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, groundSlamRadius);

        // Draw hellfire radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hellFireRadius);
    }
    public override void Die()
    {
        base.Die()
        if (PersistentPlayerManager.Instance != null)
        {
            PersistentPlayerManager.Instance.bossCoins += 1;
        }
    }
}