using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for power ups using an Strategy pattern
/// </summary>
public interface IPowerUp
{ 
    void ApplyPowerUp(Player player);
}
