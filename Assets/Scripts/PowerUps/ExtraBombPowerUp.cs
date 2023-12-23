using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraBombPowerUp : PowerUp
{
    public override void ApplyPowerUp(Player player)
    {
        player.maxBombs++;
    }
}
