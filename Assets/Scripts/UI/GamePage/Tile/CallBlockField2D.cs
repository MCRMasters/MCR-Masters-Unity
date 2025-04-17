// Assets/Scripts/UI/CallBlockField2D.cs
using UnityEngine;
using System.Collections.Generic;
using MCRGame.Common;
using System;

namespace MCRGame.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class CallBlockField2D : MonoBehaviour
    {
        [Tooltip("새 블록 간 간격")]
        public float blockSpacing = 0f;

        private List<CallBlock2D> _blocks = new List<CallBlock2D>();
        private RectTransform _rt;

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();

            // 기존 anchoredPosition 저장
            Vector2 prevAnchoredPos = _rt.anchoredPosition;

            // Anchor & Pivot을 우하단으로 설정
            _rt.anchorMin = new Vector2(1, 0);
            _rt.anchorMax = new Vector2(1, 0);
            _rt.pivot = new Vector2(0, 0);

            // 저장해둔 위치로 복원
            // _rt.anchoredPosition = prevAnchoredPos;
        }


        /// <summary>
        /// 필드를 초기화하고 기존 블록들을 제거합니다.
        /// </summary>
        public void InitializeField()
        {
            foreach (var cb in _blocks) Destroy(cb.gameObject);
            _blocks.Clear();
        }

        /// <summary>
        /// 새 콜블록을 추가하고, SHOMIN_KONG은 기존 PUNG 블록에 적용합니다.
        /// </summary>
        public void AddCallBlock(CallBlockData data)
        {
            if (data.Type == CallBlockType.SHOMIN_KONG)
            {
                AddCallBlock(new CallBlockData(type: CallBlockType.PUNG, firstTile: data.FirstTile, sourceSeat: data.SourceSeat, sourceTileIndex: data.SourceTileIndex));
                if (_blocks.Count > 0)
                {
                    _blocks[_blocks.Count - 1].ApplyShominKong();
                }
                return;
            }

            var go = new GameObject("CallBlock2D", typeof(RectTransform));
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            var cb2d = go.AddComponent<CallBlock2D>();
            cb2d.Initialize(data);
            _blocks.Add(cb2d);

            LayoutBlocks();
        }

        private void LayoutBlocks()
        {
            float x = 0f;
            foreach (var cb in _blocks)
            {
                var rt = cb.GetComponent<RectTransform>();
                // 우하단 원점을 기준으로 왼쪽으로 배치
                rt.anchoredPosition = new Vector2(-x, 0f);
                x += cb.TotalWidth + blockSpacing;
            }
        }
    }
}
