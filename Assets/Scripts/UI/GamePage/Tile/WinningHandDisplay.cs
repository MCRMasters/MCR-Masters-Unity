using UnityEngine;
using UnityEngine.UI;
using MCRGame.Common;

using System.Collections.Generic;

namespace MCRGame.UI
{
    public class WinningHandDisplay : MonoBehaviour
    {
        [Tooltip("타일 간 기본 간격")]
        public float tileSpacing = 0f;
        [Tooltip("승리 타일과 나머지 타일 사이에 추가로 줄 간격")]
        public float extraGap = 20f;

        /// <summary>
        /// handTiles 중 winningTile 하나만 뒤로 빼고, extraGap 띄워서 화면에 띄운다.
        /// </summary>
        public void ShowWinningHand(WinningScoreData data)
        {
            // 1) 기존 자식들 삭제
            foreach (Transform c in transform) Destroy(c.gameObject);

            // 2) 승리 타일은 따로 빼두고
            GameTile win = data.winningTile;
            bool winFlag = true;
            List<GameTile> others = new List<GameTile>();
            foreach (var t in data.handTiles)
            {
                if (t == win && winFlag){
                    winFlag = false;
                    continue;
                }
                others.Add(t);
            }

            // 3) 순서대로 배치
            float x = 0f;
            float blockHeight = ((RectTransform)transform).rect.height;

            // a) 일반 타일
            foreach (var tile in others)
            {
                x = CreateTileAt(tile, x, blockHeight);
                x += tileSpacing;
            }

            // b) extra gap
            x += extraGap;

            // c) 승리 타일
            CreateTileAt(win, x, blockHeight);
        }

        /// <summary>
        /// 특정 GameTile을 현재 오브젝트의 자식으로 띄우고,
        /// pivot (0,0), anchor(1,0) 기준으로 startX 위치에 배치한 뒤,
        /// 리턴 값으로는 그 타일의 너비만큼 더한 x 값을 돌려준다.
        /// </summary>
        private float CreateTileAt(GameTile tv, float startX, float blockHeight)
        {
            // 1) 스프라이트 가져오기
            Sprite spr = Tile2DManager.Instance.get_sprite_by_name(tv.ToCustomString());
            if (spr == null) return startX;

            // 2) GameObject+RectTransform+Image 직접 생성 (원본 CallBlock2D 방식)
            var go = new GameObject($"Tile_{tv}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);

            var img = go.GetComponent<Image>();
            img.sprite = spr;
            img.preserveAspect = true;

            // 3) RectTransform 세팅
            var rt = go.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0, 0);
            rt.anchorMin = rt.anchorMax = new Vector2(1, 0);
            float ratio = spr.rect.width / spr.rect.height;
            rt.sizeDelta = new Vector2(blockHeight * ratio, blockHeight);

            // 4) 위치 세팅 (pivot 기준: 오른쪽 하단에서 -startX-너비 만큼 왼쪽)
            rt.anchoredPosition = new Vector2(startX + rt.sizeDelta.x, 0);

            // 5) 다음 x 계산
            return startX + rt.sizeDelta.x;
        }
    }
}