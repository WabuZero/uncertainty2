using UnityEngine;
using UnityEngine.UI;

public class ColorStepUI : MonoBehaviour
{
    [Header("Indicator Slots (assign 5 Image components, left to right)")]
    public Image[] slots = new Image[5];

    [Header("Appearance") ]
    [Tooltip("If true, uses per-color sprites. If false, tints a base (ideally WHITE) sprite.")]
    public bool useSprites = false;

    [Tooltip("(Tint mode only) A WHITE circle sprite to tint. If left null, the slot's existing sprite is used.")]
    public Sprite tintBaseWhiteSprite;

    [Header("Tint Colors (if useSprites=false)")]
    public Color red = new Color(1f, 0f, 0f, 1f);
    public Color blue = new Color(0.1f, 0.5f, 1f, 1f);
    public Color green = new Color(0.2f, 0.85f, 0.2f, 1f);
    public Color yellow = new Color(1f, 0.9f, 0.2f, 1f);
    public Color pink = new Color(1f, 0.6f, 0.8f, 1f);

    [Header("Sprites (if useSprites=true)")]
    public Sprite redSprite;
    public Sprite blueSprite;
    public Sprite greenSprite;
    public Sprite yellowSprite;
    public Sprite pinkSprite;

    private int _nextIndex = 0;

    private void Awake()
    {
        ClearAll();
        ValidateSlots();
    }

    public void AddStep(PanelColor color)
    {
        if (slots == null || slots.Length == 0) { Debug.LogWarning("[ColorStepUI] No slots assigned."); return; }
        if (_nextIndex >= slots.Length) { Debug.Log("[ColorStepUI] Slots are full; ignoring extra steps."); return; }

        var img = slots[_nextIndex];
        if (img == null) { Debug.LogWarning($"[ColorStepUI] Slot {_nextIndex} is null."); return; }

        // Ensure Image is visible and not hidden by CanvasGroup
        img.raycastTarget = false; // irrelevant but avoids blocking clicks
        var cg = img.GetComponentInParent<CanvasGroup>();
        if (cg != null && cg.alpha < 0.99f) Debug.LogWarning("[ColorStepUI] Parent CanvasGroup alpha < 1; image may appear dim/hidden.");

        if (useSprites)
        {
            var s = GetSprite(color);
            if (s == null) Debug.LogWarning($"[ColorStepUI] Missing sprite for {color}.");
            img.sprite = s != null ? s : img.sprite;
            img.color = Color.white; // ensure full tint
        }
        else
        {
            if (tintBaseWhiteSprite != null) img.sprite = tintBaseWhiteSprite;
            // NOTE: the base sprite should be WHITE to tint properly.
            img.color = GetColor(color);
        }

        img.gameObject.SetActive(true);
        _nextIndex++;
    }

    public void ClearAll()
    {
        _nextIndex = 0;
        if (slots == null) return;
        foreach (var img in slots)
        {
            if (img != null) img.gameObject.SetActive(false);
        }
    }

    private void ValidateSlots()
    {
        if (slots == null || slots.Length != 5)
        {
            Debug.LogWarning("[ColorStepUI] You should assign exactly 5 Image slots (array size must be 5).");
        }
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) Debug.LogWarning($"[ColorStepUI] Slot {i} is not assigned.");
        }
    }

    private Color GetColor(PanelColor c)
    {
        switch (c)
        {
            case PanelColor.Red: return red;
            case PanelColor.Blue: return blue;
            case PanelColor.Green: return green;
            case PanelColor.Yellow: return yellow;
            case PanelColor.Pink: return pink;
            default: return Color.white;
        }
    }

    private Sprite GetSprite(PanelColor c)
    {
        switch (c)
        {
            case PanelColor.Red: return redSprite;
            case PanelColor.Blue: return blueSprite;
            case PanelColor.Green: return greenSprite;
            case PanelColor.Yellow: return yellowSprite;
            case PanelColor.Pink: return pinkSprite;
            default: return null;
        }
    }
}
