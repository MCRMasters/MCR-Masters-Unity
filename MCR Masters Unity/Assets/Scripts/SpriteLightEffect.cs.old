using UnityEngine;
using System.Collections;


public class SpriteLightEffect : MonoBehaviour
{
    public SpriteRenderer lightSprite;  // 스프라이트 렌더러 연결
    private float blinkDuration = 0.5f;   // 깜빡임 주기
    private float maxAlpha = 1f;      // 최대 투명도
    private float minAlpha = 0.2f;      // 최소 투명도

    void Start()
    {
        //StartCoroutine(BlinkEffect());
    }

    public void BlinkEffectOn()
    {
        StartCoroutine(BlinkEffect());
    }

    private IEnumerator BlinkEffect()
    {
        while (true)
        {
            for (float t = 0; t < blinkDuration; t += Time.deltaTime)
            {
                float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(t, blinkDuration) / blinkDuration);
                lightSprite.color = new Color(1f, 1f, 1f, alpha);

                yield return null;
            }
            for (float t = 0; t < blinkDuration; t += Time.deltaTime)
            {
                float alpha = maxAlpha - Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(t, blinkDuration) / blinkDuration);
                lightSprite.color = new Color(1f, 1f, 1f, alpha);

                yield return null;
            }
        }
    }
}
