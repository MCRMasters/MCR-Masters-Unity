using UnityEngine;
using System.Collections;
using MCRGame.Common; // RelativeSeat, GameTile, 등
using System.Collections.Generic;

namespace MCRGame.UI
{
    public class DiscardManager : MonoBehaviour
    {
        public Transform discardPosE;
        public Transform discardPosS;
        public Transform discardPosW;
        public Transform discardPosN;

        [Header("타일 간격 설정")]
        public float tileSpacing = 15f;
        public float rowSpacing = 20f;
        public int maxTilesPerRow = 6;

        [Header("Discard Animation Settings")]
        public float dropHeight = 30f;            // 위에서 얼마나 높이 시작할지
        public float fadeDuration = 0.05f;        // 투명→불투명 페이드 시간
        public float dropDuration = 0.15f;        // 낙하 애니메이션 시간
        public float extraRowOffset = 1f;        // 대각선 뒤로 밀릴 추가 row 단위

        private Dictionary<RelativeSeat, List<GameObject>> kawas = new Dictionary<RelativeSeat, List<GameObject>>();
        private Dictionary<GameTile, List<GameObject>> tileObjectDictionary = new Dictionary<GameTile, List<GameObject>>();

        void Awake()
        {
            kawas[RelativeSeat.SELF] = new List<GameObject>();
            kawas[RelativeSeat.SHIMO] = new List<GameObject>();
            kawas[RelativeSeat.TOI] = new List<GameObject>();
            kawas[RelativeSeat.KAMI] = new List<GameObject>();
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

            // 리스트에 추가
            kawas[seat].Add(instantiatedTile);
            if (!tileObjectDictionary.TryGetValue(tile, out var list))
            {
                list = new List<GameObject>();
                tileObjectDictionary[tile] = list;
            }
            list.Add(instantiatedTile);

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
            // --- 1) 초기 위치 계산 (대각선 뒤 + 위로 띄우기) ---
            // 방향벡터 분리
            Vector3 dirCol, dirRow;
            switch (seat)
            {
                case RelativeSeat.SELF:
                    dirCol = Vector3.right;
                    dirRow = Vector3.back;
                    break;
                case RelativeSeat.SHIMO:
                    dirCol = Vector3.forward;
                    dirRow = Vector3.right;
                    break;
                case RelativeSeat.TOI:
                    dirCol = Vector3.left;
                    dirRow = Vector3.forward;
                    break;
                default: // KAMI
                    dirCol = Vector3.back;
                    dirRow = Vector3.left;
                    break;
            }

            // 대각선 뒤로(extraRowOffset) + 높이(dropHeight)
            Vector3 startOffset = dirCol * (col * tileSpacing)
                                + dirRow * ((row + extraRowOffset) * rowSpacing);
            Vector3 startPos = origin.position + startOffset + Vector3.up * dropHeight;

            tile.transform.position = startPos;
            tile.transform.rotation = finalRot;

            // --- 2) 머티리얼들 투명 모드로 전환 & 알파 0 세팅 ---
            var renderers = tile.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                    SetMaterialTransparent(mat);
                // 바로 알파 0
                foreach (var mat in r.materials)
                    if (mat.HasProperty("_Color"))
                    {
                        var c = mat.color;
                        c.a = 0f;
                        mat.color = c;
                    }
            }

            // --- 3) 낙하 + 페이드인 동시에 수행 ---
            float elapsed = 0f;
            float totalTime = dropDuration;
            // 물리: v0 = 0, a = 2*(Δy) / t^2
            float y0 = startPos.y;
            float y1 = finalPos.y;
            float a = 2f * (y1 - y0) / (totalTime * totalTime);

            Vector3 horizStart = new Vector3(startPos.x, 0f, startPos.z);
            Vector3 horizEnd = new Vector3(finalPos.x, 0f, finalPos.z);

            while (elapsed < totalTime)
            {
                elapsed += Time.deltaTime;
                float tNorm = Mathf.Clamp01(elapsed / totalTime);

                // 수평 이동 (선형)
                Vector3 horiz = Vector3.Lerp(horizStart, horizEnd, tNorm);
                // 수직 이동 (가속도)
                float y = y0 + 0.5f * a * elapsed * elapsed;

                tile.transform.position = new Vector3(horiz.x, y, horiz.z);

                // 페이드인 (dropDuration 중 fadeDuration 만큼)
                float alphaT = Mathf.Clamp01(elapsed / fadeDuration);
                foreach (var r in renderers)
                {
                    foreach (var mat in r.materials)
                        if (mat.HasProperty("_Color"))
                        {
                            var c = mat.color;
                            c.a = alphaT;
                            mat.color = c;
                        }
                }

                yield return null;
            }

            // --- 4) 최종 보정 ---
            tile.transform.position = finalPos;
            foreach (var r in renderers)
                foreach (var mat in r.materials)
                    if (mat.HasProperty("_Color"))
                    {
                        var c = mat.color;
                        c.a = 1f;
                        mat.color = c;
                        // (필요하면 Opaque 모드 복원)
                    }
        }


        // Standard Shader를 Transparent 모드로 전환
        private void SetMaterialTransparent(Material mat)
        {
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        private Transform GetDiscardPosition(RelativeSeat seat) => seat switch
        {
            RelativeSeat.SELF => discardPosE,
            RelativeSeat.SHIMO => discardPosS,
            RelativeSeat.TOI => discardPosW,
            RelativeSeat.KAMI => discardPosN,
            _ => null,
        };
    }
}
