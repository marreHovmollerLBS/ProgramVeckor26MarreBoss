using UnityEngine;

[CreateAssetMenu(fileName = "Unlock Bomb", menuName = "Upgrades/ Unlocks/ Unlock Bomb")]
public class UnlockBomb : Upgrade
{
    public override void ApplyUpgrade(Player player)
    {
        base.ApplyUpgrade(player);
        player.UnlockBombAttack();
    }
}
