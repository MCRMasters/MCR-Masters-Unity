using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MCRGame.Net;
using System.Linq;
using Unity.VisualScripting;


namespace MCRGame.UI
{
    public class RoomUIController : MonoBehaviour
    {
        [SerializeField] private Text roomTitleText;
        [SerializeField] private Transform playersParent;
        [SerializeField] private GameObject playerSlotPrefab;
        [SerializeField] private Button readyButton;
        [SerializeField] private Button startButton;
        [SerializeField] private Button leaveButton;

        private Dictionary<int, PlayerSlotUI> slots = new();

        private void OnEnable()
        {
            RoomService.Instance.OnRoomJoined += OnRoomJoined;
            RoomService.Instance.OnRoomUsersUpdated += OnRoomUsersUpdated;
            RoomService.Instance.OnUserReadyChanged += OnUserReadyChanged;
            RoomService.Instance.OnGameStarted += OnGameStarted;

            readyButton.onClick.AddListener(OnReadyClicked);
            startButton.onClick.AddListener(OnStartClicked);
            leaveButton.onClick.AddListener(OnLeaveClicked);
        }

        private void OnDisable()
        {
            RoomService.Instance.OnRoomJoined -= OnRoomJoined;
            RoomService.Instance.OnRoomUsersUpdated -= OnRoomUsersUpdated;
            RoomService.Instance.OnUserReadyChanged -= OnUserReadyChanged;
            RoomService.Instance.OnGameStarted -= OnGameStarted;

            readyButton.onClick.RemoveListener(OnReadyClicked);
            startButton.onClick.RemoveListener(OnStartClicked);
            leaveButton.onClick.RemoveListener(OnLeaveClicked);
        }

        private void OnRoomJoined(RoomJoinedInfo info)
        {
            roomTitleText.text = RoomService.Instance.CurrentRoomTitle;
        }

        private void OnRoomUsersUpdated(string hostUid, List<RoomUserInfo> users)
        {
            // 1) 기존 슬롯들 모두 제거
            foreach (Transform t in playersParent) Destroy(t.gameObject);
            slots.Clear();

            // 2) 0~3 슬롯 고정 순회
            for (int slotIndex = 0; slotIndex < 4; slotIndex++)
            {
                var go = Instantiate(playerSlotPrefab, playersParent);
                var slotUI = go.GetComponent<PlayerSlotUI>();
                // 이 인덱스에 해당하는 유저 찾기
                var user = users.FirstOrDefault(u => u.slot_index == slotIndex);
                if (user != null)
                {
                    // 실제 유저가 있으면 세팅
                    slotUI.Setup(user, hostUid, slotIndex);
                    slots.Add(slotIndex, slotUI);
                }
                else
                {
                    // 비어있는 슬롯: 빈 UI로 표시
                    slotUI.SetEmpty(slotIndex);
                }
            }

            // 3) 버튼 활성화 처리
            bool isHost = RoomService.Instance.HostUid == RoomService.Instance.GetMyUid();
            readyButton.gameObject.SetActive(true);
            startButton.gameObject.SetActive(isHost);
        }


        private void OnUserReadyChanged(string uid, bool ready)
        {
            foreach (var kv in slots)
                if (kv.Value.Uid == uid)
                    kv.Value.SetReady(ready);
        }

        private void OnGameStarted()
        {
            SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }

        private void OnReadyClicked()
        {
            RoomService.Instance.SendReady(!RoomService.Instance.GetMyReadyState());
        }

        private void OnStartClicked()
        {
            RoomService.Instance.StartGame();
        }

        private void OnLeaveClicked()
        {
            RoomService.Instance.LeaveRoom();
        }
    }
}