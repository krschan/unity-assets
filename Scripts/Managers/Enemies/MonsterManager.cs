using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    [SerializeField] private int damage = 1; // Amount of damage to deal
    [SerializeField] private float stompThreshold = 0.5f; // Threshold to detect stomp from above

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Only react if the player collides
        if (!collision.gameObject.CompareTag("Player")) return;

        if (collision.gameObject.TryGetComponent(out PlayerHealth playerHealth))
        {
            // Get the contact point of the collision
            ContactPoint2D contact = collision.GetContact(0);

            // Check if the player is stomping from above
            bool isStomping = contact.normal.y < -stompThreshold;

            // If not stomping, deal damage to the player
            if (!isStomping)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}
