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
            Instantiate(bombPowerUpPrefab, position, Quaternion.identity);
        }
    }

    public void AddToQueue(Vector3 position)
    {
        spawnQueue.Enqueue(position);
    }
}
