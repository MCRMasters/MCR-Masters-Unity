using UnityEngine;
using UnityEngine.UI;


namespace MCRGame
{
    public class LobbyRoomChange : MonoBehaviour
    {
        public GameObject scrollView;    // 방 목록이 담긴 Scroll View
        public GameObject RoomPanel;    // 로비 UI 패널 (방 입장 후 표시)
        public GameObject roomSetting;   // 방 설정(생성) 패널

        // 방 번호와 제목을 표시할 UI 텍스트
        public Text roomNumberText;
        public Text roomTitleText;

        /// <summary>
        /// 방 번호와 제목을 로비 UI에 표시하고, Scroll View를 비활성화 후 LobbyPanel을 활성화
        /// </summary>
        public void JoinRoom(string roomId, string roomTitle)
        {
            Debug.Log($"[LobbyRoomChange] JoinRoom() called. Room ID: {roomId}, Title: {roomTitle}");

            if (roomNumberText != null)
                roomNumberText.text = roomId;
            if (roomTitleText != null)
                roomTitleText.text = roomTitle;

            if (scrollView != null)
                scrollView.SetActive(false);
            if (RoomPanel != null)
                RoomPanel.SetActive(true);
        }

        public void ShowRoomList()
        {
            if (RoomPanel != null)
                RoomPanel.SetActive(false);
            if (scrollView != null)
                scrollView.SetActive(true);
            if (roomSetting != null)
                roomSetting.SetActive(false);

            Debug.Log("[LobbyRoomChange] Returned to room list");
        }

        public void OnClickMakeRoomSet()
        {
            if (scrollView != null)
                scrollView.SetActive(false);
            if (RoomPanel != null)
                RoomPanel.SetActive(false);
            if (roomSetting != null)
                roomSetting.SetActive(true);

            Debug.Log("[LobbyRoomChange] MakeRoomSet button clicked → RoomSetting activated");
        }
    }
}