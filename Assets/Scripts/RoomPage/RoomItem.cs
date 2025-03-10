using UnityEngine;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    public Text roomNameText;   // Displays the room name on the UI
    private string roomId;      // Room identifier

    // A reference to LobbyRoomChange, allowing us to call the room entry logic
    public LobbyRoomChange lobbyRoomChange;

    // Setup function: assigns the room ID, room name, and the LobbyRoomChange reference
    public void Setup(string id, string name, LobbyRoomChange manager)
    {
        roomId = id;
        lobbyRoomChange = manager;
        if (roomNameText != null)
        {
            roomNameText.text = name;
        }
    }

    // Called from the Inspector as OnClickRoom(string), 
    // allowing a string parameter to be set in the Inspector
    public void OnClickRoom(string param)
    {
        // The string parameter (e.g., "Room001") can be used for roomId or just for debugging
        Debug.Log($"Clicked room param: {param}");

        // If needed, assign param to roomId
        roomId = param;

        // If the LobbyRoomChange script is assigned, call JoinRoom() with the room ID
        if (lobbyRoomChange != null)
        {
            lobbyRoomChange.JoinRoom(roomId);
        }
        else
        {
            Debug.LogError("LobbyRoomChange reference is missing in RoomItem!");
        }
    }
}
