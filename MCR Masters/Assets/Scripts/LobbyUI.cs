using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    public TMP_InputField playerNameInput; // 이름 입력 필드
    public Button setNameButton;          // 이름 설정 버튼
    public Button readyButton;            // 준비 버튼
    private CustomNetworkRoomPlayer roomPlayer;

    void Start()
    {
        // 서버에서 실행 시 로직 무시
        if (!NetworkClient.active)
        {
            Debug.Log("LobbyUI: This script is intended to run on the client. Here is Server.");
            return;
        }

        // 로컬 플레이어 가져오기
        var networkIdentity = NetworkClient.connection.identity;
        roomPlayer = networkIdentity?.GetComponent<CustomNetworkRoomPlayer>();
        if (roomPlayer == null)
        {
            Debug.LogError("LobbyUI: Unable to find CustomNetworkRoomPlayer for the local client.");
            return;
        }

        // ReadyButton 및 PlayerNameInput 동적 찾기
        readyButton = GameObject.Find("ReadyButton")?.GetComponent<Button>();
        playerNameInput = GameObject.Find("PlayerNameInput")?.GetComponent<TMP_InputField>();
        setNameButton = GameObject.Find("SetNameButton")?.GetComponent<Button>();
        if (readyButton == null || playerNameInput == null || setNameButton == null)
        {
            Debug.LogError("LobbyUI: UI elements not found. Check their names.");
            return;
        }

        // 버튼 클릭 이벤트 추가
        readyButton.onClick.AddListener(OnReadyButtonClicked);
        setNameButton.onClick.AddListener(OnSetNameClicked);
        // 초기 버튼 상태 설정
        UpdateButtonUI(false);
    }

    void Update()
    {
        // 'R' 또는 'Space' 키가 눌리면 OnReadyButtonClicked 호출
        //if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space))
        //{
        //    OnReadyButtonClicked();
        //}
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnSetNameClicked();
        }
    }

    public void OnReadyButtonClicked()
    {
        if (roomPlayer != null)
        {
            // 현재 상태의 반대로 설정
            bool newReadyState = !roomPlayer.readyToBegin;
            roomPlayer.CmdChangeReadyState(newReadyState);

            // UI 업데이트
            UpdateButtonUI(newReadyState);
        }
    }

    public void OnSetNameClicked()
    {
        if (roomPlayer != null && !string.IsNullOrWhiteSpace(playerNameInput.text))
        {
            // 입력한 이름을 서버로 전송
            roomPlayer.CmdSetPlayerName(playerNameInput.text);
            Debug.Log($"Player name set to: {playerNameInput.text}");
        }
    }
    void UpdateButtonUI(bool isReady)
    {
        TMP_Text buttonText = readyButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = isReady ? "Cancel" : "Ready";
        }
        else
        {
            Debug.LogError("LobbyUI: TMP_Text component not found on ReadyButton.");
        }
    }
}
