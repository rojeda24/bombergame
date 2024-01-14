using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Singleton class that manages the game.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject _playerPrefab; // Reference to the player prefab
    [SerializeField] private Vector3[] _locationList;

    // Start is called before the first frame update
    void Start()
    {
        SpawnPlayers();
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
        for (int n = 0; n<_locationList.Length; n++)
        {
            // Instantiate a new player at the origin
            GameObject playerObj = Instantiate(_playerPrefab, _locationList[n], Quaternion.identity);
            Player player = playerObj.GetComponent<Player>();
            player.Id = n + 1;
            player.WallsTilemap = GameObject.Find("WallsTilemap").GetComponent<Tilemap>();
            //get player sprite renderer
            SpriteRenderer playerSpriteRenderer = playerObj.GetComponent<SpriteRenderer>();
            //set player color
            playerSpriteRenderer.color = n == 0 ? Color.red : Color.cyan;

            player.InputReader = ScriptableObject.CreateInstance<InputReader>();
            if (n == 1)
            {
                player.InputReader.SetPlayer2();
            }

            player.DeadEvent += OnDieEvent;
        }
    }

    private void OnDieEvent(Player player)
    {
        Debug.Log($"Player {player.Id} died");
    }
}
