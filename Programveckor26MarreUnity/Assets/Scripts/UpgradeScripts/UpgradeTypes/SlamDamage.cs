using UnityEngine;

[CreateAssetMenu(fileName = "Slam Damage Upgrade", menuName = "Upgrades/Attack Upgrades/Slam Damage Upgrade")]
public class SlamDamageUpgrade : Upgrade
{
    public int Amount;
    public override void ApplyUpgrade(Player player)
    {
        base.ApplyUpgrade(player);
        player.SetSlamDamage(Amount);
    }
}
