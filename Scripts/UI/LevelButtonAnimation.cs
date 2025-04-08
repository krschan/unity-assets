using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LevelButtonAnimation : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int levelNumber;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private float pulseScale = 1.1f;
    [SerializeField] private float pulseDuration = 1f;

    private Vector3 originalScale;
    private Coroutine pulseCoroutine;
    private Button button;

    // Initializes the button scale and subscribes to game data updates
    void Start()
    {
        originalScale = transform.localScale;
        button = GetComponent<Button>();

        if (GameManager.Instance != null)
        {
            UpdateButtonState();
            GameManager.Instance.OnGameDataUpdated += UpdateButtonState;
        }
    }

    // Unsubscribes from game data updates when the object is destroyed
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameDataUpdated -= UpdateButtonState;
        }
    }

    // Updates the button state based on whether the level is unlocked
    private void UpdateButtonState()
    {
        if (GameManager.Instance == null || button == null) return;

        bool isUnlocked = GameManager.Instance.IsLevelUnlocked(levelNumber);

        if (isUnlocked)
        {
            if (pulseCoroutine == null)
                pulseCoroutine = StartCoroutine(PulseAnimation());

            button.image.color = new Color(0.925f, 0.722f, 0.0f); // Yellow (ECB700)
            if (buttonText) buttonText.color = Color.white;
            button.interactable = true;
        }
        else
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
                transform.localScale = originalScale;
            }
            button.image.color = new Color(0.44f, 0.44f, 0.44f); // Gray (707070)
            if (buttonText) buttonText.color = Color.white;
            button.interactable = false;
        }
    }

    // Handles the pulse animation, continuously alternating between scale states
    private IEnumerator PulseAnimation()
    {
        while (true)
        {
            yield return PulseEffect(originalScale, originalScale * pulseScale);
            yield return PulseEffect(originalScale * pulseScale, originalScale);
        }
    }

    // Smoothly scales the button from one size to another over time
    private IEnumerator PulseEffect(Vector3 start, Vector3 end)
    {
        float elapsedTime = 0f;
        while (elapsedTime < pulseDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, end, elapsedTime / (pulseDuration / 2));
            yield return null;
        }
    }
}
