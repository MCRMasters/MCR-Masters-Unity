using System;
using System.Collections;
using System.Collections.Generic;
using MCRGame.Game;
using MCRGame.UI;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace MCRGame.Net
{
    public class GoogleLoginManager : MonoBehaviour
    {
        private string backendLoginUrl = CoreServerConfig.GetHttpUrl("/auth/login/google");
        private string backendStatusUrl = CoreServerConfig.GetHttpUrl("/auth/login/status");
        private string backendIsPlayingUrl = CoreServerConfig.GetHttpUrl("/user/me/is-playing");
        private string getUserInfoUrl = CoreServerConfig.GetHttpUrl("/user/me");
        private string backendRoomDetailUrl = CoreServerConfig.GetHttpUrl("/room/me");

        private string sessionId;

        /// <summary>
        /// 구글 로그인 버튼 클릭 시 호출됩니다.
        /// </summary>
        public void OnGoogleLoginClick()
        {
            Debug.Log($"login url: {backendLoginUrl}");
            StartCoroutine(RequestGoogleAuthUrl());
        }


        private IEnumerator GetUserInfoFromServer()
        {
            // AccessToken이 저장되어 있어야 함
            string token = PlayerDataManager.Instance.AccessToken;
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("[LobbyInfoManager] 토큰이 없습니다. 로그인이 안 된 상태인가요?");
                yield break;
            }

            using (UnityWebRequest www = UnityWebRequest.Get(getUserInfoUrl))
            {
                // 인증 헤더 추가
                www.SetRequestHeader("Authorization", $"Bearer {token}");
                www.certificateHandler = new BypassCertificateHandler();
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("[LobbyInfoManager] 유저 정보 가져오기 성공: " + www.downloadHandler.text);

                    // JSON 파싱
                    UserMeResponse userData = JsonUtility.FromJson<UserMeResponse>(www.downloadHandler.text);

                    // PlayerDataManager에 저장
                    PlayerDataManager.Instance.SetUserData(userData.uid, userData.nickname, userData.email);

                    PlayerDataManager.Instance.SetCharacterData(userData.owned_characters, userData.current_character.code);
                }
                else
                {
                    Debug.LogError("[LobbyInfoManager] 유저 정보 가져오기 실패: " + www.error);
                }
            }
        }

        /// <summary>
        /// 백엔드에서 OAuth 인증 URL과 session_id를 받아온 후, 외부 브라우저를 열어 인증을 진행하고 폴링을 시작합니다.
        /// </summary>
        private IEnumerator RequestGoogleAuthUrl()
        {
            using (UnityWebRequest www = UnityWebRequest.Get(backendLoginUrl))
            {
                www.certificateHandler = new BypassCertificateHandler();
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
                    www.certificateHandler = new BypassCertificateHandler();
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
                        Debug.Log("토큰 폴링 실패: " + www.error);
                    }
                }
                yield return new WaitForSeconds(2f); // 2초 간격 폴링
            }
        }

        private void OnGoogleAuthCallbackReceived(TokenResponse token)
        {
            Debug.Log($"[GoogleLoginManager] AccessToken = {token.access_token}");
            Debug.Log($"[GoogleLoginManager] RefreshToken = {token.refresh_token}");
            Debug.Log($"[GoogleLoginManager] isNewUser = {token.is_new_user}");

            PlayerDataManager.Instance.SetTokenData(token.access_token, token.refresh_token, token.is_new_user);
            StartCoroutine(ProceedAfterAuth());
        }

        private IEnumerator ProceedAfterAuth()
        {
            // 1) GetUserInfoFromServer가 끝날 때까지 대기
            yield return StartCoroutine(GetUserInfoFromServer());

            // 2) 그 다음에 CheckIsPlayingAndLoadScene 실행
            yield return StartCoroutine(CheckIsPlayingAndLoadScene());
        }

        private IEnumerator CheckIsPlayingAndLoadScene()
        {
            Debug.Log("[GoogleLoginManager] Checking if user is currently in a playing room...");



            using (UnityWebRequest www = UnityWebRequest.Get(backendIsPlayingUrl))
            {
                string token = PlayerDataManager.Instance.AccessToken;
                if (string.IsNullOrEmpty(token))
                {
                    Debug.LogError("[GoogleLoginManager] Access token is null or empty.");
                    yield break;
                }

                www.SetRequestHeader("Authorization", "Bearer " + token);
                www.certificateHandler = new BypassCertificateHandler();
                yield return www.SendWebRequest();

                Debug.Log($"[GoogleLoginManager] Response Code: {www.responseCode}");
                Debug.Log($"[GoogleLoginManager] Raw JSON: {www.downloadHandler.text}");

                if (www.result == UnityWebRequest.Result.Success)
                {
                    IsPlayingResponse response = JsonUtility.FromJson<IsPlayingResponse>(www.downloadHandler.text);
                    bool isPlaying = response.data.in_playing_room;
                    string gameId = response.data.game_id;

                    if (isPlaying && !string.IsNullOrEmpty(gameId))
                    {
                        Debug.Log("[GoogleLoginManager] User is in a playing room. Loading GameScene...");
                        GameObject charImgManager = new GameObject("CharacterImageManager");
                        charImgManager.AddComponent<CharacterImageManager>();

                        GameObject gameMediator = new GameObject("GameMessageMediator");
                        gameMediator.AddComponent<GameMessageMediator>();

                        // 코루틴을 시작해서, REST 호출 → Players 목록 만들기 → WS 메시지 큐에 넣기
                        yield return FetchPlayersAndEnqueue();

                        // 씬을 로드할 때 gameId를 GameServerConfig에 설정
                        GameServerConfig.UpdateWebSocketConfigWithGameId(gameId);

                        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
                        yield break;
                    }
                    else
                    {
                        Debug.Log("[GoogleLoginManager] User is not in a playing room.");
                    }
                }
                else
                {
                    Debug.LogError("[GoogleLoginManager] Failed to get is-playing status: " + www.error);
                }

                Debug.Log("[GoogleLoginManager] Loading LobbyScene...");
                SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);

            }
        }


        private IEnumerator FetchPlayersAndEnqueue()
        {
            // 1) 현재 사용자가 속한 룸 정보를 가져오기 위해 /api/v1/room/me 호출
            string token = PlayerDataManager.Instance.AccessToken;
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("[GoogleLoginManager] 토큰이 없습니다. /room/me 요청을 못 합니다.");
                yield break;
            }

            using (UnityWebRequest www = UnityWebRequest.Get(backendRoomDetailUrl))
            {
                www.SetRequestHeader("Authorization", "Bearer " + token);
                www.certificateHandler = new BypassCertificateHandler();
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[GoogleLoginManager] /room/me 요청 실패: {www.error}");
                    yield break;
                }

                JObject root = JObject.Parse(www.downloadHandler.text);
                JArray usersArray = (JArray)root["users"];
                List<RoomUserInfo> Players = new List<RoomUserInfo>();

                foreach (JToken userTok in usersArray)
                {
                    var nickname = (string)userTok["nickname"];
                    var userUid = (string)userTok["user_uid"];
                    var isReady = (bool)userTok["is_ready"];
                    var slotIdx = (int)userTok["slot_index"];

                    // current_character 필드에서 code, name을 꺼내서 RoomUserInfo에 담을 수 있도록 가정
                    string charCode = (string)userTok["current_character"]["code"];
                    string charName = (string)userTok["current_character"]["name"];

                    RoomUserInfo info = new RoomUserInfo
                    {
                        nickname = nickname,
                        uid = userUid,
                        is_ready = isReady,
                        slot_index = slotIdx,
                        current_character = new CharacterResponse { code = charCode, name = charName }
                    };

                    Players.Add(info);
                }

                // 3) WS 메시지를 만들어 GameMessageMediator.Instance에 넣기
                var wsMessage = new GameWSMessage
                {
                    Event = GameWSActionType.CLIENT_GAME_START_INFO,
                    Data = new JObject { ["players"] = JArray.FromObject(Players) }
                };

                GameMessageMediator.Instance.EnqueueMessage(wsMessage);
            }
        }

    }
}
