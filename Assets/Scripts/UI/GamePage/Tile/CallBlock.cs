using UnityEngine;
using System.Collections.Generic;
using MCRGame.Common;
using System;

namespace MCRGame.UI
{
    public class CallBlock : MonoBehaviour
    {
        public CallBlockType type;
        public GameTile firstTile;
        public RelativeSeat sourceSeat;
        public int sourceTileIndex;
        public List<GameObject> tiles = new List<GameObject>();

        /// <summary>
        /// CallBlock을 초기화하여 타일들을 생성 및 배치합니다.
        /// 타일을 오른쪽에서 왼쪽 순서로 놓되, 각 타일의 '오른쪽 모서리'를 기준으로 배치합니다.
        /// 회전된 타일도 정확히 오른쪽이 offsetX에 맞춰지므로, 왼쪽으로 밀리는 문제를 해결합니다.
        /// </summary>
        public void InitializeCallBlock()
        {
            // 기존 자식 타일 제거
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            tiles.Clear();

            // CHII, PUNG는 3개, 그 외(KONG)는 4개
            int tileCount = (type == CallBlockType.CHII || type == CallBlockType.PUNG) ? 3 : 4;

            List<int> chiiIndex = new List<int> { sourceTileIndex };
            for (int i = 0; i < 3; ++i)
            {
                if (i == sourceTileIndex)
                {
                    continue;
                }
                chiiIndex.Add(i);
            }

            // 1) 타일 생성 (기본 회전 없음)
            for (int i = 0; i < tileCount; i++)
            {
                GameTile tileValue = firstTile;
                if (type == CallBlockType.CHII)
                {
                    tileValue = (GameTile)((int)firstTile + chiiIndex[i]);
                }

                if (Tile3DManager.Instance == null)
                {
                    Debug.LogError("Tile3DManager 인스턴스가 없습니다.");
                    return;
                }

                // 3D 타일 생성
                GameObject tileObj = Tile3DManager.Instance.Make3DTile(tileValue.ToCustomString(), transform);
                if (tileObj == null)
                {
                    Debug.LogError($"타일 생성 실패: {tileValue.ToCustomString()}");
                    continue;
                }

                // 우선 (0,0,0)에 두고, 회전은 나중에 결정
                tileObj.transform.localPosition = Vector3.zero;
                tileObj.transform.localRotation = Quaternion.identity;

                tiles.Add(tileObj);
            }
            tiles.Reverse();
            // 2) 회전할 타일 인덱스 결정 후 90도 회전
            int rotateIndex = -1;
            switch (sourceSeat)
            {
                case RelativeSeat.KAMI:
                    rotateIndex = tiles.Count - 1;
                    break;
                case RelativeSeat.TOI:
                    rotateIndex = tiles.Count - 2;
                    break;
                case RelativeSeat.SHIMO:
                    rotateIndex = 0;
                    break;
                case RelativeSeat.SELF:
                    rotateIndex = -1;
                    break;
            }
            if (rotateIndex >= 0 && rotateIndex < tiles.Count)
            {
                tiles[rotateIndex].transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }

            // 3) 오른쪽에서 왼쪽 순으로 배치
            //    (offsetX에 '오른쪽 모서리'를 맞추고, 아래쪽(minLocal.y)을 y=0으로 정렬)
            float offsetX = 0f;

            for (int i = 0; i < tileCount; i++)
            {
                GameObject tile = tiles[i];
                if (tile == null) continue;

                // 일단 대략 offsetX 근처에 둔다 (정확 위치는 Renderer 측정 후 조정)
                tile.transform.localPosition = new Vector3(offsetX, 0f, 0f);

                // Renderer로부터 bounding box 가져오기
                Renderer rend = tile.GetComponent<Renderer>();
                if (rend == null)
                {
                    // Renderer가 없으면 대충 1만큼 폭 가정
                    offsetX -= 1f;
                    continue;
                }

                // ---- [A] 오른쪽 모서리 맞추기 ----
                // (1) 현재 월드 좌표의 min, max
                Vector3 minWorld = rend.bounds.min;
                Vector3 maxWorld = rend.bounds.max;
                // (2) 부모 기준 local 좌표로 변환
                Vector3 minLocal = transform.InverseTransformPoint(minWorld);
                Vector3 maxLocal = transform.InverseTransformPoint(maxWorld);
                // (3) 타일의 '오른쪽'이 offsetX에 맞도록 x축 이동
                float deltaX = offsetX - maxLocal.x;
                tile.transform.localPosition += new Vector3(deltaX, 0f, 0f);

                // ---- [B] 아래쪽 바닥 맞추기 ----
                // 다시 minWorld/minLocal 갱신
                minWorld = rend.bounds.min;
                minLocal = transform.InverseTransformPoint(minWorld);
                // 바닥(y=0)에 맞추기
                float deltaY = 0f - minLocal.y;
                tile.transform.localPosition += new Vector3(0f, deltaY, 0f);

                // ---- [C] 폭만큼 offsetX 이동 (왼쪽으로 -=)
                // 마지막으로 bounding box 갱신 후 폭 계산
                minWorld = rend.bounds.min;
                maxWorld = rend.bounds.max;
                minLocal = transform.InverseTransformPoint(minWorld);
                maxLocal = transform.InverseTransformPoint(maxWorld);

                float widthLocal = maxLocal.x - minLocal.x;
                offsetX -= widthLocal;
            }
        }
    }
}
