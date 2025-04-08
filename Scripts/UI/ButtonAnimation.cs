using UnityEngine;
using System.Collections;

public class ButtonAnimation : MonoBehaviour 
{
    [SerializeField] private float pulseScale = 1.1f;
    [SerializeField] private float pulseDuration = 1f;

    private Vector3 originalScale;

    // Initializes the button scale and starts the pulse animation
    private void Start()
    {
        originalScale = transform.localScale;
        StartCoroutine(PulseAnimation());
    }

    // Handles the pulse animation, alternating between scale states
    private IEnumerator PulseAnimation()
    {
        while (true)
        {
            yield return ScaleButton(originalScale, originalScale * pulseScale, pulseDuration / 2);
            yield return ScaleButton(originalScale * pulseScale, originalScale, pulseDuration / 2);
        }
    }

    // Scales the button smoothly from one size to another
    private IEnumerator ScaleButton(Vector3 from, Vector3 to, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(from, to, elapsedTime / duration);
            yield return null;
        }
    }
}
