using UnityEngine;

[CreateAssetMenu(fileName = "Bomb Radius Upgrade", menuName = "Upgrades/Attack Upgrades/Bomb Radius Upgrade")]
public class BombRadiusUpgrade : Upgrade
{
    public int Amount;
    public override void ApplyUpgrade(Player player)
    {
        base.ApplyUpgrade(player);
        player.SetBombAttackRadius(Amount);
    }
}
