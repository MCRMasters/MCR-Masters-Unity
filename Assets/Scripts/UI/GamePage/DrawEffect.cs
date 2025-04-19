using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MCRGame.UI
{
    public class DrawEffect : MonoBehaviour
    {
        public Image fadeImage;        // 검정색 배경 이미지
        public Image drawImage;        // "DRAW" 스프라이트 이미지

        public float fadeDuration = 1.5f;   // 어두워지는 데 걸리는 시간
        public float holdDuration = 2f;     // 유지 시간
        public float maxAlpha = 0.5f;       // 최종 어두움 정도 (0 ~ 1)

        private Coroutine playCoroutine;

        public void PlayDrawEffect()
        {
            gameObject.SetActive(true); // 전체 오브젝트 켜기

            if (playCoroutine != null)
                StopCoroutine(playCoroutine);

            playCoroutine = StartCoroutine(PlaySequence());
        }

        private IEnumerator PlaySequence()
        {
            // 초기화
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            drawImage.gameObject.SetActive(true);
            drawImage.rectTransform.localScale = Vector3.one;

            float timer = 0f;

            // 페이드 인 (알파 부드럽게 증가)
            while (timer < fadeDuration)
            {
                float t = Mathf.Clamp01(timer / fadeDuration);
                float alpha = Mathf.SmoothStep(0f, maxAlpha, t); //부드러운 알파 곡선
                fadeImage.color = new Color(0f, 0f, 0f, alpha);

                timer += Time.deltaTime;
                yield return null;
            }

            // 유지 구간
            fadeImage.color = new Color(0f, 0f, 0f, maxAlpha);
            yield return new WaitForSeconds(holdDuration);

            // 연출 종료
            drawImage.gameObject.SetActive(false);
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            gameObject.SetActive(false);
        }
    }
}
