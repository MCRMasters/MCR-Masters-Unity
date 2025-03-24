using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
public class AuthUrlResponse
{
    public string auth_url;
}

[System.Serializable]
public class LoginStatusResponse
{
    public bool logged_in;
    public string access_token;
}

public class APIGoogleLogin : MonoBehaviour
{
    // Inspector에서 이 값을 "http://0.0.0.0:8000/api/v1/auth/login/google"로 지정
    [SerializeField]
    private string googleAuthUrlEndpoint = "http://0.0.0.0:8000/api/v1/auth/login/google";

    // 로그인 상태를 확인할 베이스 URL (로그인 완료 여부를 반환하는 API)
    [SerializeField]
    private string loginStatusBaseUrl = "http://0.0.0.0:8000/api/v1/auth/login/status";

    // 실제 존재하는 사용자 ID로 업데이트되어야 함 (콜백 처리에서 받아올 값)
    [SerializeField]
    private string userId = "actual_user_id";

    // 폴링 간격 (초)
    [SerializeField]
    private float checkInterval = 2f;

    // 최대 폴링 시간 (타임아웃, 예: 60초)
    [SerializeField]
    private float pollingTimeout = 60f;

    // 로그인 성공 시 전환할 씬 이름
    [SerializeField]
    private string nextSceneName = "MainScene";

    // 로딩 UI 텍스트 (선택 사항)
    [SerializeField]
    private Text loadingUIText;

    private bool isPolling = false;

    /// <summary>
    /// Google 로그인 프로세스를 시작합니다.
    /// 1. 서버에서 Google 로그인 URL을 받아 브라우저에서 로그인 페이지를 엽니다.
    /// 2. 이후 서버에 저장된 로그인 완료 상태를 폴링하여 로그인 성공 시 다음 씬으로 전환합니다.
    /// </summary>
    public void StartGoogleLogin()
    {
        // 1. Google 로그인 URL 요청 및 브라우저 열기
        StartCoroutine(GetAndOpenAuthUrl());

        // 2. 로그인 상태 폴링 시작 (실제 userId가 업데이트된 후에 호출해야 함)
        if (!isPolling)
        {
            isPolling = true;
            StartCoroutine(PollLoginStatusWithTimeout());
        }
    }

    /// <summary>
    /// 서버에서 Google 로그인 URL을 받아 브라우저에서 열어줍니다.
    /// </summary>
    private IEnumerator GetAndOpenAuthUrl()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(googleAuthUrlEndpoint))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Failed to get Google Auth URL: " + request.error);
            }
            else
            {
                AuthUrlResponse responseData = JsonUtility.FromJson<AuthUrlResponse>(request.downloadHandler.text);
                if (!string.IsNullOrEmpty(responseData.auth_url))
                {
                    Debug.Log("[Auth] Opening URL: " + responseData.auth_url);
                    Application.OpenURL(responseData.auth_url);
                }
                else
                {
                    Debug.LogError("❌ auth_url is empty in the response.");
                }
            }
        }
    }

    /// <summary>
    /// 서버의 로그인 상태 API를 폴링하여 로그인 완료 여부를 확인합니다.
    /// 로그인 완료 시, 다음 씬으로 전환합니다.
    /// </summary>
    private IEnumerator PollLoginStatusWithTimeout()
    {
        float elapsedTime = 0f;
        while (elapsedTime < pollingTimeout)
        {
            if (loadingUIText != null)
            {
                loadingUIText.text = $"로그인 처리 중... {Mathf.FloorToInt(pollingTimeout - elapsedTime)}초 남음";
            }

            // 실제 호출할 URL: 예: http://0.0.0.0:8000/api/v1/auth/login/status?user_id=actual_user_id
            string pollUrl = $"{loginStatusBaseUrl}?user_id={UnityWebRequest.EscapeURL(userId)}";
            Debug.Log("[Polling] Sending request to: " + pollUrl);

            using (UnityWebRequest request = UnityWebRequest.Get(pollUrl))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("[Polling] Request failed: " + request.error);
                }
                else if (request.responseCode == 200)
                {
                    string json = request.downloadHandler.text;
                    Debug.Log("[Polling] Response JSON: " + json);
                    LoginStatusResponse status = JsonUtility.FromJson<LoginStatusResponse>(json);
                    if (status.logged_in)
                    {
                        Debug.Log("✅ Login completed. Access token: " + status.access_token);
                        if (loadingUIText != null) loadingUIText.text = "";
                        SceneManager.LoadScene(nextSceneName);
                        yield break;
                    }
                    else
                    {
                        Debug.Log("⌛ Not logged in yet. Retrying...");
                    }
                }
                else
                {
                    Debug.LogError("[Polling] Unexpected response code: " + request.responseCode);
                    Debug.LogError("[Polling] Response: " + request.downloadHandler.text);
                }
            }

            yield return new WaitForSeconds(checkInterval);
            elapsedTime += checkInterval;
        }

        Debug.LogError("❌ Login polling timed out.");
        if (loadingUIText != null)
        {
            loadingUIText.text = "로그인 처리 시간이 초과되었습니다. 다시 시도해주세요.";
        }
        isPolling = false;
    }
}
