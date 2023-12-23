using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoreExplosionPowerUp : PowerUp
{
    public override void ApplyPowerUp(Player player)
    {
        player.powerLevel++;
    }
}
