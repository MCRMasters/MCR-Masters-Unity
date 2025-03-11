using UnityEngine;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    public Text titleText; // 방 제목 표시
    public Text infoText;  // 방 설명 표시

    private string roomId;
    private LobbyRoomChange lobbyRoomChange;

    // 3개 파라미터 (ID, Title, Info) + LobbyRoomChange
    public void Setup(string id, string title, string info, LobbyRoomChange manager)
    {
        roomId = id;
        lobbyRoomChange = manager;

        if (titleText != null) titleText.text = title;
        if (infoText != null) infoText.text = info;
    }

    // 방 아이템 클릭 시 호출
    public void OnClickRoom()
    {
        Debug.Log($"Clicked room: {roomId}");
        // lobbyRoomChange.JoinRoom(roomId) 등 로직 가능
    }
}
