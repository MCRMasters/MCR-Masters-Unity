using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MCRGame.Net;  // RoomApiManager, AvailableRoomResponseList 등 포함

namespace MCRGame.UI
{
    public class RoomListManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform contentParent; // Scroll View의 Content
        [SerializeField] private GameObject roomItemPrefab; // RoomItem 프리팹
        [SerializeField] private Button refreshButton;      // 수동 갱신용 Refresh 버튼

        [Header("Dependencies")]
        [SerializeField] private RoomApiManager roomApiManager; // 방 목록을 가져올 API 매니저

        [Header("Settings")]
        [SerializeField] private float refreshInterval = 5f; // 자동 갱신 주기(초)

        private Coroutine autoRefreshCoroutine;

        private void Start()
        {
            // Refresh 버튼 클릭 시 즉시 방 목록 갱신
            if (refreshButton != null)
                refreshButton.onClick.AddListener(RefreshRoomList);

            // 자동 갱신 코루틴 시작
            autoRefreshCoroutine = StartCoroutine(AutoRefreshRoomList());

            // 씬 시작 시 초기 방 목록 갱신
            RefreshRoomList();
        }

        /// <summary>
        /// 5초마다 자동으로 방 목록을 갱신하는 코루틴입니다.
        /// </summary>
        private IEnumerator AutoRefreshRoomList()
        {
            while (true)
            {
                yield return new WaitForSeconds(refreshInterval);
                RefreshRoomList();
            }
        }

        /// <summary>
        /// RoomApiManager의 FetchRooms를 호출하여 방 목록을 새로고침합니다.
        /// </summary>
        public void RefreshRoomList()
        {
            Debug.Log("[RoomListManager] 방 목록 갱신 요청.");
            StartCoroutine(roomApiManager.FetchRooms(OnRoomsFetched, OnFetchError));
        }

        /// <summary>
        /// 서버에서 방 목록을 정상적으로 받아왔을 때 호출되는 콜백입니다.
        /// 기존에 생성된 RoomItem들을 제거한 후, 새롭게 RoomItem을 생성합니다.
        /// </summary>
        /// <param name="roomListResponse">서버에서 받아온 방 목록 응답</param>
        private void OnRoomsFetched(AvailableRoomResponseList roomListResponse)
        {
            // 기존 RoomItem 제거
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }

            if (roomListResponse == null || roomListResponse.rooms == null)
            {
                Debug.LogWarning("[RoomListManager] 방 목록이 비어 있습니다.");
                return;
            }

            // 받아온 방 목록 순회하며 RoomItem 생성
            foreach (var roomInfo in roomListResponse.rooms)
            {
                CreateRoomItem(roomInfo);
            }
        }

        /// <summary>
        /// 방 목록을 받아오지 못했을 때 호출되는 콜백입니다.
        /// </summary>
        /// <param name="error">오류 메시지</param>
        private void OnFetchError(string error)
        {
            Debug.LogError("[RoomListManager] 방 목록 가져오기 실패: " + error);
        }

        /// <summary>
        /// RoomItem 프리팹을 인스턴스화하여, 방 정보를 표시하고 RoomApiManager 참조를 설정합니다.
        /// </summary>
        /// <param name="roomInfo">방 정보</param>
        private void CreateRoomItem(AvailableRoomResponse roomInfo)
        {
            GameObject itemObj = Instantiate(roomItemPrefab, contentParent);
            RoomItem roomItem = itemObj.GetComponent<RoomItem>();
            if (roomItem != null)
            {
                // RoomItem의 Setup 메서드를 통해 UI 갱신
                roomItem.Setup(roomResponse: roomInfo);
                // 방 참가 등의 API 호출에 사용할 RoomApiManager 연결
                roomItem.roomApiManager = roomApiManager;
            }
            else
            {
                Debug.LogError("[RoomListManager] RoomItem 컴포넌트를 찾을 수 없습니다.");
            }
        }
    }
}
