using UnityEngine;
using UnityEngine.UI;

public class LobbyRoomChange : MonoBehaviour
{
    public GameObject scrollView;     // Scroll View (방 목록)
    public GameObject lobbyPanel;     // Lobby UI
    public GameObject roomSetting;    // RoomSetting (인풋필드 패널)

    // 새로 추가: 방 번호를 표시할 Text
    public Text roomNumberText;

    // 기존: 방 제목을 표시할 Text (예: roomTitleText)
    public Text roomTitleText;

    // 방 아이템을 클릭하면 (ID, Title) 둘 다 전달받음
    public void JoinRoom(string roomId, string roomTitle)
    {
        Debug.Log($"JoinRoom() called. Room ID: {roomId}, Title: {roomTitle}");

        // 1) 로비에 방 번호, 방 제목 표시
        if (roomNumberText != null)
        {
            roomNumberText.text = roomId;    // 예: "Room001"
        }
        if (roomTitleText != null)
        {
            roomTitleText.text = roomTitle;  // 예: "My Awesome Room"
        }

        // 2) Scroll View 비활성화
        if (scrollView != null)
            scrollView.SetActive(false);

        // 3) 로비 패널 활성화
        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);
    }

    // 로비에서 방 목록으로 돌아가기
    public void ShowRoomList()
    {
        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);

        if (scrollView != null)
            scrollView.SetActive(true);

        if (roomSetting != null)
            roomSetting.SetActive(false);

        Debug.Log("Returned to room list");
    }

    // MakeRoomSet 버튼 클릭 시 RoomSetting 패널 열기
    public void OnClickMakeRoomSet()
    {
        if (scrollView != null) scrollView.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);

        if (roomSetting != null)
            roomSetting.SetActive(true);

        Debug.Log("MakeRoomSet button clicked → RoomSetting activated");
    }
}
