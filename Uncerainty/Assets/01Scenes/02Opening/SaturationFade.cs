using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class SaturationFade : MonoBehaviour
{
    [Header("Global Volume Reference")]
    public Volume globalVolume;

    [Header("Fade Settings")]
    [Tooltip("How long it takes for saturation to fade out.")]
    public float fadeDuration = 3.5f;

    [Tooltip("Target saturation value (default = -100 for grayscale).")]
    [Range(-100f, 100f)]
    public float targetSaturation = -100f;

    private ColorAdjustments _colorAdjustments;
    private Coroutine _fadeRoutine;

    private void Awake()
    {
        if (globalVolume == null)
        {
            globalVolume = GetComponent<Volume>();
        }

        if (globalVolume != null && globalVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            _colorAdjustments = colorAdjustments;
        }
        else
        {
            Debug.LogError("[SaturationFade] No ColorAdjustments override found in Volume Profile!");
        }
    }

    /// <summary>
    /// Start fading saturation from current value down to target.
    /// </summary>
    public void StartFade()
    {
        if (_colorAdjustments == null) return;

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeSaturationRoutine());
    }

    private IEnumerator FadeSaturationRoutine()
    {
        float startValue = _colorAdjustments.saturation.value;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            _colorAdjustments.saturation.value = Mathf.Lerp(startValue, targetSaturation, t);
            yield return null;
        }

        _colorAdjustments.saturation.value = targetSaturation;
        _fadeRoutine = null;
    }
}
