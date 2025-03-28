using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RoomRefreshManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button refreshButton;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject roomItemPrefab;

    // 씬 오브젝트
    [Header("Scene References")]
    [SerializeField] private JoinRoomManager joinRoomManager;
    [SerializeField] private LobbyRoomChange lobbyRoomChange;

    // GetRoomList 스크립트를 Inspector에서 할당
    [SerializeField] private GetRoomList getRoomList;

    private bool hasRefreshedOnce = false; // 한 번만 자동 호출하기 위한 플래그

    private void Start()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshRoomList);

        // 씬 전환 직후(또는 게임 오브젝트 활성화 시) 한 번 자동 호출
        // 필요에 따라 Start가 아닌 OnEnable에서 처리해도 됩니다.
        RefreshRoomListOnce();
    }

    /// <summary>
    /// 씬 전환 시점에 한 번만 방 목록을 가져옵니다.
    /// </summary>
    private void RefreshRoomListOnce()
    {
        if (!hasRefreshedOnce)
        {
            hasRefreshedOnce = true;
            RefreshRoomList();
        }
    }

    /// <summary>
    /// "Refresh" 버튼 클릭 시, 또는 자동 호출 시 방 목록을 가져옵니다.
    /// </summary>
    public void RefreshRoomList()
    {
        // GetRoomList.cs의 FetchRooms 호출
        StartCoroutine(getRoomList.FetchRooms(OnRoomsFetched, OnFetchError));
    }

    /// <summary>
    /// 방 목록 요청이 성공했을 때 호출되는 콜백
    /// </summary>
    private void OnRoomsFetched(AvailableRoomResponseList roomListResponse)
    {
        // 기존 방 아이템 제거
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        bool autoJoined = false;
        string currentNickname = PlayerDataManager.Instance.Nickname;

        if (roomListResponse != null && roomListResponse.rooms != null)
        {
            foreach (var room in roomListResponse.rooms)
            {
                string roomId = room.room_number.ToString();
                string title = room.name;
                string info = $"Host: {room.host_nickname} | Users: {room.current_users}/{room.max_users}";

                RoomData newRoom = new RoomData
                {
                    roomId = roomId,
                    roomTitle = title,
                    roomInfo = info
                };

                CreateRoomItem(newRoom);

                // 자동 참가 로직
                if (!autoJoined && room.users != null && !string.IsNullOrEmpty(currentNickname))
                {
                    foreach (var user in room.users)
                    {
                        if (user.nickname == currentNickname)
                        {
                            autoJoined = true;
                            Debug.Log($"[RoomRefreshManager] Auto-joining room {roomId} (already in room).");
                            if (lobbyRoomChange != null)
                                lobbyRoomChange.JoinRoom(roomId, title);

                            // 메서드를 즉시 종료 (한 번만 자동 전환)
                            return;
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("[RoomRefreshManager] No rooms found in response.");
        }
    }

    /// <summary>
    /// 방 목록 요청이 실패했을 때 호출되는 콜백
    /// </summary>
    private void OnFetchError(string error)
    {
        Debug.LogError("[RoomRefreshManager] GET request failed: " + error);
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
            roomItem.joinRoomManager = joinRoomManager;
            roomItem.lobbyRoomChange = lobbyRoomChange;
            roomItem.Setup(data.roomId, data.roomTitle, data.roomInfo, lobbyRoomChange);
        }
        else
        {
            Debug.LogError("[RoomRefreshManager] RoomItem component not found on prefab.");
        }
    }
}