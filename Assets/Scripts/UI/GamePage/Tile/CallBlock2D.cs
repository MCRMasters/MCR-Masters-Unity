// Assets/Scripts/UI/CallBlock2D.cs
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using MCRGame.Common;
using UnityEngine.Animations;

namespace MCRGame.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class CallBlock2D : MonoBehaviour
    {
        public CallBlockData Data { get; private set; }
        public List<RectTransform> Tiles { get; private set; } = new List<RectTransform>();

        [Tooltip("타일 간 간격")]
        public float tileSpacing = 0f;

        /// <summary>
        /// 블록 전체 가로 폭 (ArrangeTiles() 실행 후 설정됨)
        /// </summary>
        public float TotalWidth { get; private set; }

        private RectTransform _rt;

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            _rt.pivot = new Vector2(2, 0);
            _rt.anchorMin = new Vector2(1, 0);
            _rt.anchorMax = new Vector2(1, 0);
            var rt_parent = transform.parent.GetComponent<RectTransform>();
            _rt.sizeDelta = rt_parent.sizeDelta;
            _rt.localScale = rt_parent.localScale;
        }

        /// <summary>
        /// 최초 생성 또는 SHOMIN_KONG 이후 초기화
        /// </summary>
        public void Initialize(CallBlockData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            ClearTiles();
            CreateTiles();
            RotateSourceTile();
            ArrangeTiles();
        }

        private void ClearTiles()
        {
            foreach (Transform c in transform) Destroy(c.gameObject);
            Tiles.Clear();
        }

        private void CreateTiles()
        {
            // CHII/PUNG는 3장, 그 외(칸)는 4장
            int count = (Data.Type == CallBlockType.CHII || Data.Type == CallBlockType.PUNG) ? 3 : 4;
            var chiiIndices = new List<int> { Data.SourceTileIndex };
            if (Data.Type == CallBlockType.CHII)
                for (int i = 0; i < 3; i++)
                    if (i != Data.SourceTileIndex) chiiIndices.Add(i);

            // CallBlock 높이를 기준 삼아 타일 높이를 통일
            float blockHeight = _rt.rect.height;

            for (int i = 0; i < count; i++)
            {
                GameTile tv = (Data.Type == CallBlockType.CHII)
                    ? (GameTile)((int)Data.FirstTile + chiiIndices[i])
                    : Data.FirstTile;

                // Tile2DManager에서 스프라이트만 받아옴
                Sprite sprite = Tile2DManager.Instance.get_sprite_by_name(tv.ToCustomString());
                if (sprite == null) continue;

                // 새 GameObject에 Image 컴포넌트만 달아서 생성
                var go = new GameObject($"Tile_{tv}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(_rt, false);

                var img = go.GetComponent<Image>();
                img.sprite = sprite;
                img.preserveAspect = true;

                var tileRt = go.GetComponent<RectTransform>();
                tileRt.pivot = new Vector2(0, 0);
                tileRt.anchorMin = new Vector2(1, 0);
                tileRt.anchorMax = new Vector2(1, 0);

                // 스프라이트 비율에 맞춰 사이즈 설정
                float spriteRatio = sprite.rect.width / sprite.rect.height;
                tileRt.sizeDelta = new Vector2(blockHeight * spriteRatio, blockHeight);
                tileRt.localScale = _rt.localScale;

                Tiles.Add(tileRt);
            }
            Tiles.Reverse();
        }

        private void RotateSourceTile()
        {
            int idx = GetRotateIndex();
            if (idx >= 0 && idx < Tiles.Count)
            {
                Tiles[idx].localRotation = Quaternion.Euler(0, 0, 90f);
            }
        }

        private int GetRotateIndex()
        {
            return Data.SourceSeat switch
            {
                RelativeSeat.KAMI => Tiles.Count - 1,
                RelativeSeat.TOI => Tiles.Count - 2,
                RelativeSeat.SHIMO => 0,
                _ => -1
            };
        }

        private void ArrangeTiles()
        {
            float x = 0f;
            for (int i = 0; i < Tiles.Count; i++)
            {
                var t = Tiles[i];
                float w = t.sizeDelta.x;
                float h = t.sizeDelta.y;

                // 기본 위치
                float posX = -x;

                if (i == GetRotateIndex())
                {
                    posX += w;
                    x += h + tileSpacing;
                }
                else
                {
                    x += w + tileSpacing;
                }
                t.anchoredPosition = new Vector2(posX, 0f);
            }

            TotalWidth = x - tileSpacing;
            // height는 그대로 두고, width만 필요하다면 _rt.sizeDelta.x를 TotalWidth로 바꿔주시면 됩니다.
        }

        /// <summary>
        /// PUNG 블록에 SHOMIN_KONG 효과를 적용합니다.
        /// </summary>
        public void ApplyShominKong()
        {
            if (Data.Type != CallBlockType.PUNG)
            {
                Debug.LogWarning("SHOMIN_KONG은 PUNG 블록에서만 적용됩니다.");
                return;
            }

            int idx = GetRotateIndex();
            if (idx < 0 || idx >= Tiles.Count) return;

            // CallBlock 높이
            float blockHeight = _rt.rect.height;
            GameTile tv = Data.FirstTile;
            Sprite sprite = Tile2DManager.Instance.get_sprite_by_name(tv.ToCustomString());
            if (sprite == null) return;

            // 새 타일(앞면) 생성
            var go = new GameObject($"Tile_{tv}_Kong", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_rt);

            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;

            var rt = go.GetComponent<RectTransform>();
            rt.pivot = Tiles[idx].pivot;
            rt.anchorMin = Tiles[idx].anchorMin;
            rt.anchorMax = Tiles[idx].anchorMax;
            rt.localRotation = Tiles[idx].localRotation;
            rt.localScale = Tiles[idx].localScale;
            rt.sizeDelta = Tiles[idx].sizeDelta;
            

            // 회전된 타일의 높이만큼 Y-offset
            float heightOffset = rt.sizeDelta.x;
            var basePos = Tiles[idx].anchoredPosition;
            rt.anchoredPosition = basePos + Vector2.up * heightOffset;

            Tiles.Add(rt);
            Data.Type = CallBlockType.SHOMIN_KONG;
            // ArrangeTiles();
        }
    }
}
