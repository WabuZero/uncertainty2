using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(PanelColorTag))]
public class UIOnPanelStep : MonoBehaviour
{
    public string playerTag = "Player";
    public ColorStepUI ui; // If left null, will auto-find the first ColorStepUI in scene

    private PanelColorTag _tag;
    private bool _inside = false;

    private void Awake()
    {
        _tag = GetComponent<PanelColorTag>();
        if (ui == null) ui = FindObjectOfType<ColorStepUI>();
        if (ui == null) Debug.LogWarning("[UIOnPanelStep] No ColorStepUI found/assigned.");        
    }

    private void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_inside) return;
        if (!other.CompareTag(playerTag)) return;
        _inside = true;

        if (ui != null && _tag != null)
        {
            ui.AddStep(_tag.color);
        }
        else
        {
            Debug.LogWarning("[UIOnPanelStep] Missing UI or PanelColorTag; cannot add step.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        _inside = false;
    }
}
