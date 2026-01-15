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
    [SerializeField] private float shieldRegenDelay = 8f;

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
}

/// <summary>
/// BOSS: The Mare - Second boss, uses shadow clones and area denial
/// </summary>
public class TheMare : Enemy
{
    [Header("Mare Settings")]
    [SerializeField] private int numberOfClones = 3;
    [SerializeField] private float cloneSpawnCooldown = 8f;
    [SerializeField] private float shadowDashSpeed = 15f;
    [SerializeField] private float shadowDashCooldown = 4f;
    [SerializeField] private float shadowDashDuration = 0.5f;
    [SerializeField] private GameObject clonePrefab;

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
        for (int i = 0; i < numberOfClones; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * 3f;
            Vector3 spawnPos = transform.position + (Vector3)randomOffset;

            // Create a weaker clone enemy
            GameObject clone = new GameObject("Shadow Clone");
            clone.transform.position = spawnPos;

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

            // Make clones visually distinct
            if (cloneEnemy.SpriteRenderer != null)
            {
                cloneEnemy.SpriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
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
}

/// <summary>
/// BOSS: The Devil - Final boss, multi-phase with increasing difficulty
/// </summary>
public class TheDevil : Enemy
{
    [Header("Devil Settings")]
    [SerializeField] private float phase2HealthThreshold = 0.66f;
    [SerializeField] private float phase3HealthThreshold = 0.33f;
    [SerializeField] private float hellFireCooldown = 3f;
    [SerializeField] private float hellFireRadius = 4f;
    [SerializeField] private int hellFireCount = 8;
    [SerializeField] private GameObject hellFirePrefab;
    [SerializeField] private float enrageDamageMultiplier = 1.5f;
    [SerializeField] private float enrageSpeedMultiplier = 1.3f;

    private int currentPhase = 1;
    private float nextHellFireTime;
    private bool isEnraged = false;
    private float baseSpeed;
    private float baseDamage;

    protected override void Awake()
    {
        usesAttackState = true;
        attackDistance = 3f;

        base.Awake();

        gameObject.name = "The Devil";
        baseSpeed = movementSpeed;
        baseDamage = damage;
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

        // Hellfire attack
        if (Time.time >= nextHellFireTime && !isGoodDream && target != null)
        {
            SpawnHellFire();
            nextHellFireTime = Time.time + hellFireCooldown;
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

        if (currentPhase == 1 && healthPercent <= phase2HealthThreshold)
        {
            EnterPhase2();
        }
        else if (currentPhase == 2 && healthPercent <= phase3HealthThreshold)
        {
            EnterPhase3();
        }
    }

    private void EnterPhase2()
    {
        currentPhase = 2;

        // Increase stats
        movementSpeed = baseSpeed * 1.2f;
        damage = baseDamage * 1.2f;
        hellFireCooldown *= 0.8f;
        hellFireCount = 12;

        Debug.Log("The Devil entered Phase 2!");

        // Visual indicator - could make sprite darker/redder
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.7f, 0.7f);
        }
    }

    private void EnterPhase3()
    {
        currentPhase = 3;
        isEnraged = true;

        // Dramatically increase stats
        movementSpeed = baseSpeed * enrageSpeedMultiplier;
        damage = baseDamage * enrageDamageMultiplier;
        hellFireCooldown *= 0.5f;
        hellFireCount = 16;

        Debug.Log("The Devil entered Phase 3 - ENRAGED!");

        // Visual indicator - make sprite red
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.3f, 0.3f);
        }
    }

    private void SpawnHellFire()
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

            Projectile proj = hellFire.AddComponent<Projectile>();
            proj.Initialize(direction, 5f, damage * 0.8f, this, 5f);

            // Add visual component
            SpriteRenderer sr = hellFire.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.red;
            }

            Destroy(hellFire, 3f);
        }

        Debug.Log($"The Devil unleashed {hellFireCount} hellfire projectiles!");
    }

    public override void InitializeEnemy(float speed, float health, float dmg, float sze, AttackType attack, bool goodDream = false, float spawnIdleTime = 1f)
    {
        base.InitializeEnemy(speed, health, dmg, sze, attack, goodDream, spawnIdleTime);
        gameObject.name = "The Devil";
        usesAttackState = true;
        baseSpeed = speed;
        baseDamage = dmg;
    }

    protected override void Die()
    {
        Debug.Log("THE DEVIL HAS BEEN DEFEATED!");

        // Could trigger special victory sequence here
        base.Die();
    }
}