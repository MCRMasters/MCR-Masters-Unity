using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MCRGame.Tester
{
    public class WaterColumnTester : MonoBehaviour
    {
        [Header("Water Column Effect")]
        [Tooltip("스폰할 물기둥 이펙트 Prefab")]
        public GameObject waterColumnPrefab;
        [Tooltip("전체 지속 시간(초), 페이드인·페이드아웃 포함)")]
        public float effectLifetime = 5f;

        private float spawnX = 0f, spawnY = 0f, spawnZ = 0f;

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 240), "Water Column Tester", GUI.skin.window);

            GUILayout.Label("Water Column Prefab:");
            GUILayout.Label(waterColumnPrefab ? waterColumnPrefab.name : "None", GUI.skin.box);

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("X:", GUILayout.Width(20));
            float.TryParse(GUILayout.TextField(spawnX.ToString("F2"), GUILayout.Width(60)), out spawnX);
            GUILayout.Label("Y:", GUILayout.Width(20));
            float.TryParse(GUILayout.TextField(spawnY.ToString("F2"), GUILayout.Width(60)), out spawnY);
            GUILayout.Label("Z:", GUILayout.Width(20));
            float.TryParse(GUILayout.TextField(spawnZ.ToString("F2"), GUILayout.Width(60)), out spawnZ);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Lifetime:", GUILayout.Width(60));
            float.TryParse(GUILayout.TextField(effectLifetime.ToString("F2"), GUILayout.Width(60)), out effectLifetime);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (waterColumnPrefab == null)
            {
                GUILayout.Label("Prefab을 할당하세요.", GUI.skin.box);
            }
            else if (GUILayout.Button("Spawn Water Column", GUILayout.Height(30)))
            {
                StartCoroutine(SpawnWaterColumn(new Vector3(spawnX, spawnY, spawnZ)));
            }

            GUILayout.EndArea();
        }

        IEnumerator SpawnWaterColumn(Vector3 worldPos)
        {
            GameObject go = Instantiate(waterColumnPrefab, worldPos, Quaternion.identity);

            if (effectLifetime > 0f)
            {
                float fadeInDur = 0.5f;                      // 페이드인 시간
                float fadeOutDur = 1.5f;                      // 페이드아웃 시간
                float holdDur = Mathf.Max(0f, effectLifetime - fadeInDur - fadeOutDur); // 중간 유지 시간

                // 실제 페이드 인/아웃 코루틴 실행
                yield return StartCoroutine(FadeInHoldFadeOut(go, fadeInDur, holdDur, fadeOutDur));
            }
        }

        /// <summary>
        /// 1) 페이드인 (1→0), 2) 유지, 3) 페이드아웃 (0→1), 4) 파괴
        /// </summary>
        IEnumerator FadeInHoldFadeOut(GameObject go, float fadeInDuration, float holdDuration, float fadeOutDuration)
        {
            // 1) 자식 포함 모든 Renderer 수집
            Renderer[] rends = go.GetComponentsInChildren<Renderer>();
            List<Material> clipMats = new List<Material>();
            foreach (var r in rends)
            {
                Material mat = r.material; // 인스턴스 생성
                if (mat.HasProperty("_clip"))
                    clipMats.Add(mat);
            }

            // --- 페이드인 (1 → 0) ---
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                float t = Mathf.Clamp01(elapsed / fadeInDuration);
                float clipValue = Mathf.SmoothStep(1f, 0f, t);
                foreach (var mat in clipMats) mat.SetFloat("_clip", clipValue);

                elapsed += Time.deltaTime;
                yield return null;
            }
            clipMats.ForEach(m => m.SetFloat("_clip", 0f)); // 완전 보이게

            // --- 유지 ---
            if (holdDuration > 0f)
                yield return new WaitForSeconds(holdDuration);

            // --- 페이드아웃 (0 → 1) ---
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                float t = Mathf.Clamp01(elapsed / fadeOutDuration);
                float clipValue = Mathf.SmoothStep(0f, 1f, t);
                foreach (var mat in clipMats) mat.SetFloat("_clip", clipValue);

                elapsed += Time.deltaTime;
                yield return null;
            }
            clipMats.ForEach(m => m.SetFloat("_clip", 1f)); // 완전 클립

            // 4) 오브젝트 파괴
            Destroy(go);
        }
    }
}
