using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnerabilityTime = 1f;
    private int currentHealth;
    private bool isInvulnerable;
    private float invulnerabilityTimer;

    private void Start()
    {
        currentHealth = maxHealth;
        isInvulnerable = false;
        UpdateUI(); // Update UI at start
    }

    private void Update()
    {
        // Handle invulnerability timer
        if (isInvulnerable && (invulnerabilityTimer -= Time.deltaTime) <= 0)
        {
            isInvulnerable = false;
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0 || isInvulnerable) return;

        currentHealth -= damage;
        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityTime;
        
        UpdateUI(); // Update health UI

        if (currentHealth <= 0) {
            Die(); // Handle death
            GameManager.Instance.SaveAndProceed(false); // Save and proceed after death
        }
    }

    private void UpdateUI()
    {
        CanvasManager.Instance.UpdateHealthUI(currentHealth); // Update health display
    }

    private void Die()
    {
        Destroy(gameObject); // Destroy player object on death
    }
}
