using UnityEngine;

public class UnlockBomb : Upgrade
{
    public override void ApplyUpgrade(Player player)
    {
        base.ApplyUpgrade(player);
        player.UnlockBombAttack();
    }
}
