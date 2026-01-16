using UnityEngine;

[CreateAssetMenu(fileName = "Attack Damage Upgrade", menuName = "Upgrades/Attack Upgrades/Attack Damage Upgrade")]
public class MeleeDamageUpgrade : Upgrade
{
    public int Amount;
    public override void ApplyUpgrade(Player player)
    {
        base.ApplyUpgrade(player);
        player.SetMeleeAttackDamage(Amount);
    }
}
