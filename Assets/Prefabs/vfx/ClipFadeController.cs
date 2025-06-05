using UnityEngine;
using System.Collections;

/// <summary>
/// 1) Cylinder는 씬 시작과 동시에 “위(높은 Y 위치)에서 아래(목표 위치)로 0.5초 동안 내려오는 연출”
/// 2) Cylinder가 목표 위치에 도달한 뒤에는 2초간 그대로 유지
/// 3) 그 뒤 Circle(물 퍼짐)이 활성화되어 1초 동안 페이드아웃(clip 0→1)
/// </summary>
public class CylinderDropController : MonoBehaviour
{
    [Header("Cylinder (기둥) 설정")]
    [Tooltip("실린더 Transform (MeshRenderer 등을 포함)")]
    public Transform cylinderTransform;

    [Tooltip("Cylinder가 최종적으로 놓일 위치")]
    public Vector3 targetPosition = Vector3.zero;
    [Tooltip("시작 시 Cylinder를 목표 위치에서 얼마나 위로 띄울지")]
    public float startHeightOffset = 5.0f;
    [Tooltip("Cylinder가 위→아래로 내려오는 데 걸리는 시간(초)")]
    public float dropDuration = 0.5f;
    [Tooltip("Cylinder가 목표 위치에 도달한 뒤 머무를 시간(초)")]
    public float keepDuration = 2.0f;

    [Header("Circle(물 퍼짐) 설정")]
    [Tooltip("Cylinder가 떨어진 뒤 활성화할 Circle MeshRenderer")]
    public MeshRenderer circleRenderer;
    [Tooltip("Circle이 페이드아웃될 때 걸리는 시간(clip 0→1)")]
    public float circleFadeDuration = 1.0f;
    [Tooltip("Cylinder 도착 + 유지가 끝난 뒤 Circle을 활성화하기 전 추가 딜레이(초)")]
    public float circleStartDelay = 0.0f;

    // 내부에서 쓸 MaterialPropertyBlock
    private MaterialPropertyBlock _mpbCircle;

    void Awake()
    {
        if (cylinderTransform == null || circleRenderer == null)
        {
            Debug.LogError("CylinderDropController: cylinderTransform 또는 circleRenderer가 할당되지 않았습니다.");
            enabled = false;
            return;
        }

        // Circle은 처음에 비활성화
        _mpbCircle = new MaterialPropertyBlock();
        circleRenderer.GetPropertyBlock(_mpbCircle);
        _mpbCircle.SetFloat("_clip", 0f);
        circleRenderer.SetPropertyBlock(_mpbCircle);
        circleRenderer.gameObject.SetActive(false);

        // Cylinder를 목표 위치 위쪽(startHeightOffset)으로 옮겨 놓음
        Vector3 startPos = targetPosition + Vector3.up * startHeightOffset;
        cylinderTransform.position = startPos;
    }

    void Start()
    {
        // Drop 시퀀스를 바로 실행
        StartCoroutine(DropSequence());
    }

    private IEnumerator DropSequence()
    {
        // 1) Cylinder를 위(목표위치+오프셋) → 아래(목표위치)로 dropDuration 동안 Lerp
        Vector3 startPos = targetPosition + Vector3.up * startHeightOffset;
        Vector3 endPos = targetPosition;
        float elapsed = 0f;

        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dropDuration);
            cylinderTransform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // 확실히 목표 위치에 세팅
        cylinderTransform.position = endPos;

        // 2) 목표 위치에 도착한 뒤 keepDuration 동안 대기
        if (keepDuration > 0f)
            yield return new WaitForSeconds(keepDuration);

        // 3) Cylinder 유지 끝난 뒤 Circle 활성화 및 페이드아웃
        if (circleStartDelay > 0f)
            yield return new WaitForSeconds(circleStartDelay);

        circleRenderer.gameObject.SetActive(true);
        StartCoroutine(CircleFadeRoutine());
    }

    private IEnumerator CircleFadeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < circleFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / circleFadeDuration);
            float clipValue = Mathf.Lerp(0f, 1f, t);

            circleRenderer.GetPropertyBlock(_mpbCircle);
            _mpbCircle.SetFloat("_clip", clipValue);
            circleRenderer.SetPropertyBlock(_mpbCircle);

            yield return null;
        }

        // 확실히 clip=1 (완전 사라짐) 세팅
        circleRenderer.GetPropertyBlock(_mpbCircle);
        _mpbCircle.SetFloat("_clip", 1f);
        circleRenderer.SetPropertyBlock(_mpbCircle);

        // 필요하다면 이 시점에 오브젝트 비활성화
        // circleRenderer.gameObject.SetActive(false);
    }
}
