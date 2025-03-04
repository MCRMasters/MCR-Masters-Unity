using UnityEngine;

public class LockTransform : MonoBehaviour
{
    private RectTransform canvasRect;

    void Start()
    {
        // "Main Canvas"의 RectTransform 가져오기
        canvasRect = GameObject.Find("Main Canvas").GetComponent<RectTransform>();

        // 오브젝트 이름에 따라 위치, 회전, 크기 고정
        switch (gameObject.name)
        {
            // 기존 오브젝트
            case "EnemyFuroFieldKami":
                ApplyOffsetTransform(new Vector3(-749, -460, -14.5603f), new Vector3(0, 0, 270), Vector3.one);
                break;
            case "EnemyFuroFieldToi":
                ApplyOffsetTransform(new Vector3(-758, 446, -14.5603f), new Vector3(0, 0, 180), Vector3.one);
                break;
            case "EnemyFuroFieldShimo":
                ApplyOffsetTransform(new Vector3(721, 459, -14.5603f), new Vector3(0, 0, 90), Vector3.one);
                break;
            case "PlayerFuroField":
                ApplyOffsetTransform(new Vector3(747, -388.76f, -14.5603f), Vector3.zero, Vector3.one);
                break;
            case "EnemyFlowerField3":
                ApplyOffsetTransform(new Vector3(-870, -401, -1.740234f), new Vector3(0, 0, -90), Vector3.one);
                break;
            case "EnemyFlowerField2":
                ApplyOffsetTransform(new Vector3(-823, 456, -1.740234f), new Vector3(0, 0, 180), Vector3.one);
                break;
            case "EnemyFlowerField1":
                ApplyOffsetTransform(new Vector3(866, 406, -1.740234f), new Vector3(0, 0, 90), Vector3.one);
                break;
            case "PlayerFlowerField":
                ApplyOffsetTransform(new Vector3(813, -452.5f, -1.740234f), Vector3.zero, Vector3.one);
                break;
            case "PlayerHaipai":
                ApplyCenteredTransform(new Vector3(255, -879.74f, -12.19625f), Vector3.zero, Vector3.one);
                break;
            case "PlayerKawa":
                ApplyCenteredTransform(new Vector3(810, -639.74f, -15.75012f), Vector3.zero, Vector3.one);
                break;

            // 추가된 오브젝트
            case "EnemyHaipaiToi":
                ApplyCenteredTransform(new Vector3(1273, -127, -12.19625f), new Vector3(0, 0, 180), Vector3.one);
                break;
            case "EnemyHaipaiShimo":
                ApplyCenteredTransform(new Vector3(1738, -904, -12.19625f), new Vector3(0, 0, 90), Vector3.one);
                break;
            case "EnemyHaipaiKami":
                ApplyCenteredTransform(new Vector3(168, -148, -12.19625f), new Vector3(0, 0, -90), Vector3.one);
                break;
            case "EnemyKawaToi":
                ApplyCenteredTransform(new Vector3(1110, -367, -15.75012f), new Vector3(0, 0, 180), Vector3.one);
                break;
            case "EnemyKawaKami":
                ApplyCenteredTransform(new Vector3(770, -350, -15.75012f), new Vector3(0, 0, -90), Vector3.one);
                break;
            case "EnemyKawaShimo":
                ApplyCenteredTransform(new Vector3(1149, -651, -15.75012f), new Vector3(0, 0, 90), Vector3.one);
                break;
        }
    }

    // Canvas의 Y축 길이만큼 오프셋 적용
    void ApplyCenteredTransform(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        // Canvas 중심 + Y축 길이만큼 오프셋 추가
        Vector3 centeredPosition = new Vector3(
            position.x - canvasRect.rect.width / 2,
            position.y + canvasRect.rect.height / 2,
            position.z
        );

        transform.localPosition = centeredPosition;
        transform.localEulerAngles = rotation;
        transform.localScale = scale;
    }

    void ApplyOffsetTransform(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        transform.localPosition = position;
        transform.localEulerAngles = rotation;
        transform.localScale = scale;
    }

    void LateUpdate()
    {
        // 실행 도중에도 고정 유지
        Start();
    }
}
