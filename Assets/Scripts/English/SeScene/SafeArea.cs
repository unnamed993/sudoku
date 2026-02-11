using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    RectTransform rt;
    Rect lastSafe;
    Vector2Int lastScreen;
    void Awake()
    {
        rt = GetComponent<RectTransform>();
        Apply();
    }
    void Update()
    {
        if (Screen.safeArea != lastSafe || lastScreen.x != Screen.width || lastScreen.y != Screen.height)
            Apply();
    }
    void Apply()
    {
        lastSafe = Screen.safeArea;
        lastScreen = new Vector2Int(Screen.width, Screen.height);

        var safe = lastSafe;

        Vector2 anchorMin = safe.position;
        Vector2 anchorMax = safe.position + safe.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
