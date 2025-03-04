using UnityEngine;
using Mirror;

public class CustomNetworkManagerHUD : MonoBehaviour
{
    NetworkManager manager;

    // 화면 오프셋(원하는 위치로 조정)
    public int offsetX;
    public int offsetY;

    // 클라이언트 연결 시 HUD를 숨길지 여부
    public bool hideWhenClientConnected = true;

    void Awake()
    {
        manager = GetComponent<NetworkManager>();
    }

    void OnGUI()
    {
        // 클라이언트가 연결되어 있고, 서버는 활성화되어 있지 않으면 HUD를 숨김
        if (hideWhenClientConnected && NetworkClient.isConnected && !NetworkServer.active)
            return;

        // HUD 영역 크기 (필요에 따라 조정)
        int width = 500;
        GUILayout.BeginArea(new Rect(10 + offsetX, 40 + offsetY, width, 9999));

        if (!NetworkClient.isConnected && !NetworkServer.active)
            StartButtons();
        else
            StatusLabels();

        if (NetworkClient.isConnected && !NetworkClient.ready)
        {
            if (GUILayout.Button("Client Ready"))
            {
                // 클라이언트 준비 상태 전환
                NetworkClient.Ready();
                if (NetworkClient.localPlayer == null)
                    NetworkClient.AddPlayer();
            }
        }

        StopButtons();

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (!NetworkClient.active)
        {
#if UNITY_WEBGL
            // WebGL 빌드에서는 서버 기능이 제한됨
            if (GUILayout.Button("Single Player"))
            {
                NetworkServer.dontListen = true;
                manager.StartHost();
            }
#else
            if (GUILayout.Button("Host (Server + Client)"))
                manager.StartHost();
#endif

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Client"))
                manager.StartClient();

            manager.networkAddress = GUILayout.TextField(manager.networkAddress);

            // 포트 필드 (사용 중인 Transport가 PortTransport인 경우)
            if (Transport.active is PortTransport portTransport)
            {
                if (ushort.TryParse(GUILayout.TextField(portTransport.Port.ToString()), out ushort port))
                    portTransport.Port = port;
            }
            GUILayout.EndHorizontal();

#if UNITY_WEBGL
            GUILayout.Box("( WebGL cannot be server )");
#else
            if (GUILayout.Button("Server Only"))
                manager.StartServer();
#endif
        }
        else
        {
            GUILayout.Label($"Connecting to {manager.networkAddress}...");
            if (GUILayout.Button("Cancel Connection Attempt"))
                manager.StopClient();
        }
    }

    void StatusLabels()
    {
        if (NetworkServer.active && NetworkClient.active)
        {
            GUILayout.Label($"<b>Host</b>: running via {Transport.active}");
        }
        else if (NetworkServer.active)
        {
            GUILayout.Label($"<b>Server</b>: running via {Transport.active}");
        }
        else if (NetworkClient.isConnected)
        {
            GUILayout.Label($"<b>Client</b>: connected to {manager.networkAddress} via {Transport.active}");
        }
    }

    void StopButtons()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            GUILayout.BeginHorizontal();
#if UNITY_WEBGL
            if (GUILayout.Button("Stop Single Player"))
                manager.StopHost();
#else
            if (GUILayout.Button("Stop Host"))
                manager.StopHost();
            if (GUILayout.Button("Stop Client"))
                manager.StopClient();
#endif
            GUILayout.EndHorizontal();
        }
        else if (NetworkClient.isConnected)
        {
            if (GUILayout.Button("Stop Client"))
                manager.StopClient();
        }
        else if (NetworkServer.active)
        {
            if (GUILayout.Button("Stop Server"))
                manager.StopServer();
        }
    }
}
