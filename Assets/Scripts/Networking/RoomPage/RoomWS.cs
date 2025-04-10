using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using MCRGame.UI;

namespace MCRGame.Net
{
    public class RoomWS : MonoBehaviour
    {
        // 싱글톤 인스턴스
        public static RoomWS Instance { get; private set; }

        public int roomNumber = 1; // 기본 접속할 방 번호
        private ClientWebSocket webSocket;
        private CancellationTokenSource cancellation;

        // 연결 완료 시 호출할 콜백 (클라이언트 내부용)
        public Action OnWebSocketConnected;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        async void Start()
        {
            // RoomDataManager에 저장된 방 번호 업데이트
            if (RoomDataManager.Instance != null && !string.IsNullOrEmpty(RoomDataManager.Instance.RoomId))
            {
                if (int.TryParse(RoomDataManager.Instance.RoomId, out int parsedRoomNumber))
                {
                    roomNumber = parsedRoomNumber;
                    Debug.Log($"[RoomWS] RoomNumber updated from RoomDataManager: {roomNumber}");
                }
                else
                {
                    Debug.LogWarning($"[RoomWS] RoomDataManager의 RoomId '{RoomDataManager.Instance.RoomId}'를 int로 파싱할 수 없습니다.");
                }
            }

            cancellation = new CancellationTokenSource();
            await Connect();
            // 연결 성공 시 주기적인 Ping 전송 (필요한 경우)
            // _ = StartPingLoop();
        }

        async Task Connect()
        {
            webSocket = new ClientWebSocket();
            string token = PlayerDataManager.Instance != null ? PlayerDataManager.Instance.AccessToken : "";
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("토큰이 없습니다. PlayerDataManager를 확인하세요.");
                return;
            }
            string authHeader = token;
            webSocket.Options.SetRequestHeader("authorization", authHeader);
            Debug.Log("[DEBUG] 설정된 Authorization 헤더: " + authHeader);

            string endpoint = $"/ws/room/{roomNumber}";
            Uri uri = new Uri(CoreServerConfig.GetWebSocketUrl(endpoint));
            Debug.Log("[DEBUG] WebSocket 연결 URL: " + uri);

            try
            {
                await webSocket.ConnectAsync(uri, cancellation.Token);
                Debug.Log("WebSocket 연결 성공!");
                // 연결이 완료되면 연결 완료 콜백 호출
                OnWebSocketConnected?.Invoke();
                _ = ReceiveLoop();
            }
            catch (Exception ex)
            {
                Debug.LogError("WebSocket 연결 에러: " + ex.Message);
            }
        }

        async Task ReceiveLoop()
        {
            var buffer = new ArraySegment<byte>(new byte[4096]);

            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, cancellation.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellation.Token);
                        Debug.Log("WebSocket 연결 종료");
                    }
                    else
                    {
                        int count = result.Count;
                        string message = Encoding.UTF8.GetString(buffer.Array, 0, count);
                        Debug.Log("메시지 수신: " + message);
                        ProcessMessage(message);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("수신 중 에러: " + ex.Message);
                    break;
                }
            }
        }

        void ProcessMessage(string message)
        {
            try
            {
                WSMessage response = JsonConvert.DeserializeObject<WSMessage>(message);
                if (response == null)
                {
                    Debug.LogWarning("메시지 파싱 실패");
                    return;
                }

                if (response.Data == null)
                {
                    Debug.LogWarning("수신 메시지에 data가 없습니다.");
                }

                switch (response.Action)
                {
                    case WSActionType.PING:
                        Debug.Log("서버로부터 PING 수신. PONG 전송.");
                        SendPong();
                        break;
                    case WSActionType.PONG:
                        Debug.Log("서버로부터 PONG 수신.");
                        break;
                    case WSActionType.USER_READY_CHANGED:
                        {
                            WSUserReadyData readyData = JsonConvert.DeserializeObject<WSUserReadyData>(response.Data?.ToString() ?? "{}");
                            Debug.Log($"유저 준비 상태 변경: {readyData.UserUid} -> {readyData.IsReady}");
                            RoomManager roomManagerInstance = FindFirstObjectByType<RoomManager>();
                            if (roomManagerInstance != null)
                            {
                                roomManagerInstance.UpdatePlayerReadyState(readyData.UserUid, readyData.IsReady);
                            }
                        }
                        break;
                    case WSActionType.USER_LEFT:
                        {
                            WSUserLeftData leftData = JsonConvert.DeserializeObject<WSUserLeftData>(response.Data?.ToString() ?? "{}");
                            Debug.Log($"유저 퇴장: {leftData.UserUid}");
                        }
                        break;
                    case WSActionType.USER_JOINED:
                        {
                            WSUserJoinedData joinedData = JsonConvert.DeserializeObject<WSUserJoinedData>(response.Data?.ToString() ?? "{}");
                            Debug.Log($"유저 입장: {joinedData.UserUid} - 닉네임: {joinedData.Nickname}, slot_index: {joinedData.SlotIndex}");

                            // RoomDataManager 업데이트: Players 배열에 새 사용자 정보 추가/갱신
                            if (RoomDataManager.Instance != null)
                            {
                                RoomUserData newUser = new RoomUserData
                                {
                                    uid = joinedData.UserUid,
                                    nickname = joinedData.Nickname,
                                    isReady = joinedData.IsReady,
                                    slot_index = joinedData.SlotIndex
                                };
                                RoomDataManager.Instance.AddOrUpdateUser(newUser);
                            }
                            else
                            {
                                Debug.LogWarning("[RoomWS] RoomDataManager 인스턴스가 없습니다.");
                            }

                            // RoomManager UI 업데이트 (예: 플레이어 목록 갱신)
                            RoomManager roomManagerInstance = FindFirstObjectByType<RoomManager>();

                            if (roomManagerInstance != null)
                            {
                                roomManagerInstance.UpdatePlayerUI();
                            }
                            break;
                        }
                    case WSActionType.GAME_STARTED:
                        {
                            WSGameStartedData gameData = JsonConvert.DeserializeObject<WSGameStartedData>(response.Data?.ToString() ?? "{}");
                            Debug.Log("게임 시작! game_url: " + gameData.GameUrl);

                            GameServerConfig.UpdateWebSocketConfig(gameData.GameUrl);

                            if (GameWS.Instance == null)
                            {
                                new GameObject("GameWS").AddComponent<GameWS>();
                            }

                            // 게임 씬으로 전환
                            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
                        }
                        break;
                    default:
                        Debug.Log("알 수 없는 액션: " + response.Action);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("메시지 처리 중 에러: " + ex.Message);
            }
        }

        async void SendPong()
        {
            WSMessage pongResponse = new WSMessage
            {
                Status = "success",
                Action = WSActionType.PONG,
                Data = new { message = "pong" },
                Error = null,
                Timestamp = DateTime.UtcNow.ToString("o")
            };

            string json = JsonConvert.SerializeObject(pongResponse);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            var buffer = new ArraySegment<byte>(bytes);

            try
            {
                await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellation.Token);
                Debug.Log("PONG 메시지 전송됨");
            }
            catch (Exception ex)
            {
                Debug.LogError("PONG 전송 에러: " + ex.Message);
            }
        }

        async void SendPing()
        {
            WSMessage pingRequest = new WSMessage
            {
                Status = "success",
                Action = WSActionType.PING,
                Data = new { message = "ping" },
                Error = null,
                Timestamp = DateTime.UtcNow.ToString("o")
            };

            string json = JsonConvert.SerializeObject(pingRequest);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            var buffer = new ArraySegment<byte>(bytes);

            try
            {
                await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellation.Token);
                Debug.Log("PING 메시지 전송됨");
            }
            catch (Exception ex)
            {
                Debug.LogError("PING 전송 에러: " + ex.Message);
            }
        }

        // 새로운 Ready 상태 메시지를 서버로 전송하는 함수
        public async void SendReadyStatus(bool isReady)
        {
            if (webSocket == null || webSocket.State != WebSocketState.Open)
            {
                Debug.LogError("WebSocket 연결이 열려있지 않습니다.");
                return;
            }

            var readyMessage = new
            {
                status = "success",
                action = "ready", // WSActionType.READY와 동일한 문자열 값
                data = new { is_ready = isReady },
                error = (string)null,
                timestamp = DateTime.UtcNow.ToString("o")
            };

            string json = JsonConvert.SerializeObject(readyMessage);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            var buffer = new ArraySegment<byte>(bytes);

            try
            {
                await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellation.Token);
                Debug.Log("READY 메시지 전송됨: " + json);
            }
            catch (Exception ex)
            {
                Debug.LogError("READY 메시지 전송 에러: " + ex.Message);
            }
        }

        async void OnDestroy()
        {
            if (webSocket != null)
            {
                cancellation.Cancel();
                try
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Debug.LogError("종료 중 에러: " + ex.Message);
                }
                webSocket.Dispose();
            }
        }
    }
}
