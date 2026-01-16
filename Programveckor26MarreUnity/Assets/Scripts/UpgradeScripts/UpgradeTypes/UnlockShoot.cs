using UnityEngine;

[CreateAssetMenu(fileName = "Unlock Shoot", menuName = "Upgrades/ Unlocks/ Unlock Shoot")]
public class UnlockShoot : Upgrade
{
    public override void ApplyUpgrade(Player player)
    {
        base.ApplyUpgrade(player);
        player.UnlockShootAttack();
    }
}
