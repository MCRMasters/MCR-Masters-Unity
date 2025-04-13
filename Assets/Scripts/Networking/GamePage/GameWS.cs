using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using MCRGame.UI;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using MCRGame.Game;

namespace MCRGame.Net
{
    public class GameWS : MonoBehaviour
    {
        public static GameWS Instance { get; private set; }
        private ClientWebSocket clientWebSocket;
        private CancellationTokenSource cancellationTokenSource;

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

        // GameServerConfig에서 설정한 완전한 WebSocket URL 반환
        private string GetWebSocketUrl()
        {
            return GameServerConfig.GetWebSocketUrl();
        }

        async void Start()
        {
            cancellationTokenSource = new CancellationTokenSource();
            Debug.Log("[GameWS] Attempting to connect to: " + GetWebSocketUrl());
            await ConnectAsync();
        }

        async Task ConnectAsync()
        {
            clientWebSocket = new ClientWebSocket();

            // PlayerDataManager에서 uid와 nickname을 헤더에 추가
            if (PlayerDataManager.Instance != null)
            {
                clientWebSocket.Options.SetRequestHeader("user_id", PlayerDataManager.Instance.Uid);
                clientWebSocket.Options.SetRequestHeader("nickname", PlayerDataManager.Instance.Nickname);
                Debug.Log($"[GameWS] Set headers - user_id: {PlayerDataManager.Instance.Uid}, nickname: {PlayerDataManager.Instance.Nickname}");
            }
            else
            {
                Debug.LogError("[GameWS] PlayerDataManager 인스턴스가 없습니다.");
                return;
            }

            try
            {
                Uri uri = new Uri(GetWebSocketUrl());
                Debug.Log("[GameWS] Connecting to: " + uri);
                await clientWebSocket.ConnectAsync(uri, cancellationTokenSource.Token);
                Debug.Log("[GameWS] WebSocket connected!");
                _ = ReceiveLoopAsync(); // 메시지 수신 시작
            }
            catch (Exception ex)
            {
                Debug.LogError("[GameWS] Connect error: " + ex.Message);
            }
        }

        async Task ReceiveLoopAsync()
        {
            var buffer = new byte[4096];
            while (clientWebSocket.State == WebSocketState.Open)
            {
                try
                {
                    var segment = new ArraySegment<byte>(buffer);
                    WebSocketReceiveResult result = await clientWebSocket.ReceiveAsync(segment, cancellationTokenSource.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationTokenSource.Token);
                        Debug.Log("[GameWS] WebSocket connection closed.");
                    }
                    else
                    {
                        int count = result.Count;
                        string message = Encoding.UTF8.GetString(buffer, 0, count);
                        Debug.Log("[GameWS] Received message: " + message);
                        ProcessMessage(message);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("[GameWS] ReceiveLoop error: " + ex.Message);
                    break;
                }
            }
        }

        void ProcessMessage(string message)
        {
            try
            {
                GameWSMessage wsMessage = JsonConvert.DeserializeObject<GameWSMessage>(message);
                if (wsMessage == null)
                {
                    Debug.LogWarning("[GameWS] Failed to deserialize message.");
                    return;
                }
                Debug.Log("[GameWS] Event: " + wsMessage.Event);

                // 메시지 처리는 GameMessageMediator로 위임
                if (GameMessageMediator.Instance != null)
                {
                    GameMessageMediator.Instance.EnqueueMessage(wsMessage);
                }
                else
                {
                    Debug.LogWarning("[GameWS] GameMessageMediator 인스턴스가 존재하지 않습니다.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[GameWS] ProcessMessage error: " + ex.Message);
            }
        }

        /// <summary>
        /// 서버가 기대하는 JSON 구조 (최상위에 "event"와 "data" 필드를 포함)로 게임 이벤트를 전송합니다.
        /// 호출부에서는 payload 객체에 "event_type" 등 필요한 필드를 구성하여 전달해야 합니다.
        /// 예:
        ///     var payload = new {
        ///         event_type = (int)GameEventType.INIT_FLOWER_OK,
        ///         action_id = 0, // (필요하면 지정)
        ///         data = new { message = "ok" }
        ///     };
        ///     await GameWS.Instance.SendGameEventAsync(GameWSActionType.GAME_EVENT, payload);
        /// </summary>
        /// <param name="action">전송할 액션 타입 (예: GAME_EVENT)</param>
        /// <param name="payload">내부 data payload (반드시 event_type 등의 필드를 포함)</param>
        public async Task SendGameEventAsync(GameWSActionType action, object payload)
        {
            if (clientWebSocket == null || clientWebSocket.State != WebSocketState.Open)
            {
                Debug.LogWarning("[GameWS] WebSocket이 연결되어 있지 않습니다. 메시지를 전송할 수 없습니다.");
                return;
            }

            // 서버 측에서는 최상위 JSON 객체에 event와 data 필드가 존재하기를 기대합니다.
            // 예: { "event": "game_event", "data": { "event_type": 12, "action_id": 0, "data": { ... } } }
            var messageObj = new
            {
                @event = action,
                data = payload,
            };

            string jsonMessage = JsonConvert.SerializeObject(messageObj);
            var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
            var segment = new ArraySegment<byte>(messageBytes);
            try
            {
                await clientWebSocket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationTokenSource.Token);
                Debug.Log("[GameWS] Sent message: " + jsonMessage);
            }
            catch (Exception ex)
            {
                Debug.LogError("[GameWS] SendGameEventAsync error: " + ex.Message);
            }
        }

        async void OnDestroy()
        {
            if (clientWebSocket != null)
            {
                cancellationTokenSource.Cancel();
                try
                {
                    await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "OnDestroy", CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[GameWS] Close error: " + ex.Message);
                }
                clientWebSocket.Dispose();
            }
        }
    }
}
