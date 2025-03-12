using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewRoomManager : MonoBehaviour
{
    [System.Serializable]
    public class RoomData
    {
        public string roomId;
        public string roomTitle;  // 방 제목
        public string roomInfo;   // 방 설명 등
    }

    // 방 목록
    public List<RoomData> roomList = new List<RoomData>();

    // RoomItem 프리팹
    public GameObject roomItemPrefab;

    // Scroll View → Content 오브젝트
    public Transform contentParent;

    // 로비(또는 방) 로직 담당 스크립트
    public LobbyRoomChange lobbyRoomChange;

    // 방 인덱스 (고유 ID 생성용)
    private int roomIndex = 0;

    // 2개의 InputField (방 제목, 방 설명)
    public InputField roomTitleInput; // 아까 이미지에서 "RoomTitleInput"로 만드셨다면 Inspector에서 연결
    public InputField roomInfoInput;  // "RoomInfoInput"에 Inspector에서 연결

    void Start()
    {
        Debug.Log("No initial rooms. roomIndex = 0");
    }

    // 버튼 OnClick 이벤트: 2개 InputField로부터 텍스트를 읽어 새 방 생성
    public void CreateRoomFromInput()
    {
        // InputField에서 텍스트 가져오기
        string title = roomTitleInput.text;
        string info = roomInfoInput.text;

        // 인덱스 증가 → 고유 ID 생성
        roomIndex++;
        string newId = "Room" + roomIndex.ToString("D3");

        // RoomData 생성
        RoomData newRoom = new RoomData
        {
            roomId = newId,
            roomTitle = title,
            roomInfo = info
        };

        roomList.Add(newRoom);

        // UI에 표시
        CreateRoomItem(newRoom);

        Debug.Log($"New room created: ID={newId}, Title={title}, Info={info}");

        // InputField 초기화 (선택)
        roomTitleInput.text = "";
        roomInfoInput.text = "";
    }

    // RoomItem 프리팹을 Instantiate하고 데이터 세팅
    private void CreateRoomItem(RoomData data)
    {
        GameObject itemObj = Instantiate(roomItemPrefab, contentParent);
        RoomItem roomItem = itemObj.GetComponent<RoomItem>();
        if (roomItem != null)
        {
            // roomItem.Setup(...)에서 2개 문자열(제목, 설명) + LobbyRoomChange 전달
            roomItem.Setup(data.roomId, data.roomTitle, data.roomInfo, lobbyRoomChange);
        }
    }
}
