using System.Collections.Generic;
using UnityEngine;

public class NewRoomManager : MonoBehaviour
{
    [System.Serializable]
    public class RoomData
    {
        public string roomId;
        public string roomName;
    }

    // A list that stores room information (initially empty)
    public List<RoomData> roomList = new List<RoomData>();

    // Prefab for the RoomItem (an object with the RoomItem script attached)
    public GameObject roomItemPrefab;

    // The Content object in the Scroll View
    public Transform contentParent;

    // A script that handles lobby-related logic (e.g., room joining)
    public LobbyRoomChange lobbyRoomChange;

    // Tracks the number of rooms created (starts at 0 -> increments when the button is pressed)
    private int roomIndex = 0;

    void Start()
    {
        // Initially, no rooms are created
        // Rooms are only created when AddNewRoomNoArg() is called
        Debug.Log("No initial rooms. roomIndex = 0");
    }

    // Adds a new room without parameters: called by a button OnClick event
    public void AddNewRoomNoArg()
    {
        // Increase roomIndex by 1 (0 -> 1, 1 -> 2, ...)
        roomIndex++;

        // Generate a Room ID and Name (e.g., Room001, Room 1)
        string newId = "Room" + roomIndex.ToString("D3");
        string newName = "Room " + roomIndex;

        // Create new room data and add it to the list
        RoomData newRoom = new RoomData { roomId = newId, roomName = newName };
        roomList.Add(newRoom);

        // Instantiate the RoomItem prefab and display it in the UI
        CreateRoomItem(newRoom);

        Debug.Log($"New room created: {newId} - {newName}");
    }

    // Instantiates a RoomItem prefab under the Content using the given RoomData
    private void CreateRoomItem(RoomData data)
    {
        GameObject itemObj = Instantiate(roomItemPrefab, contentParent);
        RoomItem roomItem = itemObj.GetComponent<RoomItem>();
        if (roomItem != null)
        {
            // Assign roomId, roomName, and LobbyRoomChange to the RoomItem script
            roomItem.Setup(data.roomId, data.roomName, lobbyRoomChange);
        }
    }
}
