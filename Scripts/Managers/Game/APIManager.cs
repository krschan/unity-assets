using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class APIManager : MonoBehaviour
{
    // Singleton pattern to allow global access to this class
    public static APIManager Instance;

    // Server endpoints for different operations
    public string postRegisterUrl = "http://localhost/game/grotto-escape.php";
    public string getRankingUrl = "http://localhost/game/get_ranking.php";
    public string postScoreUrl = "http://localhost/game/post_ranking.php";

    // UI elements for registration
    public TMP_InputField inputNameField;
    public TMP_InputField inputMailField;
    public TMP_Text responseText;

    // =====================
    // Initialization
    // =====================

    void Awake()
    {
        // Ensure only one instance of this object exists
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

    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Hide error text on registration screen
        if (currentScene == "MenuRegister")
            responseText.gameObject.SetActive(false);
    }

    // =====================
    // Player Registration
    // =====================

    // Called when the user submits the registration form
    public void SubmitRegistration()
    {
        if (IsRegistrationFormEmpty())
        {
            ShowError("Error: Please fill in all fields");
            return;
        }

        StartCoroutine(SendRegistration(inputNameField.text, inputMailField.text));
    }

    // Check if name or email fields are empty
    bool IsRegistrationFormEmpty()
    {
        return string.IsNullOrEmpty(inputNameField.text) || string.IsNullOrEmpty(inputMailField.text);
    }

    // Send user registration data to the server
    IEnumerator SendRegistration(string name, string email)
    {
        WWWForm form = new WWWForm();
        form.AddField("nombre", name);
        form.AddField("email", email);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(postRegisterUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                ShowError($"Connection error: {webRequest.error}");
                yield break;
            }

            string serverResponse = webRequest.downloadHandler.text;

            // Check for success response from server
            if (serverResponse.StartsWith("success"))
            {
                SavePlayerID(serverResponse);
                PlayerPrefs.SetString("jugador_nombre", name);
                SceneManager.LoadScene("MenuLobby");
            }
            else
            {
                ShowError($"Error: {serverResponse}");
            }
        }
    }

    // Save player ID locally using PlayerPrefs
    void SavePlayerID(string response)
    {
        string[] parts = response.Split('|');
        if (parts.Length > 1 && int.TryParse(parts[1], out int id)) {
            PlayerPrefs.SetInt("jugador_id", id);
            PlayerPrefs.SetString("jugador_nombre", name);
        }
    }

    // =====================
    // Save Player Score
    // =====================

    // Call this method after the game ends to send score and time
    public void SendScoreToServer(int score, int time)
    {
        int playerId = PlayerPrefs.GetInt("jugador_id", 0);
        if (playerId == 0) return;

        StartCoroutine(SendScore(playerId, score, time));
    }

    // Send the player's score and time to the server
    IEnumerator SendScore(int playerId, int score, int time)
    {
        WWWForm form = new WWWForm();
        form.AddField("jugador_id", playerId);
        form.AddField("puntuacion", score);
        form.AddField("tiempo", time);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(postScoreUrl, form))
        {
            yield return webRequest.SendWebRequest();
        }
    }

    // =====================
    // Ranking
    // =====================

    // Load the ranking from the server
    public void LoadRanking(Transform contentContainer, GameObject textPrefab)
    {
        StartCoroutine(FetchRanking(contentContainer, textPrefab));
    }

    // Make a GET request to fetch the ranking list
    IEnumerator FetchRanking(Transform contentContainer, GameObject textPrefab)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(getRankingUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
                HandleRankingData(webRequest.downloadHandler.text, contentContainer, textPrefab);
        }
    }

    // Parse JSON data and create UI ranking items
    void HandleRankingData(string json, Transform contentContainer, GameObject textPrefab)
    {
        ClearRankingList(contentContainer);

        try
        {
            string fixedJson = "{\"players\":" + json + "}";
            RankingData data = JsonUtility.FromJson<RankingData>(fixedJson);

            if (data.players.Count == 0) return;

            foreach (RankingItem item in data.players)
                CreateRankingItem(item, contentContainer, textPrefab);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error processing ranking: " + ex.Message);
        }
    }

    // Remove all existing UI elements from the ranking list
    void ClearRankingList(Transform contentContainer)
    {
        foreach (Transform child in contentContainer)
            Destroy(child.gameObject);
    }

    // Instantiate and fill a new UI item with ranking info
    void CreateRankingItem(RankingItem item, Transform contentContainer, GameObject textPrefab)
    {
        GameObject newItem = Instantiate(textPrefab, contentContainer);
        TMP_Text textComponent = newItem.GetComponent<TMP_Text>();

        if (textComponent != null)
        {
            textComponent.text = $"{item.nombre}\nScore: {item.mejor_puntuacion}\nGames: {item.total_partidas}";
        }
    }

    // =====================
    // Utils
    // =====================

    // Display an error message in the UI
    void ShowError(string message)
    {
        responseText.text = message;
        responseText.gameObject.SetActive(true);
    }

    // =====================
    // JSON Data Models
    // =====================

    [System.Serializable]
    public class RankingData
    {
        public List<RankingItem> players;
    }

    [System.Serializable]
    public class RankingItem
    {
        public string nombre;
        public int mejor_puntuacion;
        public int total_partidas;
    }
}
