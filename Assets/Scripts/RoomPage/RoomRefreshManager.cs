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
}

// 배열 파싱용 래퍼 클래스
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
    [SerializeField] private Button refreshButton;    // 방 새로고침 버튼
    [SerializeField] private Transform contentParent; // 스크롤뷰 Content
    [SerializeField] private GameObject roomItemPrefab; // RoomItem 프리팹

    // **씬 오브젝트**를 Inspector에서 드래그하여 할당
    [Header("Scene References")]
    [SerializeField] private JoinRoomManager joinRoomManager;  // 방 참가 API 담당
    [SerializeField] private LobbyRoomChange lobbyRoomChange;  // 로비 UI 전환 담당

    // 서버 URL (GET /api/v1/room)
    private string getRoomsUrl = "http://0.0.0.0:8000/api/v1/room";

    private void Start()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshRoomList);
    }

    /// <summary>
    /// 리프레시 버튼 클릭 시, GET 요청으로 방 목록을 가져와 스크롤뷰를 갱신
    /// </summary>
    public void RefreshRoomList()
    {
        StartCoroutine(GetRoomsAndPopulate());
    }

    private IEnumerator GetRoomsAndPopulate()
    {
        // 기존에 생성된 방 아이템 모두 제거 (옵션)
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

                // JsonUtility는 배열을 직접 파싱할 수 없으므로, 래퍼 객체로 감싼 후 파싱
                string wrappedJson = "{\"rooms\":" + jsonResponse + "}";
                AvailableRoomResponseList roomListResponse = JsonUtility.FromJson<AvailableRoomResponseList>(wrappedJson);

                if (roomListResponse != null && roomListResponse.rooms != null)
                {
                    foreach (var room in roomListResponse.rooms)
                    {
                        // room_number를 문자열로 변환하여 RoomItem의 roomId로 사용
                        string roomId = room.room_number.ToString();
                        string title = room.name;
                        string info = $"Host: {room.host_nickname} | Users: {room.current_users}/{room.max_users}";

                        // 로컬 RoomData 생성
                        RoomData newRoom = new RoomData
                        {
                            roomId = roomId,
                            roomTitle = title,
                            roomInfo = info
                        };

                        // 스크롤뷰에 RoomItem을 생성
                        CreateRoomItem(newRoom);
                    }
                }
                else
                {
                    Debug.LogWarning("[RoomRefreshManager] No rooms found in response.");
                }
            }
            else
            {
                Debug.LogError("[RoomRefreshManager] GET request failed: " + getRequest.error);
            }
        }
    }

    /// <summary>
    /// RoomItem 프리팹을 Instantiate하고 데이터를 설정
    /// </summary>
    private void CreateRoomItem(RoomData data)
    {
        GameObject itemObj = Instantiate(roomItemPrefab, contentParent);
        RoomItem roomItem = itemObj.GetComponent<RoomItem>();

        if (roomItem != null)
        {
            // **씬 오브젝트**(JoinRoomManager, LobbyRoomChange)를 런타임에 할당
            roomItem.joinRoomManager = joinRoomManager;
            roomItem.lobbyRoomChange = lobbyRoomChange;

            // 데이터 세팅 (Room ID, Title, Info, LobbyRoomChange)
            roomItem.Setup(data.roomId, data.roomTitle, data.roomInfo, lobbyRoomChange);
        }
        else
        {
            Debug.LogError("[RoomRefreshManager] RoomItem component not found on prefab.");
        }
    }
}
