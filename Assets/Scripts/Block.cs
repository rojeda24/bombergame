using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public void Explode()
    {
        PowerUpSpawner.Instance.AddToQueue(transform.position);
        Destroy(gameObject);
    }
}
