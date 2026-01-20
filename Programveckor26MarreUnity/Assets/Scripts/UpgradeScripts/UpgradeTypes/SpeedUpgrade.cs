using UnityEngine;

[CreateAssetMenu(fileName = "Speed Upgrade", menuName = "Upgrades/Player Stats/Speed Upgrade")]
public class SpeedUpgrade : Upgrade
{
    public int Amount;
    public override void ApplyUpgrade(Player player)
    {
        base.ApplyUpgrade(player);
        player.MovementSpeed += Amount;
    }
}
