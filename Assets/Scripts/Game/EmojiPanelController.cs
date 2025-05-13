using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using MCRGame.Common;
using MCRGame.Net;

namespace MCRGame.Game
{
    public class EmojiPanelController : MonoBehaviour
    {
        [Header("Buttons & Panels")]
        [SerializeField] private Button openButton;         // OpenEmojiButton
        [SerializeField] private RectTransform panelRect;   // EmojiPanel RectTransform
        [SerializeField] private Button closeButton;        // 패널 왼쪽 닫기 버튼

        [Header("Scroll & Content")]
        [SerializeField] private Transform contentContainer;   // ScrollRect → Viewport → Content
        [SerializeField] private GameObject emojiButtonPrefab; // Prefab: Button + Image

        [Header("Popup")]
        [SerializeField] private RectTransform[] popupAnchors = new RectTransform[4];
        // [SerializeField] private Vector2 popupOffset     = new Vector2(-320, -150);
        [SerializeField] private Transform popupRoot;          // 캔버스 최상단에 빈 Transform
        [SerializeField] private GameObject emojiPopupPrefab;  // Prefab: Image 단일 (튕겨 나올 이모티콘)

        [Header("Animation Settings")]
        [SerializeField] private float animDuration = 0.4f;
        [SerializeField] private Ease animEase = Ease.OutBack;
        [SerializeField] private float popupAnimDuration = 0.3f;

        [Header("Hidden / Shown X Positions")]
        [SerializeField] private float panelHiddenX = 1188.15f;  // 패널이 숨겨질 때 localX
        [SerializeField] private float panelShownX = 728.2f;  // 패널이 보일 때 localX
        [SerializeField] private float buttonHiddenX = 998.5f;  // openButton 숨길 때 localX
        [SerializeField] private float buttonShownX = 932.8f;  // openButton 보일 때 localX

        // Resources 로드용 경로 (Assets/Resources/Images/CharacterEmoji)
        private const string EmojiPath = "Images/CharacterEmoji";
        private Sprite[] emojiSprites;


        private void Awake()
        {
            // Resources 폴더에서 스프라이트 일괄 로드
            emojiSprites = Resources.LoadAll<Sprite>(EmojiPath);
            Debug.Log($"[EmojiPanel] Loaded {emojiSprites.Length} sprites from Resources/{EmojiPath}");
        }


        private void Start()
        {
            // 1) 패널·버튼 초기 위치
            openButton.transform.localPosition = new Vector3(buttonShownX, openButton.transform.localPosition.y, 0);
            panelRect.localPosition = new Vector3(panelHiddenX, panelRect.localPosition.y, 0);

            // 2) 버튼 리스너
            openButton.onClick.AddListener(ShowPanel);
            closeButton.onClick.AddListener(HidePanel);

            // 3) 이모티콘 버튼 생성 (내 클릭용)
            foreach (var sp in emojiSprites)
            {
                var go = Instantiate(emojiButtonPrefab, contentContainer);
                var img = go.GetComponentInChildren<Image>();
                img.sprite = sp;
                go.GetComponent<Button>().onClick.AddListener(() =>
                {
                    var myAnchor = popupAnchors[(int)RelativeSeat.SELF];
                    ShowPopup(sp, myAnchor);
                    SendEmojiToServer(sp.name);
                });
            }
        }

        private void ShowPanel()
        {
            openButton.interactable = false;
            openButton.transform
                      .DOLocalMoveX(buttonHiddenX, animDuration)
                      .SetEase(animEase);

            panelRect
                .DOLocalMoveX(panelShownX, animDuration)
                .SetEase(animEase)
                .OnComplete(() => closeButton.interactable = true);
        }

        private void HidePanel()
        {
            closeButton.interactable = false;
            panelRect
                .DOLocalMoveX(panelHiddenX, animDuration)
                .SetEase(animEase);
            openButton.transform
                      .DOLocalMoveX(buttonShownX, animDuration)
                      .SetEase(animEase)
                      .OnComplete(() => openButton.interactable = true);
        }

        /// <summary>
        /// 서버에서 이모티콘 키 + 절대좌석 메시지를 받았을 때 호출
        /// </summary>
        public void OnServerEmoji(string emojiKey, AbsoluteSeat seat)
        {
            // 1) 스프라이트 찾기
            var sp = Array.Find(emojiSprites, s => s.name == emojiKey);
            if (sp == null)
            {
                Debug.LogWarning($"[EmojiPanel] Sprite not found for key '{emojiKey}'");
                return;
            }

            // 2) 절대좌석 → 내 기준 상대좌석으로 변환
            var relative = RelativeSeatExtensions.CreateFromAbsoluteSeats(
                currentSeat: GameManager.Instance.MySeat,
                targetSeat: seat
            );

            // 3) 해당 상대좌석의 앵커에서 팝업 띄우기
            var anchor = popupAnchors[(int)relative];
            ShowPopup(sp, anchor);
        }

        /// <summary>
        /// 실제 팝업 생성 + 애니메이션 (앵커 위치 기준)
        /// </summary>
        private void ShowPopup(Sprite sprite, RectTransform anchor)
        {
            // 1) 팝업 인스턴스화
            var popupGO = Instantiate(emojiPopupPrefab, popupRoot);
            var img = popupGO.GetComponentInChildren<Image>();
            img.sprite = sprite;

            // 2) RectTransform 세팅
            var rt = popupGO.GetComponent<RectTransform>();
            // popupRoot와 동일한 캔버스 레이어일 때, anchoredPosition으로 맞춰줍니다
            rt.anchoredPosition = anchor.anchoredPosition;
            rt.localScale = Vector3.one * 0.5f;

            // 3) 애니메이션
            DOTween.Sequence()
                .Append(rt.DOScale(1f, popupAnimDuration).SetEase(Ease.OutBack))
                .AppendInterval(1.0f)
                .Append(rt.DOScale(0f, popupAnimDuration).SetEase(Ease.InBack))
                .OnComplete(() => Destroy(popupGO));
        }

        private void SendEmojiToServer(string emojiKey)
        {
            var payload = new
            {
                emoji_key = emojiKey,
            };
            GameWS.Instance.SendGameEvent(GameWSActionType.EMOJI_SEND, payload);
            Debug.Log($"[EmojiPanel] Sent to server: {emojiKey}");
        }
    }
}
