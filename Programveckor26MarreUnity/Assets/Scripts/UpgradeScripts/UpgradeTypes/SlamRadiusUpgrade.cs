using UnityEngine;

[CreateAssetMenu(fileName = "Slam Radius Upgrade", menuName = "Upgrades/Attack Upgrades/Slam Radius Upgrade")]
public class SlamRadiusUpgrade : Upgrade
{
    public int Amount;
    public override void ApplyUpgrade(Player player)
    {
        base.ApplyUpgrade(player);
        player.SetSlamRadius(Amount);
    }
}