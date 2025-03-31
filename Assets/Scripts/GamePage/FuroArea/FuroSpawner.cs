using UnityEngine;
using System.Collections.Generic;

namespace MCRGame
{
    public class FuroSpawner : MonoBehaviour
    {
        [Header("Furo Positions (Seat-based)")]
        [SerializeField] private Transform FuroPosition_E;  // seat=0
        [SerializeField] private Transform FuroPosition_S;  // seat=1
        [SerializeField] private Transform FuroPosition_W;  // seat=2
        [SerializeField] private Transform FuroPosition_N;  // seat=3

        /// <summary>
        /// 예: furoType="chii", seat=0(East), tileString="234m"
        /// → FuroPosition_E 아래에 "2m", "3m", "4m" 타일 프리팹을 TileLoader를 통해 가져와 일정 간격으로 배치합니다.
        /// </summary>
        public void SpawnFuro(string furoType, int seat, string tileString)
        {
            Debug.Log($"[FuroSpawner] SpawnFuro called: furoType={furoType}, seat={seat}, tileString={tileString}");

            // 기준 오브젝트(FuroPosition_E 등)를 기준으로 후로 그룹 생성
            Transform seatTransform = GetSeatTransform(seat);
            if (seatTransform == null)
            {
                Debug.LogError("[FuroSpawner] Seat transform is null. Check Inspector assignments.");
                return;
            }
            GameObject furoParent = new GameObject($"Furo_{furoType}_{tileString}");
            furoParent.transform.SetParent(seatTransform, false);

            // "234m" → ["2m", "3m", "4m"]
            List<string> tileList = ParseTiles(tileString);
            Debug.Log($"[FuroSpawner] Parsed tiles: {string.Join(", ", tileList)}");

            // 타일 배치: X축 기준 일정 간격 계산
            float spacing = 15f;  // 간격을 1.5로 늘림
            float startX = -(tileList.Count - 1) * spacing / 2f;

            foreach (string tile in tileList)
            {
                // 예: "2m" → value=2, suit="m"
                char valueChar = tile[0];
                int value = valueChar - '0';
                string suit = tile.Substring(1).ToLower();

                // TileLoader를 통해 3D 프리팹 가져오기
                GameObject prefab3D = TileLoader.Instance.Get3DPrefab(suit, value);
                if (prefab3D == null)
                {
                    Debug.LogWarning($"[FuroSpawner] Prefab for tile {tile} not found.");
                    continue;
                }

                // 프리팹 Instantiate 및 배치
                GameObject tileObj = Instantiate(prefab3D, furoParent.transform);
                tileObj.transform.localPosition = new Vector3(startX, 0, 0);
                // 부모의 회전값을 물려받음 (localRotation을 identity로 설정하면 부모의 회전이 적용됨)
                tileObj.transform.localRotation = Quaternion.identity;
                Debug.Log($"[FuroSpawner] Placed tile {tile} at localPosition: {tileObj.transform.localPosition}");

                // 다음 타일 배치를 위해 X 좌표 갱신
                startX += spacing;
            }
        }

        /// <summary>
        /// 입력 문자열 예: "234m"를 ["2m", "3m", "4m"]로 파싱합니다.
        /// </summary>
        private List<string> ParseTiles(string tileString)
        {
            if (string.IsNullOrEmpty(tileString) || tileString.Length < 2)
            {
                Debug.LogWarning("[FuroSpawner] Invalid tile string.");
                return new List<string>();
            }

            char suit = tileString[tileString.Length - 1]; // 예: 'm'
            string ranks = tileString.Substring(0, tileString.Length - 1); // 예: "234"

            List<string> result = new List<string>();
            foreach (char c in ranks)
            {
                result.Add($"{c}{suit}");
            }
            return result;
        }

        /// <summary>
        /// seat 번호에 따른 FuroPosition 반환 (0=E, 1=S, 2=W, 3=N)
        /// </summary>
        private Transform GetSeatTransform(int seat)
        {
            switch (seat)
            {
                case 0: return FuroPosition_E;
                case 1: return FuroPosition_S;
                case 2: return FuroPosition_W;
                case 3: return FuroPosition_N;
                default: return null;
            }
        }
    }
}
