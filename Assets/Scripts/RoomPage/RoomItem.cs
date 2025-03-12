using UnityEngine;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    public Text titleText;  // 방 제목 표시
    public Text infoText;   // 방 설명 표시 (선택)
    public Text idText;  // 방 ID 표시용

    private string roomId;
    private string roomTitle;
    private LobbyRoomChange lobbyRoomChange;

    public void Setup(string id, string title, string info, LobbyRoomChange manager)
    {
        roomId = id;
        roomTitle = title;
        lobbyRoomChange = manager;

        // 새로 만든 Text 필드
        if (idText != null)
            idText.text = id;

        if (titleText != null)
            titleText.text = title;

        if (infoText != null)
            infoText.text = info;
    }


    public void OnClickRoom()
    {
        Debug.Log($"Clicked room: {roomId}");
        // 방 번호, 방 제목을 LobbyRoomChange로 전달
        if (lobbyRoomChange != null)
        {
            lobbyRoomChange.JoinRoom(roomId, roomTitle);
        }
    }
}
