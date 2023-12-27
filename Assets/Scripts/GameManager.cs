using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Singleton class that manages the game.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int numberOfPlayers = 1; // Set the number of players
    public GameObject playerPrefab; // Reference to the player prefab
    [SerializeField] private Vector3[] playerSpawnList;

    // Start is called before the first frame update
    void Start()
    {
        SpawnPlayers();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SpawnPlayers()
    {
        foreach (Vector3 playerSpawn in playerSpawnList)
        {
            // Instantiate a new player at the origin
            GameObject playerObj = Instantiate(playerPrefab, playerSpawn, Quaternion.identity);
            Player player = playerObj.GetComponent<Player>();
            player.wallsTilemap = GameObject.Find("WallsTilemap").GetComponent<Tilemap>();
        }
    }
}
