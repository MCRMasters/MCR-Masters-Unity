using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MCRGame.Common;

namespace MCRGame.UI
{
    public class CallBlockField : MonoBehaviour
    {
        [Header("CallBlock Settings")]
        public List<GameObject> callBlocks;
        private Dictionary<GameTile, List<GameObject>> tileObjectDictionary = new();


        [Header("Animation Settings")]
        public float cbDropHeight = 10f;      // 위에서 얼마나 높이 시작할지
        public float cbFadeDuration = 0.1f;   // 투명→불투명 페이드 시간
        public float cbDropDuration = 0.2f;   // 낙하 애니메이션 시간
        public float cbDiagOffset = 50f;      // 대각선으로 얼마나 멀리 시작할지

        [Header("Bulge Settings")]
        public float cbBulgeHeight = 10f;     // Y축으로 얼마나 불룩하게
        public bool cbBulgeUp = true;         // true면 위로, false면 아래로

        [Header("Gap Settings")]
        [Tooltip("바로 직전 블록의 너비에 대한 간격 비율")]
        public float gapRatio = 0.1f;

        private readonly List<(Material mat, Color original)> _highlightedMats
    = new List<(Material, Color)>();

        /// <summary>
        /// CallBlockField를 초기화합니다.
        /// 기존의 callBlocks 리스트를 초기화하고 모든 블록을 제거합니다.
        /// </summary>
        public void InitializeCallBlockField()
        {
            Debug.Log("CallBlockField: Initializing call block field.");
            if (callBlocks == null)
            {
                callBlocks = new List<GameObject>();
                tileObjectDictionary.Clear();
                _highlightedMats.Clear();
            }
            else
            {
                ClearAllCallBlocks();
            }
        }

        /// <summary>
        /// CallBlock 내부에서 새로 생성된 타일 GameObject를 사전에 등록합니다.
        /// </summary>
        public void RegisterCallBlockTile(GameTile tile, GameObject tileObj)
        {
            if (!tileObjectDictionary.TryGetValue(tile, out var list))
                tileObjectDictionary[tile] = list = new List<GameObject>();
            list.Add(tileObj);
        }

        public void ReloadCallBlockListImmediate(List<CallBlockData> data)
        {
            ClearAllCallBlocks();
            foreach (CallBlockData block in data)
            {
                AddCallBlockImmediate(block);
            }
        }

        public void AddCallBlockImmediate(CallBlockData data)
        {
            if (data.Type == CallBlockType.SHOMIN_KONG)
            {
                foreach (var go in callBlocks)
                {
                    var cb = go.GetComponent<CallBlock>();
                    if (cb != null && cb.Data.Type == CallBlockType.PUNG && cb.Data.FirstTile == data.FirstTile)
                    {
                        cb.ApplyShominKong();
                        return;
                    }
                }
                return;
            }

            // 1) 새 GameObject 생성
            GameObject callBlockObj = new GameObject("CallBlock");
            callBlockObj.transform.SetParent(transform, false);
            callBlockObj.transform.localRotation = Quaternion.identity;
            callBlockObj.transform.localScale = Vector3.one;

            // 2) CallBlock 컴포넌트 세팅
            var callBlock = callBlockObj.AddComponent<CallBlock>();
            callBlock.Data = new CallBlockData(
                data.Type,
                data.FirstTile,
                data.SourceSeat,
                data.SourceTileIndex
            );
            callBlock.InitializeCallBlock();

            // 3) 리스트에 보관
            callBlocks.Add(callBlockObj);

            if (data.Type != CallBlockType.AN_KONG)
            {
                var cb = callBlockObj.GetComponent<CallBlock>();
                foreach (var tileObj in cb.Tiles)
                {
                    if (GameTileExtensions.TryParseCustom(tileObj.name, out var tile))
                    {
                        if (!tileObjectDictionary.TryGetValue(tile, out var list))
                            tileObjectDictionary[tile] = list = new List<GameObject>();
                        list.Add(tileObj);
                    }
                }
            }

            // 4) 위치 계산
            PositionNewCallBlock(callBlockObj);
            callBlockObj.transform.localPosition = callBlockObj.transform.localPosition; // final 위치

            // 5) 머티리얼을 불투명(opaque) 상태로 보장
            var renderers = callBlockObj.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                {
                    SetMaterialOpaque(mat);
                    if (mat.HasProperty("_Color"))
                    {
                        var c = mat.color;
                        c.a = 1f;
                        mat.color = c;
                    }
                }
            }
        }

        public void AddCallBlock(CallBlockData data)
        {
            if (data.Type == CallBlockType.SHOMIN_KONG)
            {
                Debug.Log($"[CallBlockField] SHOMIN_KONG 처리 시작 - FirstTile: {data.FirstTile}");

                foreach (var call_block in callBlocks)
                {
                    CallBlock cb = call_block.GetComponent<CallBlock>();
                    if (cb == null)
                    {
                        Debug.LogWarning("[CallBlockField] call_block에 CallBlock 컴포넌트가 없습니다.");
                        continue;
                    }

                    Debug.Log($"[CallBlockField] 검사 중 - cb.Data.Type: {cb.Data.Type}, cb.Data.FirstTile: {cb.Data.FirstTile}");
                    if (cb.Data.Type == CallBlockType.PUNG && cb.Data.FirstTile == data.FirstTile)
                    {
                        Debug.Log($"[CallBlockField] 대상 PUNG 블록 발견 (FirstTile={data.FirstTile}) → ApplyShominKong 호출");
                        cb.ApplyShominKong();
                        break;
                    }
                }

                Debug.Log("[CallBlockField] SHOMIN_KONG 처리 종료");
                return;
            }

            // 1) 블록 GameObject 생성 및 부모에 추가
            GameObject callBlockObj = new GameObject("CallBlock");
            callBlockObj.transform.SetParent(transform, false);
            callBlockObj.transform.localRotation = Quaternion.identity;
            callBlockObj.transform.localScale = Vector3.one;

            // 2) CallBlock 컴포넌트 추가 및 초기화
            var callBlock = callBlockObj.AddComponent<CallBlock>();
            callBlock.Data = data;
            callBlock.InitializeCallBlock();
            callBlocks.Add(callBlockObj);

            if (data.Type != CallBlockType.AN_KONG)
            {
                var cb = callBlockObj.GetComponent<CallBlock>();
                foreach (var tileObj in cb.Tiles)
                {
                    if (GameTileExtensions.TryParseCustom(tileObj.name, out var tile))
                    {
                        if (!tileObjectDictionary.TryGetValue(tile, out var list))
                            tileObjectDictionary[tile] = list = new List<GameObject>();
                        list.Add(tileObj);
                    }
                }
            }

            // 3) 새 블록의 최종 로컬 위치 결정 (바로 직전 블록을 기준)
            PositionNewCallBlock(callBlockObj);
            Vector3 finalLocalPos = callBlockObj.transform.localPosition;

            // 4) 시작 위치: 대각선 오프셋과 cbDropHeight를 더해 애니메이션 시작 위치로 설정
            Vector3 diagDir = new Vector3(1f, -1f, 1f).normalized;
            Vector3 startLocalPos = finalLocalPos + diagDir * cbDiagOffset + Vector3.up * cbDropHeight;
            callBlockObj.transform.localPosition = startLocalPos;

            // 5) 머티리얼 투명 모드 및 알파 0 설정
            var renderers = callBlockObj.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                    SetMaterialTransparent(mat);
                foreach (var mat in r.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        var c = mat.color;
                        c.a = 0f;
                        mat.color = c;
                    }
                }
            }

            // 6) 낙하 및 페이드인 애니메이션 실행
            StartCoroutine(AnimateCallBlock(callBlockObj, finalLocalPos, renderers));
        }

        /// <summary>
        /// 바로 직전 블록의 로컬 경계(오른쪽 경계)를 기준으로, gap을 더한 위치에 새 블록의 좌측 경계가 맞도록 배치합니다.
        /// 첫 번째 블록은 (0,0,0)에 배치합니다.
        /// </summary>
        private void PositionNewCallBlock(GameObject newCallBlock)
        {
            // 새 블록의 transform을 초기화
            newCallBlock.transform.localRotation = Quaternion.identity;
            newCallBlock.transform.localScale = Vector3.one;
            newCallBlock.transform.localPosition = Vector3.zero;

            // 첫 번째 블록이면 그대로 (0,0,0)
            if (callBlocks.Count <= 1)
                return;

            // 바로 직전 블록
            GameObject previousBlock = callBlocks[callBlocks.Count - 2];
            Vector3 prevPos = previousBlock.transform.localPosition; // 이전 블록의 부모 기준 위치
            var (prevLocalMin, prevLocalMax) = GetLocalBounds(previousBlock);
            // 이전 블록의 절대 좌측, 우측 경계 (부모의 로컬 좌표계에서)
            float prevLeft = prevPos.x + prevLocalMin.x;
            float prevRight = prevPos.x + prevLocalMax.x;
            float prevWidth = prevLocalMax.x - prevLocalMin.x;
            // float gap = prevWidth * gapRatio;
            float gap = 0f;
            // 새 블록의 desired 좌측 경계
            float desiredNewLeft = prevRight + gap;

            // 새 블록은 지금 (0,0,0)에 있으므로, 자신의 경계를 계산
            var (newLocalMin, _) = GetLocalBounds(newCallBlock);
            // 새 블록의 실제 좌측 경계는 (newBlock.transform.localPosition.x + newLocalMin.x)
            float offsetX = desiredNewLeft - newLocalMin.x;
            newCallBlock.transform.localPosition = new Vector3(offsetX, 0f, 0f);
        }

        /// <summary>
        /// CallBlock 내 모든 타일의 로컬 경계를 부모의 로컬 좌표계 기준으로 종합해서 구합니다.
        /// </summary>
        private (Vector3 min, Vector3 max) GetLocalBounds(GameObject obj)
        {
            var cb = obj.GetComponent<CallBlock>();
            if (cb == null || cb.Tiles.Count == 0)
                return (Vector3.zero, Vector3.zero);

            Vector3 overallMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 overallMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach (var tile in cb.Tiles)
            {
                var bounds = GetTileLocalBounds(tile);
                overallMin = Vector3.Min(overallMin, bounds.min);
                overallMax = Vector3.Max(overallMax, bounds.max);
            }
            return (overallMin, overallMax);
        }

        /// <summary>
        /// 회전 및 스케일을 포함한 tile의 TRS 변환을 적용하여 부모의 로컬 좌표계 기준으로 tile의 경계를 계산합니다.
        /// </summary>
        private (Vector3 min, Vector3 max) GetTileLocalBounds(GameObject tile)
        {
            MeshFilter mf = tile.GetComponent<MeshFilter>();
            if (mf == null)
                return (Vector3.zero, Vector3.zero);

            Bounds b = mf.mesh.bounds;
            Vector3[] corners = new Vector3[8];
            corners[0] = new Vector3(b.min.x, b.min.y, b.min.z);
            corners[1] = new Vector3(b.min.x, b.min.y, b.max.z);
            corners[2] = new Vector3(b.min.x, b.max.y, b.min.z);
            corners[3] = new Vector3(b.min.x, b.max.y, b.max.z);
            corners[4] = new Vector3(b.max.x, b.min.y, b.min.z);
            corners[5] = new Vector3(b.max.x, b.min.y, b.max.z);
            corners[6] = new Vector3(b.max.x, b.max.y, b.min.z);
            corners[7] = new Vector3(b.max.x, b.max.y, b.max.z);

            // tile의 local TRS (로컬 위치, 로컬 회전, 로컬 스케일)
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
        private IEnumerator AnimateCallBlock(GameObject obj, Vector3 finalPos, Renderer[] renderers)
        {
            float elapsed = 0f;
            float tTotal = cbDropDuration;

            // 시작 위치 조정: X, Y, Z 모두 finalPos에서 약간만 떨어진 위치로 세팅
            Vector3 startLocal = obj.transform.localPosition;
            startLocal.x = finalPos.x + 1f;    // X축 오프셋 (bulge 효과로 움직일 것이므로 작은 값)
            startLocal.y = finalPos.y + 4f;      // Y축 높게 (Ease-out으로 떨어지도록)
            startLocal.z = finalPos.z + 2f;    // Z축 오프셋 (단순 선형 보간)
            obj.transform.localPosition = startLocal;

            // bulge 효과 스케일을 약간 더 크게 (예: 0.1f)
            float bulgeScale = 0.1f;

            while (elapsed < tTotal)
            {
                elapsed += Time.deltaTime;
                float tNorm = Mathf.Clamp01(elapsed / tTotal);

                // Y: Ease-Out 보간 (높은 곳에서 아래로 떨어지는 듯한 효과)
                float y = Mathf.Lerp(startLocal.y, finalPos.y, 1f - Mathf.Pow(1f - tNorm, 2f));

                // Z: startLocal.z에서 finalPos.z로 선형 보간
                float z = Mathf.Lerp(startLocal.z, finalPos.z, tNorm);

                // X: 선형 보간 + bulge 효과
                float baseX = Mathf.Lerp(startLocal.x, finalPos.x, tNorm);
                float dir = cbBulgeUp ? 1f : -1f;
                float bulge = cbBulgeHeight * bulgeScale * 4f * tNorm * (1f - tNorm) * dir;
                float x = baseX + bulge;

                obj.transform.localPosition = new Vector3(x, y, z);

                // 페이드인: 알파값 보간
                float alphaT = Mathf.Clamp01(elapsed / cbFadeDuration);
                foreach (var r in renderers)
                {
                    foreach (var mat in r.materials)
                    {
                        if (mat.HasProperty("_Color"))
                        {
                            var c = mat.color;
                            c.a = alphaT;
                            mat.color = c;
                        }
                    }
                }

                yield return null;
            }

            // 최종 위치 및 알파/투명도 초기화
            obj.transform.localPosition = finalPos;
            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        var c = mat.color;
                        c.a = 1f;
                        mat.color = c;
                        SetMaterialOpaque(mat);
                    }
                }
            }
        }

        private void SetMaterialOpaque(Material mat)
        {
            mat.SetFloat("_Mode", 0);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = -1;
        }

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

        /// <summary>tile과 일치하는 모든 CallBlock을 하이라이트(하늘색)합니다.</summary>
        public void HighlightBlocks(GameTile tile)
        {
            ClearHighlights();     // 이전 하이라이트 복원

            if (!tileObjectDictionary.TryGetValue(tile, out var goList))
                return;

            foreach (var go in goList)
            {
                foreach (var r in go.GetComponentsInChildren<Renderer>())
                {
                    foreach (var mat in r.materials)
                    {
                        if (!mat.HasProperty("_Color")) continue;

                        // (1) 현재 색 저장
                        _highlightedMats.Add((mat, mat.color));

                        // (2) 하이라이트 색 적용 (연한 하늘색)
                        Color c = mat.color;
                        mat.color = new Color(0.7f, 0.9f, 1f, c.a);
                    }
                }
            }
        }

        /// <summary>이전 하이라이트를 모두 해제하고, 머티리얼을 원본 색으로 되돌립니다.</summary>
        public void ClearHighlights()
        {
            foreach (var (mat, orig) in _highlightedMats)
            {
                if (mat == null) continue;
                if (mat.HasProperty("_Color"))
                    mat.color = orig;      // 저장한 색으로 복원
            }
            _highlightedMats.Clear();
        }

        public void ClearAllCallBlocks()
        {
            foreach (var cb in callBlocks)
            {
                Destroy(cb);
            }
            callBlocks.Clear();
            tileObjectDictionary.Clear();
            ClearHighlights();
        }
    }
}
