// Assets/Editor/TextToTMPBatchConverter.cs
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class TextToTMPBatchConverter
{
    // 프로젝트에서 한 번만 찾아서 캐싱할 기본 TMP_FontAsset
    private static TMP_FontAsset defaultFontAsset;

    [MenuItem("Tools/모든 UI Text → TextMeshPro로 변환")]
    public static void ConvertAllUIText()
    {
        // 1) TMP 폰트 에셋 체크
        if (GetDefaultFontAsset() == null)
        {
            Debug.LogError(
                "[TextToTMP] TMP 폰트 에셋을 찾을 수 없습니다.\n" +
                "Window → TextMeshPro → Import TMP Essential Resources 를 먼저 실행하세요."
            );
            return;
        }

        // 2) 비활성 포함, 소트 없이 빠르게 Text 컴포넌트 수집
        var texts = Object.FindObjectsByType<Text>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        int count = 0;
        foreach (var text in texts)
        {
            if (text == null) 
                continue;

            var go = text.gameObject;

            // 이미 TMPUGUI가 붙어 있으면 건너뛰기
            if (go.GetComponent<TextMeshProUGUI>() != null) 
                continue;

            // (선택) Image 등 다른 Graphic 이 있다면 스킵하고 싶으면 아래 코드 추가
            // if (go.GetComponent<Image>() != null) continue;

            // 3) 기존 Text 속성 백업
            string    content           = text.text;
            int       fontSize          = text.fontSize;
            Color     color             = text.color;
            TextAnchor anchor            = text.alignment;
            bool      wrap              = (text.horizontalOverflow == HorizontalWrapMode.Wrap);
            bool      raycastTarget     = text.raycastTarget;
            bool      supportRichText   = text.supportRichText;
            Vector2   sizeDelta         = text.rectTransform.sizeDelta;

            // 4) Text 컴포넌트 삭제 (Undo 지원)
            Undo.DestroyObjectImmediate(text);

            // 5) TMPUGUI 컴포넌트 추가 (Undo 지원)
            var tmp = Undo.AddComponent<TextMeshProUGUI>(go);

            // 6) TMP 속성 복사
            tmp.font               = defaultFontAsset;
            tmp.fontSharedMaterial = defaultFontAsset.material;
            tmp.text               = content;
            tmp.fontSize           = fontSize;
            tmp.color              = color;
            tmp.alignment          = (TextAlignmentOptions)anchor;
            tmp.textWrappingMode   = wrap
                                     ? TextWrappingModes.Normal
                                     : TextWrappingModes.NoWrap;
            tmp.raycastTarget      = raycastTarget;
            tmp.richText           = supportRichText;
            tmp.rectTransform.sizeDelta = sizeDelta;

            count++;
        }

        Debug.Log($"[TextToTMP] 변환 완료: {count}개의 Text → TextMeshProUGUI");
    }

    // 프로젝트 내 첫 TMP_FontAsset을 찾아서 반환
    private static TMP_FontAsset GetDefaultFontAsset()
    {
        if (defaultFontAsset != null) 
            return defaultFontAsset;

        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            defaultFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        }
        return defaultFontAsset;
    }
}
