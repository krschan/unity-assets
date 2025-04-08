using UnityEngine;

public class CoinManager : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the player touches the coin
        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject); // Remove the coin
            GameManager.Instance.numberCoinDestroyed++; // Increase coin counter
        }
    }
}
