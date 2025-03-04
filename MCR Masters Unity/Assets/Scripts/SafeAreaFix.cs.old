using UnityEngine;

public class SafeAreaFix : MonoBehaviour
{
    void Start()
    {
        Rect safeArea = Screen.safeArea;
        RectTransform rectTransform = GetComponent<RectTransform>();
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
