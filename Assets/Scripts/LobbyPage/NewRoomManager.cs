using System.Collections.Generic;
using UnityEngine;

public class NewRoomManager : MonoBehaviour
{
    // A simple class to hold room data
    [System.Serializable]
    public class RoomData
    {
        public string roomId;
        public string roomName;
    }

    // List of rooms (can be set in the Inspector or added dynamically)
    public List<RoomData> roomList = new List<RoomData>();

    // The prefab for each room item (make sure this prefab has the RoomItem script attached)
    public GameObject roomItemPrefab;

    // The parent transform where the room items will be instantiated (e.g., the Content of a Scroll View)
    public Transform contentParent;

    // Reference to the LobbyRoomChange component, which handles the room joining logic.
    public LobbyRoomChange lobbyRoomChange;

    void Start()
    {
        // Add temporary test room data
        roomList.Add(new RoomData { roomId = "Room001", roomName = "Room 1" });
        roomList.Add(new RoomData { roomId = "Room002", roomName = "Room 2" });
        roomList.Add(new RoomData { roomId = "Room003", roomName = "Room 3" });
        roomList.Add(new RoomData { roomId = "Room004", roomName = "Room 4" });
        roomList.Add(new RoomData { roomId = "Room005", roomName = "Room 5" });
        // Add more rooms if needed

        GenerateRoomItems();
    }

    // Generates room items based on the roomList data
    public void GenerateRoomItems()
    {
        // Remove any existing children (previous room items)
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Instantiate a RoomItem prefab for each room in the roomList
        foreach (RoomData room in roomList)
        {
            GameObject itemObj = Instantiate(roomItemPrefab, contentParent);
            RoomItem roomItem = itemObj.GetComponent<RoomItem>();
            if (roomItem != null)
            {
                // Setup the room item with its roomId, roomName, and the reference to LobbyRoomChange.
                roomItem.Setup(room.roomId, room.roomName, lobbyRoomChange);
            }
        }
    }
}
