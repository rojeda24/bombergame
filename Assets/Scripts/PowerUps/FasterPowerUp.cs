using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FasterPowerUp : PowerUp
{
    public override void ApplyPowerUp(Player player)
    {
        player.moveSpeed++;
    }
}
