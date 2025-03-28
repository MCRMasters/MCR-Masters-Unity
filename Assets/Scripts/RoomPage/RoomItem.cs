using UnityEngine;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    public Text titleText;   // 방 제목
    public Text infoText;    // 방 정보
    public Text idText;      // 방 번호 (문자열)

    private string roomId;      // 방 번호 (API에는 int 변환해서 보냄)
    private string roomTitle;   // 방 제목

    // 기존 UI 전환 담당 (로비 이동 등)
    public LobbyRoomChange lobbyRoomChange;

    // 새로운 JoinRoomManager (API 호출 담당)
    public JoinRoomManager joinRoomManager;

    /// <summary>
    /// RoomItem 초기화
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
    /// RoomItem 클릭 시 (Button OnClick)
    /// </summary>
    public void OnClickRoom()
    {
        Debug.Log($"[RoomItem] Clicked room: {roomId} - {roomTitle}");

        if (joinRoomManager != null)
        {
            // 1) JoinRoomManager를 통해 API 호출
            joinRoomManager.JoinRoom(roomId, (bool success) =>
            {
                if (success)
                {
                    // 2) 성공 시 LobbyRoomChange에서 UI 전환
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
