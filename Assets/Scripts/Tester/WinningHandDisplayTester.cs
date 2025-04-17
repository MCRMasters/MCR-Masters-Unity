using UnityEngine;
using System.Collections.Generic;
using MCRGame.Common;

namespace MCRGame.Tester
{
    /// <summary>
    /// OnGUI로 승리 핸드를 그려보는 테스트용 컴포넌트입니다.
    /// handTiles 목록에서 winningTile을 제외한 타일들을 먼저,
    /// extraGap 만큼 띄운 뒤 winningTile을 빨간 테두리로 표시합니다.
    /// </summary>
    public class WinningHandDisplayTester : MonoBehaviour
    {
        [Tooltip("타일 이미지를 담은 스프라이트들 (이름은 GameTile.ToCustomString()와 일치)")]
        public List<Sprite> tileSprites;

        [Tooltip("현재 손패 (winningTile 제외한 순서대로)")]
        public List<GameTile> handTiles;

        [Tooltip("승리 타일")]
        public GameTile winningTile;

        [Tooltip("그릴 타일 크기")]
        public float tileSize = 64f;

        [Tooltip("타일 간 기본 간격")]
        public float spacing = 8f;

        [Tooltip("승리 타일 앞 extra 간격")]
        public float extraGap = 24f;

        private Dictionary<string, Texture2D> _cache;

        void Awake()
        {
            // Sprite → Texture2D 캐시
            _cache = new Dictionary<string, Texture2D>();
            foreach (var sp in tileSprites)
            {
                if (sp != null && !_cache.ContainsKey(sp.name))
                    _cache.Add(sp.name, sp.texture);
            }
        }

        void OnGUI()
        {
            float x = 10f;
            float y = 10f;

            // 1) 일반 타일들
            foreach (var tile in handTiles)
            {
                DrawTile(tile, ref x, y);
                x += tileSize + spacing;
            }

            // 2) extra gap
            x += extraGap;

            // 3) 승리 타일 (빨간 테두리)
            var prevColor = GUI.color;
            GUI.color = Color.red;
            DrawTile(winningTile, ref x, y);
            GUI.color = prevColor;
        }

        /// <summary>
        /// GameTile을 화면에 그리고, x를 업데이트합니다.
        /// </summary>
        private void DrawTile(GameTile tv, ref float x, float y)
        {
            string key = tv.ToCustomString();
            if (_cache.TryGetValue(key, out var tex))
            {
                Rect r = new Rect(x, y, tileSize, tileSize);
                GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit);
                x += tileSize;
            }
        }
    }
}
