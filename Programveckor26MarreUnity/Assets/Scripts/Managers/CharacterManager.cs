using UnityEngine;

/// <summary>
/// Manager class to spawn and configure characters from external scripts
/// Updated to support player attack visuals
/// </summary>
public class CharacterManager : MonoBehaviour
{
    [Header("Prefabs (Optional - will create if not assigned)")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject defaultEnemyPrefab;
    [SerializeField] private GameObject rangedEnemyPrefab;
    [SerializeField] private GameObject tankEnemyPrefab;

    [Header("Attack Types")]
    [SerializeField] private AttackType meleeAttack;
    [SerializeField] private AttackType rangedAttack;
    [SerializeField] private AttackType aoeAttack;

    [Header("Sprites (Optional)")]
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private Sprite defaultEnemySprite;
    [SerializeField] private Sprite rangedEnemySprite;
    [SerializeField] private Sprite tankEnemySprite;

    [Header("Player Attack Visuals (Optional)")]
    [SerializeField] private Sprite meleeAttackSprite; // Sprite for melee attack hitbox visual
    [SerializeField] private Color meleeAttackColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private bool showMeleeVisual = true;

    [Header("Dream State")]
    [SerializeField] private bool isGoodDream = true;

    [Header("Enemy Spawn Settings")]
    [SerializeField] private float defaultSpawnIdleTime = 1f; // Default idle time for all enemies

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

        // Configure melee attack visuals
        ConfigurePlayerAttackVisuals(player);

        Debug.Log($"Player spawned at {position}");
        return player;
    }

    /// <summary>
    /// Configure player attack visual settings
    /// </summary>
    private void ConfigurePlayerAttackVisuals(Player player)
    {
        // Get the melee attack component
        PlayerMeleeAttack meleeAttackComponent = player.GetMeleeAttack();

        if (meleeAttackComponent != null)
        {
            // Set the melee attack sprite if provided
            if (meleeAttackSprite != null)
            {
                meleeAttackComponent.SetHitboxSprite(meleeAttackSprite);
            }

            // Set the visual color
            meleeAttackComponent.SetHitboxColor(meleeAttackColor);

            // Set whether to show visual
            meleeAttackComponent.SetShowVisual(showMeleeVisual);

            Debug.Log($"Player melee attack visuals configured: Sprite={meleeAttackSprite != null}, Color={meleeAttackColor}, Show={showMeleeVisual}");
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

        // Use default spawn idle time if not specified
        float idleTime = spawnIdleTime < 0 ? defaultSpawnIdleTime : spawnIdleTime;

        // Default enemies don't need an attack type, but we'll pass it for consistency
        AttackType attack = attackType ?? meleeAttack;
        defaultEnemy.InitializeEnemy(speed, health, damage, size, attack, isGoodDream, idleTime);

        if (defaultEnemySprite != null)
        {
            defaultEnemy.SetSprite(defaultEnemySprite);
        }

        Debug.Log($"Default Enemy spawned at {position} with {idleTime}s spawn idle time (collision damage mode)");
        return defaultEnemy;
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

        // Use default spawn idle time if not specified
        float idleTime = spawnIdleTime < 0 ? defaultSpawnIdleTime : spawnIdleTime;

        // Ranged enemies need ranged attack type
        rangedEnemy.InitializeEnemy(speed, health, damage, size, rangedAttack, isGoodDream, idleTime);

        if (rangedEnemySprite != null)
        {
            rangedEnemy.SetSprite(rangedEnemySprite);
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

        // Use default spawn idle time if not specified
        float idleTime = spawnIdleTime < 0 ? defaultSpawnIdleTime : spawnIdleTime;

        // Tank enemies can use melee or AOE attack
        AttackType attack = aoeAttack ?? meleeAttack;
        tankEnemy.InitializeEnemy(speed, health, damage, size, attack, isGoodDream, idleTime);

        if (tankEnemySprite != null)
        {
            tankEnemy.SetSprite(tankEnemySprite);
        }

        Debug.Log($"Tank Enemy spawned at {position} with {idleTime}s spawn idle time (attack state mode)");
        return tankEnemy;
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

        // Update all existing characters
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

    /// <summary>
    /// Set the melee attack sprite at runtime
    /// </summary>
    public void SetMeleeAttackSprite(Sprite sprite)
    {
        meleeAttackSprite = sprite;
    }

    /// <summary>
    /// Set the melee attack color at runtime
    /// </summary>
    public void SetMeleeAttackColor(Color color)
    {
        meleeAttackColor = color;
    }
}