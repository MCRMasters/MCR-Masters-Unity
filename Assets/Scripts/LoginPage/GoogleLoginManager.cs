using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class GoogleLoginManager : MonoBehaviour
{
    [SerializeField] private WebViewController webViewController;
    private string backendLoginUrl = "http://localhost:8000/api/v1/auth/login/google";

    /// <summary>
    /// 구글 로그인 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnGoogleLoginClick()
    {
        StartCoroutine(RequestGoogleAuthUrl());
    }

    /// <summary>
    /// 백엔드에서 auth_url을 GET 요청으로 받아와 WebViewController에 전달합니다.
    /// </summary>
    private IEnumerator RequestGoogleAuthUrl()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(backendLoginUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                var authData = JsonUtility.FromJson<AuthUrlResponse>(json);

                // WebViewController를 통해 auth_url 로드
                webViewController.OpenUrl(authData.auth_url);

                // 토큰 수신 시 처리할 콜백 등록
                webViewController.OnTokenReceived = OnGoogleAuthCallbackReceived;
            }
            else
            {
                Debug.LogError("로그인 URL 요청 실패: " + www.error);
            }
        }
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

    [System.Serializable]
    private class AuthUrlResponse
    {
        public string auth_url;
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
