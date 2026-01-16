using UnityEngine;

[CreateAssetMenu(fileName = "Attack Damage Upgrade", menuName = "Upgrades/Attack Upgrades/Bomb Damage Upgrade")]
public class BombDamageUpgrade : Upgrade
{
    public int Amount;
    public override void ApplyUpgrade(Player player)
    {
        base.ApplyUpgrade(player);
        player.SetBombDamage(Amount);
    }
}
