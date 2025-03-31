using UnityEngine;
using UnityEngine.UI;

namespace MCRGame
{
    public class RoomItem : MonoBehaviour
    {
        public Text titleText;   // 방 제목
        public Text infoText;    // 방 정보
        public Text idText;      // 방 번호 (문자열)

        private string roomId;      // API 전송 시 사용될 방 번호 (문자열)
        private string roomTitle;   // 방 제목

        // 기존 UI 전환 담당 (로비 이동 등)
        public LobbyRoomChange lobbyRoomChange;

        // 기존 방 참가 API 담당
        public JoinRoomManager joinRoomManager;

        /// <summary>
        /// RoomItem 초기화: 방 ID, 제목, 정보, 그리고 로비 전환 담당 객체 할당
        /// </summary>
        public void Setup(string id, string title, string info, LobbyRoomChange manager)
        {
            roomId = id;
            roomTitle = title;
            lobbyRoomChange = manager;

            if (idText != null)
                idText.text = id;
            if (titleText != null)
                titleText.text = title;
            if (infoText != null)
                infoText.text = info;
        }

        /// <summary>
        /// RoomItem 클릭 시 호출됨.
        /// 웹소켓 연결(Connect) 후, joinRoomManager를 통해 방 참가 요청을 보내고,
        /// 성공 시 lobbyRoomChange를 통해 UI 전환합니다.
        /// </summary>
        public void OnClickRoom()
        {
            Debug.Log($"[RoomItem] Clicked room: {roomId} - {roomTitle}");

            // 웹소켓 연결이 필요한 경우, 아직 연결되지 않았다면 연결 시도
            if (RoomWebsocketManager.Instance != null && !RoomWebsocketManager.Instance.IsConnected)
            {
                Debug.Log("[RoomItem] WebSocket is not connected. Initiating connection...");
                RoomWebsocketManager.Instance.Connect();
            }

            if (joinRoomManager != null)
            {
                joinRoomManager.JoinRoom(roomId, (bool success) =>
                {
                    if (success)
                    {
                        if (lobbyRoomChange != null)
                        {
                            lobbyRoomChange.JoinRoom(roomId, roomTitle);
                        }
                    }
                    else
                    {
                        Debug.LogError($"[RoomItem] Failed to join room {roomId}. Cannot transition to room UI.");
                    }
                });
            }
            else
            {
                Debug.LogWarning("[RoomItem] joinRoomManager is not assigned!");
            }
        }
    }
}
