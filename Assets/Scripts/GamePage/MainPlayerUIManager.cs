using UnityEngine;
using UnityEngine.UI;

public class MainPlayerUIManager : MonoBehaviour
{
    [Header("UI 배치 설정")]
    public RectTransform ePlayerPanel;       // EPlayerPanel의 RectTransform
    public GameObject mahjongTileUIPrefab;   // 2D UI 타일 프리팹
    public int tileCount = 13;               // 생성할 타일 수
    public float tileSpacing = 25f;          // 타일 간 간격(px)

    [Header("Discard Manager 참조")]
    public DiscardManager discardManager;    // 인스펙터에서 직접 할당

    void Start()
    {
        PlaceTilesUI();
    }

    void PlaceTilesUI()
    {
        float totalWidth = (tileCount - 1) * tileSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < tileCount; i++)
        {
            // 1) UI 타일 생성 (부모: ePlayerPanel)
            GameObject newTile = Instantiate(mahjongTileUIPrefab, ePlayerPanel);
            Debug.Log($"[MainPlayerUIManager] Created tile index={i}, name={newTile.name}");

            // 2) RectTransform 설정
            RectTransform tileRect = newTile.GetComponent<RectTransform>();
            float xPos = startX + i * tileSpacing;
            tileRect.anchoredPosition = new Vector2(xPos, 0f);

            // 3) 버튼 클릭 이벤트 연결
            Button btn = newTile.GetComponent<Button>();
            if (btn != null)
            {
                Debug.Log($"[MainPlayerUIManager] Tile index={i} - Found Button. Registering onClick listener.");

                // 람다를 사용해 newTile 참조
                btn.onClick.AddListener(() => OnTileClicked(newTile));



            }
        }
    }

    /// <summary>
    /// 타일이 클릭되면 호출되는 함수
    /// </summary>
    /// <param name="tile">클릭된 타일 오브젝트</param>
    void OnTileClicked(GameObject tile)
    {
        Debug.Log("타일 클릭됨 → 버림패로 이동합니다.");

        // 실제로는 타일마다 다른 TileData가 있어야 하지만,
        // 여기서는 테스트용으로 더미 데이터만 전달
        TileData dummyData = new TileData { suit = "manzu", value = 1 };

        // 인스펙터에서 할당한 DiscardManager 사용
        if (discardManager != null)
        {
            discardManager.DiscardTile(PlayerSeat.E, dummyData);
        }
        else
        {
            Debug.LogWarning("DiscardManager가 인스펙터에 할당되지 않았습니다!");
        }

        // 손패 UI에서 타일 제거
        Destroy(tile);
    }
}
