using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

// 서버에서 반환하는 방 정보 모델
[System.Serializable]
public class AvailableRoomResponse
{
    public string name;
    public int room_number;
    public int max_users;
    public int current_users;
    public string host_nickname;
    // 필요에 따라 users 배열 등 추가 가능
}

// 래퍼 클래스: JsonUtility로 배열을 파싱하기 위해 사용합니다.
[System.Serializable]
public class AvailableRoomResponseList
{
    public AvailableRoomResponse[] rooms;
}

// 로컬 UI에 표시할 방 데이터 모델 (RoomItem에 전달)
[System.Serializable]
public class RoomData
{
    public string roomId;      // 여기서는 room_number를 문자열로 사용
    public string roomTitle;   // 방 제목 (name)
    public string roomInfo;    // 추가 정보 (예: 호스트와 인원 정보)
}

public class RoomRefreshManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button refreshButton; // 방 새로고침(리프레쉬) 버튼
    [SerializeField] private Transform contentParent; // 스크롤뷰 Content 오브젝트
    [SerializeField] private GameObject roomItemPrefab; // RoomItem 프리팹

    // 서버 URL (GET /api/v1/room)
    private string getRoomsUrl = "http://0.0.0.0:8000/api/v1/room";

    private void Start()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshRoomList);
    }

    /// <summary>
    /// 버튼 클릭 시, GET 요청으로 현재 방 정보를 가져와 스크롤뷰를 갱신합니다.
    /// </summary>
    public void RefreshRoomList()
    {
        StartCoroutine(GetRoomsAndPopulate());
    }

    private IEnumerator GetRoomsAndPopulate()
    {
        // 이전에 생성된 방 아이템 모두 제거 (옵션)
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        using (UnityWebRequest getRequest = UnityWebRequest.Get(getRoomsUrl))
        {
            getRequest.SetRequestHeader("Content-Type", "application/json");
            getRequest.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");

            yield return getRequest.SendWebRequest();

            if (getRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = getRequest.downloadHandler.text;
                Debug.Log("[RoomRefreshManager] GET available rooms: " + jsonResponse);

                // JsonUtility는 배열을 직접 파싱할 수 없으므로, 래퍼 객체로 감싸줍니다.
                string wrappedJson = "{\"rooms\":" + jsonResponse + "}";
                AvailableRoomResponseList roomListResponse = JsonUtility.FromJson<AvailableRoomResponseList>(wrappedJson);

                if (roomListResponse.rooms != null)
                {
                    foreach (var room in roomListResponse.rooms)
                    {
                        // room_number를 문자열로 변환하여 RoomItem의 id로 사용하고,
                        // name은 방 제목, 추가 정보는 호스트와 인원 수 정보로 구성
                        string roomId = room.room_number.ToString();
                        string title = room.name;
                        string info = "Host: " + room.host_nickname + " | Users: " + room.current_users + "/" + room.max_users;
                        RoomData newRoom = new RoomData { roomId = roomId, roomTitle = title, roomInfo = info };
                        CreateRoomItem(newRoom);
                    }
                }
            }
            else
            {
                Debug.LogError("[RoomRefreshManager] GET request failed: " + getRequest.error);
            }
        }
    }

    /// <summary>
    /// 스크롤뷰에 새로운 RoomItem을 생성하여 추가합니다.
    /// </summary>
    private void CreateRoomItem(RoomData data)
    {
        GameObject itemObj = Instantiate(roomItemPrefab, contentParent);
        RoomItem roomItem = itemObj.GetComponent<RoomItem>();
        if (roomItem != null)
        {
            // LobbyRoomChange가 있다면 전달, 없으면 null 전달
            roomItem.Setup(data.roomId, data.roomTitle, data.roomInfo, null);
        }
    }
}
