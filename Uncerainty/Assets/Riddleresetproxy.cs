using UnityEngine;

/// <summary>
/// Game Creator-friendly reset/restart proxy.
/// Call these no-arg methods from Game Creator's "Call Method" action.
/// - Soft methods call the manager's APIs (which also invoke events).
/// - Hard methods ALSO force-clear any SimpleColorPanel indicators found in the scene,
///   and clear the optional ColorStepUI if present.
/// </summary>
public class RiddleResetProxyPlus : MonoBehaviour
{
    [Header("References")]
    public RiddleManagerLite manager;
    [Tooltip("Optional: assign if you use the 5-slot UI; otherwise we'll try to FindObjectOfType.")]
    public ColorStepUI colorUI;

    void Awake()
    {
        if (colorUI == null) colorUI = FindObjectOfType<ColorStepUI>();
    }

    // ---------- SOFT: use manager APIs only ----------

    /// <summary>
    /// Clears current riddle inputs via manager (like tapping R). Manager will call panel.ResetIndicator()
    /// for panels in the current riddle's solution list and fire onCurrentRiddleReset.
    /// </summary>
    public void ResetCurrentSoft()
    {
        if (manager != null) manager.ResetCurrentInputs();
        else Debug.LogWarning("[RiddleResetProxyPlus] No manager assigned for ResetCurrentSoft().");
    }

    /// <summary>
    /// Restarts to riddle 1 via manager (like holding R). Also clears wrong flags and fires events.
    /// </summary>
    public void RestartAllSoft()
    {
        if (manager != null) manager.ResetToFirstRiddle(true);
        else Debug.LogWarning("[RiddleResetProxyPlus] No manager assigned for RestartAllSoft().");
    }

    // ---------- HARD: force-clear indicators/UI in addition to manager ----------

    /// <summary>
    /// Clears current via manager AND force-clears ALL SimpleColorPanel indicators in the scene.
    /// Also clears optional ColorStepUI if present.
    /// </summary>
    public void ResetCurrentHard()
    {
        if (manager != null) manager.ResetCurrentInputs();
        else Debug.LogWarning("[RiddleResetProxyPlus] No manager assigned for ResetCurrentHard().");

        ForceClearAllIndicators();
        ClearColorUI();
    }

    /// <summary>
    /// Restarts to riddle 1 via manager AND force-clears ALL SimpleColorPanel indicators in the scene.
    /// Also clears optional ColorStepUI if present.
    /// </summary>
    public void RestartAllHard()
    {
        if (manager != null) manager.ResetToFirstRiddle(true);
        else Debug.LogWarning("[RiddleResetProxyPlus] No manager assigned for RestartAllHard().");

        ForceClearAllIndicators();
        ClearColorUI();
    }

    // ---------- Helpers ----------

    private void ForceClearAllIndicators()
    {
        var panels = FindObjectsOfType<SimpleColorPanel>(includeInactive: true);
        foreach (var p in panels)
        {
            if (p != null) p.ResetIndicator();
        }
    }

    private void ClearColorUI()
    {
        if (colorUI == null) colorUI = FindObjectOfType<ColorStepUI>();
        if (colorUI != null) colorUI.ClearAll();
    }
}
