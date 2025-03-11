using UnityEngine;

public class LobbyRoomChange : MonoBehaviour
{
    public GameObject scrollView;      // Scroll View UI (room list)
    public GameObject lobbyPanel;      // Lobby UI
    public GameObject roomSetting;     // RoomSetting panel (contains the two InputFields)

    // Called when a room item is clicked
    public void JoinRoom(string roomId)
    {
        Debug.Log($"JoinRoom() called. Room ID: {roomId}");

        // Disable the room list (scroll view)
        if (scrollView != null)
            scrollView.SetActive(false);

        // Enable the lobby UI
        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);
    }

    // Called to return to the room list from the lobby
    public void ShowRoomList()
    {
        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);

        if (scrollView != null)
            scrollView.SetActive(true);

        // Enable the RoomSetting panel
        if (roomSetting != null)
            roomSetting.SetActive(false);

        Debug.Log("Returned to room list");
    }

    // New: Called by the "MakeRoomSet" button to show the RoomSetting panel
    public void OnClickMakeRoomSet()
    {
        // Disable room list and lobby UI if needed
        if (scrollView != null)
            scrollView.SetActive(false);
        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);

        // Enable the RoomSetting panel
        if (roomSetting != null)
            roomSetting.SetActive(true);

        Debug.Log("MakeRoomSet button clicked → RoomSetting activated");
    }
}
