using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningBolt : MonoBehaviour
{
    [Header("번개 시작점 → 끝점(끝 위치)")]
    public Transform startPoint;    // 번개 출발 지점 (예: 하늘 위 특정 위치나 아무 Empty)
    public Transform endPoint;      // 번개 도착 지점 (예: 땅 위 특정 위치)

    [Header("번개 세부 설정")]
    [Range(2, 30)] public int segmentCount = 10;         // 분절 개수 (버텍스 개수 = segmentCount + 1)
    public float swayAmount = 0.5f;                      // 좌우로 얼마나 흔들릴지 (m 단위)
    public float jitterFrequency = 0.6f;                 // 노이즈 변동 속도
    public bool animateContinuously = true;              // 계속 요동시킬지 (loop)
    public float flickerIntensity = 0.2f;                // “빛 번쩍 움직임”을 줄 때 곱할 랜덤 요인 (0~1 범위)

    private LineRenderer lr;
    private Vector3[] positions;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        if (segmentCount < 2) segmentCount = 2;
        positions = new Vector3[segmentCount + 1];

        // LineRenderer 분절(Positions) 카운트 설정
        lr.positionCount = segmentCount + 1;
    }

    void Update()
    {
        if (startPoint == null || endPoint == null) return;

        // 두 지점 사이 Baseline: 똑바로 이어지는 방향, 나중에 이 방향에 수직으로 스웨이(흔들림)을 줄 것
        Vector3 baselineDirection = endPoint.position - startPoint.position;
        float segmentLength = baselineDirection.magnitude / segmentCount;
        Vector3 unitDir = baselineDirection.normalized;

        // 선형으로 배치된 기준점 배열 생성
        positions[0] = startPoint.position;
        for (int i = 1; i < segmentCount; i++)
        {
            // i번째 분절 지점의 기본 위치 = start + i * (unitDir * segmentLength)
            Vector3 pointOnLine = startPoint.position + unitDir * (segmentLength * i);

            // 수직 축을 구하기 위해 baselineDirection과 90도에 가까운 벡터를 하나 생성
            Vector3 perp = Vector3.Cross(baselineDirection, Vector3.up).normalized;
            if (perp == Vector3.zero)  // 만약 baseline 이 수직(up) 벡터와 거의 평행이라면
                perp = Vector3.Cross(baselineDirection, Vector3.right).normalized;

            // Perlin 노이즈로 약간의 흔들림(Offset)을 계산
            float noise = (Mathf.PerlinNoise(i * jitterFrequency, Time.time * jitterFrequency) - 0.5f) * 2f;
            Vector3 offset = perp * (noise * swayAmount);

            // 위치 + 흔들림 합쳐서 최종 좌표 저장
            positions[i] = pointOnLine + offset;
        }
        positions[segmentCount] = endPoint.position;

        // 매 프레임마다 살짝 밝기나 얇기 등을 변경하려면 flickerIntensity를 곱해서 조절 가능
        float flicker = 1f + (Random.value - 0.5f) * flickerIntensity;
        lr.widthMultiplier = flicker;  // 라인 너비에 약간 랜덤 요동을 줘서 번쩍이는 느낌 강화

        // LineRenderer에 새 좌표 배열을 한꺼번에 넘겨 줍니다.
        lr.SetPositions(positions);

        // animateContinuously가 false라면, 첫 1프레임만 랜덤 → 그 뒤에는 고정
        if (!animateContinuously)
        {
            this.enabled = false;
        }
    }
}
