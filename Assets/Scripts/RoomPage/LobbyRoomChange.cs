using UnityEngine;
using UnityEngine.UI;

public class LobbyRoomChange : MonoBehaviour
{
    public GameObject scrollView;    // 방 목록이 담긴 Scroll View
    public GameObject lobbyPanel;    // 로비 UI 패널 (방 입장 후 표시)
    public GameObject roomSetting;   // 방 설정(생성) 패널

    // 방 번호와 제목을 표시할 UI 텍스트
    public Text roomNumberText;
    public Text roomTitleText;

    /// <summary>
    /// 방 아이템 클릭 시 호출되며, 선택한 방의 번호와 제목을 로비 UI에 표시합니다.
    /// </summary>
    public void JoinRoom(string roomId, string roomTitle)
    {
        Debug.Log($"JoinRoom() called. Room ID: {roomId}, Title: {roomTitle}");

        if (roomNumberText != null)
        {
            roomNumberText.text = roomId;    // 예: "1" 또는 "Room001"
        }
        if (roomTitleText != null)
        {
            roomTitleText.text = roomTitle;  // 예: "My Awesome Room"
        }

        // 방 목록(Scroll View) 비활성화 후 로비 패널 활성화
        if (scrollView != null)
            scrollView.SetActive(false);

        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);

        // (필요하다면, 실제 방 입장 API 호출 로직 추가)
    }

    /// <summary>
    /// 로비에서 방 목록으로 돌아갑니다.
    /// </summary>
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

    /// <summary>
    /// 방 생성 버튼 클릭 시, 방 설정 패널(RoomSetting)을 엽니다.
    /// </summary>
    public void OnClickMakeRoomSet()
    {
        if (scrollView != null) scrollView.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);

        if (roomSetting != null)
            roomSetting.SetActive(true);

        Debug.Log("MakeRoomSet button clicked → RoomSetting activated");
    }
}
