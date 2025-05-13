using UnityEngine;
using System.Collections;
using MCRGame.Common; // RelativeSeat, GameTile, 등
using System.Collections.Generic;
using System;
using MCRGame.Game;
using UnityEngine.Rendering;

namespace MCRGame.UI
{
    public class DiscardManager : MonoBehaviour
    {
        public Transform discardPosSELF;
        public Transform discardPosSHIMO;
        public Transform discardPosTOI;
        public Transform discardPosKAMI;

        [Header("타일 간격 설정")]
        public float tileSpacing = 14f;
        public float rowSpacing = 20f;
        public int maxTilesPerRow = 6;

        [Header("Discard Animation Settings")]
        public float dropHeight = 30f;            // 위에서 얼마나 높이 시작할지
        public float fadeDuration = 0.05f;        // 투명→불투명 페이드 시간
        public float dropDuration = 0.15f;        // 낙하 애니메이션 시간
        public float extraRowOffset = 1f;         // 대각선 뒤로 밀릴 추가 row 단위

        private Dictionary<RelativeSeat, List<GameObject>> kawas = new Dictionary<RelativeSeat, List<GameObject>>();
        private Dictionary<GameTile, List<GameObject>> tileObjectDictionary = new Dictionary<GameTile, List<GameObject>>();


        private readonly List<(Material mat, Color original)> _highlightedMats
            = new List<(Material, Color)>();

        void Awake()
        {
            kawas[RelativeSeat.SELF] = new List<GameObject>();
            kawas[RelativeSeat.SHIMO] = new List<GameObject>();
            kawas[RelativeSeat.TOI] = new List<GameObject>();
            kawas[RelativeSeat.KAMI] = new List<GameObject>();
        }

        public void InitRound()
        {
            foreach (var kvp in kawas)
            {
                List<GameObject> list = kvp.Value;
                foreach (var go in list)
                {
                    if (go != null)
                        Destroy(go);
                }
                list.Clear();
            }
            tileObjectDictionary.Clear();
            _highlightedMats.Clear();
        }

        /// <summary>
        /// 현재 GameManager에 기록된 hover 타일과 같다면 하이라이트를 다시 적용합니다.
        /// </summary>
        private void HighlightIfHovering(GameTile tile)
        {
            if (GameManager.Instance.NowHoverTile.HasValue
                && GameManager.Instance.NowHoverTile.Value == tile)
            {
                HighlightTiles(tile);
            }
        }



        /// tile과 일치하는 discard 타일을 하늘색으로 하이라이트
        public void HighlightTiles(GameTile tile)
        {
            ClearHighlights();   // 이전 것 복원

            if (!tileObjectDictionary.TryGetValue(tile, out var goList))
                return;

            foreach (var go in goList)
            {
                foreach (var r in go.GetComponentsInChildren<Renderer>())
                {
                    foreach (var mat in r.materials)
                    {
                        if (!mat.HasProperty("_Color")) continue;

                        // ── (1) 원본 색 저장
                        _highlightedMats.Add((mat, mat.color));

                        // ── (2) 하이라이트 색 적용
                        Color c = mat.color;
                        mat.color = new Color(0.7f, 0.9f, 1f, c.a);   // 연한 하늘색
                    }
                }
            }
        }

        /// 하이라이트 해제 → 원래 색 복원
        public void ClearHighlights()
        {
            foreach (var (mat, orig) in _highlightedMats)
            {
                if (mat == null) continue;
                if (mat.HasProperty("_Color"))
                    mat.color = orig;          // 저장해 둔 색으로 되돌림
            }
            _highlightedMats.Clear();
        }

        public void ReloadAllDiscards(List<List<GameTile>> allTilesBySeat)
        {
            // 1) 기존 오브젝트 전부 삭제 & 리스트 초기화
            foreach (var kvp in kawas)
            {
                var goList = kvp.Value;
                foreach (var go in goList)
                    if (go != null) Destroy(go);
                goList.Clear();
            }
            tileObjectDictionary.Clear();

            // 2) 각 좌석별로 받은 타일 리스트 즉시 배치
            foreach (RelativeSeat seat in Enum.GetValues(typeof(RelativeSeat)))
            {
                int seatIdx = (int)seat;
                if (seatIdx < 0 || seatIdx >= allTilesBySeat.Count)
                    continue;

                var tiles = allTilesBySeat[(int)RelativeSeatExtensions.ToAbsoluteSeat(rel: seat, mySeat: GameManager.Instance.MySeat)];
                Transform origin = GetDiscardPosition(seat);
                if (origin == null)
                {
                    Debug.LogWarning($"ReloadAllDiscards: discard position for {seat} is null");
                    continue;
                }

                for (int i = 0; i < tiles.Count; i++)
                {
                    var tile = tiles[i];
                    int row = i / maxTilesPerRow;
                    int col = i % maxTilesPerRow;

                    Vector3 offset = ComputeOffset(seat, col, row);
                    Vector3 finalPos = origin.position + offset;
                    Quaternion finalRot = origin.rotation;

                    // 3D 타일 생성
                    string prefabName = tile.ToCustomString();
                    var go = Tile3DManager.Instance.Make3DTile(prefabName);
                    if (go == null)
                    {
                        Debug.LogWarning($"ReloadAllDiscards: prefab not found for {tile}");
                        continue;
                    }

                    // 부모 설정 및 위치/회전 즉시 적용
                    go.transform.SetParent(origin, true);
                    go.transform.position = finalPos;
                    go.transform.rotation = finalRot;

                    // 머티리얼을 Opaque, Alpha=1 로
                    var rends = go.GetComponentsInChildren<Renderer>();
                    foreach (var r in rends)
                        foreach (var mat in r.materials)
                        {
                            if (mat.HasProperty("_Color"))
                            {
                                Color c = mat.color;
                                c.a = 1f;
                                mat.color = c;
                            }
                            SetMaterialOpaque(mat);
                        }

                    // 내부 데이터에 추가
                    kawas[seat].Add(go);
                    if (!tileObjectDictionary.TryGetValue(tile, out var list))
                    {
                        list = new List<GameObject>();
                        tileObjectDictionary[tile] = list;
                    }
                    list.Add(go);
                }
            }
        }

        public void RemoveLastDiscard(RelativeSeat seat)
        {
            // kawas에서 해당 좌석의 폐기 타일 리스트 가져오기
            if (!kawas.TryGetValue(seat, out List<GameObject> discardList) || discardList.Count == 0)
            {
                Debug.LogWarning($"RemoveLastDiscard: No discarded tile for seat {seat}");
                return;
            }

            // 리스트의 마지막 요소(가장 최근에 추가된 타일)를 가져오기
            int lastIndex = discardList.Count - 1;
            GameObject lastDiscard = discardList[lastIndex];
            discardList.RemoveAt(lastIndex);

            // 타일 이름을 이용해 GameTile으로 파싱 (미리 prefabTileName으로 설정했던 값)
            string tileName = lastDiscard.name;
            if (GameTileExtensions.TryParseCustom(tileName, out GameTile tile))
            {
                // tileObjectDictionary 업데이트: 해당 GameTile에 해당하는 List<GameObject>에서 삭제
                if (tileObjectDictionary.TryGetValue(tile, out List<GameObject> tileList))
                {
                    bool removed = tileList.Remove(lastDiscard);
                    if (!removed)
                    {
                        Debug.LogWarning($"RemoveLastDiscard: Could not remove tile {tile} from dictionary list");
                    }
                }
                else
                {
                    Debug.LogWarning($"RemoveLastDiscard: tileObjectDictionary does not contain tile {tile}");
                }
            }
            else
            {
                Debug.LogWarning($"RemoveLastDiscard: Cannot parse GameTile from tile name '{tileName}'");
            }

            // 마지막 타일 Destroy 처리
            Destroy(lastDiscard);
        }

        public void DiscardTile(RelativeSeat seat, GameTile tile)
        {
            Transform origin = GetDiscardPosition(seat);

            int index = kawas[seat].Count;
            int row = index / maxTilesPerRow;
            int col = index % maxTilesPerRow;

            // 최종 위치 오프셋
            Vector3 offset = ComputeOffset(seat, col, row);
            Vector3 finalPos = origin.position + offset;
            Quaternion finalRot = origin.rotation;

            // 타일 생성
            string prefabTileName = tile.ToCustomString();
            GameObject instantiatedTile = Tile3DManager.Instance.Make3DTile(prefabTileName);
            if (instantiatedTile == null)
            {
                Debug.LogWarning($"3D prefab not found: {tile}");
                return;
            }

            // ★ 부모를 discardPos의 자식으로 설정
            instantiatedTile.transform.SetParent(origin, true);

            // 리스트에 추가
            kawas[seat].Add(instantiatedTile);
            if (!tileObjectDictionary.TryGetValue(tile, out var list))
            {
                list = new List<GameObject>();
                tileObjectDictionary[tile] = list;
            }
            list.Add(instantiatedTile);

            HighlightIfHovering(tile);

            // 애니메이션 코루틴 시작
            StartCoroutine(AnimateDiscard(instantiatedTile, seat, col, row, origin, finalPos, finalRot));
        }

        private Vector3 ComputeOffset(RelativeSeat seat, int col, int row)
        {
            return seat switch
            {
                RelativeSeat.SELF => Vector3.right * (col * tileSpacing) + Vector3.back * (row * rowSpacing),
                RelativeSeat.SHIMO => Vector3.forward * (col * tileSpacing) + Vector3.right * (row * rowSpacing),
                RelativeSeat.TOI => Vector3.left * (col * tileSpacing) + Vector3.forward * (row * rowSpacing),
                RelativeSeat.KAMI => Vector3.left * (row * rowSpacing) + Vector3.back * (col * tileSpacing),
                _ => Vector3.zero,
            };
        }
        private IEnumerator AnimateDiscard(
            GameObject tile,
            RelativeSeat seat,
            int col,
            int row,
            Transform origin,
            Vector3 finalPos,
            Quaternion finalRot)
        {
            /* ───────────── ① 시작 위치 세팅 ───────────── */
            Vector3 dirCol, dirRow;
            switch (seat)
            {
                case RelativeSeat.SELF: dirCol = Vector3.right; dirRow = Vector3.back; break;
                case RelativeSeat.SHIMO: dirCol = Vector3.forward; dirRow = Vector3.right; break;
                case RelativeSeat.TOI: dirCol = Vector3.left; dirRow = Vector3.forward; break;
                default: dirCol = Vector3.back; dirRow = Vector3.left; break;
            }
            Vector3 startOffset = dirCol * (col * tileSpacing)
                                + dirRow * ((row + extraRowOffset) * rowSpacing);
            Vector3 startPos = origin.position + startOffset + Vector3.up * dropHeight;
            tile.transform.SetPositionAndRotation(startPos, finalRot);

            /* ───────────── ② 머티리얼 캐싱 & 투명 모드 ───────────── */
            var cachedMats = new List<Material>();
            foreach (var r in tile.GetComponentsInChildren<Renderer>())
            {
                var mats = r.materials;           // ← 여기서 한 번만 복사
                cachedMats.AddRange(mats);

                foreach (var mat in mats)
                {
                    SetMaterialTransparent(mat);
                    if (mat.HasProperty("_Color"))
                    {
                        Color c = mat.color; c.a = 0f; mat.color = c;
                    }
                }
            }

            /* ───────────── ③ 낙하 + 페이드인 ───────────── */
            float elapsed = 0f;
            float a = 2f * (finalPos.y - startPos.y) / (dropDuration * dropDuration);

            Vector3 horizStart = new Vector3(startPos.x, 0f, startPos.z);
            Vector3 horizEnd = new Vector3(finalPos.x, 0f, finalPos.z);

            while (elapsed < dropDuration)
            {
                elapsed += Time.deltaTime;
                float tNorm = Mathf.Clamp01(elapsed / dropDuration);

                // 위치 보간
                Vector3 horiz = Vector3.Lerp(horizStart, horizEnd, tNorm);
                float y = startPos.y + 0.5f * a * elapsed * elapsed;
                tile.transform.position = new Vector3(horiz.x, y, horiz.z);

                // 알파 보간
                float alphaT = Mathf.Clamp01(elapsed / fadeDuration);
                foreach (var mat in cachedMats)
                    if (mat.HasProperty("_Color"))
                    {
                        Color c = mat.color; c.a = alphaT; mat.color = c;
                    }

                yield return null;
            }

            /* ───────────── ④ 최종 보정 & Opaque 복구 ───────────── */
            tile.transform.position = finalPos;
            foreach (var mat in cachedMats)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color c = mat.color; c.a = 1f; mat.color = c;
                }
                SetMaterialOpaque(mat);
            }
        }

        // Standard Shader를 Transparent 모드로 전환
        private void SetMaterialTransparent(Material mat)
        {
            mat.SetFloat("_Mode", 3f);
            mat.SetOverrideTag("RenderType", "Transparent");                           // 추가
            mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)RenderQueue.Transparent;
        }

        // 머티리얼을 다시 Opaque 모드로 복원
        private void SetMaterialOpaque(Material mat)
        {
            mat.SetFloat("_Mode", 0f);
            mat.SetOverrideTag("RenderType", "Opaque");                               // 추가
            mat.SetInt("_SrcBlend", (int)BlendMode.One);
            mat.SetInt("_DstBlend", (int)BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)RenderQueue.Geometry;                              // 기본 Opaque 큐로 명시
        }

        private Transform GetDiscardPosition(RelativeSeat seat) => seat switch
        {
            RelativeSeat.SELF => discardPosSELF,
            RelativeSeat.SHIMO => discardPosSHIMO,
            RelativeSeat.TOI => discardPosTOI,
            RelativeSeat.KAMI => discardPosKAMI,
            _ => null,
        };
    }
}
