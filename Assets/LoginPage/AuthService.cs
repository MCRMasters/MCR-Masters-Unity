using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public static class AuthService
{
    public static IEnumerator GetAuthUrl(string authUrl, Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(authUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                AuthResponse response = JsonUtility.FromJson<AuthResponse>(jsonResponse);
                callback?.Invoke(response.auth_url);
            }
            else
            {
                Debug.LogError("❌ Failed to get Google Auth URL: " + request.error);
                callback?.Invoke(null);
            }
        }
    }

    public static IEnumerator ExchangeCodeForToken(string callbackUrl, string code, Action<string> callback)
    {
        Debug.Log("🔄 Google 로그인 완료, Access Token 요청 중...");

        using (UnityWebRequest request = UnityWebRequest.Get(callbackUrl + code))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                TokenResponse response = JsonUtility.FromJson<TokenResponse>(jsonResponse);
                callback?.Invoke(response.access_token);
            }
            else
            {
                Debug.LogError("❌ Access Token 요청 실패: " + request.error);
                callback?.Invoke(null);
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
