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

    // 방 정보를 저장하는 리스트 (초기엔 비어 있음)
    public List<RoomData> roomList = new List<RoomData>();

    // RoomItem 프리팹 (RoomItem 스크립트가 붙어 있는 UI 오브젝트)
    public GameObject roomItemPrefab;

    // Scroll View의 Content 오브젝트
    public Transform contentParent;

    // 로비 입장 관련 로직 담당 스크립트
    public LobbyRoomChange lobbyRoomChange;

    // 현재까지 생성된 방의 인덱스 (0에서 시작 → 버튼 누를 때 1부터 증가)
    private int roomIndex = 0;

    void Start()
    {
        // 초기에는 아무 방도 생성하지 않음
        // 원하는 시점에만 AddNewRoomNoArg()를 통해 방 생성
        Debug.Log("초기 방은 없음. roomIndex = 0");
    }

    // 매개변수 없이 방을 추가: 버튼 OnClick 이벤트에서 호출
    public void AddNewRoomNoArg()
    {
        // 방 인덱스를 1 증가 (0 → 1, 1 → 2, ...)
        roomIndex++;

        // Room ID, Name 생성 (예: Room001, Room 1)
        string newId = "Room" + roomIndex.ToString("D3");
        string newName = "Room " + roomIndex;

        // 새 방 데이터를 리스트에 추가
        RoomData newRoom = new RoomData { roomId = newId, roomName = newName };
        roomList.Add(newRoom);

        // 실제 RoomItem 프리팹을 생성하고 UI에 표시
        CreateRoomItem(newRoom);

        Debug.Log($"새로운 방 생성: {newId} - {newName}");
    }

    // RoomData를 받아 RoomItem 프리팹을 Content 밑에 생성
    private void CreateRoomItem(RoomData data)
    {
        GameObject itemObj = Instantiate(roomItemPrefab, contentParent);
        RoomItem roomItem = itemObj.GetComponent<RoomItem>();
        if (roomItem != null)
        {
            // RoomItem 스크립트에 roomId, roomName, LobbyRoomChange 할당
            roomItem.Setup(data.roomId, data.roomName, lobbyRoomChange);
        }
    }
}
