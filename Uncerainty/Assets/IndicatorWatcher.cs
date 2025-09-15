using UnityEngine;

public class IndicatorWatcher : MonoBehaviour
{
    [Tooltip("The 5 indicator GameObjects you want to watch.")]
    public GameObject[] indicators;

    [Tooltip("This GameObject will be activated when 5 indicators are ON, deactivated otherwise.")]
    public GameObject targetObject;

    void Update()
    {
        if (indicators == null || targetObject == null) return;

        int activeCount = 0;
        foreach (var ind in indicators)
        {
            if (ind != null && ind.activeSelf) activeCount++;
        }

        targetObject.SetActive(activeCount == 5);
    }
}
