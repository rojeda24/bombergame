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
    [SerializeField] private GameObject fasterPowerUpPrefab = null;

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
            int random = Random.Range(0, 10);
            // 30% chance of spawning a more explosion power up
            if (random < 3)
            {
                Instantiate(moreExplosionPowerUpPrefab, position, Quaternion.identity);
            }
            // 40% chance of spawning a +1 bomb power up
            else if (random < 7)
            {
                Instantiate(bombPowerUpPrefab, position, Quaternion.identity);
            }
            // 30% chance of spawning a +1 speed power up
            else
            {
                Instantiate(fasterPowerUpPrefab, position, Quaternion.identity);
            }
        }
    }

    public void AddToQueue(Vector3 position)
    {
        spawnQueue.Enqueue(position);
    }
}
