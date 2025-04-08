using UnityEngine;
using UnityEngine.UI;

public class MultiGraphicButton : Button
{
    [SerializeField] 
    public Graphic[] additionalGraphics; // Tint를 적용할 Image, Text 등

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

        // Button이 기본적으로 계산하는 "색상"을 다시 구하는 로직
        Color tintColor;
        switch (state)
        {
            case SelectionState.Normal:
                tintColor = colors.normalColor;
                break;
            case SelectionState.Highlighted:
                tintColor = colors.highlightedColor;
                break;
            case SelectionState.Pressed:
                tintColor = colors.pressedColor;
                break;
            case SelectionState.Selected:
                tintColor = colors.selectedColor;
                break;
            case SelectionState.Disabled:
                tintColor = colors.disabledColor;
                break;
            default:
                tintColor = Color.white;
                break;
        }

        // 추가로 등록된 모든 그래픽에 동일한 Tint 적용
        foreach (var g in additionalGraphics)
        {
            if (g != null)
            {
                // Fade Duration 동안 부드럽게 색 변화
                g.CrossFadeColor(tintColor, instant ? 0f : colors.fadeDuration, true, true);
            }
        }
    }
}
