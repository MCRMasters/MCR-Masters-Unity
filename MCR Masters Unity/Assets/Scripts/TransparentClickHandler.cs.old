using UnityEngine;
using UnityEngine.UI;

public class TransparentClickHandler : MonoBehaviour
{
    void Start()
    {
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.raycastTarget = true; // Raycast Target 활성화
            img.alphaHitTestMinimumThreshold = 0.1f; // 10% 이상 불투명한 부분만 클릭 가능
        }
    }
}