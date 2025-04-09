using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MCRGame.Net
{

    public class GameRoomWS : MonoBehaviour
    {
        public int roomNumber = 1; // 접속할 방 번호 (기본값)
        private ClientWebSocket clientWebSocket;
        private CancellationTokenSource cancellationTokenSource;

        // GameServerConfig에서 최신 설정값을 기반으로 웹소켓 URL 생성
        private string GetWebSocketUrl()
        {
            string endpoint = $"/ws/room/{roomNumber}";
            return GameServerConfig.GetWebSocketUrl(endpoint);
        }

        async void Start()
        {
            cancellationTokenSource = new CancellationTokenSource();
            // RoomScene에서 GameServerConfig.UpdateWebSocketConfig(newUrl)를 호출했으므로, 최신 URL이 설정됨.
            await ConnectAsync();
        }

        async Task ConnectAsync()
        {
            clientWebSocket = new ClientWebSocket();
            try
            {
                Uri uri = new Uri(GetWebSocketUrl());
                Debug.Log("[GameRoomWS] Connecting to: " + uri);
                await clientWebSocket.ConnectAsync(uri, cancellationTokenSource.Token);
                Debug.Log("[GameRoomWS] WebSocket connected!");
                _ = ReceiveLoopAsync(); // 메시지 수신 시작
            }
            catch (Exception ex)
            {
                Debug.LogError("[GameRoomWS] Connect error: " + ex.Message);
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
                        Debug.Log("[GameRoomWS] WebSocket connection closed.");
                    }
                    else
                    {
                        int count = result.Count;
                        string message = Encoding.UTF8.GetString(buffer, 0, count);
                        Debug.Log("[GameRoomWS] Received message: " + message);
                        ProcessMessage(message);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("[GameRoomWS] ReceiveLoop error: " + ex.Message);
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
                    Debug.LogWarning("[GameRoomWS] Failed to deserialize message.");
                    return;
                }
                Debug.Log("[GameRoomWS] Event: " + wsMessage.Event);

                // 이벤트 분기 처리: 필요에 맞게 이벤트 별 데이터를 처리합니다.
                switch (wsMessage.Event)
                {
                    case GameWSActionType.INIT_EVENT:
                        Debug.Log("[GameRoomWS] Init event received.");
                        if (wsMessage.Data["hand"] != null)
                        {
                            Debug.Log("[GameRoomWS] Hand: " + wsMessage.Data["hand"].ToString());
                        }
                        break;

                    case GameWSActionType.DISCARD:
                        Debug.Log("[GameRoomWS] Discard event received.");
                        // 추가 데이터 처리...
                        break;

                    case GameWSActionType.TSUMO_ACTIONS:
                        Debug.Log("[GameRoomWS] Tsumo actions received.");
                        // 예: action_id 및 actions 배열 처리
                        break;

                    default:
                        Debug.Log("[GameRoomWS] Unhandled event: " + wsMessage.Event);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[GameRoomWS] ProcessMessage error: " + ex.Message);
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
                    Debug.LogError("[GameRoomWS] Close error: " + ex.Message);
                }
                clientWebSocket.Dispose();
            }
        }
    }
}
