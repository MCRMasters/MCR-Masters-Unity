using UnityEngine;
using System.Collections.Generic;
using MCRGame.Common;
using System;

namespace MCRGame.UI
{
    public class CallBlock : MonoBehaviour
    {
        public CallBlockData Data;
        public List<GameObject> Tiles = new List<GameObject>();

        // SHOMIN_KONG 적용 시, 회전된 타일 위쪽(로컬 Y 방향)으로 배치할 오프셋 값
        public float shominKongOffset = 0f;

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
                if (Data.Type == CallBlockType.AN_KONG)
                    tileValue = GameTile.Z5;

                GameObject tileObj = Create3DTile(tileValue);
                if (tileObj == null) continue;

                tileObj.transform.localPosition = Vector3.zero;
                tileObj.transform.localRotation = Quaternion.identity;

                if (Data.Type == CallBlockType.AN_KONG)
                {
                    // 뒤집기
                    tileObj.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);

                    // Z축 보정만: bounds 높이(deltaZ) 만큼 앞으로(+) 이동
                    var bounds = GetTileLocalBounds(tileObj);
                    if (bounds.HasValue)
                    {
                        float deltaZ = bounds.Value.max.z - bounds.Value.min.z;
                        Debug.Log($"DeltaZ: {deltaZ}");
                        tileObj.transform.localPosition += new Vector3(0f, 0f, deltaZ);
                    }
                }

                Tiles.Add(tileObj);
            }
        }


        private int GetTileCount()
        {
            // AN_KONG의 경우 4장, 나머지는 CHII,PUNG은 3장, 그 외에는 4장
            if (Data.Type == CallBlockType.AN_KONG)
                return 4;
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


        // 기존의 RotateSourceTile()는 필요에 따라 호출됩니다.
        private void RotateSourceTile()
        {
            int rotateIndex = GetRotateIndex();
            if (rotateIndex >= 0 && rotateIndex < Tiles.Count)
            {
                // 필요하면 해당 타일에 대한 추가 회전을 적용 (AN_KONG은 이미 180도 회전했으므로 건너뛰거나 따로 처리)
                if (Data.Type != CallBlockType.AN_KONG)
                {
                    Tiles[rotateIndex].transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                }
            }
        }

        // PUNG 블록에서 회전된 타일의 인덱스를 반환합니다.
        private int GetRotateIndex()
        {
            switch (Data.SourceSeat)
            {
                case RelativeSeat.KAMI: return 0;
                case RelativeSeat.TOI: return 1;
                case RelativeSeat.SHIMO: return Tiles.Count - 1;
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
            // 초기 localPosition을 offsetX로 설정
            tile.transform.localPosition = new Vector3(offsetX, 0f, tile.transform.localPosition.z);

            // 각 타일의 부모(로컬) 좌표계 내 경계값 계산
            var bounds = GetTileLocalBounds(tile);
            if (bounds == null)
            {
                offsetX -= 1f;
                return;
            }
            Vector3 tileMin = bounds.Value.min;
            Vector3 tileMax = bounds.Value.max;

            AdjustRightEdge(tile, ref offsetX, tileMax);
            AdjustBottom(tile, tileMin);
            UpdateOffsetX(ref offsetX, tileMin, tileMax);
        }

        // 타일의 MeshFilter.mesh.bounds는 타일 자체 로컬 공간의 경계를 제공합니다.
        // 이를 tile의 local TRS 행렬에 곱해 부모의 로컬 좌표 내 코너들을 계산합니다.
        private (Vector3 min, Vector3 max)? GetTileLocalBounds(GameObject tile)
        {
            MeshFilter mf = tile.GetComponent<MeshFilter>();
            if (mf == null) return null;

            Bounds meshBounds = mf.mesh.bounds; // 타일의 로컬 공간 경계
            Vector3 min = meshBounds.min;
            Vector3 max = meshBounds.max;

            // 8개의 모서리 좌표 계산
            Vector3[] corners = new Vector3[8];
            corners[0] = new Vector3(min.x, min.y, min.z);
            corners[1] = new Vector3(min.x, min.y, max.z);
            corners[2] = new Vector3(min.x, max.y, min.z);
            corners[3] = new Vector3(min.x, max.y, max.z);
            corners[4] = new Vector3(max.x, min.y, min.z);
            corners[5] = new Vector3(max.x, min.y, max.z);
            corners[6] = new Vector3(max.x, max.y, min.z);
            corners[7] = new Vector3(max.x, max.y, max.z);

            // tile의 local TRS 행렬 구성 (localPosition, localRotation, localScale 반영)
            Matrix4x4 m = Matrix4x4.TRS(tile.transform.localPosition, tile.transform.localRotation, tile.transform.localScale);
            Vector3 localMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 localMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for (int i = 0; i < 8; i++)
            {
                Vector3 p = m.MultiplyPoint3x4(corners[i]);
                localMin = Vector3.Min(localMin, p);
                localMax = Vector3.Max(localMax, p);
            }
            return (localMin, localMax);
        }

        private void AdjustRightEdge(GameObject tile, ref float offsetX, Vector3 tileMax)
        {
            float deltaX = offsetX - tileMax.x;
            tile.transform.localPosition += new Vector3(deltaX, 0f, 0f);
        }

        private void AdjustBottom(GameObject tile, Vector3 tileMin)
        {
            float deltaY = 0f - tileMin.y;
            tile.transform.localPosition += new Vector3(0f, deltaY, 0f);
        }

        private void UpdateOffsetX(ref float offsetX, Vector3 tileMin, Vector3 tileMax)
        {
            float widthLocal = tileMax.x - tileMin.x;
            offsetX -= widthLocal;
        }

        public void ApplyShominKong()
        {
            Debug.Log("[ApplyShominKong] 시작");

            // 1. 타입 검사
            if (Data.Type != CallBlockType.PUNG)
            {
                Debug.LogWarning($"[ApplyShominKong] 실패: 현재 타입이 PUNG이 아님 (현재: {Data.Type})");
                return;
            }
            Debug.Log("[ApplyShominKong] PUNG 타입 확인");

            // 2. 타일 개수 검사
            Debug.Log($"[ApplyShominKong] 현재 타일 개수: {Tiles.Count}");
            if (Tiles.Count != 3)
            {
                Debug.LogWarning($"[ApplyShominKong] 실패: PUNG 블록은 3장의 타일이어야 합니다. (현재: {Tiles.Count})");
                return;
            }
            Debug.Log("[ApplyShominKong] 타일 개수 3개 확인");

            // 3. 회전 인덱스 계산
            int rotatedIndex = GetRotateIndex();
            Debug.Log($"[ApplyShominKong] 회전될 타일 인덱스: {rotatedIndex}");
            if (rotatedIndex < 0 || rotatedIndex >= Tiles.Count)
            {
                Debug.LogWarning($"[ApplyShominKong] 실패: 유효한 회전 인덱스가 아님 ({rotatedIndex})");
                return;
            }

            // 4. 새 타일 생성
            Debug.Log($"[ApplyShominKong] 새 타일 생성 (기준 타일: {Data.FirstTile})");
            GameObject newTile = Create3DTile(Data.FirstTile);
            if (newTile == null)
            {
                Debug.LogError("[ApplyShominKong] 실패: 새 타일 생성 중 오류");
                return;
            }
            newTile.transform.SetParent(transform, false);
            newTile.transform.localRotation = Tiles[rotatedIndex].transform.localRotation;
            Debug.Log("[ApplyShominKong] 새 타일 생성 및 부모/회전 설정 완료");

            // 5. Y 오프셋 계산
            var bounds = GetTileLocalBounds(Tiles[rotatedIndex]);
            float yOffset;
            if (bounds.HasValue)
            {
                yOffset = bounds.Value.max.y - bounds.Value.min.y;
                Debug.Log($"[ApplyShominKong] 경계 계산 성공: yOffset = {yOffset:F3}");
            }
            else
            {
                yOffset = shominKongOffset;
                Debug.LogWarning($"[ApplyShominKong] 경계 계산 실패, 기본 offset 사용: {yOffset:F3}");
            }

            // 6. 위치 조정
            Vector3 basePos = Tiles[rotatedIndex].transform.localPosition;
            Vector3 targetPos = basePos + new Vector3(0f, yOffset, 0f);
            newTile.transform.localPosition = targetPos;
            Debug.Log($"[ApplyShominKong] 새 타일 위치 설정: basePos={basePos}, targetPos={targetPos}");

            // 7. 리스트 및 타입 업데이트
            Tiles.Add(newTile);
            Debug.Log($"[ApplyShominKong] Tiles 리스트에 추가 (새 개수: {Tiles.Count})");

            Data.Type = CallBlockType.SHOMIN_KONG;
            Debug.Log($"[ApplyShominKong] CallBlock 타입 업데이트: {CallBlockType.PUNG} → {Data.Type}");

            var field = GetComponentInParent<CallBlockField>();
            if (field != null)
                field.RegisterCallBlockTile(Data.FirstTile, newTile);

            Debug.Log("[ApplyShominKong] 완료");
        }

    }
}
