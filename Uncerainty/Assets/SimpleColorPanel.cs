using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SimpleColorPanel : MonoBehaviour
{
    [Tooltip("Tag that the stepping object must have (usually 'Player').")]
    public string playerTag = "Player";

    [Tooltip("(Optional) UI GameObject to activate when this panel is stepped on).")]
    public GameObject uiIndicator;

    [Tooltip("Reference to the riddle manager.")]
    public RiddleManagerLite manager;

    private bool _inside = false;

    private void Reset()
    {
        // Ensure collider is set to trigger
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnEnable()
    {
        // Make sure UI starts off
        if (uiIndicator != null) uiIndicator.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_inside) return;
        if (!other.CompareTag(playerTag)) return;
        _inside = true;

        if (uiIndicator != null) uiIndicator.SetActive(true);
        if (manager != null) manager.RegisterStep(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        _inside = false;
    }

    /// <summary>
    /// Turns off this panel's UI indicator (if assigned).
    /// Called automatically when the manager resets.
    /// </summary>
    public void ResetIndicator()
    {
        if (uiIndicator != null) uiIndicator.SetActive(false);
    }
}
