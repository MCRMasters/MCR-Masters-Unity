using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace MCRGame
{
    [System.Serializable]
    public class AuthUrlResponse
    {
        public string auth_url;
        public string session_id;
    }

    [System.Serializable]
    public class TokenResponse
    {
        public string access_token;
        public string refresh_token;
        public bool is_new_user;
        public string token_type;
    }

    public class GoogleLoginManager : MonoBehaviour
    {
        // CORE_SERVER_URL을 인스펙터에서 설정 (예: http://localhost:8000/)
        [SerializeField] private string coreServerUrl = "http://localhost:8000/";

        // CORE_SERVER_URL을 기반으로 backend URL을 구성합니다.
        private string backendLoginUrl => coreServerUrl + "api/v1/auth/login/google";
        private string backendStatusUrl => coreServerUrl + "api/v1/auth/login/status";

        private string sessionId;

        /// <summary>
        /// 구글 로그인 버튼 클릭 시 호출됩니다.
        /// </summary>
        public void OnGoogleLoginClick()
        {
            StartCoroutine(RequestGoogleAuthUrl());
        }

        /// <summary>
        /// 백엔드에서 OAuth 인증 URL과 session_id를 받아온 후, 외부 브라우저를 열어 인증을 진행하고 폴링을 시작합니다.
        /// </summary>
        private IEnumerator RequestGoogleAuthUrl()
        {
            using (UnityWebRequest www = UnityWebRequest.Get(backendLoginUrl))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string json = www.downloadHandler.text;
                    AuthUrlResponse authData = JsonUtility.FromJson<AuthUrlResponse>(json);
                    sessionId = authData.session_id;

                    // 외부 브라우저를 열어 인증 URL을 실행합니다.
                    Application.OpenURL(authData.auth_url);

                    // 백엔드에서 토큰 정보가 준비되었는지 폴링 시작
                    StartCoroutine(PollForToken());
                }
                else
                {
                    Debug.LogError("로그인 URL 요청 실패: " + www.error);
                }
            }
        }

        /// <summary>
        /// 주기적으로 백엔드의 /login/status 엔드포인트에 session_id를 전달하여, 토큰 정보가 준비되었는지 확인합니다.
        /// </summary>
        private IEnumerator PollForToken()
        {
            while (true)
            {
                string url = $"{backendStatusUrl}?session_id={sessionId}";
                using (UnityWebRequest www = UnityWebRequest.Get(url))
                {
                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        string json = www.downloadHandler.text;
                        if (!string.IsNullOrEmpty(json))
                        {
                            TokenResponse token = JsonUtility.FromJson<TokenResponse>(json);
                            if (!string.IsNullOrEmpty(token.access_token))
                            {
                                OnGoogleAuthCallbackReceived(token);
                                yield break; // 토큰을 받으면 폴링 종료
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("토큰 폴링 실패: " + www.error);
                    }
                }
                yield return new WaitForSeconds(2f); // 2초 간격 폴링
            }
        }

        /// <summary>
        /// 백엔드에서 토큰 정보를 전달받으면 호출됩니다.
        /// 토큰 정보를 저장하고 이후 로직(예: 씬 전환 등)을 실행합니다.
        /// </summary>
        private void OnGoogleAuthCallbackReceived(TokenResponse token)
        {
            Debug.Log($"[GoogleLoginManager] AccessToken = {token.access_token}");
            Debug.Log($"[GoogleLoginManager] RefreshToken = {token.refresh_token}");
            Debug.Log($"[GoogleLoginManager] isNewUser = {token.is_new_user}");

            // 예시: PlayerDataManager에 토큰 정보를 저장 (해당 매니저를 프로젝트에 맞게 구현)
            PlayerDataManager.Instance.SetTokenData(token.access_token, token.refresh_token, token.is_new_user);

            // 로그인 완료 후 로비 씬으로 전환 (LobbyScene이 Build Settings에 추가되어 있어야 합니다)
            SceneManager.LoadScene("LobbyScene");
        }
    }
}
