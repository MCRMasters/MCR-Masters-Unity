using UnityEngine;
using UnityEngine.UI;

public class DrawEffect : MonoBehaviour
{
    public Image fadeImage;        // FadeImage 오브젝트 (검정색 배경)
    public Image drawImage;        // Draw 스프라이트 이미지

    public float fadeDuration = 1.5f;     // 어두워지는 시간
    public float holdDuration = 2f;       // DRAW 유지 시간
    public float maxAlpha = 0.5f;         // 최종 어두움 정도 (0~1)

    private float timer = 0f;
    private bool isPlaying = false;

    public void PlayDrawEffect()
    {
        gameObject.SetActive(true); // 오브젝트 켜기
        timer = 0f;
        isPlaying = true;

        // 배경은 완전 투명 검정에서 시작
        fadeImage.color = new Color(0f, 0f, 0f, 0f);

        // DRAW 이미지는 바로 등장
        drawImage.gameObject.SetActive(true);
        drawImage.rectTransform.localScale = Vector3.one;
    }

    void Update()
    {
        if (!isPlaying) return;

        timer += Time.deltaTime;

        // 점점 어두워짐 (검정색 + 알파 증가)
        if (timer < fadeDuration)
        {
            float alpha = Mathf.Clamp01(timer / fadeDuration) * maxAlpha;
            fadeImage.color = new Color(0f, 0f, 0f, alpha);
        }
        // 유지 구간 (DRAW 이미지와 배경 그대로)
        else if (timer < fadeDuration + holdDuration)
        {
            fadeImage.color = new Color(0f, 0f, 0f, maxAlpha);
        }
        // 연출 종료
        else
        {
            isPlaying = false;

            drawImage.gameObject.SetActive(false);
            fadeImage.color = new Color(0f, 0f, 0f, 0f); // 초기화
            gameObject.SetActive(false);                // 전체 비활성화
        }
    }
}
