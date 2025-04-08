using UnityEngine;
using UnityEngine.UI;

public class LoadingBarImageController : MonoBehaviour
{
    public Image fillImage;              // LoadingBar_Fill
    public RectTransform effectImage;   // Loading_Effect
    public RectTransform fillArea;      // LoadingBar_BG (기준 위치용)

    public float slowSpeed = 0.1f;
    public float fastSpeed = 2f;

    private float fillAmount = 0f;
    private bool isConnected = false;

    void Update()
    {
        float target = isConnected ? 1f : 0.95f;
        float speed = isConnected ? fastSpeed : slowSpeed;

        fillAmount = Mathf.MoveTowards(fillAmount, target, Time.deltaTime * speed);
        fillImage.fillAmount = fillAmount;

        // 새로운 이펙트 위치 계산 (좌측 기준)
        float width = fillArea.rect.width;
        Vector2 anchoredPos = new Vector2(width * fillAmount, effectImage.anchoredPosition.y);
        effectImage.anchoredPosition = anchoredPos;
    }

    public void OnConnectionComplete()
    {
        isConnected = true;
    }
}
