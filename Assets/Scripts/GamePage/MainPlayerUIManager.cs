using UnityEngine;

public class MainPlayerUIManager : MonoBehaviour
{
    public RectTransform ePlayerPanel;       // EPlayerPanel의 RectTransform
    public GameObject mahjongTileUIPrefab;   // 2D UI 타일 프리팹
    public int tileCount = 13;               // 생성할 타일 수
    public float tileSpacing = 25f;          // 타일 간 간격(px) - 25로 수정

    void Start()
    {
        PlaceTilesUI();
    }

    void PlaceTilesUI()
    {
        // 타일 전체 폭 계산: (tileCount - 1) * tileSpacing
        float totalWidth = (tileCount - 1) * tileSpacing;
        // 중앙 기준으로 왼쪽/오른쪽으로 배치
        float startX = -totalWidth / 2f;

        for (int i = 0; i < tileCount; i++)
        {
            // 1) UI 타일 생성 (부모: ePlayerPanel)
            GameObject newTile = Instantiate(mahjongTileUIPrefab, ePlayerPanel);

            // 2) RectTransform 설정
            RectTransform tileRect = newTile.GetComponent<RectTransform>();

            // 3) anchoredPosition 설정 (Panel 중심 기준)
            float xPos = startX + i * tileSpacing;
            tileRect.anchoredPosition = new Vector2(xPos, 0f);

            // 필요하면 크기, 스프라이트 변경, 클릭 이벤트 연결 등 추가
        }
    }
}
