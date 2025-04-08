using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLobbyManager : MonoBehaviour
{
    // UI references for ranking display
    public Transform rankingContent;
    public GameObject rankingTextPrefab;

    void Start()
    {
        LoadRanking(); // Load ranking data
    }

    void LoadRanking()
    {
        if (APIManager.Instance != null)
        {
            APIManager.Instance.LoadRanking(rankingContent, rankingTextPrefab); // Load ranking from API
        }
    }
    
    // Start a new game
    public void StartNewGame()
    {
        PlayerPrefs.SetInt("StartGameMode", 1); // Set new game mode
        PlayerPrefs.Save();
        SceneManager.LoadScene("Level1"); // Load first level
    }

    // Go to the load game menu
    public void GoToLoadGameMenu()
    {
        PlayerPrefs.SetInt("StartGameMode", 0); // Set load game mode
        PlayerPrefs.Save();
        SceneManager.LoadScene("MenuLevels"); // Load levels menu
    }
}
