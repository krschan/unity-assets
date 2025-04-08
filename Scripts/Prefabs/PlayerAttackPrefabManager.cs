using TMPro;
using UnityEngine;

public class PlayerAttackPrefabManager : MonoBehaviour
{
    [SerializeField] private float attackDuration = 0.5f;

    // Starts the attack and schedules its destruction
    void Start()
    {
        Invoke("SelfDestroy", attackDuration);
    }

    // Destroys the attack prefab after a set duration
    void SelfDestroy()
    {
        Destroy(gameObject);
    }

    // Handles collision with coins and updates the coin count
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            Destroy(collision.gameObject);
            GameManager.Instance.numberCoinDestroyed++;
            GameObject.Find("TextCoinUI").GetComponent<TextMeshProUGUI>().SetText(GameManager.Instance.numberCoinDestroyed.ToString());
        }
    }
}
