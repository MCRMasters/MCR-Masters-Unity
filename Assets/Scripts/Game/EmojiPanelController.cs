using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using MCRGame.Common;
using MCRGame.Net;
using MCRGame.UI;

namespace MCRGame.Game
{
    public class EmojiPanelController : MonoBehaviour
    {
        private Button openButton;
        private RectTransform panelRect;
        private Button closeButton;

        private Transform contentContainer;
        private GameObject emojiButtonPrefab;

        private RectTransform[] popupAnchors;
        private Transform popupRoot;
        private GameObject emojiPopupPrefab;

        private float animDuration;
        private Ease animEase;
        private float popupAnimDuration;

        private float panelHiddenX;
        private float panelShownX;
        private float buttonHiddenX;
        private float buttonShownX;

        private Sprite[] emojiSprites;
        private bool _initialized = false;
        /// <summary>
        /// GameManager에서 한 번만 호출해 모든 의존성을 주입합니다.
        /// </summary>
        public void Initialize(EmojiPanelReferences refs)
        {
            if (_initialized) return;
            _initialized = true;

            // 1) copy references
            openButton = refs.OpenButton;
            panelRect = refs.PanelRect;
            closeButton = refs.CloseButton;
            contentContainer = refs.ContentContainer;
            emojiButtonPrefab = refs.EmojiButtonPrefab;
            popupAnchors = refs.PopupAnchors;
            popupRoot = refs.PopupRoot;
            emojiPopupPrefab = refs.EmojiPopupPrefab;
            animDuration = refs.AnimDuration;
            animEase = refs.AnimEase;
            popupAnimDuration = refs.PopupAnimDuration;
            panelHiddenX = refs.PanelHiddenX;
            panelShownX = refs.PanelShownX;
            buttonHiddenX = refs.ButtonHiddenX;
            buttonShownX = refs.ButtonShownX;

            // 2) now do what used to be in Start()
            SetupUI();
        }

        private void Awake()
        {
            // 이모지 스프라이트만 Resources 폴더에서 즉시 로드
            emojiSprites = Resources.LoadAll<Sprite>("Images/CharacterEmoji");
        }


        private void SetupUI()
        {
            // initial positions
            openButton.transform.localPosition = new Vector3(buttonShownX, openButton.transform.localPosition.y, 0);
            panelRect.localPosition = new Vector3(panelHiddenX, panelRect.localPosition.y, 0);

            // button listeners
            openButton.onClick.AddListener(ShowPanel);
            closeButton.onClick.AddListener(HidePanel);

            // emoji buttons
            foreach (var sp in emojiSprites)
            {
                var go = Instantiate(emojiButtonPrefab, contentContainer);
                var img = go.GetComponentInChildren<Image>();
                img.sprite = sp;
                go.GetComponent<Button>().onClick.AddListener(() =>
                {
                    ShowPopup(sp, popupAnchors[(int)RelativeSeat.SELF]);
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
