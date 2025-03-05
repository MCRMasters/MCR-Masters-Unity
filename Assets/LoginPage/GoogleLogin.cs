using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GoogleLogin : MonoBehaviour
{
    private string authUrl = "http://0.0.0.0:8000/api/v1/auth/login/google";
    private string callbackUrl = "http://0.0.0.0:8000/api/v1/auth/login/google/callback?code=";

    public void StartGoogleLogin()
    {
        StartCoroutine(GetAuthUrl());
    }

    IEnumerator GetAuthUrl()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(authUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                AuthResponse response = JsonUtility.FromJson<AuthResponse>(jsonResponse);
                Application.OpenURL(response.auth_url);
            }
            else
            {
                Debug.LogError("❌ Failed to get Google Auth URL: " + request.error);
            }
        }
    }

    public void OnLoginCallback(string authCode)
    {
        StartCoroutine(ExchangeCodeForToken(authCode));
    }

    IEnumerator ExchangeCodeForToken(string code)
    {
        Debug.Log("🔄 Google 로그인 완료, Access Token 요청 중..."); // ✅ 로그 추가

        using (UnityWebRequest request = UnityWebRequest.Get(callbackUrl + code))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                TokenResponse response = JsonUtility.FromJson<TokenResponse>(jsonResponse);

                Debug.Log("✅ Access Token Received: " + response.access_token);
                WebSocketManager.Instance.ConnectWebSocket(response.access_token);
            }
            else
            {
                Debug.LogError("❌ Access Token 요청 실패: " + request.error);
            }
        }
    }


    [System.Serializable]
    private class AuthResponse
    {
        public string auth_url;
    }

    [System.Serializable]
    private class TokenResponse
    {
        public string access_token;
    }
}
