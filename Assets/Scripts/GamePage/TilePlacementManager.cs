using UnityEngine;

namespace MCRGame
{
    public class TilePlacementManager : MonoBehaviour
    {
        public GameObject mahjongTilePrefab; // 마작 패 Prefab
        public int tileCount = 13;           // 생성할 타일 수
        public float tileSpacing = 1f;       // 타일 간 간격 (원하는 값으로 조절)

        // 각 방향별 기준이 될 Transform
        public Transform tilePositionN;      // 북쪽
        public Transform tilePositionE;      // 동쪽
        public Transform tilePositionS;      // 남쪽
        public Transform tilePositionW;      // 서쪽

        void Start()
        {
            // 북(N): z축 -방향으로 나열
            PlaceTiles(tilePositionN, Vector3.back);

            // 동(E): x축 +방향으로 나열
            PlaceTiles(tilePositionE, Vector3.right);

            // 남(S): z축 +방향으로 나열
            PlaceTiles(tilePositionS, Vector3.forward);

            // 서(W): x축 -방향으로 나열
            PlaceTiles(tilePositionW, Vector3.left);
        }

        /// <summary>
        /// 기준 Transform(origin)에서 시작해, direction 방향으로 tileCount만큼 타일 생성
        /// </summary>
        void PlaceTiles(Transform origin, Vector3 direction)
        {
            for (int i = 0; i < tileCount; i++)
            {
                // 타일이 생성될 위치
                Vector3 newPosition = origin.position + direction * tileSpacing * i;

                // 회전은 필요에 따라 origin.rotation, 혹은 추가 Euler 회전 적용
                Quaternion newRotation = origin.rotation;

                // 타일 생성
                Instantiate(mahjongTilePrefab, newPosition, newRotation);
            }
        }
    }
}