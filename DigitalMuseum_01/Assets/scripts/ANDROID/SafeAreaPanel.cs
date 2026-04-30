using UnityEngine;

/// <summary>
/// Attach this to a RectTransform that acts as the safe area container.
/// All your UI elements should be children of this RectTransform, not 
/// direct children of the Canvas.
///
/// SETUP:
///   Canvas
///     └── SafeAreaPanel  ← attach this script here, set anchors to stretch/stretch
///           ├── YourButton
///           ├── YourIcon
///           └── etc.
/// </summary>
public class SafeAreaPanel : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea = Rect.zero;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    // Re-check every frame in case orientation changes at runtime
    void Update()
    {
        if (Screen.safeArea != lastSafeArea)
            ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        lastSafeArea = safeArea;

        // Convert safe area rectangle from absolute pixels to anchor coordinates (0-1)
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}