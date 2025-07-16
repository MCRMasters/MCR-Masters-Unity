using UnityEngine;
using DG.Tweening;  // DOTween 네임스페이스

public class KunaiTest1 : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Kunai prefab to instantiate and drop")]
    public GameObject kunaiPrefab;
    [Tooltip("Prefab containing particle systems to spawn on impact")]
    public GameObject impactEffectPrefab;

    [Header("Drop Settings")]
    [Tooltip("Offset above target position")]
    public Vector3 startOffset = new Vector3(50f, 50f, 50f);
    [Tooltip("Time to drop from start to target (seconds)")]
    public float dropDuration = 1.0f;

    // GUI input fields for target position
    private string inputX = "0", inputY = "0", inputZ = "0";

    void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 300, 180), "Kunai Drop (DOTween)");

        GUI.Label(new Rect(20, 40, 80, 20), "Target X:");
        inputX = GUI.TextField(new Rect(100, 40, 80, 20), inputX);
        GUI.Label(new Rect(20, 70, 80, 20), "Target Y:");
        inputY = GUI.TextField(new Rect(100, 70, 80, 20), inputY);
        GUI.Label(new Rect(20, 100, 80, 20), "Target Z:");
        inputZ = GUI.TextField(new Rect(100, 100, 80, 20), inputZ);

        GUI.Label(new Rect(20, 130, 100, 20), "Duration(s):");
        string dur = GUI.TextField(new Rect(120, 130, 60, 20), dropDuration.ToString("F2"));
        float.TryParse(dur, out dropDuration);

        if (GUI.Button(new Rect(200, 130, 80, 25), "Drop"))
        {
            if (float.TryParse(inputX, out float tx) &&
                float.TryParse(inputY, out float ty) &&
                float.TryParse(inputZ, out float tz))
            {
                LaunchKunai(new Vector3(tx, ty, tz));
            }
            else
            {
                Debug.LogError("Invalid target input.");
            }
        }
    }

    private void LaunchKunai(Vector3 targetPos)
    {
        if (kunaiPrefab == null) { Debug.LogError("kunaiPrefab is null"); return; }

        // 시작 위치 = 목표점 + offset
        Vector3 startPos = targetPos + startOffset;
        GameObject kunai = Instantiate(kunaiPrefab, startPos, Quaternion.identity);

        // 1) 낙하 애니메이션: InQuad 이징으로 가속감 추가
        kunai.transform
            .DOMove(targetPos, dropDuration)
            .SetEase(Ease.InQuad);

        // 2) 회전 애니메이션: 0° → 60° (Z축 예시)
        kunai.transform
            .DORotate(new Vector3(20f, 30f, 45f), dropDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => {
                // 임팩트 이펙트 실행
                SpawnImpact(kunai.transform, targetPos);
                // 2초 뒤에 쿠나이 삭제
                Destroy(kunai, 2f);
            });
    }

    private void SpawnImpact(Transform kunaiTransform, Vector3 targetPos)
    {
        if (impactEffectPrefab == null) return;

        // TipAnchor가 있으면 그 위치, 없으면 목표점
        Vector3 spawnPos = targetPos;
        var tip = kunaiTransform.Find("TipAnchor");
        if (tip != null) spawnPos = tip.position;

        GameObject fx = Instantiate(impactEffectPrefab, spawnPos, Quaternion.identity);
        var systems = fx.GetComponentsInChildren<ParticleSystem>();
        float maxDur = 0f;
        foreach (var ps in systems)
        {
            ps.Play();
            maxDur = Mathf.Max(maxDur, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        Destroy(fx, maxDur + 0.5f);
    }
}
