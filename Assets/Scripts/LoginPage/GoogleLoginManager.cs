using UnityEngine;

public class GoogleLoginManager : MonoBehaviour
{
    [SerializeField] private WebViewController webViewController;
    private string backendLoginUrl = "http://localhost:8000/api/v1/auth/login/google";

    // 예: 버튼 OnClick에 연결
    public void OnGoogleLoginClick()
    {
        // 예시로 그냥 직접 URL 로드 (테스트용)
        // 만약 백엔드에서 받은 auth_url을 여기에 전달하려면,
        // 아래처럼 웹 요청을 한 뒤 webViewController.OpenUrl(auth_url) 하면 됨
        webViewController.OpenUrl(backendLoginUrl);
    }

    public void OnCloseWebViewClick()
    {
        webViewController.SetVisibility(false);
    }
}
