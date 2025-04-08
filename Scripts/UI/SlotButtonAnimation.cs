using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SlotButtonAnimation : MonoBehaviour
{
    [Header("Slot Settings")]
    [SerializeField] private int slotNumber;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private float pulseScale = 1.1f;
    [SerializeField] private float pulseDuration = 1f;

    private Vector3 originalScale;
    private Coroutine pulseCoroutine;
    private Button button;

    void Start()
    {
        originalScale = transform.localScale;
        button = GetComponent<Button>();
        UpdateSlotVisual();
    }

    void Update()
    {
        UpdateSlotVisual();
    }

    private void UpdateSlotVisual()
    {
        bool isSelected = GameManager.Instance.currentSlot == slotNumber;

        if (isSelected)
        {
            if (pulseCoroutine == null)
                pulseCoroutine = StartCoroutine(PulseAnimation());

            button.image.color = new Color(0.89f, 0.69f, 0.00f); // Yellow (E3B000)
            if (buttonText) buttonText.color = Color.white;
        }
        else
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
                transform.localScale = originalScale;
            }

            button.image.color = Color.white;
            if (buttonText) buttonText.color = Color.black;
        }
    }

    private IEnumerator PulseAnimation()
    {
        while (true)
        {
            yield return PulseEffect(originalScale, originalScale * pulseScale);
            yield return PulseEffect(originalScale * pulseScale, originalScale);
        }
    }

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
