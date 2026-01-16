using UnityEngine;

/// <summary>
/// Manager class to spawn and configure characters from external scripts
/// </summary>
public class CharacterManager : MonoBehaviour
{
    [Header("Prefabs (Optional - will create if not assigned)")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject defaultEnemyPrefab;
    [SerializeField] private GameObject healEnemyPrefab;
    [SerializeField] private GameObject rangedEnemyPrefab;
    [SerializeField] private GameObject tankEnemyPrefab;
    [SerializeField] private GameObject evilFatherPrefab;
    [SerializeField] private GameObject theMarePrefab;
    [SerializeField] private GameObject theDevilPrefab;

    [Header("Attack Types")]
    [SerializeField] private AttackType meleeAttack;
    [SerializeField] private AttackType rangedAttack;
    [SerializeField] private AttackType aoeAttack;

    [Header("Sprites (Optional)")]
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private Sprite defaultEnemySprite;
    [SerializeField] private Sprite healEnemySprite;
    [SerializeField] private Sprite rangedEnemySprite;
    [SerializeField] private Sprite tankEnemySprite;
    [SerializeField] private Sprite evilFatherSprite;
    [SerializeField] private Sprite theMareSprite;
    [SerializeField] private Sprite theDevilSprite;
    [SerializeField] private Sprite hellFireSprite;
    [SerializeField] private Sprite groundSlamWaveSprite; // For Devil's ground slam
    [SerializeField] private Sprite meteorWarningSprite; // For Devil's meteor warning
    [SerializeField] private Sprite meteorImpactSprite; // For Devil's meteor impact

    [Header("Player Melee Attack Animation")]
    [SerializeField] private Sprite[] meleeAttackAnimationFrames;
    [SerializeField] private float meleeAnimationFrameRate = 24f;
    [SerializeField] private Color meleeAttackColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private bool showMeleeVisual = true;
    [SerializeField] private Vector2 meleeAttackAspectRatio = new Vector2(1f, 1f);

    [Header("Player Bomb Animation")]
    [SerializeField] private Sprite[] bombAnimationFrames;
    [SerializeField] private float bombAnimationFrameRate = 12f;

    [Header("Player Explosion Animation")]
    [SerializeField] private Sprite[] explosionAnimationFrames;
    [SerializeField] private float explosionAnimationFrameRate = 24f;

    [Header("Player Ground Slam Animation")]
    [SerializeField] private Sprite[] groundSlamAnimationFrames;
    [SerializeField] private float groundSlamAnimationFrameRate = 24f;

    [Header("Player Shoot Projectile")]
    [SerializeField] private Sprite projectileSprite;
    [SerializeField] private bool shootTowardsMouse = false;

    [Header("Dream State")]
    [SerializeField] private bool isGoodDream = true;

    [Header("Enemy Spawn Settings")]
    [SerializeField] private float defaultSpawnIdleTime = 1f;

    [Header("Health Bar Settings")]
    [SerializeField] private bool createHealthBars = true;

    [Header("Enemy Health Bar Settings")]
    [SerializeField] private Vector2 enemyHealthBarSize = new Vector2(1f, 0.1f);
    [SerializeField] private Vector3 enemyHealthBarOffset = new Vector3(0, 0.8f, 0);
    [SerializeField] private Vector2 bossHealthBarSize = new Vector2(2f, 0.2f);
    [SerializeField] private Vector3 bossHealthBarOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] private Color enemyHealthBarBackground = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color enemyHealthBarFill = new Color(0f, 1f, 0f, 1f);
    [SerializeField] private Color enemyHealthBarLowHealth = new Color(1f, 0f, 0f, 1f);
    [SerializeField] private float enemyHealthBarDisplayDuration = 5f;

    [Header("Player Health Bar Settings")]
    [SerializeField] private Vector2 playerHealthBarSize = new Vector2(300f, 40f);
    [SerializeField] private Vector2 playerHealthBarPosition = new Vector2(20f, -20f);
    [SerializeField] private Color playerHealthBarBackground = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    [SerializeField] private Color playerHealthBarFill = new Color(0f, 1f, 0f, 1f);
    [SerializeField] private Color playerHealthBarLowHealth = new Color(1f, 0f, 0f, 1f);
    [SerializeField] private bool showPlayerHealthText = true;

    // Reference to player health bar
    private PlayerHealthBar activePlayerHealthBar;

    /// <summary>
    /// Spawn and initialize a player character
    /// </summary>
    public Player SpawnPlayer(Vector3 position, float speed = 4f, float health = 100f, float damage = 10f, float size = 0.5f, AttackType attackType = null)
    {
        GameObject playerObj;

        if (playerPrefab != null)
        {
            playerObj = Instantiate(playerPrefab, position, Quaternion.identity);
        }
        else
        {
            playerObj = new GameObject("Player");
            playerObj.transform.position = position;
        }

        playerObj.transform.localScale = Vector3.one * size;

        Player player = playerObj.GetComponent<Player>();
        if (player == null)
        {
            player = playerObj.AddComponent<Player>();
        }

        // Use provided attack type or default to melee
        AttackType attack = attackType ?? meleeAttack;
        player.InitializePlayer(speed, health, damage, attack, isGoodDream);

        if (playerSprite != null)
        {
            player.SetSprite(playerSprite);
        }

        // Configure all attack visuals
        ConfigurePlayerAttackVisuals(player);

        if (createHealthBars)
        {
            CreatePlayerHealthBar(player);
        }

        Debug.Log($"Player spawned at {position}");
        return player;
    }

    /// <summary>
    /// Configure all player attack visual settings
    /// </summary>
    private void ConfigurePlayerAttackVisuals(Player player)
    {
        // Configure melee attack
        ConfigureMeleeAttack(player);

        // Configure bomb attack
        ConfigureBombAttack(player);

        // Configure ground slam attack
        ConfigureGroundSlamAttack(player);

        // Configure shoot attack
        ConfigureShootAttack(player);
    }

    /// <summary>
    /// Configure melee attack visuals
    /// </summary>
    private void ConfigureMeleeAttack(Player player)
    {
        PlayerMeleeAttack meleeAttackComponent = player.GetMeleeAttack();

        if (meleeAttackComponent != null)
        {
            if (meleeAttackAnimationFrames != null && meleeAttackAnimationFrames.Length > 0)
            {
                meleeAttackComponent.SetAnimationFrames(meleeAttackAnimationFrames);
                meleeAttackComponent.SetFrameRate(meleeAnimationFrameRate);
                Debug.Log($"Player melee attack animation configured: {meleeAttackAnimationFrames.Length} frames at {meleeAnimationFrameRate} fps");
            }

            meleeAttackComponent.SetHitboxColor(meleeAttackColor);
            meleeAttackComponent.SetShowVisual(showMeleeVisual);
            meleeAttackComponent.SetSpriteAspectRatio(meleeAttackAspectRatio);

            Debug.Log($"Player melee attack visuals configured: Animation Frames={meleeAttackAnimationFrames?.Length ?? 0}, Color={meleeAttackColor}, Show={showMeleeVisual}, AspectRatio={meleeAttackAspectRatio}");
        }
    }

    /// <summary>
    /// Configure bomb attack visuals
    /// </summary>
    private void ConfigureBombAttack(Player player)
    {
        PlayerBombAttack bombAttackComponent = player.GetBombAttack();

        if (bombAttackComponent != null)
        {
            if (bombAnimationFrames != null && bombAnimationFrames.Length > 0)
            {
                bombAttackComponent.SetBombAnimationFrames(bombAnimationFrames);
                bombAttackComponent.SetBombAnimationFrameRate(bombAnimationFrameRate);
                Debug.Log($"Player bomb animation configured: {bombAnimationFrames.Length} frames at {bombAnimationFrameRate} fps");
            }

            if (explosionAnimationFrames != null && explosionAnimationFrames.Length > 0)
            {
                bombAttackComponent.SetExplosionAnimationFrames(explosionAnimationFrames);
                bombAttackComponent.SetExplosionAnimationFrameRate(explosionAnimationFrameRate);
                Debug.Log($"Player explosion animation configured: {explosionAnimationFrames.Length} frames at {explosionAnimationFrameRate} fps");
            }

            Debug.Log($"Player bomb attack visuals configured: Bomb Frames={bombAnimationFrames?.Length ?? 0}, Explosion Frames={explosionAnimationFrames?.Length ?? 0}");
        }
    }

    /// <summary>
    /// Configure ground slam attack visuals
    /// </summary>
    private void ConfigureGroundSlamAttack(Player player)
    {
        PlayerGroundSlamAttack groundSlamComponent = player.GetGroundSlamAttack();

        if (groundSlamComponent != null)
        {
            if (groundSlamAnimationFrames != null && groundSlamAnimationFrames.Length > 0)
            {
                groundSlamComponent.SetSlamAnimationFrames(groundSlamAnimationFrames);
                groundSlamComponent.SetSlamAnimationFrameRate(groundSlamAnimationFrameRate);
                Debug.Log($"Player ground slam animation configured: {groundSlamAnimationFrames.Length} frames at {groundSlamAnimationFrameRate} fps");
            }

            Debug.Log($"Player ground slam attack visuals configured: Animation Frames={groundSlamAnimationFrames?.Length ?? 0}");
        }
    }

    /// <summary>
    /// Configure shoot attack visuals
    /// </summary>
    private void ConfigureShootAttack(Player player)
    {
        PlayerShootAttack shootAttackComponent = player.GetShootAttack();

        if (shootAttackComponent != null)
        {
            if (projectileSprite != null)
            {
                shootAttackComponent.SetProjectileSprite(projectileSprite);
                Debug.Log($"Player projectile sprite configured");
            }

            shootAttackComponent.SetAttackTowardsMouse(shootTowardsMouse);

            Debug.Log($"Player shoot attack visuals configured: Has Sprite={projectileSprite != null}, TowardsMouse={shootTowardsMouse}");
        }
    }

    /// <summary>
    /// Spawn and initialize a Default enemy (uses collision damage)
    /// </summary>
    public DefaultEnemy SpawnDefaultEnemy(Vector3 position, float speed = 2f, float health = 50f, float damage = 8f, float size = 0.5f, AttackType attackType = null, float spawnIdleTime = -1f)
    {
        GameObject enemyObj = CreateEnemyObject(defaultEnemyPrefab, position, "Default Enemy");
        enemyObj.transform.localScale = Vector3.one * size;

        DefaultEnemy defaultEnemy = enemyObj.GetComponent<DefaultEnemy>();
        if (defaultEnemy == null)
        {
            defaultEnemy = enemyObj.AddComponent<DefaultEnemy>();
        }

        float idleTime = spawnIdleTime < 0 ? defaultSpawnIdleTime : spawnIdleTime;

        AttackType attack = attackType ?? meleeAttack;
        defaultEnemy.InitializeEnemy(speed, health, damage, size, attack, isGoodDream, idleTime);

        if (defaultEnemySprite != null)
        {
            defaultEnemy.SetSprite(defaultEnemySprite);
        }

        if (createHealthBars)
        {
            CreateEnemyHealthBar(defaultEnemy, false);
        }

        Debug.Log($"Default Enemy spawned at {position} with {idleTime}s spawn idle time (collision damage mode)");
        return defaultEnemy;
    }

    /// <summary>
    /// Spawn and initialize a Heal enemy (always flees, heals player on death)
    /// </summary>
    public HealEnemy SpawnHealEnemy(Vector3 position, float speed = 3.5f, float health = 20f, float damage = 0f, float size = 0.4f, float spawnIdleTime = -1f)
    {
        GameObject enemyObj = CreateEnemyObject(healEnemyPrefab, position, "Heal Enemy");
        enemyObj.transform.localScale = Vector3.one * size;

        HealEnemy healEnemy = enemyObj.GetComponent<HealEnemy>();
        if (healEnemy == null)
        {
            healEnemy = enemyObj.AddComponent<HealEnemy>();
        }

        float idleTime = spawnIdleTime < 0 ? defaultSpawnIdleTime : spawnIdleTime;

        healEnemy.InitializeEnemy(speed, health, damage, size, null, isGoodDream, idleTime);

        if (healEnemySprite != null)
        {
            healEnemy.SetSprite(healEnemySprite);
        }

        if (createHealthBars)
        {
            CreateEnemyHealthBar(healEnemy, false);
        }

        Debug.Log($"Heal Enemy spawned at {position} with {idleTime}s spawn idle time");
        return healEnemy;
    }

    /// <summary>
    /// Spawn and initialize a Ranged enemy (uses attack state)
    /// </summary>
    public RangedEnemy SpawnRangedEnemy(Vector3 position, float speed = 1.5f, float health = 30f, float damage = 12f, float size = 0.5f, float spawnIdleTime = -1f)
    {
        GameObject enemyObj = CreateEnemyObject(rangedEnemyPrefab, position, "Ranged Enemy");
        enemyObj.transform.localScale = Vector3.one * size;

        RangedEnemy rangedEnemy = enemyObj.GetComponent<RangedEnemy>();
        if (rangedEnemy == null)
        {
            rangedEnemy = enemyObj.AddComponent<RangedEnemy>();
        }

        float idleTime = spawnIdleTime < 0 ? defaultSpawnIdleTime : spawnIdleTime;

        rangedEnemy.InitializeEnemy(speed, health, damage, size, rangedAttack, isGoodDream, idleTime);

        if (rangedEnemySprite != null)
        {
            rangedEnemy.SetSprite(rangedEnemySprite);
        }

        if (createHealthBars)
        {
            CreateEnemyHealthBar(rangedEnemy, false);
        }

        Debug.Log($"Ranged Enemy spawned at {position} with {idleTime}s spawn idle time (attack state mode)");
        return rangedEnemy;
    }

    /// <summary>
    /// Spawn and initialize a Tank enemy (uses attack state)
    /// </summary>
    public TankEnemy SpawnTankEnemy(Vector3 position, float speed = 1f, float health = 150f, float damage = 20f, float size = 0.75f, float spawnIdleTime = -1f)
    {
        GameObject enemyObj = CreateEnemyObject(tankEnemyPrefab, position, "Tank Enemy");
        enemyObj.transform.localScale = Vector3.one * size;

        TankEnemy tankEnemy = enemyObj.GetComponent<TankEnemy>();
        if (tankEnemy == null)
        {
            tankEnemy = enemyObj.AddComponent<TankEnemy>();
        }

        float idleTime = spawnIdleTime < 0 ? defaultSpawnIdleTime : spawnIdleTime;

        AttackType attack = aoeAttack ?? meleeAttack;
        tankEnemy.InitializeEnemy(speed, health, damage, size, attack, isGoodDream, idleTime);

        if (tankEnemySprite != null)
        {
            tankEnemy.SetSprite(tankEnemySprite);
        }

        if (createHealthBars)
        {
            CreateEnemyHealthBar(tankEnemy, false);
        }

        Debug.Log($"Tank Enemy spawned at {position} with {idleTime}s spawn idle time (attack state mode)");
        return tankEnemy;
    }

    /// <summary>
    /// Spawn and initialize Evil Father boss
    /// </summary>
    public EvilFather SpawnEvilFather(Vector3 position, float speed = 2.5f, float health = 300f, float damage = 25f, float size = 1f, float spawnIdleTime = -1f)
    {
        GameObject bossObj = CreateEnemyObject(evilFatherPrefab, position, "Evil Father");
        bossObj.transform.localScale = Vector3.one * size;

        EvilFather boss = bossObj.GetComponent<EvilFather>();
        if (boss == null)
        {
            boss = bossObj.AddComponent<EvilFather>();
        }

        float idleTime = spawnIdleTime < 0 ? defaultSpawnIdleTime : spawnIdleTime;

        AttackType attack = meleeAttack ?? aoeAttack;
        boss.InitializeEnemy(speed, health, damage, size, attack, isGoodDream, idleTime);

        if (evilFatherSprite != null)
        {
            boss.SetSprite(evilFatherSprite);
        }

        if (createHealthBars)
        {
            CreateEnemyHealthBar(boss, true); // true = boss health bar
        }

        Debug.Log($"BOSS: Evil Father spawned at {position} with {idleTime}s spawn idle time");
        return boss;
    }

    /// <summary>
    /// Spawn and initialize The Mare boss
    /// </summary>
    public TheMare SpawnTheMare(Vector3 position, float speed = 3f, float health = 500f, float damage = 30f, float size = 1.2f, float spawnIdleTime = -1f)
    {
        GameObject bossObj = CreateEnemyObject(theMarePrefab, position, "The Mare");
        bossObj.transform.localScale = Vector3.one * size;

        TheMare boss = bossObj.GetComponent<TheMare>();
        if (boss == null)
        {
            boss = bossObj.AddComponent<TheMare>();
        }

        float idleTime = spawnIdleTime < 0 ? defaultSpawnIdleTime : spawnIdleTime;

        AttackType attack = meleeAttack ?? aoeAttack;
        boss.InitializeEnemy(speed, health, damage, size, attack, isGoodDream, idleTime);

        if (theMareSprite != null)
        {
            boss.SetSprite(theMareSprite);
        }

        if (createHealthBars)
        {
            CreateEnemyHealthBar(boss, true); // true = boss health bar
        }

        Debug.Log($"BOSS: The Mare spawned at {position} with {idleTime}s spawn idle time");
        return boss;
    }

    /// <summary>
    /// Spawn and initialize The Devil boss (final boss)
    /// </summary>
    public TheDevil SpawnTheDevil(Vector3 position, float speed = 2f, float health = 650f, float damage = 15f, float size = 1.5f, float spawnIdleTime = -1f)
    {
        GameObject bossObj = CreateEnemyObject(theDevilPrefab, position, "The Devil");
        bossObj.transform.localScale = Vector3.one * size;

        TheDevil boss = bossObj.GetComponent<TheDevil>();
        if (boss == null)
        {
            boss = bossObj.AddComponent<TheDevil>();
        }

        float idleTime = spawnIdleTime < 0 ? defaultSpawnIdleTime : spawnIdleTime;

        AttackType attack = aoeAttack ?? meleeAttack;
        boss.InitializeEnemy(speed, health, damage, size, attack, isGoodDream, idleTime);

        if (theDevilSprite != null)
        {
            boss.SetSprite(theDevilSprite);
        }

        // Set hellfire sprite if available
        if (hellFireSprite != null)
        {
            boss.SetHellFireSprite(hellFireSprite);
        }

        // Set ground slam wave sprite if available
        if (groundSlamWaveSprite != null)
        {
            boss.SetGroundSlamWaveSprite(groundSlamWaveSprite);
        }

        // Set meteor sprites if available
        if (meteorWarningSprite != null)
        {
            boss.SetMeteorWarningSprite(meteorWarningSprite);
        }
        if (meteorImpactSprite != null)
        {
            boss.SetMeteorImpactSprite(meteorImpactSprite);
        }

        if (createHealthBars)
        {
            CreateEnemyHealthBar(boss, true); // true = boss health bar
        }

        Debug.Log($"BOSS: The Devil spawned at {position} with {idleTime}s spawn idle time");
        return boss;
    }

    /// <summary>
    /// Helper method to create enemy game object
    /// </summary>
    private GameObject CreateEnemyObject(GameObject prefab, Vector3 position, string name)
    {
        if (prefab != null)
        {
            return Instantiate(prefab, position, Quaternion.identity);
        }
        else
        {
            GameObject obj = new GameObject(name);
            obj.transform.position = position;
            return obj;
        }
    }

    /// <summary>
    /// Change the dream state for all characters
    /// </summary>
    public void SetDreamState(bool goodDream)
    {
        isGoodDream = goodDream;

        Character[] allCharacters = FindObjectsByType<Character>(FindObjectsSortMode.None);
        foreach (Character character in allCharacters)
        {
            character.IsGoodDream = goodDream;
        }

        Debug.Log($"Dream state changed to: {(goodDream ? "Good Dream" : "Bad Dream")}");
    }

    /// <summary>
    /// Toggle dream state
    /// </summary>
    public void ToggleDreamState()
    {
        SetDreamState(!isGoodDream);
    }

    /// <summary>
    /// Set the default spawn idle time for all future enemy spawns
    /// </summary>
    public void SetDefaultSpawnIdleTime(float time)
    {
        defaultSpawnIdleTime = time;
    }

    #region Melee Attack Setters

    public void SetMeleeAttackColor(Color color)
    {
        meleeAttackColor = color;
    }

    public void SetMeleeAttackAnimationFrames(Sprite[] frames)
    {
        meleeAttackAnimationFrames = frames;
    }

    public void SetMeleeAnimationFrameRate(float frameRate)
    {
        meleeAnimationFrameRate = frameRate;
    }

    public void SetMeleeAttackAspectRatio(Vector2 aspectRatio)
    {
        meleeAttackAspectRatio = aspectRatio;
    }

    #endregion

    #region Bomb Attack Setters

    public void SetBombAnimationFrames(Sprite[] frames)
    {
        bombAnimationFrames = frames;
    }

    public void SetBombAnimationFrameRate(float frameRate)
    {
        bombAnimationFrameRate = frameRate;
    }

    public void SetExplosionAnimationFrames(Sprite[] frames)
    {
        explosionAnimationFrames = frames;
    }

    public void SetExplosionAnimationFrameRate(float frameRate)
    {
        explosionAnimationFrameRate = frameRate;
    }

    #endregion

    #region Ground Slam Attack Setters

    public void SetGroundSlamAnimationFrames(Sprite[] frames)
    {
        groundSlamAnimationFrames = frames;
    }

    public void SetGroundSlamAnimationFrameRate(float frameRate)
    {
        groundSlamAnimationFrameRate = frameRate;
    }

    #endregion

    #region Shoot Attack Setters

    public void SetProjectileSprite(Sprite sprite)
    {
        projectileSprite = sprite;
    }

    public void SetShootTowardsMouse(bool towardsMouse)
    {
        shootTowardsMouse = towardsMouse;
    }

    /// <summary>
    /// Create a floating health bar for an enemy
    /// </summary>
    private FloatingHealthBar CreateEnemyHealthBar(Enemy enemy, bool isBoss = false)
    {
        if (!createHealthBars) return null;

        GameObject healthBarObj = new GameObject($"{enemy.name}_HealthBar");
        FloatingHealthBar healthBar = healthBarObj.AddComponent<FloatingHealthBar>();

        // Use boss or regular enemy settings
        Vector2 barSize = isBoss ? bossHealthBarSize : enemyHealthBarSize;
        Vector3 barOffset = isBoss ? bossHealthBarOffset : enemyHealthBarOffset;

        healthBar.Initialize(enemy, barSize, barOffset);
        healthBar.SetColors(enemyHealthBarBackground, enemyHealthBarFill, enemyHealthBarLowHealth);
        healthBar.SetDisplayDuration(enemyHealthBarDisplayDuration);

        enemy.SetHealthBar(healthBar);

        return healthBar;
    }

    /// <summary>
    /// Create static health bar for player
    /// </summary>
    private PlayerHealthBar CreatePlayerHealthBar(Player player)
    {
        if (!createHealthBars) return null;

        // Destroy old health bar if it exists
        if (activePlayerHealthBar != null)
        {
            Destroy(activePlayerHealthBar.gameObject);
        }

        GameObject healthBarObj = new GameObject("PlayerHealthBar");
        DontDestroyOnLoad(healthBarObj); // Keep health bar between scenes

        PlayerHealthBar healthBar = healthBarObj.AddComponent<PlayerHealthBar>();
        healthBar.Initialize(player);
        healthBar.SetPosition(playerHealthBarPosition);
        healthBar.SetSize(playerHealthBarSize);
        healthBar.SetColors(playerHealthBarBackground, playerHealthBarFill, playerHealthBarLowHealth);
        healthBar.SetShowHealthText(showPlayerHealthText);

        player.SetPlayerHealthBar(healthBar);
        activePlayerHealthBar = healthBar;

        return healthBar;
    }

    #endregion

    #region Health Bar Configuration Methods

    /// <summary>
    /// Enable or disable health bar creation
    /// </summary>
    public void SetCreateHealthBars(bool create)
    {
        createHealthBars = create;
    }

    /// <summary>
    /// Set enemy health bar size
    /// </summary>
    public void SetEnemyHealthBarSize(Vector2 size)
    {
        enemyHealthBarSize = size;
    }

    /// <summary>
    /// Set boss health bar size
    /// </summary>
    public void SetBossHealthBarSize(Vector2 size)
    {
        bossHealthBarSize = size;
    }

    /// <summary>
    /// Set enemy health bar offset
    /// </summary>
    public void SetEnemyHealthBarOffset(Vector3 offset)
    {
        enemyHealthBarOffset = offset;
    }

    /// <summary>
    /// Set boss health bar offset
    /// </summary>
    public void SetBossHealthBarOffset(Vector3 offset)
    {
        bossHealthBarOffset = offset;
    }

    /// <summary>
    /// Set enemy health bar colors
    /// </summary>
    public void SetEnemyHealthBarColors(Color background, Color fill, Color lowHealth)
    {
        enemyHealthBarBackground = background;
        enemyHealthBarFill = fill;
        enemyHealthBarLowHealth = lowHealth;
    }

    /// <summary>
    /// Set enemy health bar display duration
    /// </summary>
    public void SetEnemyHealthBarDisplayDuration(float duration)
    {
        enemyHealthBarDisplayDuration = duration;
    }

    /// <summary>
    /// Set player health bar size
    /// </summary>
    public void SetPlayerHealthBarSize(Vector2 size)
    {
        playerHealthBarSize = size;
        if (activePlayerHealthBar != null)
        {
            activePlayerHealthBar.SetSize(size);
        }
    }

    /// <summary>
    /// Set player health bar position
    /// </summary>
    public void SetPlayerHealthBarPosition(Vector2 position)
    {
        playerHealthBarPosition = position;
        if (activePlayerHealthBar != null)
        {
            activePlayerHealthBar.SetPosition(position);
        }
    }

    /// <summary>
    /// Set player health bar colors
    /// </summary>
    public void SetPlayerHealthBarColors(Color background, Color fill, Color lowHealth)
    {
        playerHealthBarBackground = background;
        playerHealthBarFill = fill;
        playerHealthBarLowHealth = lowHealth;

        if (activePlayerHealthBar != null)
        {
            activePlayerHealthBar.SetColors(background, fill, lowHealth);
        }
    }

    /// <summary>
    /// Toggle player health text display
    /// </summary>
    public void SetShowPlayerHealthText(bool show)
    {
        showPlayerHealthText = show;
        if (activePlayerHealthBar != null)
        {
            activePlayerHealthBar.SetShowHealthText(show);
        }
    }

    /// <summary>
    /// Get the active player health bar
    /// </summary>
    public PlayerHealthBar GetActivePlayerHealthBar()
    {
        return activePlayerHealthBar;
    }

    #endregion
}