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

namespace MCRGame.Net
{
    public class GameWS : MonoBehaviour
    {
        // 싱글톤 인스턴스
        public static GameWS Instance { get; private set; }

        private ClientWebSocket clientWebSocket;
        private CancellationTokenSource cancellationTokenSource;

        private void Awake()
        {
            // 이미 인스턴스가 존재하면 파괴
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

                switch (wsMessage.Event)
                {
                    case GameWSActionType.INIT_EVENT:
                        Debug.Log("[GameWS] Init event received.");
                        if (wsMessage.Data["hand"] != null)
                        {
                            Debug.Log("[GameWS] Hand: " + wsMessage.Data["hand"].ToString());
                        }
                        break;
                    case GameWSActionType.DISCARD:
                        Debug.Log("[GameWS] Discard event received.");
                        break;
                    case GameWSActionType.TSUMO_ACTIONS:
                        Debug.Log("[GameWS] Tsumo actions received.");
                        break;
                    case GameWSActionType.GAME_START_INFO:
                        Debug.Log("[GameWS] GAME_START_INFO event received.");
                        Debug.Log("[GameWS] Data: " + wsMessage.Data.ToString());
                        break;
                    case GameWSActionType.INIT_FLOWER_REPLACEMENT:
                        Debug.Log("[GameWS] INIT_FLOWER_REPLACEMENT event received.");

                        // new_tiles 파싱
                        if (wsMessage.Data.TryGetValue("new_tiles", out JToken tilesToken))
                        {
                            List<int> newTiles = tilesToken.ToObject<List<int>>();
                            Debug.Log("[GameWS] New flower replacement tiles: " + string.Join(", ", newTiles));
                        }
                        else
                        {
                            Debug.LogWarning("[GameWS] INIT_FLOWER_REPLACEMENT: new_tiles 키가 없습니다.");
                        }

                        // flower_count 파싱
                        if (wsMessage.Data.TryGetValue("flower_count", out JToken countToken))
                        {
                            List<int> flowerCount = countToken.ToObject<List<int>>();
                            Debug.Log("[GameWS] Flower counts for each hand: " + string.Join(", ", flowerCount));
                        }
                        else
                        {
                            Debug.LogWarning("[GameWS] INIT_FLOWER_REPLACEMENT: flower_count 키가 없습니다.");
                        }
                        break;
                    default:
                        Debug.Log("[GameWS] Unhandled event: " + wsMessage.Event);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[GameWS] ProcessMessage error: " + ex.Message);
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
