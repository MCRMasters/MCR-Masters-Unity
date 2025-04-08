using UnityEngine;
using System.Collections.Generic;
using MCRGame.Common;
using System;

namespace MCRGame.UI
{
    public class CallBlockField : MonoBehaviour
    {
        public List<GameObject> callBlocks;
        private float offset;

        public void AddCallBlock(CallBlockData data)
        {
            // 새 CallBlock 게임오브젝트 생성
            GameObject callBlockObj = new GameObject("CallBlock");
            callBlockObj.transform.SetParent(transform);

            // CallBlock 컴포넌트 추가 및 초기화
            CallBlock callBlock = callBlockObj.AddComponent<CallBlock>();
            callBlock.Data = data;
            callBlock.InitializeCallBlock();

            // 리스트에 추가
            callBlocks.Add(callBlockObj);

            // 위치 조정
            PositionNewCallBlock(callBlockObj);
        }

        private void PositionNewCallBlock(GameObject newCallBlock)
        {
            // 회전은 그대로 둠
            newCallBlock.transform.localRotation = Quaternion.identity;

            // 첫 블록이면 (0,0,0)에 배치
            if (callBlocks.Count == 1)
            {
                newCallBlock.transform.localPosition = Vector3.zero;
                return;
            }

            // gap을 tile 너비의 일정 비율로 설정 (예: 10%)
            float gapRatio = 0.1f;

            // 이전 블록 가져오기 및 유효성 검사
            GameObject prevCallBlockObj = callBlocks[callBlocks.Count - 2];
            CallBlock prevCallBlock = prevCallBlockObj.GetComponent<CallBlock>();
            if (prevCallBlock == null || prevCallBlock.Tiles.Count == 0)
                return;

            // 이전 블록의 첫 타일을 기준으로 너비 측정
            Renderer prevTileRenderer = prevCallBlock.Tiles[0].GetComponent<Renderer>();
            if (prevTileRenderer == null)
                return;
            float tileWidth = prevTileRenderer.bounds.size.x;
            float gap = tileWidth * gapRatio;

            // ----- 이전 블록의 로컬 경계 구하기 -----
            var (prevMin, prevMax) = GetLocalBounds(prevCallBlockObj);

            // ----- 새 블록을 일단 (0,0,0)에 두고, 로컬 경계 구하기 -----
            newCallBlock.transform.localPosition = Vector3.zero;
            var (newMin, newMax) = GetLocalBounds(newCallBlock);

            // "새 블록의 왼쪽(newMin.x)"이 "이전 블록의 오른쪽(prevMax.x)" + gap이 되도록 배치
            float desiredX = prevMax.x + gap;
            float offsetX = desiredX - newMin.x;

            newCallBlock.transform.localPosition += new Vector3(offsetX, 0f, 0f);
        }

        /// <summary>
        /// 해당 CallBlock 오브젝트(및 그 자식 타일들)의
        /// 부모 로컬 좌표 기준 최소/최대 경계점을 구함.
        /// 각 타일의 8개 모서리를 변환하여 보다 정확한 값을 구함.
        /// </summary>
        private (Vector3 min, Vector3 max) GetLocalBounds(GameObject callBlockObj)
        {
            CallBlock cb = callBlockObj.GetComponent<CallBlock>();
            if (cb == null || cb.Tiles.Count == 0)
                return (Vector3.zero, Vector3.zero);

            Vector3 overallMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 overallMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var tile in cb.Tiles)
            {
                Renderer rend = tile.GetComponent<Renderer>();
                if (rend == null)
                    continue;

                Bounds b = rend.bounds;
                // 8개의 모서리 좌표 (월드 좌표)
                Vector3[] corners = new Vector3[8];
                corners[0] = new Vector3(b.min.x, b.min.y, b.min.z);
                corners[1] = new Vector3(b.min.x, b.min.y, b.max.z);
                corners[2] = new Vector3(b.min.x, b.max.y, b.min.z);
                corners[3] = new Vector3(b.min.x, b.max.y, b.max.z);
                corners[4] = new Vector3(b.max.x, b.min.y, b.min.z);
                corners[5] = new Vector3(b.max.x, b.min.y, b.max.z);
                corners[6] = new Vector3(b.max.x, b.max.y, b.min.z);
                corners[7] = new Vector3(b.max.x, b.max.y, b.max.z);

                // 각 모서리를 부모의 로컬 좌표로 변환한 후 최소/최대값 갱신
                foreach (var corner in corners)
                {
                    Vector3 localCorner = transform.InverseTransformPoint(corner);
                    overallMin = Vector3.Min(overallMin, localCorner);
                    overallMax = Vector3.Max(overallMax, localCorner);
                }
            }
            return (overallMin, overallMax);
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
