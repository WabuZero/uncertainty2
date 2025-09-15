using UnityEngine;

public enum PanelColor { Red, Blue, Green, Yellow, Pink }

/// <summary>
/// Put this on each colored panel to declare its logical color.
/// </summary>
public class PanelColorTag : MonoBehaviour
{
    public PanelColor color = PanelColor.Red;
}
