using UnityEngine;

public class FixedAspectWindow : MonoBehaviour
{
    public float targetAspect = 16.0f / 9.0f;
    public float stableTime = 0.5f; // 0.5초 동안 창 크기 변동이 없으면 “드래그 완료”로 간주

    private float sizeChangedTimer = 0f;
    private int lastWidth;
    private int lastHeight;

    void Start()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;
    }

    void Update()
    {
        if (Screen.fullScreen) return;

        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            // 창 크기가 바뀐 프레임
            sizeChangedTimer = 0f;
            lastWidth = Screen.width;
            lastHeight = Screen.height;
        }
        else
        {
            // 크기가 바뀌지 않은 프레임
            sizeChangedTimer += Time.deltaTime;

            // 일정 시간 유지되면 드래그 끝난 것으로 간주
            if (sizeChangedTimer >= stableTime)
            {
                // 강제 비율 맞추기
                int newWidth = Screen.width;
                int newHeight = Mathf.RoundToInt(newWidth / targetAspect);

                Screen.SetResolution(newWidth, newHeight, false);
                // 크기 재설정 후 다시 체크할 수 있도록 width/height 갱신
                lastWidth = newWidth;
                lastHeight = newHeight;
            }
        }
    }
}
