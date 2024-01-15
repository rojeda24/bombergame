using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton class that spawns power ups.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    public static PowerUpSpawner Instance { get; private set; }
    private Queue<Vector3> _spawnQueue = new Queue<Vector3>();
    [SerializeField] private GameObject _extraBombPowerUpPrefab = null;
    [SerializeField] private GameObject _moreExplosionPowerUpPrefab = null;
    [SerializeField] private GameObject _fasterPowerUpPrefab = null;

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
        while (_spawnQueue.Count > 0)
        {
            var position = _spawnQueue.Dequeue();
            int random = Random.Range(0, 100);
            // 15% chance of spawning a more explosion power up
            if (random < 15)
            {
                Instantiate(_moreExplosionPowerUpPrefab, position, Quaternion.identity);
            }
            // 20% chance of spawning a +1 bomb power up
            else if (random < 35)
            {
                Instantiate(_extraBombPowerUpPrefab, position, Quaternion.identity);
            }
            // 15% chance of spawning a +1 speed power up
            else if (random < 50)
            {
                Instantiate(_fasterPowerUpPrefab, position, Quaternion.identity);
            }
            // 50% chance of spawning nothing
        }
    }

    public void AddToQueue(Vector3 position)
    {
        _spawnQueue.Enqueue(position);
    }
}
