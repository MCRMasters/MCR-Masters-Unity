using System.Linq;
using System.Collections; // 코루틴 사용을 위한 네임스페이스 추가
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace MCRGame.Net
{
    public class RoomManager : MonoBehaviour
    {
        // 4명의 플레이어 Ready 상태 (Players 배열의 인덱스 기준; host는 RoomDataManager.Instance.HostSlotIndex)
        public bool[] playerReady = new bool[4];

        // 단일 버튼: host이면 "Start", 게스트이면 "Ready"를 표시
        public Button actionButton;

        // 모든 플레이어의 Ready 상태를 나타내는 UI Image 배열 (길이 4)
        public Image[] readyIndicators;

        // RoomDataManager에 저장된 방 정보를 표시할 UI 텍스트 (RoomScene에서)
        public Text roomTitleText;
        public Text roomNumberText;

        // 웹소켓 통신을 위한 RoomWS 인스턴스 (Inspector에서 할당)
        public RoomWS roomWS;

        // 플레이어 별 닉네임 텍스트 배열 (길이 4; 인덱스 0: host, 1~3: 게스트)
        public Text[] playerNicknameTexts;
        // 플레이어 별 캐릭터 이미지 배열 (길이 4)
        public Image[] playerCharacterImages;
        // Inspector에서 할당할 기본 캐릭터 이미지
        public Sprite defaultCharacterSprite;

        // 현재 플레이어가 host인지 여부 및 Ready 상태 (개별 변수 대신 RoomDataManager.Instance.mySlotIndex로 슬롯 참조)
        private bool isHost = false;
        private bool isReady = false;

        // 추가: WebSocket 연결 상태 및 연결 전 Ready 버튼 클릭 여부를 추적
        private bool wsConnected = false;
        private bool clickedReadyBeforeWS = false;

        void Start()
        {
            if (RoomDataManager.Instance == null)
            {
                // RoomDataManager 인스턴스가 없으면 기본값 host=인덱스 0
                playerReady[0] = true;
                for (int i = 1; i < playerReady.Length; i++)
                    playerReady[i] = false;
            }

            // 방 정보 UI 업데이트 (방 제목, 방 번호)
            if (RoomDataManager.Instance != null)
            {
                if (roomTitleText != null)
                    roomTitleText.text = RoomDataManager.Instance.RoomTitle;
                if (roomNumberText != null)
                    roomNumberText.text = RoomDataManager.Instance.RoomId;
            }

            // 현재 플레이어가 host인지 결정 (uid 비교)
            if (PlayerDataManager.Instance != null && RoomDataManager.Instance != null)
            {
                isHost = PlayerDataManager.Instance.Uid == RoomDataManager.Instance.HostUser.uid;
                Debug.Log($"IsHost: {isHost}");
                if (isHost)
                {
                    // host의 슬롯은 RoomDataManager.Instance.HostSlotIndex로 설정
                    RoomUserData[] players = RoomDataManager.Instance.Players;
                    RoomDataManager.Instance.mySlotIndex = RoomDataManager.Instance.HostSlotIndex;
                    players[RoomDataManager.Instance.mySlotIndex] = new RoomUserData
                    {
                        uid = PlayerDataManager.Instance.Uid,
                        nickname = PlayerDataManager.Instance.Nickname,
                        isReady = false,
                        slot_index = RoomDataManager.Instance.mySlotIndex
                    };
                    playerReady[RoomDataManager.Instance.mySlotIndex] = true;
                    isReady = true;
                }
                else
                {
                    // 게스트: Players 배열에서 자신의 uid로 슬롯 찾기
                    RoomUserData[] players = RoomDataManager.Instance.Players;
                    if (RoomDataManager.Instance.mySlotIndex >= 0 && RoomDataManager.Instance.mySlotIndex < players.Length)
                    {
                        players[RoomDataManager.Instance.mySlotIndex] = new RoomUserData
                        {
                            uid = PlayerDataManager.Instance.Uid,
                            nickname = PlayerDataManager.Instance.Nickname,
                            isReady = false,
                            slot_index = RoomDataManager.Instance.mySlotIndex
                        };
                    }
                }
            }
            else
            {
                Debug.LogError("PlayerDataManager 또는 RoomDataManager 인스턴스가 없습니다.");
            }

            // 단일 버튼 설정: host이면 "Start", 게스트이면 "Ready"
            if (actionButton != null)
            {
                if (isHost)
                {
                    actionButton.GetComponentInChildren<Text>().text = "Start";
                    actionButton.interactable = false;
                }
                else
                {
                    actionButton.GetComponentInChildren<Text>().text = "Ready";
                    actionButton.interactable = true;
                }
                actionButton.onClick.AddListener(OnActionButtonClicked);
            }
            // 초기 UI 업데이트: Players 배열 기반으로 readyIndicators를 관리
            UpdatePlayerUI();

            // WebSocket 연결 완료 콜백 등록
            if (roomWS != null)
            {
                roomWS.OnWebSocketConnected = OnWebSocketConnectedHandler;
            }
        }

        // WebSocket 연결 완료 시 호출되는 콜백 처리
        private void OnWebSocketConnectedHandler()
        {
            wsConnected = true;
            Debug.Log("[RoomManager] WebSocket 연결 완료. wsConnected = true");
            // 연결 전에 Ready 버튼이 클릭되어 있던 상태라면(레디 상태가 유지 중이라면) 신호 전송
            if (!isHost && clickedReadyBeforeWS && roomWS != null)
            {
                roomWS.SendReadyStatus(isReady);
                Debug.Log("[RoomManager] WebSocket 연결 후 미리 설정한 Ready 상태 전송됨: " + isReady);
            }
        }

        /// <summary>
        /// 단일 버튼 클릭 시 호출. host는 Start 버튼 동작, 게스트는 Ready 상태 토글 수행.
        /// </summary>
        private void OnActionButtonClicked()
        {
            if (isHost)
            {
                OnHostStartGame();
            }
            else
            {
                // 게스트 Ready 상태 토글
                isReady = !isReady;
                int slot = RoomDataManager.Instance.mySlotIndex; // RoomDataManager에서 참조
                playerReady[slot] = isReady;
                // Players 배열 업데이트: 현재 플레이어의 Ready 상태 변경
                if (RoomDataManager.Instance.Players[slot] != null)
                {
                    RoomDataManager.Instance.Players[slot].isReady = isReady;
                }
                // UI 업데이트는 UpdatePlayerUI에서 처리
                UpdatePlayerUI();
                if (actionButton != null)
                {
                    actionButton.GetComponentInChildren<Text>().text = isReady ? "Ready ✔" : "Ready";
                }

                // Ready 버튼 클릭 시 WebSocket 연결 상태에 따라 처리:
                // 연결이 완료되어 있다면 바로 신호 전송, 아니라면 추후 전송을 위해 플래그 설정.
                if (wsConnected && roomWS != null)
                {
                    roomWS.SendReadyStatus(isReady);
                }
                else
                {
                    clickedReadyBeforeWS = true;
                    Debug.Log("[RoomManager] WebSocket 미연결 상태에서 Ready 버튼 클릭, 나중에 신호 전송 예정.");
                }
            }
        }

        /// <summary>
        /// host가 Start 버튼 클릭 시 호출. 모든 플레이어가 Ready여야 게임 시작.
        /// </summary>
        public void OnHostStartGame()
        {
            if (roomWS != null)
            {
                roomWS.SendReadyStatus(isReady);
            }
            int readyCount = playerReady.Count(r => r);
            Debug.Log($"[RoomManager] Host Start button clicked. {readyCount}/4 players ready.");

            if (readyCount == playerReady.Length)
            {
                Debug.Log("All players are ready! Calling start game API.");
                StartCoroutine(CallStartGameApi());
            }
            else
            {
                Debug.LogWarning("Not all players are ready yet!");
            }
        }
        IEnumerator CallStartGameApi()
        {
            string url = CoreServerConfig.GetHttpUrl("/room/" + RoomDataManager.Instance.RoomId + "/game-start");
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(new byte[0]);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");
                request.certificateHandler = new BypassCertificateHandler();

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Start game API success: " + request.downloadHandler.text);
                    // API 호출 성공 시 추가 GameWS 인스턴스 생성 및 씬 전환 로직은 외부에서 처리합니다.
                }
                else
                {
                    Debug.LogError("Start game API failed: " + request.error);
                }
            }
        }




        /// <summary>
        /// 외부(예: 웹소켓 이벤트)에서 호출하여 해당 유저의 Ready 상태 및 전체 상태를 업데이트합니다.
        /// uid 기준으로 업데이트하며, Players 배열의 슬롯에 맞게 배치합니다.
        /// </summary>
        public void UpdatePlayerReadyState(string uid, bool readyStatus)
        {
            if (RoomDataManager.Instance == null)
            {
                Debug.LogWarning("RoomDataManager instance is null.");
                return;
            }

            print($"new uid: {uid}, player uid: {PlayerDataManager.Instance.Uid}");
            if (uid == PlayerDataManager.Instance.Uid)
            {
                Debug.Log("already updated player's state");
                return;
            }

            if (RoomDataManager.Instance.HostUser != null && uid == RoomDataManager.Instance.HostUser.uid)
            {
                int hostSlot = RoomDataManager.Instance.HostSlotIndex;
                playerReady[hostSlot] = readyStatus;
                if (RoomDataManager.Instance.Players[hostSlot] != null)
                {
                    RoomDataManager.Instance.Players[hostSlot].isReady = readyStatus;
                }
                Debug.Log($"[RoomManager] Host ready status updated: {readyStatus}");
            }
            else
            {
                RoomUserData[] players = RoomDataManager.Instance.Players;
                int guestSlot = -1;
                for (int i = 0; i < players.Length; i++)
                {
                    if (i == RoomDataManager.Instance.HostSlotIndex)
                        continue;
                    if (players[i] != null && players[i].uid == uid)
                    {
                        guestSlot = i;
                        break;
                    }
                }
                if (guestSlot == -1)
                {
                    // 빈 슬롯에 등록 (host 슬롯 제외)
                    for (int i = 0; i < players.Length; i++)
                    {
                        if (i == RoomDataManager.Instance.HostSlotIndex)
                            continue;
                        if (players[i] == null)
                        {
                            players[i] = new RoomUserData
                            {
                                uid = uid,
                                nickname = "Unknown",
                                isReady = readyStatus,
                                slot_index = i
                            };
                            guestSlot = i;
                            break;
                        }
                    }
                }
                if (guestSlot == -1)
                    guestSlot = 1;
                playerReady[guestSlot] = readyStatus;
                if (players[guestSlot] != null)
                {
                    players[guestSlot].isReady = readyStatus;
                }
                Debug.Log($"[RoomManager] Guest (uid: {uid}) ready status updated (slot {guestSlot}): {readyStatus}");
            }

            // UI 업데이트: 플레이어 UI 및 readyIndicators 업데이트
            UpdatePlayerUI();

            // host인 경우, 모든 플레이어가 Ready이면 Start 버튼 활성화
            if (isHost && actionButton != null)
            {
                int readyCount = playerReady.Count(r => r);
                actionButton.interactable = (readyCount == playerReady.Length);
            }
        }

        public void UpdatePlayerUI()
        {
            if (RoomDataManager.Instance != null)
            {
                RoomDataManager.Instance.PrintPlayers();
                RoomUserData[] players = RoomDataManager.Instance.Players;
                for (int i = 0; i < players.Length; i++)
                {
                    // 디버깅: 현재 슬롯 i의 플레이어 정보 출력
                    if (players[i] != null)
                    {
                        Debug.Log($"[UpdatePlayerUI] Slot {i}: {players[i].nickname}, isReady: {players[i].isReady}");
                    }
                    else
                    {
                        Debug.Log($"[UpdatePlayerUI] Slot {i}: Empty");
                    }

                    // 플레이어 닉네임과 캐릭터 이미지 업데이트
                    if (players[i] != null)
                    {
                        if (playerNicknameTexts != null && playerNicknameTexts.Length > i)
                        {
                            playerNicknameTexts[i].text = players[i].nickname;
                            playerNicknameTexts[i].gameObject.SetActive(true);
                        }
                        if (playerCharacterImages != null && playerCharacterImages.Length > i)
                        {
                            playerCharacterImages[i].sprite = defaultCharacterSprite;
                            playerCharacterImages[i].gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        if (playerNicknameTexts != null && playerNicknameTexts.Length > i)
                            playerNicknameTexts[i].gameObject.SetActive(false);
                        if (playerCharacterImages != null && playerCharacterImages.Length > i)
                            playerCharacterImages[i].gameObject.SetActive(false);
                    }

                    // Ready 상태에 따른 readyIndicators 업데이트
                    if (readyIndicators != null && readyIndicators.Length > i)
                    {
                        if (players[i] != null)
                        {
                            readyIndicators[i].gameObject.SetActive(true);
                            readyIndicators[i].color = players[i].isReady ? Color.green : Color.red;
                            Debug.Log($"[UpdatePlayerUI] ReadyIndicator Slot {i}: Active, Color: {(players[i].isReady ? "Green" : "Red")}");
                        }
                        else
                        {
                            readyIndicators[i].gameObject.SetActive(false);
                            Debug.Log($"[UpdatePlayerUI] ReadyIndicator Slot {i}: Inactive");
                        }
                    }
                }
            }
        }
    }
}
