using UnityEngine;

[CreateAssetMenu(fileName = "Unlock Slam", menuName = "Upgrades/ Unlocks/ Unlock Ground Slam")]
public class UnlockGroundSlam : Upgrade
{
    public override void ApplyUpgrade(Player player)
    {
        base.ApplyUpgrade(player);
        player.UnlockGroundSlamAttack();
    }
}
