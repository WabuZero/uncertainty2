using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Optional helper that clears the 5-slot UI whenever the manager resets current riddle
/// or restarts the whole sequence. Drag your RiddleManagerLite and ColorStepUI into the inspector.
/// </summary>
public class ColorUIResetBinder : MonoBehaviour
{
    public RiddleManagerLite manager;
    public ColorStepUI ui;

    private UnityAction _onCurrResetAction;
    private UnityAction _onAllRestartedAction;

    private void OnEnable()
    {
        if (manager == null || ui == null) return;
        _onCurrResetAction = () => ui.ClearAll();
        _onAllRestartedAction = () => ui.ClearAll();

        if (manager.onCurrentRiddleReset != null) manager.onCurrentRiddleReset.AddListener(_onCurrResetAction);
        if (manager.onAllRiddlesRestarted != null) manager.onAllRiddlesRestarted.AddListener(_onAllRestartedAction);
    }

    private void OnDisable()
    {
        if (manager == null || ui == null) return;
        if (manager.onCurrentRiddleReset != null && _onCurrResetAction != null) manager.onCurrentRiddleReset.RemoveListener(_onCurrResetAction);
        if (manager.onAllRiddlesRestarted != null && _onAllRestartedAction != null) manager.onAllRiddlesRestarted.RemoveListener(_onAllRestartedAction);
    }
}
