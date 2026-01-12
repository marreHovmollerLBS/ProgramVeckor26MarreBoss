using UnityEngine;

/// <summary>
/// Fast, weak enemy that uses melee attacks
/// </summary>
public class DefaultEnemy : Enemy
{
    //[Header("Default Enemy Settings")]

    protected override void Awake()
    {
        base.Awake();

        // Scout defaults
        if (attackType == null)
        {
            // Will need to assign a melee attack type
            gameObject.name = "Default Enemy";
        }
    }

    public override void InitializeEnemy(float speed, float health, float dmg, float sze, AttackType attack, bool goodDream = false, float spawnIdleTime = 1f)
    {
        base.InitializeEnemy(speed, health, dmg, sze, attack, goodDream, spawnIdleTime);
        gameObject.name = "Default Enemy";
    }
}