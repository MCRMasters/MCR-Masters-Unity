using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GoogleLoginManager : MonoBehaviour
{
    [SerializeField] private WebViewController webViewController;
    [SerializeField] private GetLoginApi getLogin; // GetLogin 컴포넌트를 에디터에서 할당하세요.

    /// <summary>
    /// 구글 로그인 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnGoogleLoginClick()
    {
        StartCoroutine(getLogin.RequestGoogleAuthUrl(OnAuthUrlReceived, OnRequestError));
    }

    /// <summary>
    /// auth_url을 성공적으로 받아왔을 때 호출됩니다.
    /// </summary>
    /// <param name="authUrl">백엔드에서 받아온 auth_url</param>
    private void OnAuthUrlReceived(string authUrl)
    {
        // WebViewController를 통해 auth_url 로드
        webViewController.OpenUrl(authUrl);

        // 토큰 수신 시 처리할 콜백 등록
        webViewController.OnTokenReceived = OnGoogleAuthCallbackReceived;
    }

    /// <summary>
    /// auth_url 요청 실패 시 호출됩니다.
    /// </summary>
    /// <param name="error">에러 메시지</param>
    private void OnRequestError(string error)
    {
        Debug.LogError("로그인 URL 요청 실패: " + error);
    }

    /// <summary>
    /// WebViewController로부터 토큰이 전달되면 호출됩니다.
    /// </summary>
    private void OnGoogleAuthCallbackReceived(TokenResponse token)
    {
        Debug.Log($"[GoogleLoginManager] AccessToken = {token.access_token}");
        Debug.Log($"[GoogleLoginManager] RefreshToken = {token.refresh_token}");
        Debug.Log($"[GoogleLoginManager] isNewUser = {token.is_new_user}");

        // PlayerDataManager에 토큰 정보 저장
        PlayerDataManager.Instance.SetTokenData(token.access_token, token.refresh_token, token.is_new_user);

        // 로그인 완료 처리 (예: 씬 전환 등)
        SceneManager.LoadScene("LobbyScene");
    }
}

[System.Serializable]
public class TokenResponse
{
    public string access_token;
    public string refresh_token;
    public bool is_new_user;
    public string token_type;
}
