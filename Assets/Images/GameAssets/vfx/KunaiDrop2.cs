using UnityEngine;
using DG.Tweening;  // DOTween 네임스페이스

public class KunaiDropManager2 : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Kunai prefab to instantiate and drop")]
    public GameObject kunaiPrefab;
    [Tooltip("Prefab containing particle systems to spawn on impact")]
    public GameObject impactEffectPrefab;

    [Header("Offsets")]
    [Tooltip("도착 지점에서 얼마만큼 보정하여 임팩트 위치를 계산할지")]
    public Vector3 arrivalOffset = Vector3.zero;
    [Tooltip("도착 위치(보정 후)에서 얼마만큼 위/뒤에서 시작할지")]
    public Vector3 startOffset = new Vector3(30f, 50f, 50f);

    [Header("Drop Settings")]
    [Tooltip("Time to drop from start to target (seconds)")]
    public float dropDuration = 1.0f;

    // GUI input fields for target position
    private string inputX = "0", inputY = "0", inputZ = "0";

    void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 320, 220), "Kunai Drop (DOTween)");

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

        // 1) 도착 보정 위치 계산
        Vector3 arrivalPos = targetPos + arrivalOffset;
        // 2) 시작 위치는 arrivalPos + startOffset
        Vector3 startPos = arrivalPos + startOffset;

        GameObject kunai = Instantiate(kunaiPrefab, startPos, Quaternion.identity);

        // 낙하 애니메이션 (가속감)
        kunai.transform
            .DOMove(arrivalPos, dropDuration)
            .SetEase(Ease.InQuad);

        // 회전 애니메이션 (딱 dropDuration 동안)
        kunai.transform
            .DORotate(new Vector3(30f, 30f, 30f), dropDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => {
                SpawnImpact(kunai.transform, arrivalPos);
                Destroy(kunai, 2f);
            });
    }

    private void SpawnImpact(Transform kunaiTransform, Vector3 impactPos)
    {
        if (impactEffectPrefab == null) return;

        // TipAnchor가 있으면 그 위치, 없으면 impactPos
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
