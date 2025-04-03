using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using MCRGame.Net; // RoomDataManager, PlayerDataManager 등이 포함된 네임스페이스

namespace MCRGame.Net
{
    public class RoomManager : MonoBehaviour
    {
        // 4명의 플레이어 Ready 상태 (인덱스 0: Host, 1~3: Guests)
        public bool[] playerReady = new bool[4];

        // 단일 버튼: 호스트이면 "Start", 게스트이면 "Ready"를 표시
        public Button actionButton;

        // 모든 플레이어의 Ready 상태를 나타내는 UI Image 배열 (길이 4)
        public Image[] readyIndicators;

        // RoomDataManager에 저장된 방 정보를 표시할 UI 텍스트 (RoomScene에서)
        public Text roomTitleText;
        public Text roomNumberText;

        // 웹소켓 통신을 위한 RoomWS 인스턴스 (Inspector에서 할당)
        public RoomWS roomWS;

        // 현재 플레이어가 호스트인지 여부
        private bool isHost = false;
        // 게스트의 Ready 상태 (초기 false)
        private bool isReady = false;
        // 자신의 슬롯 번호 (호스트: 0, 게스트: 1~3)
        private int mySlotIndex = 0;

        void Start()
        {
            // 초기화: 호스트는 기본적으로 Ready(true), 게스트들은 Not Ready(false)
            playerReady[0] = true; // Host
            for (int i = 1; i < playerReady.Length; i++)
            {
                playerReady[i] = false;
            }

            // UI 초기 업데이트: 인덱스 0 (호스트)는 녹색, 나머지는 빨간색
            if (readyIndicators != null && readyIndicators.Length >= 4)
            {
                readyIndicators[0].color = Color.green;
                for (int i = 1; i < readyIndicators.Length; i++)
                {
                    readyIndicators[i].color = Color.red;
                }
            }

            // RoomDataManager에 저장된 방 정보를 기반으로 UI 업데이트 (방 제목, 방 번호)
            if (RoomDataManager.Instance != null)
            {
                if (roomTitleText != null)
                    roomTitleText.text = RoomDataManager.Instance.RoomTitle;
                if (roomNumberText != null)
                    roomNumberText.text = RoomDataManager.Instance.RoomId;
            }

            // 현재 플레이어가 호스트인지 결정 (여기서는 Nickname으로 비교)
            if (PlayerDataManager.Instance != null && RoomDataManager.Instance != null)
            {
                isHost = PlayerDataManager.Instance.Nickname == RoomDataManager.Instance.HostNickname;
                Debug.Log($"IsHost: {isHost}");
                if (isHost)
                {
                    mySlotIndex = 0;
                }
                else
                {
                    // 게스트의 경우, GuestNicknames 배열에서 자신의 닉네임 위치를 확인합니다.
                    string[] guestNicknames = RoomDataManager.Instance.GuestNicknames;
                    int slot = -1;
                    for (int i = 0; i < guestNicknames.Length; i++)
                    {
                        if (guestNicknames[i] == PlayerDataManager.Instance.Nickname)
                        {
                            slot = i + 1; // 게스트 슬롯은 1부터 시작
                            break;
                        }
                    }
                    // 빈 슬롯에 등록 (없으면 기본 1번 슬롯)
                    if (slot == -1)
                    {
                        for (int i = 0; i < guestNicknames.Length; i++)
                        {
                            if (string.IsNullOrEmpty(guestNicknames[i]))
                            {
                                guestNicknames[i] = PlayerDataManager.Instance.Nickname;
                                slot = i + 1;
                                break;
                            }
                        }
                    }
                    if (slot == -1)
                    {
                        slot = 1;
                    }
                    mySlotIndex = slot;
                }
            }
            else
            {
                Debug.LogError("PlayerDataManager 또는 RoomDataManager 인스턴스가 없습니다.");
            }

            // 단일 버튼의 텍스트 설정 및 클릭 이벤트 등록
            if (actionButton != null)
            {
                if (isHost)
                {
                    actionButton.GetComponentInChildren<Text>().text = "Start";
                    // 호스트는 자신의 상태가 이미 Ready이므로 서버에 전송
                    if (roomWS != null)
                    {
                        roomWS.SendReadyStatus(true);
                    }
                    // 호스트의 Start 버튼은 초기엔 비활성화; 모든 플레이어가 준비되면 UpdateHostStartButtonState()에서 활성화
                    actionButton.interactable = false;
                }
                else
                {
                    actionButton.GetComponentInChildren<Text>().text = "Ready";
                    actionButton.interactable = true;
                }
                actionButton.onClick.AddListener(OnActionButtonClicked);
            }
        }

        /// <summary>
        /// 단일 버튼 클릭 시 호출. 호스트는 게임 시작, 게스트는 Ready 상태 토글을 수행합니다.
        /// </summary>
        private void OnActionButtonClicked()
        {
            if (isHost)
            {
                OnHostStartGame();
            }
            else
            {
                // 게스트: Ready 상태 토글
                isReady = !isReady;
                playerReady[mySlotIndex] = isReady;
                if (readyIndicators != null && readyIndicators.Length > mySlotIndex)
                {
                    readyIndicators[mySlotIndex].color = isReady ? Color.green : Color.red;
                }
                if (actionButton != null)
                {
                    actionButton.GetComponentInChildren<Text>().text = isReady ? "Ready ✔" : "Ready";
                }
                if (roomWS != null)
                {
                    roomWS.SendReadyStatus(isReady);
                }
            }
        }

        /// <summary>
        /// 호스트가 Start 버튼을 클릭했을 때 호출됩니다.
        /// 모든 플레이어가 Ready 상태여야 게임을 시작할 수 있습니다.
        /// </summary>
        public void OnHostStartGame()
        {
            int readyCount = playerReady.Count(r => r);
            Debug.Log($"[RoomManager] 호스트 Start 버튼 클릭됨. {readyCount}/4 플레이어가 Ready 상태.");

            if (readyCount == playerReady.Length)
            {
                Debug.Log("모든 플레이어가 Ready! → 게임 시작!!");
                // 여기에 실제 게임 씬 전환 또는 서버 통신 등 게임 시작 로직 추가
            }
            else
            {
                Debug.LogWarning("아직 준비되지 않은 플레이어가 있습니다!");
            }
        }

        /// <summary>
        /// 외부(예: 웹소켓 이벤트)에서 호출하여 해당 유저의 준비 상태와 전체 상태를 업데이트합니다.
        /// </summary>
        /// <param name="nickname">업데이트할 유저의 닉네임 (문자열)</param>
        /// <param name="isReady">새로운 준비 상태</param>
        public void UpdatePlayerReadyState(string nickname, bool isReady)
        {
            if (RoomDataManager.Instance == null)
            {
                Debug.LogWarning("RoomDataManager 인스턴스가 없습니다.");
                return;
            }

            if (nickname == RoomDataManager.Instance.HostNickname)
            {
                // 호스트 업데이트 (항상 슬롯 0)
                playerReady[0] = isReady;
                if (readyIndicators != null && readyIndicators.Length > 0)
                {
                    readyIndicators[0].color = isReady ? Color.green : Color.red;
                }
                Debug.Log($"[RoomManager] Host 준비 상태 업데이트: {isReady}");
            }
            else
            {
                // 게스트의 경우, GuestNicknames 배열에서 슬롯 번호 결정
                int guestSlot = -1;
                string[] guestNicknames = RoomDataManager.Instance.GuestNicknames;
                if (guestNicknames != null)
                {
                    for (int i = 0; i < guestNicknames.Length; i++)
                    {
                        if (guestNicknames[i] == nickname)
                        {
                            guestSlot = i + 1; // 게스트 슬롯은 1부터 시작
                            break;
                        }
                    }
                    // 빈 슬롯에 등록
                    if (guestSlot == -1)
                    {
                        for (int i = 0; i < guestNicknames.Length; i++)
                        {
                            if (string.IsNullOrEmpty(guestNicknames[i]))
                            {
                                guestNicknames[i] = nickname;
                                guestSlot = i + 1;
                                break;
                            }
                        }
                    }
                }
                if (guestSlot == -1)
                {
                    guestSlot = 1;
                }
                playerReady[guestSlot] = isReady;
                if (readyIndicators != null && readyIndicators.Length > guestSlot)
                {
                    readyIndicators[guestSlot].color = isReady ? Color.green : Color.red;
                }
                Debug.Log($"[RoomManager] 게스트 '{nickname}' 준비 상태 업데이트 (슬롯 {guestSlot}): {isReady}");
            }

            // 호스트인 경우, 모든 플레이어가 준비되면 Start 버튼 활성화
            if (isHost && actionButton != null)
            {
                int readyCount = playerReady.Count(r => r);
                actionButton.interactable = (readyCount == playerReady.Length);
            }
        }
    }
}
