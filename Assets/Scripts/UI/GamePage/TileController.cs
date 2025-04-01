using UnityEngine;
using UnityEngine.UI;

namespace MCRGame.UI
{
    public class TileController : MonoBehaviour
    {
        public TileData tileData;             // 이 타일의 정보
        public MainPlayerUIManager uiManager; // 생성 시 MainPlayerUIManager로 할당됨

        void Start()
        {
            // Button 컴포넌트가 있으면 클릭 이벤트 등록
            Button btn = GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(OnTileClicked);
            }
        }

        private void OnTileClicked()
        {
            if (uiManager != null)
            {
                uiManager.OnTileClicked(this);
            }
            else
            {
                Debug.LogWarning("uiManager가 TileController에 할당되지 않음: " + gameObject.name);
            }
        }
    }
}