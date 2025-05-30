using UnityEngine;
using System.Collections;

namespace MCRGame.View
{
    public class CameraResultAnimator : MonoBehaviour
    {
        // [1] "기본" 상태 (Transform)
        private Vector3 defaultPosition = new Vector3(0f, 60f, -40f);
        private Vector3 defaultEulerAngles = new Vector3(60f, 0f, 0f);

        // [2] "결과 고지" 상태 (Transform)
        private Vector3 changedPosition = new Vector3(0f, 100f, -40f);
        private Vector3 changedEulerAngles = new Vector3(70f, 0f, 0f);

        // [3] 이동 애니메이션 시간 (초 단위)
        [SerializeField] private float moveDuration = 0.3f;

        // 코루틴 중첩 방지용
        private Coroutine resultCoroutine = null;

        private void Start()
        {
            // 시작 시 기본값으로 세팅
            transform.position = defaultPosition;
            transform.rotation = Quaternion.Euler(defaultEulerAngles);
        }

        /// <summary>
        /// 결과 상태(상단 카메라 뷰)로 부드럽게 전환
        /// </summary>
        public IEnumerator PlayResultAnimation()
        {
            if (resultCoroutine != null)
            {
                StopCoroutine(resultCoroutine);
            }
            resultCoroutine = StartCoroutine(AnimateToResultState());
            yield return resultCoroutine;
        }

        /// <summary>
        /// 즉시 기본 상태(하단 카메라 뷰)로 복귀
        /// </summary>
        public void ResetCameraState()
        {
            if (resultCoroutine != null)
            {
                StopCoroutine(resultCoroutine);
                resultCoroutine = null;
            }
            transform.position = defaultPosition;
            transform.rotation = Quaternion.Euler(defaultEulerAngles);
        }

        /// <summary>
        /// 실제로 카메라를 결과 상태로 움직이는 코루틴 (Quadratic Easing 예시)
        /// </summary>
        private IEnumerator AnimateToResultState()
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            Quaternion endRot = Quaternion.Euler(changedEulerAngles);

            float elapsed = 0f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / moveDuration);
                // 가속 이징: t^2
                float easeT = t * t;

                transform.position = Vector3.Lerp(startPos, changedPosition, easeT);
                transform.rotation = Quaternion.Slerp(startRot, endRot, easeT);
                yield return null;
            }

            // 애니메이션 종료 후 최종 값 확정
            transform.position = changedPosition;
            transform.rotation = endRot;
        }
    }
}
