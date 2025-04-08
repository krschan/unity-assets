using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStompMonster : MonoBehaviour
{
    // The method is triggered when the player collides with another object
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Weakpoint")) return;

        // Destroy the object that the player collided
        Destroy(collision.gameObject);

        GameManager.Instance.SaveAndProceed(true);

        // If the current scene is "Level1", unlock the next level and load it
        if (SceneManager.GetActiveScene().name == "Level1")
        {
            GameManager.Instance.UnlockNextLevel(2);
            SceneManager.LoadScene("Level2");
        }
    }
}
