using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class FlashingText : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Coroutine flashRoutine;

    [SerializeField] private float delayOnEnable = 2f;   // Delay before starting flash
    [SerializeField] private float flashDuration = 1f;   // Time to go 0 -> 1 -> 0

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f; // Ensure hidden at start
    }

    private void OnEnable()
    {
        // Always reset to hidden
        canvasGroup.alpha = 0f;

        // Start flashing after delay
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private void OnDisable()
    {
        // Reset alpha and stop flashing
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }
        canvasGroup.alpha = 0f;
    }

    private IEnumerator FlashRoutine()
    {
        // Wait before first flash
        yield return new WaitForSeconds(delayOnEnable);

        while (true)
        {
            // Fade in
            yield return StartCoroutine(Fade(0f, 1f, flashDuration * 0.5f));

            // Fade out
            yield return StartCoroutine(Fade(1f, 0f, flashDuration * 0.5f));
        }
    }

    private IEnumerator Fade(float start, float end, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }
        canvasGroup.alpha = end;
    }
}
