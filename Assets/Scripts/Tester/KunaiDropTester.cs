using UnityEngine;
using DG.Tweening;  // DOTween 네임스페이스

namespace MCRGame.Tester
{
    public class KunaiDropTester : MonoBehaviour
    {
        [Header("References")]
        public GameObject kunaiPrefab;
        public GameObject impactEffectPrefab;

        [Header("Offsets")]
        // 인스펙터 제거, 고정값으로 세팅
        private readonly Vector3 arrivalOffset = Vector3.zero;
        private readonly Vector3 startOffset = new Vector3(20f, 20f, 10f);

        [Header("Drop Settings")]
        public float dropDuration = 1.0f;

        private string inputX = "0", inputY = "0", inputZ = "0";

        void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 320, 200), "Kunai Drop (DOTween)");

            GUI.Label(new Rect(20, 40, 80, 20), "Target X:");
            inputX = GUI.TextField(new Rect(100, 40, 80, 20), inputX);
            GUI.Label(new Rect(20, 70, 80, 20), "Target Y:");
            inputY = GUI.TextField(new Rect(100, 70, 80, 20), inputY);
            GUI.Label(new Rect(20, 100, 80, 20), "Target Z:");
            inputZ = GUI.TextField(new Rect(100, 100, 80, 20), inputZ);

            GUI.Label(new Rect(20, 130, 120, 20), "Duration(s):");
            string dur = GUI.TextField(new Rect(140, 130, 60, 20), dropDuration.ToString("F2"));
            float.TryParse(dur, out dropDuration);

            if (GUI.Button(new Rect(20, 160, 260, 30), "Drop"))
            {
                if (float.TryParse(inputX, out float tx) &&
                    float.TryParse(inputY, out float ty) &&
                    float.TryParse(inputZ, out float tz))
                {
                    LaunchKunai(new Vector3(tx, ty, tz));
                }
                else Debug.LogError("Invalid target input.");
            }
        }

        private void LaunchKunai(Vector3 targetPos)
        {
            if (kunaiPrefab == null)
            {
                Debug.LogError("kunaiPrefab is null");
                return;
            }

            // arrivalOffset 은 그대로 유지하거나 필요 없으면 제거
            Vector3 arrivalPos = targetPos + arrivalOffset;
            // startOffset 을 고정값 (20,20,10) 으로 사용
            Vector3 startPos = arrivalPos + startOffset;

            GameObject kunai = Instantiate(kunaiPrefab, startPos, Quaternion.identity);

            // 낙하 애니메이션
            kunai.transform
                .DOMove(arrivalPos, dropDuration)
                .SetEase(Ease.InExpo);

            // 회전 애니메이션
            kunai.transform
                .DORotate(new Vector3(30f, 30f, 30f), dropDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    SpawnImpact(kunai.transform, arrivalPos);
                    Destroy(kunai, 1f);
                });
        }

        private void SpawnImpact(Transform kunaiTransform, Vector3 impactPos)
        {
            if (impactEffectPrefab == null) return;

            Vector3 spawnPos = impactPos;
            var tip = kunaiTransform.Find("TipAnchor");
            if (tip != null) spawnPos = tip.position;

            GameObject fx = Instantiate(impactEffectPrefab, spawnPos, Quaternion.identity);
            float maxDur = 0f;
            foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Play();
                maxDur = Mathf.Max(maxDur, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            Destroy(fx, maxDur + 0.5f);
        }
    }
}
