using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_WEBGL
using NativeWebSocket; // WebGL에서는 NativeWebSocket 사용
#else
using System.Net.WebSockets; // PC 환경에서는 ClientWebSocket 사용
#endif

public class WebSocketManager : MonoBehaviour
{
/*
    public static WebSocketManager Instance;

#if UNITY_WEBGL
    private WebSocket websocket;
#else
    private ClientWebSocket websocket;
    private CancellationTokenSource cancellation;
#endif

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 변경 시에도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async void ConnectWebSocket(string token)
    {
        Debug.Log("🔍 WebSocketManager: ConnectWebSocket() 실행됨!");

        string wsUrl = "ws://0.0.0.0:8000/ws?token=" + token; // 게임 서버 WebSocket 주소

#if UNITY_WEBGL
        Debug.Log("🌐 WebGL 환경에서 WebSocket 연결 시도");
        websocket = new WebSocket(wsUrl);
        websocket.OnOpen += () => Debug.Log("✅ WebSocket Connected (WebGL)");
        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log("📩 Received: " + message);
        };
        websocket.OnError += (e) => Debug.LogError("❌ WebSocket Error: " + e);
        websocket.OnClose += (e) => Debug.Log("🔌 WebSocket Closed");
        await websocket.Connect();
#else
        Debug.Log("💻 PC 환경에서 WebSocket 연결 시도");
        websocket = new ClientWebSocket();
        cancellation = new CancellationTokenSource();

        try
        {
            Uri serverUri = new Uri(wsUrl);
            await websocket.ConnectAsync(serverUri, cancellation.Token);
            Debug.Log("✅ WebSocket Connected! (PC)");
            _ = ReceiveMessages();
        }
        catch (Exception e)
        {
            Debug.LogError("❌ WebSocket 연결 실패: " + e.Message);
        }
#endif
    }

#if !UNITY_WEBGL
    private async Task ReceiveMessages()
    {
        byte[] buffer = new byte[1024];

        try
        {
            while (websocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellation.Token);
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Debug.Log("📩 Received Message: " + message);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ WebSocket Receive Error: " + e.Message);
        }
    }
#endif

    private async void OnApplicationQuit()
    {
#if UNITY_WEBGL
        if (websocket != null) await websocket.Close();
#else
        if (websocket != null)
        {
            await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellation.Token);
            websocket.Dispose();
        }
#endif
    }
*/
}
