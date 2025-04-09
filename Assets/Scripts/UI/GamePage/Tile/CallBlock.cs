using UnityEngine;
using System.Collections.Generic;
using MCRGame.Common;
using System;

namespace MCRGame.UI
{
    public class CallBlock : MonoBehaviour
    {
        // MCRGame.Common.CallBlockData를 통해 데이터 관리
        public CallBlockData Data;
        public List<GameObject> Tiles = new List<GameObject>();

        /// <summary>
        /// CallBlockData를 기반으로 CallBlock을 초기화하여 타일들을 생성 및 배치합니다.
        /// 타일을 오른쪽에서 왼쪽 순서로 놓되, 각 타일의 '오른쪽 모서리'를 기준으로 배치합니다.
        /// 회전된 타일도 정확히 오른쪽이 offsetX에 맞춰지므로, 왼쪽으로 밀리는 문제를 해결합니다.
        /// </summary>
        public void InitializeCallBlock()
        {
            if (Data == null)
            {
                Debug.LogError("CallBlockData가 설정되지 않았습니다.");
                return;
            }

            ClearExistingTiles();
            CreateTiles();
            RotateSourceTile();
            ArrangeTiles();
        }

        private void ClearExistingTiles()
        {
            // 기존 자식 타일 제거
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            Tiles.Clear();
        }

        private void CreateTiles()
        {
            int tileCount = GetTileCount();
            List<int> chiiIndices = GetChiiIndices();

            for (int i = 0; i < tileCount; i++)
            {
                GameTile tileValue = GetTileValueForIndex(i, chiiIndices);
                GameObject tileObj = Create3DTile(tileValue);
                if (tileObj != null)
                {
                    tileObj.transform.localPosition = Vector3.zero;
                    tileObj.transform.localRotation = Quaternion.identity;
                    Tiles.Add(tileObj);
                }
            }
            Tiles.Reverse();
        }

        private int GetTileCount()
        {
            return (Data.Type == CallBlockType.CHII || Data.Type == CallBlockType.PUNG) ? 3 : 4;
        }

        private List<int> GetChiiIndices()
        {
            List<int> chiiIndices = new List<int> { Data.SourceTileIndex };
            for (int i = 0; i < 3; ++i)
            {
                if (i == Data.SourceTileIndex) continue;
                chiiIndices.Add(i);
            }
            return chiiIndices;
        }

        private GameTile GetTileValueForIndex(int index, List<int> chiiIndices)
        {
            if (Data.Type == CallBlockType.CHII)
            {
                return (GameTile)((int)Data.FirstTile + chiiIndices[index]);
            }
            return Data.FirstTile;
        }

        private GameObject Create3DTile(GameTile tileValue)
        {
            if (Tile3DManager.Instance == null)
            {
                Debug.LogError("Tile3DManager 인스턴스가 없습니다.");
                return null;
            }

            GameObject tileObj = Tile3DManager.Instance.Make3DTile(tileValue.ToCustomString(), transform);
            if (tileObj == null)
            {
                Debug.LogError($"타일 생성 실패: {tileValue.ToCustomString()}");
            }
            return tileObj;
        }

        private void RotateSourceTile()
        {
            int rotateIndex = GetRotateIndex();
            if (rotateIndex >= 0 && rotateIndex < Tiles.Count)
            {
                Tiles[rotateIndex].transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
        }

        private int GetRotateIndex()
        {
            switch (Data.SourceSeat)
            {
                case RelativeSeat.KAMI: return Tiles.Count - 1;
                case RelativeSeat.TOI: return Tiles.Count - 2;
                case RelativeSeat.SHIMO: return 0;
                case RelativeSeat.SELF: return -1;
                default: return -1;
            }
        }

        private void ArrangeTiles()
        {
            float offsetX = 0f;
            for (int i = 0; i < Tiles.Count; i++)
            {
                if (Tiles[i] == null) continue;

                PositionTile(Tiles[i], ref offsetX);
            }
        }

        private void PositionTile(GameObject tile, ref float offsetX)
        {
            // 초기 위치 설정
            tile.transform.localPosition = new Vector3(offsetX, 0f, 0f);

            Renderer rend = tile.GetComponent<Renderer>();
            if (rend == null)
            {
                offsetX -= 1f;
                return;
            }

            AdjustRightEdge(tile, rend, ref offsetX);
            AdjustBottom(tile, rend);
            UpdateOffsetX(tile, rend, ref offsetX);
        }

        private void AdjustRightEdge(GameObject tile, Renderer rend, ref float offsetX)
        {
            Vector3 minWorld = rend.bounds.min;
            Vector3 maxWorld = rend.bounds.max;
            Vector3 minLocal = transform.InverseTransformPoint(minWorld);
            Vector3 maxLocal = transform.InverseTransformPoint(maxWorld);

            float deltaX = offsetX - maxLocal.x;
            tile.transform.localPosition += new Vector3(deltaX, 0f, 0f);
        }

        private void AdjustBottom(GameObject tile, Renderer rend)
        {
            Vector3 minWorld = rend.bounds.min;
            Vector3 minLocal = transform.InverseTransformPoint(minWorld);

            float deltaY = 0f - minLocal.y;
            tile.transform.localPosition += new Vector3(0f, deltaY, 0f);
        }

        private void UpdateOffsetX(GameObject tile, Renderer rend, ref float offsetX)
        {
            Vector3 minWorld = rend.bounds.min;
            Vector3 maxWorld = rend.bounds.max;
            Vector3 minLocal = transform.InverseTransformPoint(minWorld);
            Vector3 maxLocal = transform.InverseTransformPoint(maxWorld);

            float widthLocal = maxLocal.x - minLocal.x;
            offsetX -= widthLocal;
        }
    }
}
