using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraBombPowerUp : MonoBehaviour, IPowerUp
{
    public void ApplyPowerUp(Player player)
    {
        player.maxBombs++;
    }
}
