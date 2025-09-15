using UnityEngine;

/// <summary>
/// Use this when you don't want to rely on physics triggers (e.g., using Game Creator's controller).
/// Call these public methods from your Game Creator Actions or any other event system.
/// </summary>
public class PanelStepProxy : MonoBehaviour
{
    [Header("References")]
    public RiddleManagerLite manager;
    public SimpleColorPanel panel;     // Usually the same GameObject; if null, we'll auto-grab.
    public PanelColorTag colorTag;     // Optional; used only if you also want to update a ColorStepUI.
    public ColorStepUI colorUI;        // Optional; set if you want the 5-slot UI to fill.
    public GameObject uiIndicator;     // Optional; local indicator to toggle ON when stepped.

    private void Awake()
    {
        if (panel == null) panel = GetComponent<SimpleColorPanel>();
        if (colorTag == null) colorTag = GetComponent<PanelColorTag>();
        if (uiIndicator == null && panel != null) uiIndicator = panel.uiIndicator;
        if (colorUI == null) colorUI = FindObjectOfType<ColorStepUI>();
    }

    /// <summary>
    /// Manually "step" on this panel:
    /// - Turns on the (optional) uiIndicator
    /// - Registers the step with the manager (requires SimpleColorPanel reference)
    /// - Adds a color step to the ColorStepUI (if colorTag + colorUI are assigned)
    /// </summary>
    public void DoStep()
    {
        if (uiIndicator != null) uiIndicator.SetActive(true);

        if (manager != null && panel != null)
        {
            manager.RegisterStep(panel);
        }
        else
        {
            Debug.LogWarning("[PanelStepProxy] Missing manager or panel; cannot register step.");
        }

        if (colorUI != null && colorTag != null)
        {
            colorUI.AddStep(colorTag.color);
        }
    }

    /// <summary>
    /// Manually submit the current riddle.
    /// </summary>
    public void DoSubmit()
    {
        if (manager != null) manager.Submit();
        else Debug.LogWarning("[PanelStepProxy] No manager assigned for DoSubmit().");
    }

    /// <summary>
    /// Clears current riddle inputs (and panel indicators via manager).
    /// </summary>
    public void ResetCurrent()
    {
        if (manager != null) manager.ResetCurrentInputs();
    }

    /// <summary>
    /// Restarts the whole sequence back to riddle 1 and clears wrong flags.
    /// </summary>
    public void RestartAll()
    {
        if (manager != null) manager.ResetToFirstRiddle(true);
    }

    /// <summary>
    /// Clears ONLY the 5-slot color UI (if assigned).
    /// </summary>
    public void ClearUI()
    {
        if (colorUI != null) colorUI.ClearAll();
    }
}
