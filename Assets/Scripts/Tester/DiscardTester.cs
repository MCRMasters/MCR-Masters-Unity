using UnityEngine;
using DG.Tweening;

public class DiscardTester : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("손 모델 프리팹")]
    public GameObject handPrefab;
    [Tooltip("타일 프리팹")]
    public GameObject tilePrefab;

    [Header("Animation Settings")]
    [Tooltip("이동 시간 (초)")]
    public float moveDuration = 0.5f;
    [Tooltip("손 오프셋")]
    public Vector3 handOffset = new Vector3(0f, 0.1f, 0f);

    // 런타임 GUI로만 설정하는 위치값
    private Vector3 startPos = Vector3.zero;
    private Vector3 discardPos = Vector3.zero;

    // 현재 생성된 인스턴스
    private GameObject currentHand;
    private GameObject currentTile;

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 320, 260), "Discard Tester", GUI.skin.window);

        // Start 위치 입력
        GUILayout.Label("Start Position:");
        startPos = DrawVector3Field(startPos);

        GUILayout.Space(10);

        // Discard 위치 입력
        GUILayout.Label("Discard Position:");
        discardPos = DrawVector3Field(discardPos);

        GUILayout.Space(20);

        // 실행 버튼
        if (GUILayout.Button("Run Discard Animation", GUILayout.Height(30)))
        {
            PlayDiscardAnimation();
        }

        GUILayout.EndArea();
    }

    // Vector3 입력 필드 헬퍼
    private Vector3 DrawVector3Field(Vector3 v)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("X", GUILayout.Width(15));
        string sx = GUILayout.TextField(v.x.ToString("F2"), GUILayout.Width(60));
        GUILayout.Label("Y", GUILayout.Width(15));
        string sy = GUILayout.TextField(v.y.ToString("F2"), GUILayout.Width(60));
        GUILayout.Label("Z", GUILayout.Width(15));
        string sz = GUILayout.TextField(v.z.ToString("F2"), GUILayout.Width(60));
        GUILayout.EndHorizontal();

        float.TryParse(sx, out v.x);
        float.TryParse(sy, out v.y);
        float.TryParse(sz, out v.z);
        return v;
    }

    private void PlayDiscardAnimation()
    {
        if (handPrefab == null || tilePrefab == null)
        {
            Debug.LogError("[DiscardTester] Prefab이 할당되지 않았습니다.");
            return;
        }

        // 이전 인스턴스 정리
        if (currentHand != null) Destroy(currentHand);
        if (currentTile != null) Destroy(currentTile);

        // 손과 타일 생성 및 애니메이션
        currentHand = Instantiate(handPrefab, startPos + handOffset, Quaternion.identity);
        currentTile = Instantiate(tilePrefab, startPos, Quaternion.identity);

        var seq = DOTween.Sequence();
        seq.Append(currentHand.transform.DOMove(discardPos + handOffset, moveDuration));
        seq.Join(currentTile.transform.DOMove(discardPos, moveDuration));
        seq.OnComplete(() =>
        {
            Destroy(currentHand, 1f);
            Destroy(currentTile, 1f);
        });
        seq.Play();
    }
}
