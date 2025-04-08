using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelsManager : MonoBehaviour
{
    // Load the specified level if it's unlocked
    public void LoadLevel(int level)
    {
        if (GameManager.Instance.IsLevelUnlocked(level))
        {
            SceneManager.LoadScene("Level" + level);
        }
        else
        {
            Debug.LogWarning($"Access denied: Level {level} is locked.");
        }
    }

    // Return to the main menu
    public void BackLevel()
    {
        SceneManager.LoadScene("MenuLobby");
    }
}
