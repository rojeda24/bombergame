using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    private void OnDestroy()
    {
        PowerUpSpawner.Instance.AddToQueue(transform.position);
    }
}
