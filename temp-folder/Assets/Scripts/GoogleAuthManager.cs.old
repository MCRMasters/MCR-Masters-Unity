using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

[Serializable]
public class AuthUrlResponse
{
    public string auth_url;  // FastAPI에서 반환하는 JSON 필드
}

public class GoogleAuthManager : MonoBehaviour
{
    // FastAPI 서버에서 "구글 인증 URL"을 내려주는 엔드포인트 (예: GET /api/v1/auth/login/google)
    [SerializeField] private string getAuthUrlEndpoint = "http://0.0.0.0:8000/api/v1/auth/login/google";

    // FastAPI 서버에서 "인증 코드"를 처리하는 엔드포인트 (예: POST /api/v1/auth/login/google/callback)
    [SerializeField] private string sendCodeEndpoint = "http://0.0.0.0:8000/api/v1/auth/login/google/callback";

    /// <summary>
    /// 1) 구글 인증 URL 요청 → 2) 브라우저 열기
    /// 예: 버튼 클릭 시 이 함수를 호출
    /// </summary>
    public void StartGoogleLogin()
    {
        Debug.Log("[GoogleAuthManager] StartGoogleLogin clicked.");
        StartCoroutine(GetAuthUrlAndOpenBrowser());
    }

    /// <summary>
    /// FastAPI 서버에 GET 요청해서 { "auth_url": "..." } 형태를 받으면
    /// 해당 URL로 브라우저 열기 (Application.OpenURL)
    /// </summary>
    private IEnumerator GetAuthUrlAndOpenBrowser()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(getAuthUrlEndpoint))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // JSON 파싱
                string json = request.downloadHandler.text;
                Debug.Log($"[GoogleAuthManager] Server Response: {json}");

                // 예: { "auth_url": "https://accounts.google.com/o/oauth2/auth?client_id=..." }
                AuthUrlResponse response = JsonUtility.FromJson<AuthUrlResponse>(json);

                if (response != null && !string.IsNullOrEmpty(response.auth_url))
                {
                    Debug.Log($"[GoogleAuthManager] Opening Google Auth URL: {response.auth_url}");
                    Application.OpenURL(response.auth_url);
                }
                else
                {
                    Debug.LogError("[GoogleAuthManager] Invalid auth_url in response.");
                }
            }
            else
            {
                Debug.LogError($"[GoogleAuthManager] Error requesting auth URL: {request.error}");
            }
        }
    }

    /// <summary>
    /// (옵션) 구글 로그인 성공 후, 서버가 code를 Unity로 넘겨주거나
    /// Unity가 별도 로직으로 code를 획득했다면, 아래 메서드로 서버에 code를 전송
    /// </summary>
    public void OnGoogleLoginSuccess(string authCode)
    {
        Debug.Log($"[GoogleAuthManager] OnGoogleLoginSuccess with code: {authCode}");
        StartCoroutine(SendAuthCodeToServer(authCode));
    }

    /// <summary>
    /// code를 서버에 POST -> 토큰 교환
    /// FastAPI에서 인증 토큰 발급(또는 DB 저장) 후 필요한 응답을 준다는 가정
    /// </summary>
    private IEnumerator SendAuthCodeToServer(string authCode)
    {
        WWWForm form = new WWWForm();
        form.AddField("code", authCode);

        using (UnityWebRequest www = UnityWebRequest.Post(sendCodeEndpoint, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Token Received: " + www.downloadHandler.text);
                // 여기서 받은 토큰을 저장하고, 이후 API 요청 시 활용 가능
            }
            else
            {
                Debug.LogError("Error: " + www.error);
            }
        }
    }
}
