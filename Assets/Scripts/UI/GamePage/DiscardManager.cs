using UnityEngine;
using System.Collections;
using MCRGame.Common; // RelativeSeat, GameTile, 등
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

namespace MCRGame.UI
{
    // 기존 PlayerSeat 대신 RelativeSeat를 사용합니다.
    public class DiscardManager : MonoBehaviour
    {
        public Transform discardPosE;
        public Transform discardPosS;
        public Transform discardPosW;
        public Transform discardPosN;

        [Header("타일 간격 설정")]
        public float tileSpacing = 15f;  // 같은 행 내 타일 간격
        public float rowSpacing = 20f;   // 6개마다 다음 행으로 이동하는 간격
        public int maxTilesPerRow = 6;

        // 좌석별 폐기된 타일 UI 오브젝트 목록을 관리하는 변수명을 kawas로 변경
        private Dictionary<RelativeSeat, List<GameObject>> kawas = new Dictionary<RelativeSeat, List<GameObject>>();
        // GameTile별 타일 UI 오브젝트 목록
        private Dictionary<GameTile, List<GameObject>> tileObjectDictionary = new Dictionary<GameTile, List<GameObject>>();

        void Awake()
        {
            // RelativeSeat 4개에 대한 초기화
            kawas[RelativeSeat.SELF] = new List<GameObject>();
            kawas[RelativeSeat.SHIMO] = new List<GameObject>();
            kawas[RelativeSeat.TOI] = new List<GameObject>();
            kawas[RelativeSeat.KAMI] = new List<GameObject>();

            tileObjectDictionary = new Dictionary<GameTile, List<GameObject>>();
        }
        public void DiscardTile(RelativeSeat seat, GameTile tile)
        {
            Transform origin = GetDiscardPosition(seat);

            int index = kawas[seat].Count;
            int row = index / maxTilesPerRow;
            int col = index % maxTilesPerRow;

            Vector3 offset;
            if (seat == RelativeSeat.SELF)
                offset = Vector3.right * (col * tileSpacing) + Vector3.back * (row * rowSpacing);
            else if (seat == RelativeSeat.SHIMO)
                offset = Vector3.forward * (col * tileSpacing) + Vector3.right * (row * rowSpacing);
            else if (seat == RelativeSeat.TOI)
                offset = Vector3.left * (col * tileSpacing) + Vector3.forward * (row * rowSpacing);
            else // KAMI
                offset = Vector3.left * (row * rowSpacing) + Vector3.back * (col * tileSpacing);

            Vector3 newPos = origin.position + offset;
            Quaternion finalRotation = origin.rotation;

            // Make3DTile이 이미 Instantiate 해주므로, 반환된 오브젝트의 위치/회전만 설정
            string prefabTileName = tile.ToCustomString();
            GameObject instantiatedTile = Tile3DManager.Instance.Make3DTile(prefabTileName);
            if (instantiatedTile == null)
            {
                Debug.LogWarning($"3D prefab not found: {tile}");
                return;
            }

            instantiatedTile.transform.position = newPos;
            instantiatedTile.transform.rotation = finalRotation;

            // 좌석별 리스트에 추가
            kawas[seat].Add(instantiatedTile);

            // GameTile별 리스트에 추가
            if (!tileObjectDictionary.TryGetValue(tile, out var list))
            {
                list = new List<GameObject>();
                tileObjectDictionary[tile] = list;
            }
            list.Add(instantiatedTile);
        }


        private Transform GetDiscardPosition(RelativeSeat seat) => seat switch
        {
            RelativeSeat.SELF => discardPosE,
            RelativeSeat.SHIMO => discardPosS,
            RelativeSeat.TOI => discardPosW,
            RelativeSeat.KAMI => discardPosN,
            _ => null,
        };
    }
}
