using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton class that spawns power ups.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    public static PowerUpSpawner Instance { get; private set; }
    private Queue<Vector3> spawnQueue = new Queue<Vector3>();
    [SerializeField] private GameObject bombPowerUpPrefab = null;
    [SerializeField] private GameObject moreExplosionPowerUpPrefab = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Spawn all power ups in the queue
        while (spawnQueue.Count > 0)
        {
            var position = spawnQueue.Dequeue();
            // 50% chance of spawning a more explosion power up
            if (Random.Range(0, 10) < 5)
            {
                Instantiate(moreExplosionPowerUpPrefab, position, Quaternion.identity);
            }
            else
            {
                Instantiate(bombPowerUpPrefab, position, Quaternion.identity);
            }
        }
    }

    public void AddToQueue(Vector3 position)
    {
        spawnQueue.Enqueue(position);
    }
}
