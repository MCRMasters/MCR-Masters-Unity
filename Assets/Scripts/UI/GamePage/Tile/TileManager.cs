using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MCRGame.UI
{
    public class TileManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private string tileName; 

        private Transform imageField;
        private RectTransform imageFieldRect;
        private Image imageComponent;

        private Vector2 originalPos;

        private void Awake()
        {
            imageField = transform.Find("ImageField");
            if (imageField != null)
            {
                imageFieldRect = imageField.GetComponent<RectTransform>();
                imageComponent = imageField.GetComponent<Image>();
                if (imageComponent == null)
                {
                    Debug.LogWarning("[TileManager] 'ImageField' 오브젝트에 Image 컴포넌트가 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("[TileManager] 자식 오브젝트 'ImageField'를 찾을 수 없습니다.");
            }
        }

        private void Start()
        {
            if (imageFieldRect != null)
            {
                originalPos = imageFieldRect.anchoredPosition;
            }

            UpdateSprite();
        }

        public void SetTileName(string newName)
        {
            tileName = newName;
            gameObject.name = newName;
            UpdateSprite();
        }

        private void UpdateSprite()
        {
            if (string.IsNullOrEmpty(tileName))
            {
                Debug.LogWarning("[TileManager] tileName이 비어 있습니다.");
                return;
            }

            if (Tile2DManager.Instance == null)
            {
                Debug.LogWarning("[TileManager] TileImageManager.Instance가 없습니다.");
                return;
            }

            Sprite foundSprite = Tile2DManager.Instance.get_sprite_by_name(tileName);
            if (foundSprite == null)
            {
                Debug.LogWarning($"[TileManager] '{tileName}' 스프라이트를 찾을 수 없습니다.");
                return;
            }

            if (imageComponent != null)
            {
                imageComponent.sprite = foundSprite;
                imageComponent.color = Color.white;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (imageFieldRect == null) return;

            float moveUp = imageFieldRect.rect.height / 3f;
            imageFieldRect.anchoredPosition = originalPos + new Vector2(0, moveUp);

            //Debug.Log($"[TileManager] 마우스가 '{tileName}' 위로 올라감 -> ImageField 이동");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (imageFieldRect == null) return;

            imageFieldRect.anchoredPosition = originalPos;

            //Debug.Log($"[TileManager] 마우스가 '{tileName}' 영역에서 벗어남 -> ImageField 원위치");
        }
        
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            
            // 부모 GameHandManager 컴포넌트에 접근하여 DiscardTile(this)를 호출합니다.
            GameHandManager gameHandManager = GetComponentInParent<GameHandManager>();
            if (gameHandManager != null)
            {
                gameHandManager.DiscardTile(this);
            }
            else
            {
                Debug.LogWarning("[TileManager] 부모 GameHandManager를 찾을 수 없습니다.");
            }
        }
    }
}
