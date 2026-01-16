using UnityEngine;

[CreateAssetMenu(fileName = "Knockback Upgrade", menuName = "Upgrades/Attack Upgrades/Melee Knockback Upgrade")]
public class MeleeKnockbackUpgrade: Upgrade
{
    public int Amount;
    public override void ApplyUpgrade(Player player)
    {
        base.ApplyUpgrade(player);
        player.SetMeleeAttackKnockback(Amount);
    }
}
