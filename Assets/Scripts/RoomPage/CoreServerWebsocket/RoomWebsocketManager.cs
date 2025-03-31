using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MCRGame
{
    public class RoomWebsocketManager : MonoBehaviour
    {
        public static RoomWebsocketManager Instance { get; private set; }

        [Header("WebSocket Settings")]
        [SerializeField]
        private string websocketUrl = "ws://0.0.0.0:8000/ws/room"; // 실제 웹소켓 URL로 수정

        private ClientWebSocket clientWebSocket;
        private CancellationTokenSource cts;

        public bool IsConnected { get; private set; }  // 내부에서만 변경 가능

        private async void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            clientWebSocket = new ClientWebSocket();
            cts = new CancellationTokenSource();
            IsConnected = false;
        }

        /// <summary>
        /// 웹소켓 연결을 시작합니다.
        /// </summary>
        public async void Connect()
        {
            if (IsConnected)
            {
                Debug.Log("[RoomWebsocketManager] Already connected.");
                return;
            }

            try
            {
                Debug.Log($"[RoomWebsocketManager] Connecting to {websocketUrl}...");
                await clientWebSocket.ConnectAsync(new Uri(websocketUrl), cts.Token);
                IsConnected = true;
                Debug.Log("[RoomWebsocketManager] WebSocket connected.");

                // 메시지 수신 루프 시작 (fire and forget)
                _ = ReceiveLoop();
            }
            catch (Exception ex)
            {
                Debug.LogError("[RoomWebsocketManager] Connection failed: " + ex.Message);
            }
        }

        /// <summary>
        /// 서버로부터 메시지를 수신하는 루프.
        /// </summary>
        private async Task ReceiveLoop()
        {
            var buffer = new byte[1024];
            while (IsConnected && clientWebSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Debug.Log("[RoomWebsocketManager] Server closed connection.");
                        await Disconnect();
                        break;
                    }
                    else
                    {
                        string message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Debug.Log("[RoomWebsocketManager] Received message: " + message);
                        // 필요시 추가 메시지 처리 로직 구현
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("[RoomWebsocketManager] Receive error: " + ex.Message);
                    await Disconnect();
                    break;
                }
            }
        }

        /// <summary>
        /// 웹소켓으로 메시지를 전송합니다.
        /// </summary>
        public async Task SendMessageAsync(string message)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[RoomWebsocketManager] Not connected. Cannot send message.");
                return;
            }

            var messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
            try
            {
                await clientWebSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, cts.Token);
                Debug.Log("[RoomWebsocketManager] Sent message: " + message);
            }
            catch (Exception ex)
            {
                Debug.LogError("[RoomWebsocketManager] Send error: " + ex.Message);
            }
        }

        /// <summary>
        /// Ping 메시지를 전송합니다.
        /// </summary>
        public async void SendPing()
        {
            string pingMessage = "{\"action\":\"ping\"}";
            await SendMessageAsync(pingMessage);
        }

        /// <summary>
        /// Ready 상태 메시지를 전송합니다.
        /// </summary>
        public async void SendReady(bool isReady)
        {
            string readyMessage = $"{{\"action\":\"ready\",\"data\":{{\"is_ready\":{(isReady ? "true" : "false")}}}}}";
            await SendMessageAsync(readyMessage);
        }

        /// <summary>
        /// 웹소켓 연결을 종료합니다.
        /// </summary>
        public async Task Disconnect()
        {
            if (IsConnected && clientWebSocket.State == WebSocketState.Open)
            {
                try
                {
                    await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", cts.Token);
                    Debug.Log("[RoomWebsocketManager] WebSocket disconnected.");
                }
                catch (Exception ex)
                {
                    Debug.LogError("[RoomWebsocketManager] Disconnect error: " + ex.Message);
                }
            }

            IsConnected = false;
            clientWebSocket.Dispose();
            // 향후 연결을 위해 새 인스턴스를 생성
            clientWebSocket = new ClientWebSocket();
            cts = new CancellationTokenSource();
        }
    }
}
