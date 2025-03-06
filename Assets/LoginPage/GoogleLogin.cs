using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GoogleLogin : MonoBehaviour
{
    private string authUrl = "http://0.0.0.0:8000/api/v1/auth/login/google";
    private string callbackUrl = "http://0.0.0.0:8000/api/v1/auth/login/google/callback?code=";
    private string gameServerAuthUrl = "http://0.0.0.0:9000/api/v1/game/auth"; // 게임 서버 인증 API

    public void StartGoogleLogin()
    {
        StartCoroutine(AuthService.GetAuthUrl(authUrl, OnAuthUrlReceived));
    }

    private void OnAuthUrlReceived(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            Application.OpenURL(url);
        }
        else
        {
            Debug.LogError("❌ Failed to get Google Auth URL.");
        }
    }

    public void OnLoginCallback(string authCode)
    {
        StartCoroutine(AuthService.ExchangeCodeForToken(callbackUrl, authCode, OnTokenReceived));
    }

    private void OnTokenReceived(string accessToken)
    {
        if (!string.IsNullOrEmpty(accessToken))
        {
            Debug.Log("✅ Access Token Received: " + accessToken);
            StartCoroutine(GameServerService.AuthenticateWithGameServer(gameServerAuthUrl, accessToken));
        }
        else
        {
            Debug.LogError("❌ Failed to receive access token.");
        }
    }
}
