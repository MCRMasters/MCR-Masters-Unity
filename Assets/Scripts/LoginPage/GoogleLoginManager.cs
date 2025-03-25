using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class GoogleLoginManager : MonoBehaviour
{
    private string backendLoginUrl = "http://localhost:8000/api/v1/auth/login/google";

    public async void OnGoogleLoginClick()
    {
        // 1. HTTP 리스너 열기
        GoogleLoginCallbackListener.StartListening(OnGoogleAuthCallbackReceived);

        // 2. GET /auth/login/google
        using (UnityWebRequest www = UnityWebRequest.Get(backendLoginUrl))
        {
            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                var authData = JsonUtility.FromJson<AuthUrlResponse>(json);

                // 3. 브라우저 열기
                Application.OpenURL(authData.auth_url);
            }
            else
            {
                Debug.LogError("로그인 URL 요청 실패");
            }
        }
    }

    private void OnGoogleAuthCallbackReceived(string accessToken, string refreshToken)
    {
        Debug.Log($"엑세스 토큰: {accessToken}");
        Debug.Log($"리프레시 토큰: {refreshToken}");

        // 로그인 완료 처리 등등
    }

    [System.Serializable]
    private class AuthUrlResponse
    {
        public string auth_url;
    }
}
