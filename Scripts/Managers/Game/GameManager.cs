using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance; 
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("Game Manager is NULL");
            return _instance;
        }
    }

    // Game Progress
    public int numberCoinDestroyed = 0; // Number of coins collected
    public int currentSlot = 1; // Current save slot (1 or 2)
    private float _gameStartTime; // Start time of the game
    private bool createCoins = true; // Flag to control coin creation

    // Prefabs and Events
    [SerializeField] private GameObject coinPrefab; // Coin prefab to instantiate
    public event System.Action OnGameDataUpdated; // Event triggered when game data is updated

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // Don't destroy GameManager on scene load
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate GameManager
        }
    }

    // Start: Register for Scene Loaded event and Load Game
    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        LoadGame(); // Load game progress
    }

    // Game Time
    public float GetGameStartTime()
    {
        return _gameStartTime; // Return the game start time
    }

    // Scene Loaded: Setup based on the current scene
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Level1")
        {
            _gameStartTime = Time.time;
            if (PlayerPrefs.GetInt("StartGameMode", 0) == 1)
                ResetCoins(); // Reset coins if starting a new game
            else
                LoadGame(); // Load saved game progress

            StartCoroutine(CreateCoins()); // Start the coin creation process
            UpdateCoinUI(); // Update the coin UI display
        }
    }

    // Coin Creation: Coroutine to spawn coins periodically
    private IEnumerator CreateCoins()
    {
        while (createCoins)
        {
            float posX = UnityEngine.Random.Range(-5f, 5f);
            Vector2 positionCoin = new Vector2(posX, 5f);
            Instantiate(coinPrefab, positionCoin, Quaternion.identity);
            yield return new WaitForSeconds(5f);
        }
    }

    // Save Progress and Proceed to the next step (e.g. level or game over)
    public void SaveAndProceed(bool isVictory)
    {
        createCoins = false; // Stop coin creation
        int playtime = Mathf.FloorToInt(Time.time - _gameStartTime); // Calculate the playtime
        SendScoreToAPI(numberCoinDestroyed, playtime); // Send score to API
    }

    // Send Score to the API
    private void SendScoreToAPI(int score, int playtime)
    {
        int playerId = PlayerPrefs.GetInt("jugador_id", 0);
        if (playerId != 0)
        {
            APIManager.Instance.SendScoreToServer(score, playtime); // Send score to server
        }
    }

    // Save Local Progress
    public void SaveLocalProgress(int coins, int level)
    {
        string prefix = "Slot" + currentSlot;
        PlayerPrefs.SetInt(prefix + "_Coins", coins); // Save coin count
        int currentMaxLevel = PlayerPrefs.GetInt(prefix + "_CurrentLevel", 1);

        if (level > currentMaxLevel)
            PlayerPrefs.SetInt(prefix + "_CurrentLevel", level);
        
        PlayerPrefs.Save();
        OnGameDataUpdated?.Invoke();
    }

    // Save Game Progress (local and to API)
    public void SaveAllProgress(int playtime)
    {
        SaveLocalProgress(numberCoinDestroyed, GetCurrentLevel()); // Save local progress (coins and level)
        int playerId = PlayerPrefs.GetInt("jugador_id", 0);
        if (playerId != 0)
        {
            APIManager.Instance.SendScoreToServer(numberCoinDestroyed, playtime); // Send score to API for ranking
        }
    }

    // Load Game Data
    public void LoadGame()
    {
        string prefix = "Slot" + currentSlot;
        numberCoinDestroyed = PlayerPrefs.GetInt(prefix + "_Coins", 0); // Load coins
        UpdateCoinUI(); // Update UI display with coin count
        OnGameDataUpdated?.Invoke(); // Trigger event to notify data update
    }

    // Slot Selection and Switching
    public void SelectSlot(int slot)
    {
        currentSlot = slot; // Select the save slot
        LoadGame(); // Load the selected save slot's game data
        OnGameDataUpdated?.Invoke();
    }

    public void SwitchSlotAndSave(int newSlot)
    {
        SaveLocalProgress(numberCoinDestroyed, GetCurrentLevel()); // Save current slot's progress
        currentSlot = newSlot; // Switch to new slot
        LoadGame(); // Load new save slot's game data
    }

    public void SwitchSlot(int newSlot)
    {
        currentSlot = newSlot; // Switch to a different save slot
        LoadGame(); // Load the new save slot's game data
    }

    // Reset Coins Count
    public void ResetCoins()
    {
        numberCoinDestroyed = 0;
        UpdateCoinUI();
    }

    // Update Coin UI
    private void UpdateCoinUI()
    {
        GameObject textCoin = GameObject.Find("TextCoinUI");
        if (textCoin != null)
            textCoin.GetComponent<TextMeshProUGUI>().SetText(numberCoinDestroyed.ToString());
    }

    // Unlock Next Level
    public void UnlockNextLevel(int level)
    {
        string prefix = "Slot" + currentSlot;
        PlayerPrefs.SetInt(prefix + "_Level" + level + "Unlocked", 1); // Unlock next level
        int currentMaxLevel = PlayerPrefs.GetInt(prefix + "_CurrentLevel", 1);

        if (level > currentMaxLevel)
            PlayerPrefs.SetInt(prefix + "_CurrentLevel", level);

        PlayerPrefs.Save(); // Save progress
        OnGameDataUpdated?.Invoke();
    }

    // Check if Level is Unlocked
    public bool IsLevelUnlocked(int level)
    {
        string prefix = "Slot" + currentSlot;
        return PlayerPrefs.GetInt(prefix + "_Level" + level + "Unlocked", 0) == 1 || level == 1;
    }

    // Get Current Level
    public int GetCurrentLevel()
    {
        return PlayerPrefs.GetInt("Slot" + currentSlot + "_CurrentLevel", 1); // Return current level
    }

    // Get Level of a Specific Slot
    public int GetSlotLevel(int slot)
    {
        return PlayerPrefs.GetInt("Slot" + slot + "_CurrentLevel", 1); // Return level of specific slot
    }
}
