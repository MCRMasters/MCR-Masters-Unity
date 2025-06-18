// GameWS.cs
using System;
using System.Text;
using UnityEngine;
using NativeWebSocket;
using Newtonsoft.Json;
using MCRGame.UI;
using MCRGame.Game;
using MCRGame.Game.Events;          // Dispatcher 검색용
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

namespace MCRGame.Net
{
    public class GameWS : MonoBehaviour
    {
        /*──────────────────────────────────────────────*/
        /*  싱글톤 & 필드                                */
        /*──────────────────────────────────────────────*/

        public static GameWS Instance { get; private set; }
        private WebSocket websocket;

        // 연결/재접속 상태 플래그
        private bool isConnecting   = false;                // 중복 Connect 방지
        private bool isReconnecting = false;
        private bool manualClose    = false;
        private const int RECONNECT_DELAY = 5;              // 초

        // END_GAME 이후 정상 종료인지 판단
        private bool endGameReceived = false;

        /*──────────────────────────────────────────────*/
        /*  Unity 생명주기                              */
        /*──────────────────────────────────────────────*/

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            /* ★ GameScene 로드 시 연결 트리거 */
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            manualClose = true;
            _ = SafeCloseAsync();

            if (Instance == this)
                Instance = null;
        }

        /*──────────────────────────────────────────────*/
        /*  씬 로드 콜백                                */
        /*──────────────────────────────────────────────*/

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "GameScene") return;

            Debug.Log("[GameWS] GameScene loaded → WebSocket 연결 확인");

            if (websocket == null || websocket.State != WebSocketState.Open)
                StartCoroutine(EnsureUserDataThenConnect());
        }

        private void Start()
        {
            /* Start()에서는 Connect 코루틴을 시작하지 않음
               첫 진입이 GameScene이면 OnSceneLoaded 쪽에서만 호출 */
        }

        private void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            websocket?.DispatchMessageQueue();
#endif
        }

        /*──────────────────────────────────────────────*/
        /*  초기 접속 / 유저 정보 확보                   */
        /*──────────────────────────────────────────────*/

        private IEnumerator EnsureUserDataThenConnect()
        {
            var pdm = PlayerDataManager.Instance;
            if (string.IsNullOrEmpty(pdm.Uid) || string.IsNullOrEmpty(pdm.Nickname))
            {
                yield return StartCoroutine(FetchUserInfoCoroutine());
            }

            /* GameManager → Mediator 순으로 준비될 때까지 대기 */
            yield return new WaitUntil(() =>
                GameManager.Instance != null &&
                GameManager.Instance.isActiveAndEnabled);

            yield return new WaitUntil(() =>
                GameMessageMediator.Instance != null &&
                GameMessageMediator.Instance.isActiveAndEnabled);

            yield return null; // 한 프레임 딜레이

            Connect();
        }

        private IEnumerator FetchUserInfoCoroutine()
        {
            var url = CoreServerConfig.GetHttpUrl("/user/me");
            Debug.Log("[GameWS] ▶ Fetching user info before WS connect");

            using var www = UnityWebRequest.Get(url);
            www.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");
            www.certificateHandler = new BypassCertificateHandler();
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[GameWS] ✔ User info fetched: " + www.downloadHandler.text);
                var user = JsonConvert.DeserializeObject<UserMeResponse>(www.downloadHandler.text);
                PlayerDataManager.Instance.SetUserData(user.uid, user.nickname, user.email);
            }
            else
            {
                Debug.LogError("[GameWS] ❌ Failed to fetch user info: " + www.error);
            }
        }

        /*──────────────────────────────────────────────*/
        /*  WebSocket 연결                              */
        /*──────────────────────────────────────────────*/

        private async void Connect()
        {
            if (isConnecting) return;           // 중복 호출 방지
            isConnecting = true;

            await SafeCloseAsync();             // 기존 소켓 정리

            manualClose     = false;
            isReconnecting  = false;
            endGameReceived = false;

            /* 1) URL 준비 */
            string baseUrl = GameServerConfig.GetWebSocketUrl();
            var pdm = PlayerDataManager.Instance;

            string uid   = pdm?.Uid        ?? "";
            string nick  = pdm?.Nickname   ?? "";
            string token = pdm?.AccessToken ?? "";

            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("[GameWS] AccessToken이 없습니다.");
                isConnecting = false;
                return;
            }

            string url = $"{baseUrl}?user_id={Uri.EscapeDataString(uid)}&nickname={Uri.EscapeDataString(nick)}";

            /* 2) NativeWebSocket 인스턴스 생성 */
            websocket = new WebSocket(url);

            websocket.OnOpen  += () => Debug.Log("[GameWS] WebSocket connected!");

            websocket.OnError += err =>
            {
                Debug.LogError("[GameWS] WebSocket Error: " + err);
                TryReconnect();
            };

            websocket.OnClose += code =>
            {
                Debug.Log($"[GameWS] WebSocket Closed: {code}");
                websocket = null;
                isConnecting = false;

                if (manualClose || endGameReceived || !IsInGameScene()) return;
                TryReconnect();
            };

            websocket.OnMessage += bytes =>
            {
                string msg = Encoding.UTF8.GetString(bytes);
                Debug.Log("[GameWS] Received: " + msg);

                try
                {
                    var wsMsg = JsonConvert.DeserializeObject<GameWSMessage>(msg);
                    if (wsMsg != null)
                    {
                        if (wsMsg.Event == GameWSActionType.END_GAME)
                            endGameReceived = true;

                        GameMessageMediator.Instance?.EnqueueMessage(wsMsg);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("[GameWS] JSON deserialize error: " + ex);
                }
            };

            Debug.Log("[GameWS] Connecting to: " + url);
            try
            {
                await websocket.Connect();
            }
            finally
            {
                isConnecting = false;  // 성공·실패 무관 플래그 해제
            }
        }

        /*──────────────────────────────────────────────*/
        /*  재접속 로직                                 */
        /*──────────────────────────────────────────────*/

        private void TryReconnect()
        {
            if (manualClose || isReconnecting || isConnecting || endGameReceived || !IsInGameScene())
                return;

            isReconnecting = true;
            StartCoroutine(ReconnectCoroutine());
        }

        private IEnumerator ReconnectCoroutine()
        {
            Debug.Log($"[GameWS] 연결이 끊어졌습니다. {RECONNECT_DELAY}초 후 재접속 시도...");
            yield return new WaitForSeconds(RECONNECT_DELAY);

            if (!IsInGameScene())
            {
                isReconnecting = false;
                yield break;
            }

            Debug.Log("[GameWS] 재접속 시도...");
            Connect();
        }

        private static bool IsInGameScene() =>
            SceneManager.GetActiveScene().name == "GameScene";

        /*──────────────────────────────────────────────*/
        /*  외부 전송 API                               */
        /*──────────────────────────────────────────────*/

        public void SendGameEvent(GameWSActionType action, object payload)
        {
            Debug.Log($"[TRACE] SendGameEvent called → WS:{websocket?.State}");

            if (websocket == null || websocket.State != WebSocketState.Open)
            {
                Debug.LogWarning("[GameWS] WS not open");
                return;
            }

            var msgObj = new { @event = action, data = payload };
            string json = JsonConvert.SerializeObject(msgObj);

            _ = SendJsonSafe(json);   // 플랫폼 안전 래퍼
        }

        /// <summary>플랫폼 차이를 숨기는 전송 래퍼</summary>
        private Task SendJsonSafe(string json)
        {
            if (websocket == null || websocket.State != WebSocketState.Open)
                return Task.CompletedTask;

#if UNITY_WEBGL && !UNITY_EDITOR
            websocket.SendText(json);          // 동기
            return Task.CompletedTask;
#else
            return websocket.SendText(json);   // 비동기
#endif
        }

        /*──────────────────────────────────────────────*/
        /*  안전 종료 헬퍼                              */
        /*──────────────────────────────────────────────*/

        private Task SafeCloseAsync()
        {
            if (websocket == null) return Task.CompletedTask;

            var st = websocket.State;
            if (st == WebSocketState.Closed || st == WebSocketState.Closing || isConnecting)
                return Task.CompletedTask;

#if UNITY_WEBGL && !UNITY_EDITOR
            try { websocket.Close(); }
            catch (Exception e) { Debug.LogWarning($"[GameWS] Close ignored: {e.Message}"); }
            return Task.CompletedTask;
#else
            return websocket.Close();
#endif
        }
    }
}
