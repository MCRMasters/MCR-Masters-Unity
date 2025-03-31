using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 버튼을 통해 RoomWebsocketManager의 기능을 호출
/// </summary>
///
namespace MCRGame
{
    public class WebsocketRoomButton : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button connectButton;
        [SerializeField] private Button pingButton;
        [SerializeField] private Button readyButton;
        [SerializeField] private Button disconnectButton;

        [Header("Ready Toggle")]
        [SerializeField] private Toggle readyToggle;

        [Header("Reference to WebSocket Manager")]
        [SerializeField] private RoomWebsocketManager websocketManager;

        private void Start()
        {
            if (connectButton != null)
                connectButton.onClick.AddListener(OnConnectClicked);

            if (pingButton != null)
                pingButton.onClick.AddListener(OnPingClicked);

            if (readyButton != null)
                readyButton.onClick.AddListener(OnReadyClicked);

            if (disconnectButton != null)
                disconnectButton.onClick.AddListener(OnDisconnectClicked);
        }

        private void OnConnectClicked()
        {
            websocketManager.Connect();
        }

        private void OnPingClicked()
        {
            websocketManager.SendPing();
        }

        private void OnReadyClicked()
        {
            bool isReady = (readyToggle != null) ? readyToggle.isOn : true;
            websocketManager.SendReady(isReady);
        }

        private async void OnDisconnectClicked()
        {
            await websocketManager.Disconnect();
        }
    }
}