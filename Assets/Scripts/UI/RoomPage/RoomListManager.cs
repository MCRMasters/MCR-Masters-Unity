using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MCRGame.Net;  // RoomApiManager, RoomData 등 네임스페이스 가정

namespace MCRGame.UI
{
    public class RoomListManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform contentParent; // Scroll View의 Content
        [SerializeField] private GameObject roomItemPrefab; // RoomItem 프리팹

        [Header("Dependencies")]
        [SerializeField] private RoomApiManager roomApiManager; // 방 목록을 가져올 API 매니저

        private void Start()
        {
            // 씬이 시작되면 자동으로 방 목록을 갱신해 봅니다.
            RefreshRoomList();
        }

        /// <summary>
        /// 방 목록을 새로고침(갱신)합니다.
        /// </summary>
        public void RefreshRoomList()
        {
            // roomApiManager에서 FetchRooms를 호출하여 서버에서 방 목록을 가져옵니다.
            StartCoroutine(roomApiManager.FetchRooms(OnRoomsFetched, OnFetchError));
        }

        /// <summary>
        /// 방 목록을 정상적으로 받아왔을 때 호출되는 콜백
        /// </summary>
        /// <param name="roomListResponse">서버에서 받아온 AvailableRoomResponseList</param>
        private void OnRoomsFetched(AvailableRoomResponseList roomListResponse)
        {
            // 기존에 생성된 RoomItem들을 제거합니다.
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }

            if (roomListResponse == null || roomListResponse.rooms == null)
            {
                Debug.LogWarning("[RoomListManager] 방 목록이 비어 있습니다.");
                return;
            }

            // 받아온 방 목록을 순회하며 RoomItem을 생성합니다.
            foreach (var roomInfo in roomListResponse.rooms)
            {
                CreateRoomItem(roomInfo);
            }
        }

        /// <summary>
        /// 방 목록을 받아오지 못했을 때 호출되는 콜백
        /// </summary>
        /// <param name="error">에러 메시지</param>
        private void OnFetchError(string error)
        {
            Debug.LogError("[RoomListManager] 방 목록 가져오기 실패: " + error);
        }

        /// <summary>
        /// RoomItem 프리팹을 Instantiate하고 RoomItem 스크립트에 데이터를 설정합니다.
        /// </summary>
        /// <param name="roomInfo">방 정보</param>
        private void CreateRoomItem(AvailableRoomResponse roomInfo)
        {
            // RoomItem 프리팹을 생성하고, Scroll View의 Content를 부모로 설정
            GameObject itemObj = Instantiate(roomItemPrefab, contentParent);

            // RoomItem 컴포넌트를 가져옵니다.
            RoomItem roomItem = itemObj.GetComponent<RoomItem>();
            if (roomItem != null)
            {
                // 방 번호, 방 제목, 인원 정보 등을 문자열로 구성
                string roomId = roomInfo.room_number.ToString();
                string roomTitle = roomInfo.name;
                string info = $"Host: {roomInfo.host_nickname} | Users: {roomInfo.current_users}/{roomInfo.max_users}";

                // RoomItem의 Setup을 통해 UI를 갱신
                roomItem.Setup(roomResponse:roomInfo);

                // RoomApiManager 참조를 연결(JoinRoom 호출 등에 사용)
                roomItem.roomApiManager = roomApiManager;
            }
            else
            {
                Debug.LogError("[RoomListManager] RoomItem 컴포넌트를 찾을 수 없습니다.");
            }
        }
    }
}
