using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace MCRGame.UI
{
    /// <summary>
    /// EmojiPanelController가 필요로 하는 모든 UI/Prefab 레퍼런스를
    /// Inspector 에서 할당해 두는 바인더 스크립트
    /// </summary>
    public class EmojiPanelReferences : MonoBehaviour
    {
        [Header("Buttons & Panels")]
        public Button       OpenButton;         // OpenEmojiButton
        public RectTransform PanelRect;         // EmojiPanel RectTransform
        public Button       CloseButton;        // 패널 닫기 버튼

        [Header("Scroll & Content")]
        public Transform    ContentContainer;   // ScrollRect→Viewport→Content
        public GameObject   EmojiButtonPrefab;  // 이모지 버튼 프리팹

        [Header("Popup")]
        public RectTransform[] PopupAnchors;    //SELF→SHIMO→TOI→KAMI
        public Transform       PopupRoot;       // 캔버스 최상단
        public GameObject      EmojiPopupPrefab;// 팝업 프리팹

        [Header("Animation Settings")]
        public float AnimDuration      = 0.4f;
        public Ease  AnimEase          = Ease.OutBack;
        public float PopupAnimDuration = 0.3f;

        [Header("Hidden / Shown X Positions")]
        public float PanelHiddenX  = 1188.15f;
        public float PanelShownX   = 728.2f;
        public float ButtonHiddenX = 998.5f;
        public float ButtonShownX  = 932.8f;
    }
}
