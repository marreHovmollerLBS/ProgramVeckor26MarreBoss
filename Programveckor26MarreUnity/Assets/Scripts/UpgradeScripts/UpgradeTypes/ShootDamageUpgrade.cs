using UnityEngine;

[CreateAssetMenu(fileName = "Shoot Damage Upgrade", menuName = "Upgrades/Attack Upgrades/Shoot Damage Upgrade")]
public class ShootDamageUpgrade : Upgrade
{
    public int Amount;
    public override void ApplyUpgrade(Player player)
    {
        base.ApplyUpgrade(player);
        player.SetShootDamage(Amount);
    }
}