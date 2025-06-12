using UnityEngine;
using System.Collections;

/// <summary>
/// kunaiPrefab을 생성하여
/// 1) GUI 입력 좌표로 포물선 이동하거나
/// 2) 지정된 실린더(Transform)를 실시간으로 따라가도록 할 수 있습니다.
/// Attach this to an empty GameObject and assign kunaiPrefab and optional followCylinder.
/// </summary>
public class KunaiDropManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Kunai prefab to instantiate")]
    public GameObject kunaiPrefab;
    [Tooltip("Optional Transform of a cylinder to follow instead of arc drop")]
    public Transform followTargetCylinder;

    [Header("Arc Drop Settings")]
    [Tooltip("Height offset above start pos")]
    public float dropHeight = 5f;
    [Tooltip("Duration for the arc drop")]
    public float dropDuration = 1.0f;

    // GUI input fields for start position
    private string inputStartX = "0";
    private string inputStartY = "0";
    private string inputStartZ = "0";

    void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 320, 260), "Kunai Controller");

        // Arc drop inputs
        GUI.Label(new Rect(20, 40, 120, 20), "Start Pos X:");
        inputStartX = GUI.TextField(new Rect(140, 40, 160, 20), inputStartX);
        GUI.Label(new Rect(20, 70, 120, 20), "Start Pos Y:");
        inputStartY = GUI.TextField(new Rect(140, 70, 160, 20), inputStartY);
        GUI.Label(new Rect(20, 100, 120, 20), "Start Pos Z:");
        inputStartZ = GUI.TextField(new Rect(140, 100, 160, 20), inputStartZ);

        GUI.Label(new Rect(20, 130, 120, 20), "Arc Height:");
        string ht = GUI.TextField(new Rect(140, 130, 160, 20), dropHeight.ToString("F2"));
        float.TryParse(ht, out dropHeight);

        GUI.Label(new Rect(20, 160, 120, 20), "Duration(s):");
        string dur = GUI.TextField(new Rect(140, 160, 160, 20), dropDuration.ToString("F2"));
        float.TryParse(dur, out dropDuration);

        if (GUI.Button(new Rect(20, 190, 280, 30), "Drop to (0,0,0) Arc"))
        {
            if (float.TryParse(inputStartX, out float sx) &&
                float.TryParse(inputStartY, out float sy) &&
                float.TryParse(inputStartZ, out float sz))
            {
                Vector3 startPos = new Vector3(sx, sy, sz);
                LaunchKunaiArc(startPos, Vector3.zero);
            }
            else Debug.LogError("Invalid start position.");
        }

        // Follow target cylinder
        GUI.Label(new Rect(20, 230, 200, 20), "Follow Target Cylinder:");
        if (followTargetCylinder != null)
            GUI.Label(new Rect(180, 230, 140, 20), followTargetCylinder.name);
        if (GUI.Button(new Rect(20, 255, 280, 30), "Spawn & Follow Cylinder"))
        {
            if (followTargetCylinder != null)
                SpawnAndFollow(followTargetCylinder);
            else
                Debug.LogError("Assign followTargetCylinder in inspector.");
        }
    }

    /// <summary>
    /// Instantiate and drop in an arc.
    /// </summary>
    private void LaunchKunaiArc(Vector3 startPos, Vector3 targetPos)
    {
        if (kunaiPrefab == null) { Debug.LogError("Assign kunaiPrefab."); return; }
        GameObject k = Instantiate(kunaiPrefab);
        StartCoroutine(DropArc(k.transform, startPos, targetPos));
    }

    /// <summary>
    /// Instantiate and follow the cylinder's transform.
    /// </summary>
    private void SpawnAndFollow(Transform target)
    {
        if (kunaiPrefab == null) { Debug.LogError("Assign kunaiPrefab."); return; }
        GameObject k = Instantiate(kunaiPrefab);
        StartCoroutine(FollowCoroutine(k.transform, target));
    }

    private IEnumerator DropArc(Transform obj, Vector3 startPos, Vector3 targetPos)
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
        obj.position = targetPos;
    }

    private IEnumerator FollowCoroutine(Transform obj, Transform target)
    {
        while (target != null)
        {
            obj.position = target.position;
            obj.rotation = target.rotation;
            yield return null;
        }
    }
}