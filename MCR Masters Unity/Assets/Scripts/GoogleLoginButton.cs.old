using UnityEngine;
using UnityEngine.UI;

public class GoogleLoginButton : MonoBehaviour
{
    // GoogleAuthManager를 드래그&드롭으로 연결
    public GoogleAuthManager googleAuthManager;
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            // 버튼 클릭 시, GoogleAuthManager의 StartGoogleLogin을 호출
            button.onClick.AddListener(() =>
            {
                if (googleAuthManager != null)
                {
                    googleAuthManager.StartGoogleLogin();
                }
                else
                {
                    Debug.LogError("GoogleAuthManager가 연결되지 않았습니다.");
                }
            });
        }
    }
}
