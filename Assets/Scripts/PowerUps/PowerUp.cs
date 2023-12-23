using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp
    
    : MonoBehaviour, IPowerUp
{
    public virtual void ApplyPowerUp(Player player)
    {
        throw new System.NotImplementedException();
    }
}
