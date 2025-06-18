using UnityEngine;
using System.Collections;

/// <summary>
/// kunaiPrefab을 생성하여
/// GUI 입력 좌표로 포물선 이동 시, 목표 지점을 GUI로 입력하고
/// 도달 시 지정된 파티클 프리팹(여러 시스템 포함)을 Instantiate하여 이펙트를 실행합니다.
/// Attach this to an empty GameObject and assign kunaiPrefab 및 impactEffectPrefab.
/// </summary>
public class KunaiDropManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Kunai prefab to instantiate and drop in an arc")]
    public GameObject kunaiPrefab;
    [Tooltip("Prefab containing particle systems to spawn on impact at target")]
    public GameObject impactEffectPrefab;

    [Header("Arc Drop Settings")]
    [Tooltip("Height offset above start pos")]
    public float dropHeight = 5f;
    [Tooltip("Duration for the arc drop")]
    public float dropDuration = 1.0f;

    // GUI input fields for start and target positions
    private string inputStartX = "0";
    private string inputStartY = "0";
    private string inputStartZ = "0";
    private string inputTargetX = "0";
    private string inputTargetY = "0";
    private string inputTargetZ = "0";

    void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 360, 320), "Kunai Drop Controller");

        GUI.Label(new Rect(20, 40, 120, 20), "Start Pos X:");
        inputStartX = GUI.TextField(new Rect(140, 40, 200, 20), inputStartX);
        GUI.Label(new Rect(20, 70, 120, 20), "Start Pos Y:");
        inputStartY = GUI.TextField(new Rect(140, 70, 200, 20), inputStartY);
        GUI.Label(new Rect(20, 100, 120, 20), "Start Pos Z:");
        inputStartZ = GUI.TextField(new Rect(140, 100, 200, 20), inputStartZ);

        GUI.Label(new Rect(20, 130, 120, 20), "Target Pos X:");
        inputTargetX = GUI.TextField(new Rect(140, 130, 200, 20), inputTargetX);
        GUI.Label(new Rect(20, 160, 120, 20), "Target Pos Y:");
        inputTargetY = GUI.TextField(new Rect(140, 160, 200, 20), inputTargetY);
        GUI.Label(new Rect(20, 190, 120, 20), "Target Pos Z:");
        inputTargetZ = GUI.TextField(new Rect(140, 190, 200, 20), inputTargetZ);

        GUI.Label(new Rect(20, 220, 120, 20), "Arc Height:");
        string ht = GUI.TextField(new Rect(140, 220, 200, 20), dropHeight.ToString("F2"));
        float.TryParse(ht, out dropHeight);

        GUI.Label(new Rect(20, 250, 120, 20), "Duration(s):");
        string dur = GUI.TextField(new Rect(140, 250, 200, 20), dropDuration.ToString("F2"));
        float.TryParse(dur, out dropDuration);

        if (GUI.Button(new Rect(20, 290, 320, 30), "Drop Kunai"))
        {
            if (float.TryParse(inputStartX, out float sx) &&
                float.TryParse(inputStartY, out float sy) &&
                float.TryParse(inputStartZ, out float sz) &&
                float.TryParse(inputTargetX, out float tx) &&
                float.TryParse(inputTargetY, out float ty) &&
                float.TryParse(inputTargetZ, out float tz))
            {
                Vector3 startPos = new Vector3(sx, sy, sz);
                Vector3 targetPos = new Vector3(tx, ty, tz);
                LaunchKunaiArc(startPos, targetPos);
            }
            else
            {
                Debug.LogError("Invalid input positions.");
            }
        }
    }

    /// <summary>
    /// Instantiate kunai and start arc drop coroutine.
    /// </summary>
    private void LaunchKunaiArc(Vector3 startPos, Vector3 targetPos)
    {
        if (kunaiPrefab == null)
        {
            Debug.LogError("Assign kunaiPrefab in inspector.");
            return;
        }

        GameObject kunai = Instantiate(kunaiPrefab);
        StartCoroutine(DropArcAndImpact(kunai.transform, startPos, targetPos));
    }

    /// <summary>
    /// Drops the kunai along a parabolic arc, and spawns impact effect prefab (with multiple particle systems) upon reaching target.
    /// </summary>
    private IEnumerator DropArcAndImpact(Transform obj, Vector3 startPos, Vector3 targetPos)
    {
        Vector3 launchPos = startPos + Vector3.up * dropHeight;
        obj.position = launchPos;

        float elapsed = 0f;
        while (elapsed < dropDuration)
        {
            float t = elapsed / dropDuration;
            Vector3 flat = Vector3.Lerp(launchPos, targetPos, t);
            float arc = 4f * dropHeight * t * (1 - t);
            obj.position = flat + Vector3.up * arc;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Move to final target position
        obj.position = targetPos;

        // Determine spawn position using an optional TipAnchor child
        Vector3 spawnPos = targetPos;
        Transform tipAnchor = obj.Find("TipAnchor");
        if (tipAnchor != null)
        {
            spawnPos = tipAnchor.position;
        }

        // Instantiate the impact effect prefab (container with multiple ParticleSystems)
        if (impactEffectPrefab != null)
        {
            GameObject fx = Instantiate(impactEffectPrefab, spawnPos, Quaternion.identity);
            // Ensure all child particle systems play
            var systems = fx.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in systems)
            {
                ps.Play();
            }
            // Optional: destroy effect after longest system duration
            float maxDur = 0f;
            foreach (var ps in systems)
                maxDur = Mathf.Max(maxDur, ps.main.duration + ps.main.startLifetime.constantMax);
            Destroy(fx, maxDur + 0.5f);
        }

        // Optional: destroy kunai after impact
        // Destroy(obj.gameObject);
    }
}