using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MCRGame.Game;

namespace MCRGame.UI
{
    public class TileManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private string tileName;
        private Transform imageField;
        private RectTransform imageFieldRect;
        private Image imageComponent;
        private Vector2 originalPos;

        private GameHandManager gameHandManager;

        private void Awake()
        {
            // 부모에서 GameHandManager 찾아두기
            gameHandManager = GetComponentInParent<GameHandManager>();

            imageField = transform.Find("ImageField");
            if (imageField != null)
            {
                imageFieldRect = imageField.GetComponent<RectTransform>();
                imageComponent = imageField.GetComponent<Image>();
            }
        }

        public void SetTileName(string newName)
        {
            tileName = newName;
            gameObject.name = newName;
            UpdateSprite();
        }

        // Hover 시작
        public void OnPointerEnter(PointerEventData eventData)
        {
            // 애니메이션 중이거나 호버 불가 시 무시
            if (gameHandManager == null || !gameHandManager.CanHover) return;

            float moveUp = imageFieldRect.rect.height / 3f;
            imageFieldRect.anchoredPosition = originalPos + new Vector2(0, moveUp);
        }

        // Hover 끝
        public void OnPointerExit(PointerEventData eventData)
        {
            if (gameHandManager == null || !gameHandManager.CanHover) return;

            imageFieldRect.anchoredPosition = originalPos;
        }

        // 클릭
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (gameHandManager == null || !gameHandManager.CanClick) return;
            gameHandManager.CanClick = false;
            // 서버 검증 요청
            gameHandManager.RequestDiscard(this);
        }

        private void UpdateSprite()
        {
            if (string.IsNullOrEmpty(tileName)) return;
            var sprite = Tile2DManager.Instance?.get_sprite_by_name(tileName);
            if (sprite != null && imageComponent != null)
            {
                imageComponent.sprite = sprite;
                imageComponent.color = Color.white;
            }
        }
        public void UpdateTransparent()
        {
            imageComponent.color = new Color(1f, 1f, 1f, 0f);
        }

        private void Start()
        {
            if (imageFieldRect != null)
                originalPos = imageFieldRect.anchoredPosition;
        }
    }
}
