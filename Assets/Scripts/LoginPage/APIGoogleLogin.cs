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

public class WebGLGoogleLogin : MonoBehaviour
{
    // Google 로그인 URL을 받아오는 엔드포인트
    [SerializeField]
    private string googleAuthUrlEndpoint = "https://example.com/api/v1/auth/login/google";

    // 로그인 완료 여부를 반환하는 API (예: /api/v1/auth/login/status)
    [SerializeField]
    private string loginStatusBaseUrl = "https://example.com/api/v1/auth/login/status";

    // 서버 DB에 저장된 user_id (로그인 콜백 처리 후 유효한 값으로 설정 필요)
    [SerializeField]
    private string userId = "actual_user_id";

    // 폴링 간격(초)
    [SerializeField]
    private float checkInterval = 2f;

    // 폴링 타임아웃(초)
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
    /// WebGL 환경에서 Google 로그인 프로세스를 시작합니다.
    /// 1. 서버에서 Google 로그인 URL을 받아 새 탭(또는 현재 탭)에서 로그인 페이지 열기
    /// 2. 이후 폴링을 통해 로그인 완료 시 씬 전환
    /// </summary>
    public void StartGoogleLogin()
    {
        // 1. Google 로그인 URL 요청 및 브라우저 열기
        StartCoroutine(GetAndOpenAuthUrl());

        // 2. 로그인 상태 폴링 (userId가 이미 유효하다고 가정하거나,
        //    실제 콜백 처리 후 userId를 업데이트한 뒤에 시작)
        if (!isPolling)
        {
            isPolling = true;
            StartCoroutine(PollLoginStatusWithTimeout());
        }
    }

    /// <summary>
    /// 서버에서 Google 로그인 URL을 받아,
    /// WebGL 환경에서 새 탭을 열어 로그인 페이지로 이동
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
                    Debug.Log("[WebGL] Opening URL in browser: " + responseData.auth_url);

                    // WebGL 환경에서 새 탭/창 열기
                    // Application.OpenURL(...)는 WebGL 빌드에서 window.open() 형태로 동작
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
    /// 서버의 로그인 상태 API를 폴링하여 로그인 완료 여부 확인
    /// </summary>
    private IEnumerator PollLoginStatusWithTimeout()
    {
        float elapsedTime = 0f;
        while (elapsedTime < pollingTimeout)
        {
            // 로딩 UI 갱신
            if (loadingUIText != null)
            {
                loadingUIText.text = $"로그인 처리 중... {Mathf.FloorToInt(pollingTimeout - elapsedTime)}초 남음";
            }

            // 실제 호출할 URL (예: https://example.com/api/v1/auth/login/status?user_id=actual_user_id)
            string pollUrl = $"{loginStatusBaseUrl}?user_id={UnityWebRequest.EscapeURL(userId)}";
            Debug.Log("[WebGL Polling] Sending request to: " + pollUrl);

            using (UnityWebRequest request = UnityWebRequest.Get(pollUrl))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("[WebGL Polling] Request failed: " + request.error);
                }
                else if (request.responseCode == 200)
                {
                    string json = request.downloadHandler.text;
                    Debug.Log("[WebGL Polling] Response JSON: " + json);
                    LoginStatusResponse status = JsonUtility.FromJson<LoginStatusResponse>(json);

                    if (status.logged_in)
                    {
                        Debug.Log("✅ Login completed. Access token: " + status.access_token);

                        // 로딩 UI 초기화
                        if (loadingUIText != null) loadingUIText.text = "";

                        // 씬 전환
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
                    Debug.LogError("[WebGL Polling] Unexpected response code: " + request.responseCode);
                    Debug.LogError("[WebGL Polling] Response: " + request.downloadHandler.text);
                }
            }

            yield return new WaitForSeconds(checkInterval);
            elapsedTime += checkInterval;
        }

        // 타임아웃
        Debug.LogError("❌ Login polling timed out.");
        if (loadingUIText != null)
        {
            loadingUIText.text = "로그인 처리 시간이 초과되었습니다. 다시 시도해주세요.";
        }
        isPolling = false;
    }
}
