using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Player character class with integrated attack and movement systems
/// </summary>
public class Player : Character
{
    [Header("Player Settings")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";

    [Header("Attack Input")]
    [SerializeField] private KeyCode meleeAttackKey = KeyCode.V;
    [SerializeField] private KeyCode groundSlamKey = KeyCode.C;
    [SerializeField] private KeyCode bombKey = KeyCode.B;

    [Header("Attack Components")]
    private PlayerMeleeAttack meleeAttack;
    private PlayerShootAttack shootAttack;
    private PlayerGroundSlamAttack groundSlamAttack;
    private PlayerBombAttack bombAttack;

    [Header("Player Health Bar")]
    private PlayerHealthBar playerHealthBar;

    public PlayerHealthBar PlayerHealthBarComponent => playerHealthBar;

    protected override void Awake()
    {
        base.Awake();

        // Player specific initialization
        gameObject.tag = "Player";
        gameObject.layer = LayerMask.NameToLayer("Player");

        // Initialize attack components
        InitializeAttackComponents();
    }

    /// <summary>
    /// Initialize all attack components
    /// </summary>
    private void InitializeAttackComponents()
    {
        // Add melee attack (always available)
        meleeAttack = GetComponent<PlayerMeleeAttack>();
        if (meleeAttack == null)
        {
            meleeAttack = gameObject.AddComponent<PlayerMeleeAttack>();
        }

        // Add shoot attack (unlockable)
        shootAttack = GetComponent<PlayerShootAttack>();
        if (shootAttack == null)
        {
            shootAttack = gameObject.AddComponent<PlayerShootAttack>();
        }

        // Add ground slam attack (unlockable)
        groundSlamAttack = GetComponent<PlayerGroundSlamAttack>();
        if (groundSlamAttack == null)
        {
            groundSlamAttack = gameObject.AddComponent<PlayerGroundSlamAttack>();
        }

        // Add bomb attack (unlockable)
        bombAttack = GetComponent<PlayerBombAttack>();
        if (bombAttack == null)
        {
            bombAttack = gameObject.AddComponent<PlayerBombAttack>();
        }
    }

    protected override void HandleBehavior()
    {
        HandleInput();
        HandleRotation();
        HandleAttacks();
    }

    /// <summary>
    /// Handle player input for movement
    /// </summary>
    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw(horizontalAxis);
        float vertical = Input.GetAxisRaw(verticalAxis);

        moveDirection = new Vector3(horizontal, vertical, 0f).normalized;
    }

    /// <summary>
    /// Handle player rotation based on movement direction
    /// </summary>
    private void HandleRotation()
    {
        if (moveDirection.magnitude > 0.01f)
        {
            float angle = Mathf.Atan2(moveDirection.x, -moveDirection.y) * Mathf.Rad2Deg;

            // Apply rotation to the player
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    /// <summary>
    /// Handle all attack inputs
    /// </summary>
    private void HandleAttacks()
    {
        if (Input.GetKeyDown(meleeAttackKey))
        {
            // Perform melee attack
            if (meleeAttack != null)
            {
                meleeAttack.TryAttack();
            }

            // Also perform shoot attack if unlocked
            if (shootAttack != null && shootAttack.IsUnlocked())
            {
                shootAttack.TryAttack();
            }
        }

        // Ground slam
        if (Input.GetKeyDown(groundSlamKey))
        {
            if (groundSlamAttack != null)
            {
                groundSlamAttack.TryAttack();
            }
        }

        // Bomb
        if (Input.GetKeyDown(bombKey))
        {
            if (bombAttack != null)
            {
                bombAttack.TryAttack();
            }
        }
    }

    protected override void Die()
    {
        base.Die();
        Debug.Log("Player has died! Game Over!");

        if (playerHealthBar != null)
        {
            playerHealthBar.gameObject.SetActive(false);
        }

        // Implement game over logic
    }

    protected override void OnDreamStateChanged()
    {
        base.OnDreamStateChanged();
        Debug.Log($"Player dream state changed to: {(isGoodDream ? "Good Dream" : "Bad Dream")}");
    }

    /// <summary>
    /// Public method to initialize player from external script
    /// </summary>
    public void InitializePlayer(float speed, float health, float dmg, AttackType attack, bool goodDream = true)
    {
        movementSpeed = speed;
        maxHealth = health;
        currentHealth = health;
        damage = dmg;
        attackType = attack;
        isGoodDream = goodDream;
    }

    /// <summary>
    /// Set the player sprite
    /// </summary>
    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    /// <summary>
    /// Changes the player's stats and abilities depending on unlocked upgrades
    /// </summary>
    public void ApplyUpgrades()
    {
        foreach (Upgrade upg in PersistentPlayerManager.Instance.acquiredUpgrades)
        {
            upg.ApplyUpgrade(this);
        }
    }

    public void SetPlayerHealthBar(PlayerHealthBar healthBar)
    {
        playerHealthBar = healthBar;
        if (playerHealthBar != null)
        {
            playerHealthBar.Initialize(this);
        }
    }

    #region Attack Unlock Methods

    /// <summary>
    /// Unlock the shoot attack
    /// </summary>
    public void UnlockShootAttack()
    {
        if (shootAttack != null)
        {
            shootAttack.Unlock();
        }
    }

    /// <summary>
    /// Unlock the ground slam attack
    /// </summary>
    public void UnlockGroundSlamAttack()
    {
        if (groundSlamAttack != null)
        {
            groundSlamAttack.Unlock();
        }
    }

    /// <summary>
    /// Unlock the bomb attack
    /// </summary>
    public void UnlockBombAttack()
    {
        if (bombAttack != null)
        {
            bombAttack.Unlock();
        }
    }

    /// <summary>
    /// Unlock all attacks (for testing)
    /// </summary>
    public void UnlockAllAttacks()
    {
        UnlockShootAttack();
        UnlockGroundSlamAttack();
        UnlockBombAttack();
    }

    #endregion

    #region Attack Component Getters

    public PlayerMeleeAttack GetMeleeAttack() => meleeAttack;
    public PlayerShootAttack GetShootAttack() => shootAttack;
    public PlayerGroundSlamAttack GetGroundSlamAttack() => groundSlamAttack;
    public PlayerBombAttack GetBombAttack() => bombAttack;

    #endregion
}