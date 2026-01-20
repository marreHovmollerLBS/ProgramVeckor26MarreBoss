using UnityEngine;

[CreateAssetMenu(fileName = "Health Upgrade", menuName = "Upgrades/Player Stats/Health Upgrade")]
public class HealthUpgrade : Upgrade
{
    public int Amount;
    public override void ApplyUpgrade(Player player)
    {
        base.ApplyUpgrade(player);
        player.MaxHealth += Amount;
    }
}
