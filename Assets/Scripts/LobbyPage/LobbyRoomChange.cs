using UnityEngine;

public class LobbyRoomChange : MonoBehaviour
{
    public GameObject scrollView;   // Scroll View UI for room list
    public GameObject lobbyPanel;   // Lobby UI

    // Called when a room item is clicked
    public void JoinRoom(string roomId)
    {
        Debug.Log($"JoinRoom() 호출됨. 방 번호: {roomId}");

        // TODO: Add room entry logic here (e.g., network communication, scene change, etc.)

        // Disable the room list (scroll view)
        if (scrollView != null)
            scrollView.SetActive(false);

        // Enable the lobby UI
        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);
    }

    // (Optional) Called to return to the room list from the lobby
    public void ShowRoomList()
    {
        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);

        if (scrollView != null)
            scrollView.SetActive(true);

        Debug.Log("방 목록으로 돌아옴");
    }
}
