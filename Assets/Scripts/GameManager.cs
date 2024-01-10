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

    public GameObject playerPrefab; // Reference to the player prefab
    [SerializeField] private Vector3[] locationList;

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
        for (int n = 0; n<locationList.Length; n++)
        {
            // Instantiate a new player at the origin
            GameObject playerObj = Instantiate(playerPrefab, locationList[n], Quaternion.identity);
            Player player = playerObj.GetComponent<Player>();
            player.wallsTilemap = GameObject.Find("WallsTilemap").GetComponent<Tilemap>();
            //get player sprite renderer
            SpriteRenderer playerSpriteRenderer = playerObj.GetComponent<SpriteRenderer>();
            //set player color
            playerSpriteRenderer.color = n == 0 ? Color.red : Color.cyan;

            player.input = ScriptableObject.CreateInstance<InputReader>();
            if (n == 1)
            {
                player.input.SetPlayer2();
            }
        }
    }
}
