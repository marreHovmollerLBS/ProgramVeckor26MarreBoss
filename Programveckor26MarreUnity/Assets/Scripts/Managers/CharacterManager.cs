using UnityEngine;

/// <summary>
/// Manager class to spawn and configure characters from external scripts
/// </summary>
public class CharacterManager : MonoBehaviour
{
    [Header("Prefabs (Optional - will create if not assigned)")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject defaultEnemyPrefab;
    
    [Header("Attack Types")]
    [SerializeField] private AttackType meleeAttack;
    [SerializeField] private AttackType rangedAttack;
    [SerializeField] private AttackType aoeAttack;
    
    [Header("Sprites (Optional)")]
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private Sprite defaultEnemySprite;
    
    [Header("Dream State")]
    [SerializeField] private bool isGoodDream = true;
    
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
        
        Debug.Log($"Player spawned at {position}");
        return player;
    }

    /// <summary>
    /// Spawn and initialize a Default enemy
    /// </summary>
    public DefaultEnemy SpawnDefaultEnemy(Vector3 position, float speed = 2f, float health = 50f, float damage = 8f, float size = 0.5f, AttackType attackType = null)
    {
        GameObject enemyObj = CreateEnemyObject(defaultEnemyPrefab, position, "Default Enemy");

        DefaultEnemy defaultEnemy = enemyObj.GetComponent<DefaultEnemy>();
        if (defaultEnemy == null)
        {
            defaultEnemy = enemyObj.AddComponent<DefaultEnemy>();
        }

        AttackType attack = attackType ?? meleeAttack;
        defaultEnemy.InitializeEnemy(speed, health, damage, size, attack, isGoodDream);

        if (defaultEnemySprite != null)
        {
            defaultEnemy.SetSprite(defaultEnemySprite);
        }

        Debug.Log($"Default Enemy spawned at {position}");
        return defaultEnemy;
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
        Character[] allCharacters = FindObjectsOfType<Character>();
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
}
