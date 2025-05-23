using UnityEngine;
using UnityEngine.EventSystems;

namespace MCRGame.Game
{
    /// <summary>
    /// Assist 버튼 인터랙션
    /// - PC:  마우스 Hover → Assist 표시
    /// - 모바일(Android/iOS, WebGL-모바일): 터치/클릭 Press&Hold → Assist 표시
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class AssistButtonHover : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        [SerializeField] private TenpaiAssistDisplay display;

        /// <summary>현재 실행 환경이 모바일(또는 터치 지원)인지 캐시</summary>
        private bool isMobile;

        private void Awake()
        {
            // • Application.isMobilePlatform은 Android/iOS,  
            //   WebGL 모바일 브라우저에서도 true  
            isMobile = Application.isMobilePlatform;
        }

        // ────────── PC (Hover) ──────────
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isMobile) return;
            display?.OnAssistButtonEnter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isMobile) return;
            display?.OnAssistButtonExit();
        }

        // ────────── 모바일 (Press & Hold) ──────────
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isMobile) return;
            display?.OnAssistButtonEnter();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isMobile) return;
            display?.OnAssistButtonExit();
        }
    }
}
