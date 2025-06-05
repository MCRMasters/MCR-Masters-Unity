using UnityEngine;

/// <summary>
/// Cylinder와 Circle 두 오브젝트에 "clip" 프로퍼티를  
/// 시간에 따라 0 → 1로 서서히 증가시키며 페이드아웃(사라짐)을 구현하는 매니저 클래스
/// </summary>
public class ClipFadeController : MonoBehaviour
{
    [Header("메시에 할당된 Renderer들")]
    [Tooltip("실린더 메쉬의 MeshRenderer")]
    public MeshRenderer cylinderRenderer;
    [Tooltip("원형(Circle) 메쉬의 MeshRenderer")]
    public MeshRenderer circleRenderer;

    [Header("Fade 설정")]
    [Tooltip("clip 값이 0에서 1로 올라가는 데 걸리는 전체 시간(초)")]
    public float fadeDuration = 2.0f;
    [Tooltip("clip 값 증가 시작 딜레이(초)")]
    public float startDelay = 0.0f;

    // 내부적으로 MaterialPropertyBlock을 미리 만들어서 재사용
    private MaterialPropertyBlock _mpbCylinder;
    private MaterialPropertyBlock _mpbCircle;

    private float _elapsed = 0f;
    private bool _isFading = false;

    void Awake()
    {
        // 1) Renderer들이 제대로 설정되어 있는지 확인
        if (cylinderRenderer == null || circleRenderer == null)
        {
            Debug.LogError("ClipFadeController: Cylinder 또는 Circle의 MeshRenderer가 할당되지 않았습니다.");
            enabled = false;
            return;
        }

        // 2) 두 Renderer에 사용할 MaterialPropertyBlock을 각각 초기화
        _mpbCylinder = new MaterialPropertyBlock();
        _mpbCircle = new MaterialPropertyBlock();

        // 3) 두 오브젝트가 동일 머티리얼 에셋을 참조 중이라면,  
        //    Material 인스턴스를 만들어서 Renderer.material에 할당해 주세요.  
        //    (즉, “씬의 실린더/원형마다 고유 Material 인스턴스”를 써야  
        //     서로 다른 clip 값을 줄 때 내부적으로 인스턴싱해서 Draw Call이 늘어나는 걸 방지할 수 있음)

        //  만약 “A번 인스턴싱 방식을 이미 적용했다”면 이 부분은 필요 없으니 건너뛰세요:
        Material cylMatInst = Instantiate(cylinderRenderer.sharedMaterial);
        cylinderRenderer.material = cylMatInst;
        Material cirMatInst = Instantiate(circleRenderer.sharedMaterial);
        circleRenderer.material = cirMatInst;

        // 4) 초기 설정: 두 오브젝트 clip 값을 0으로 세팅(완전 숨김)
        cylinderRenderer.GetPropertyBlock(_mpbCylinder);
        _mpbCylinder.SetFloat("_clip", 0f);
        cylinderRenderer.SetPropertyBlock(_mpbCylinder);

        circleRenderer.GetPropertyBlock(_mpbCircle);
        _mpbCircle.SetFloat("_clip", 0f);
        circleRenderer.SetPropertyBlock(_mpbCircle);
    }

    void Start()
    {
        // startDelay 이후에 페이드 애니메이션 시작
        if (startDelay <= 0f)
        {
            _isFading = true;
        }
        else
        {
            Invoke(nameof(BeginFade), startDelay);
        }
    }

    void BeginFade()
    {
        _isFading = true;
        _elapsed = 0f;
    }

    void Update()
    {
        if (!_isFading) return;

        // 1) 경과 시간 누적
        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / fadeDuration);  // 0~1

        // 2) t 만큼 clip 값을 증가(0->1)
        float clipValue = Mathf.Lerp(0f, 1f, t);

        // 3) Cylinder에 PropertyBlock으로 clip 값 전달
        cylinderRenderer.GetPropertyBlock(_mpbCylinder);
        _mpbCylinder.SetFloat("_clip", clipValue);
        cylinderRenderer.SetPropertyBlock(_mpbCylinder);

        // 4) Circle에도 동일하게 전달 (원한다면 두 오브젝트 다른 속도로도 가능)
        circleRenderer.GetPropertyBlock(_mpbCircle);
        _mpbCircle.SetFloat("_clip", clipValue);
        circleRenderer.SetPropertyBlock(_mpbCircle);

        // 5) 애니메이션이 끝나면 _isFading 끄기
        if (t >= 1f)
        {
            _isFading = false;
            // 필요하다면 두 오브젝트를 비활성화하거나 Destroy 처리
            // gameObject.SetActive(false);
        }
    }
}
