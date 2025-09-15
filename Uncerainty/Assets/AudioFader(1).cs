using UnityEngine;
using System.Collections;

public class AudioFader : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;

    [Range(0f, 1f)] public float targetVolume = 0.2f;
    public float fadeOutDuration = 1f;
    public float fadeInDuration = 1f;

    private float originalVolume;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        originalVolume = audioSource.volume;
    }

    [ContextMenu("Fade To Target Volume")]
    public void FadeToTarget()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeAudio(audioSource.volume, targetVolume, fadeOutDuration));
    }

    [ContextMenu("Restore Original Volume")]
    public void RestoreVolume()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeAudio(audioSource.volume, originalVolume, fadeInDuration));
    }

    private IEnumerator FadeAudio(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            audioSource.volume = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = to;
    }
}
