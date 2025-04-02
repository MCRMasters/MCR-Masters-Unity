using System.Collections.Generic;
using MCRGame.Common;
using UnityEngine;

namespace MCRGame.UI
{
    public class CallBlockField : MonoBehaviour
    {
        public List<GameObject> callBlocks;
        private float offset;

        public void AddCallBlock(CallBlockType type, GameTile firstTile, RelativeSeat sourceSeat, int sourceTileIndex = 0)
        {
            // 새 CallBlock 게임오브젝트 생성
            GameObject callBlockObj = new GameObject("CallBlock");
            callBlockObj.transform.SetParent(transform);

            // CallBlock 컴포넌트 추가 및 초기화
            CallBlock callBlock = callBlockObj.AddComponent<CallBlock>();
            callBlock.type = type;
            callBlock.firstTile = firstTile;
            callBlock.sourceSeat = sourceSeat;
            callBlock.sourceTileIndex = sourceTileIndex;
            callBlock.InitializeCallBlock();

            // 리스트에 추가
            callBlocks.Add(callBlockObj);

            // 위치 조정
            PositionNewCallBlock(callBlockObj);
        }

        /// <summary>
        /// 새로운 CallBlock을 부모의 로컬 좌표계에서 
        /// 이전 블록 옆에 gap만큼 띄워서 배치.
        /// </summary>
        private void PositionNewCallBlock(GameObject newCallBlock)
        {
            float gap = 0f;
            newCallBlock.transform.localRotation = transform.localRotation;

            // 첫 블록은 (0,0,0)에 두고 끝
            if (callBlocks.Count == 1)
            {
                newCallBlock.transform.localPosition = Vector3.zero;
                return;
            }

            // ----- 이전 블록의 로컬 경계 박스 구하기 -----
            GameObject prevCallBlockObj = callBlocks[callBlocks.Count - 2];
            var (prevMin, prevMax) = GetLocalBounds(prevCallBlockObj);

            // ----- 새 블록을 일단 (0,0,0)에 두고, 로컬 경계 박스 구하기 -----
            newCallBlock.transform.localPosition = Vector3.zero;
            var (newMin, newMax) = GetLocalBounds(newCallBlock);

            // 원하는 방향(예: 왼쪽으로 쌓기 / 오른쪽으로 쌓기)에 따라 계산 달라짐.
            // 여기서는 "새 블록을 이전 블록의 오른쪽에 놓는다"고 가정:
            // "새 블록의 min.x = 이전 블록의 max.x + gap"
            float desiredX = prevMax.x + gap;
            float offsetX = desiredX - newMin.x;

            // 새 블록의 localPosition에 offset 적용
            newCallBlock.transform.localPosition += new Vector3(offsetX, 0f, 0f);
        }

        /// <summary>
        /// 해당 CallBlock 오브젝트(및 그 자식 타일들)의
        /// "부모 로컬 좌표" 기준 최소/최대 경계점을 구함
        /// </summary>
        private (Vector3 min, Vector3 max) GetLocalBounds(GameObject callBlockObj)
        {
            CallBlock cb = callBlockObj.GetComponent<CallBlock>();
            if (cb == null || cb.tiles.Count == 0)
                return (Vector3.zero, Vector3.zero);

            Vector3 minVec = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxVec = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // 각 타일의 Renderer.bounds를 월드좌표 -> 로컬좌표로 변환해
            // 전체 최소/최대값을 갱신
            foreach (var tile in cb.tiles)
            {
                Renderer rend = tile.GetComponent<Renderer>();
                if (rend == null) continue;

                Vector3 localMin = transform.InverseTransformPoint(rend.bounds.min);
                Vector3 localMax = transform.InverseTransformPoint(rend.bounds.max);

                // 최소값 갱신
                if (localMin.x < minVec.x) minVec.x = localMin.x;
                if (localMin.y < minVec.y) minVec.y = localMin.y;
                if (localMin.z < minVec.z) minVec.z = localMin.z;

                // 최대값 갱신
                if (localMax.x > maxVec.x) maxVec.x = localMax.x;
                if (localMax.y > maxVec.y) maxVec.y = localMax.y;
                if (localMax.z > maxVec.z) maxVec.z = localMax.z;
            }
            return (minVec, maxVec);
        }

        public void ClearAllCallBlocks()
        {
            foreach (GameObject callBlock in callBlocks)
            {
                Destroy(callBlock);
            }
            callBlocks.Clear();
            offset = 0;
        }
    }
}