using UnityEngine;
using System.Collections.Generic;


namespace MCRGame.UI
{
    public class PlayerHand : MonoBehaviour
    {
        /*
        * 앞으로 만들어야할 서버간의 통신 플레이어 손패 타가들은 기본적으로 내거나 후로할때만 강에 표시
        // 실제 데이터(서버에서 받은) 보관
        private List<TileData> handTiles = new List<TileData>();

        // 손패를 화면(또는 3D 공간)에 표시하기 위한 타일 프리팹
        public GameObject tilePrefab;

        // 손패를 배치할 기준점(Transform)
        public Transform handOrigin;

        // 타일 간격
        public float tileSpacing = 1.0f;

        // 타일이 화면상에 나타날 때, 타일 Data를 어떻게 표시할지
        // (예: suit에 따라 다른 모델/스프라이트 사용 등)
        // 여기서는 간단히 Prefab 하나만 사용한다고 가정
        public void DrawTile(TileData newTile)
        {
            // 1) 데이터 추가
            handTiles.Add(newTile);

            // 2) 손패 정렬
            SortMahjongTiles(handTiles);

            // 3) 실제 오브젝트 재배치
            UpdateHandDisplay();
        }

        public void DiscardTile(TileData tileToDiscard)
        {
            // 1) 데이터에서 제거
            handTiles.Remove(tileToDiscard);

            // 2) 손패 정렬 (선택 사항: 버린 뒤에도 유지하려면)
            SortMahjongTiles(handTiles);

            // 3) 실제 오브젝트 재배치
            UpdateHandDisplay();
        }

        // 손패 전체를 다시 그려주는 함수
        private void UpdateHandDisplay()
        {
            // 1) 기존에 표시된 타일 오브젝트가 있다면 모두 정리(또는 풀링)
            foreach (Transform child in handOrigin)
            {
                Destroy(child.gameObject);
            }

            // 2) handTiles를 순회하면서 순서대로 Instantiate
            for (int i = 0; i < handTiles.Count; i++)
            {
                Vector3 pos = handOrigin.position + new Vector3(tileSpacing * i, 0f, 0f);
                // 필요하다면 suit/타입에 따라 다른 prefab 선택 가능
                GameObject tileObj = Instantiate(tilePrefab, pos, Quaternion.identity, handOrigin);

                // TileController(또는 TileView) 컴포넌트를 통해 
                // 클릭 이벤트 및 타일 정보 표시
                var tileController = tileObj.GetComponent<TileController>();
                if (tileController != null)
                {
                    tileController.SetTileData(handTiles[i], this);
                }
            }
        }

        private void SortMahjongTiles(List<TileData> tiles)
        {
            tiles.Sort((a, b) =>
            {
                // suit 우선순위
                int suitOrderA = GetSuitOrder(a.suit);
                int suitOrderB = GetSuitOrder(b.suit);

                int compareSuit = suitOrderA.CompareTo(suitOrderB);
                if (compareSuit != 0) return compareSuit;

                // 같은 문양이면 숫자로 비교
                return a.value.CompareTo(b.value);
            });
        }

        private int GetSuitOrder(string suit)
        {
            switch (suit)
            {
                case "manzu": return 0;
                case "pinzu": return 1;
                case "souzu": return 2;
                default: return 3; // 특수패가 있다면 여기서 처리
            }
        }

        */
    }
}