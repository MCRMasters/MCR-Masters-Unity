using UnityEngine;
using MCRGame.View;

namespace MCRGame.Tester
{
    public class CameraResultAnimatorTester : MonoBehaviour
    {
        // Inspector에서 할당: 테스트할 CameraResultAnimator 컴포넌트
        public CameraResultAnimator cameraResultAnimator;

        private void OnGUI()
        {
            // PlayResultAnimation 버튼: 카메라 애니메이션 실행
            if (GUI.Button(new Rect(10, 10, 200, 50), "Play Result Animation"))
            {
                if (cameraResultAnimator != null)
                {
                    cameraResultAnimator.PlayResultAnimation();
                }
                else
                {
                    Debug.LogWarning("CameraResultAnimator 참조가 할당되지 않았습니다.");
                }
            }
            
            // ResetCameraState 버튼: 카메라를 즉시 기본 상태로 복귀
            if (GUI.Button(new Rect(10, 70, 200, 50), "Reset Camera State"))
            {
                if (cameraResultAnimator != null)
                {
                    cameraResultAnimator.ResetCameraState();
                }
                else
                {
                    Debug.LogWarning("CameraResultAnimator 참조가 할당되지 않았습니다.");
                }
            }
        }
    }
}
