using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

/// <summary>
/// Singleton class that manages the game.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject _playerPrefab; // Reference to the player prefab
    [SerializeField] private Vector3[] _locationList;
    [SerializeField] private UIDocument _uiGameOver;


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
            SpawnPlayers();//TODO: We dont want to spawn players on UI scenes
            VisualElement root = _uiGameOver.rootVisualElement;
            root.Q<Button>("RestartButton").RegisterCallback<ClickEvent>(ev => OnRestartGame());
        }
    }

    private void OnEnable()
    {
        SpawnPlayers();
        VisualElement root = _uiGameOver.rootVisualElement;
        root.Q<Button>("RestartButton").RegisterCallback<ClickEvent>(ev => OnRestartGame());
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
        //Choose winner
        int winnerId = player.Id == 1 ? 2 : 1;

        //Show game over screen
        VisualElement root = _uiGameOver.rootVisualElement;
        root.Q<VisualElement>("MainContainer").style.display = DisplayStyle.Flex;
        root.Q<Label>("Label").text = $"Player {winnerId} wins!";
    }

    private void OnRestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene(); // Get the current scene
        SceneManager.LoadScene(currentScene.buildIndex); // Load it again
    }
}
