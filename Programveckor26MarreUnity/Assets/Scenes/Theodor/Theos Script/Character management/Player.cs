using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Player character class
/// </summary>
public class Player : Character
{
    [Header("Player Settings")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Player specific initialization
        gameObject.tag = "Player";
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    protected override void HandleBehavior()
    {
        HandleInput();
        HandleAttack();
    }
    
    /// <summary>
    /// Handle player input for movement
    /// </summary>
    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw(horizontalAxis);
        float vertical = Input.GetAxisRaw(verticalAxis);

        moveDirection = new Vector3(horizontal,vertical,0f).normalized;
    }

    /// <summary>
    /// Handle attack input
    /// </summary>
    private void HandleAttack()
    {
        if (Input.GetKeyDown(attackKey))
        {
            //Attack();
        }
    }
    
    protected override void Die()
    {
        base.Die();
        Debug.Log("Player has died! Game Over!");
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
}
