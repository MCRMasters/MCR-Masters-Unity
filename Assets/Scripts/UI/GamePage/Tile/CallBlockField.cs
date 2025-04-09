using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MCRGame.Common;
using System;

namespace MCRGame.UI
{
    public class CallBlockField : MonoBehaviour
    {
        [Header("CallBlock Settings")]
        public List<GameObject> callBlocks;

        [Header("Animation Settings")]
        public float cbDropHeight = 10f;   // 위에서 얼마나 높이 시작할지
        public float cbFadeDuration = 0.1f;  // 투명→불투명 페이드 시간
        public float cbDropDuration = 0.2f;  // 낙하 애니메이션 시간
        public float cbDiagOffset = 50f;   // 대각선으로 얼마나 멀리 시작할지

        [Header("Bulge Settings")]
        public float cbBulgeHeight = 10f;     // Y축으로 얼마나 불룩하게
        public bool cbBulgeUp = true;   // true면 위로, false면 아래로

        public void AddCallBlock(CallBlockData data)
        {
            // 1) 블록 생성 및 초기화
            GameObject callBlockObj = new GameObject("CallBlock");
            callBlockObj.transform.SetParent(transform, false);
            var callBlock = callBlockObj.AddComponent<CallBlock>();
            callBlock.Data = data;
            callBlock.InitializeCallBlock();
            callBlocks.Add(callBlockObj);

            // 2) 최종 로컬 위치 계산
            PositionNewCallBlock(callBlockObj);
            Vector3 finalLocalPos = callBlockObj.transform.localPosition;

            // 3) 시작 위치: 반대 대각선(1f, -1f, 1f) 방향 + 높이
            Vector3 diagDir = new Vector3(1f, -1f, 1f).normalized;
            Vector3 startLocalPos = finalLocalPos + diagDir * cbDiagOffset + Vector3.up * cbDropHeight;
            callBlockObj.transform.localPosition = startLocalPos;

            // 4) 머티리얼 투명 모드 & 알파 0 세팅
            var renderers = callBlockObj.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                    SetMaterialTransparent(mat);
                foreach (var mat in r.materials)
                    if (mat.HasProperty("_Color"))
                    {
                        var c = mat.color;
                        c.a = 0f;
                        mat.color = c;
                    }
            }

            // 5) 애니메이션 코루틴 실행
            StartCoroutine(AnimateCallBlock(callBlockObj, finalLocalPos, renderers));
        }

        private IEnumerator AnimateCallBlock(GameObject obj, Vector3 finalPos, Renderer[] renderers)
        {
            float elapsed = 0f;
            float tTotal = cbDropDuration;

            Vector3 startLocal = obj.transform.localPosition;

            while (elapsed < tTotal)
            {
                elapsed += Time.deltaTime;
                float tNorm = Mathf.Clamp01(elapsed / tTotal);

                // 1) X 선형 보간
                float x = Mathf.Lerp(startLocal.x, finalPos.x, tNorm);

                // 2) Z 슬라이더 꺾임 (Ease‑out)
                float z = Mathf.Lerp(
                    startLocal.z,
                    finalPos.z,
                    1f - Mathf.Pow(1f - tNorm, 2f)
                );

                // 3) Y 불룩하게 (Bulge)
                float baseY = Mathf.Lerp(startLocal.y, finalPos.y, tNorm);
                float dir = cbBulgeUp ? 1f : -1f;
                // 4*t*(1-t) 꼴로 t=0.5일 때 최대
                float bulge = cbBulgeHeight * 4f * tNorm * (1f - tNorm) * dir;
                float y = baseY + bulge;

                obj.transform.localPosition = new Vector3(x, y, z);

                // 4) 페이드인
                float alphaT = Mathf.Clamp01(elapsed / cbFadeDuration);
                foreach (var r in renderers)
                    foreach (var mat in r.materials)
                        if (mat.HasProperty("_Color"))
                        {
                            var c = mat.color;
                            c.a = alphaT;
                            mat.color = c;
                        }

                yield return null;
            }

            // 최종 보정
            obj.transform.localPosition = finalPos;
            foreach (var r in renderers)
                foreach (var mat in r.materials)
                    if (mat.HasProperty("_Color"))
                    {
                        var c = mat.color;
                        c.a = 1f;
                        mat.color = c;
                        SetMaterialOpaque(mat);
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
            // 기본 렌더 큐(Geometry 2000)로 복원
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

        private void PositionNewCallBlock(GameObject newCallBlock)
        {
            newCallBlock.transform.localRotation = Quaternion.identity;
            if (callBlocks.Count == 1)
            {
                newCallBlock.transform.localPosition = Vector3.zero;
                return;
            }

            float gapRatio = 0.1f;
            var prevObj = callBlocks[callBlocks.Count - 2];
            var prev = prevObj.GetComponent<CallBlock>();
            if (prev == null || prev.Tiles.Count == 0) return;

            var rend = prev.Tiles[0].GetComponent<Renderer>();
            if (rend == null) return;

            float tileWidth = rend.bounds.size.x;
            float gap = tileWidth * gapRatio;

            var (prevMin, prevMax) = GetLocalBounds(prevObj);
            newCallBlock.transform.localPosition = Vector3.zero;
            var (newMin, newMax) = GetLocalBounds(newCallBlock);

            float desiredX = prevMax.x + gap;
            float offsetX = desiredX - newMin.x;
            newCallBlock.transform.localPosition += new Vector3(offsetX, 0f, 0f);
        }

        private (Vector3 min, Vector3 max) GetLocalBounds(GameObject obj)
        {
            var cb = obj.GetComponent<CallBlock>();
            if (cb == null || cb.Tiles.Count == 0)
                return (Vector3.zero, Vector3.zero);

            Vector3 overallMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 overallMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var tile in cb.Tiles)
            {
                var rend = tile.GetComponent<Renderer>();
                if (rend == null) continue;
                var b = rend.bounds;
                Vector3[] corners = {
                    new Vector3(b.min.x,b.min.y,b.min.z),
                    new Vector3(b.min.x,b.min.y,b.max.z),
                    new Vector3(b.min.x,b.max.y,b.min.z),
                    new Vector3(b.min.x,b.max.y,b.max.z),
                    new Vector3(b.max.x,b.min.y,b.min.z),
                    new Vector3(b.max.x,b.min.y,b.max.z),
                    new Vector3(b.max.x,b.max.y,b.min.z),
                    new Vector3(b.max.x,b.max.y,b.max.z),
                };
                foreach (var c in corners)
                {
                    var local = transform.InverseTransformPoint(c);
                    overallMin = Vector3.Min(overallMin, local);
                    overallMax = Vector3.Max(overallMax, local);
                }
            }
            return (overallMin, overallMax);
        }

        public void ClearAllCallBlocks()
        {
            foreach (var cb in callBlocks) Destroy(cb);
            callBlocks.Clear();
        }
    }
}
