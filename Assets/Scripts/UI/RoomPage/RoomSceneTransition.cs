using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MCRGame.Net; // RoomDataManager 포함 네임스페이스

namespace MCRGame.UI
{
    public class RoomSceneTransition : MonoBehaviour
    {
        /// <summary>
        /// 방 참가가 완료된 후, RoomScene으로 전환합니다.
        /// 방 정보를 전역 매니저에 저장하고, 이후 RoomScene에서 활용할 수 있습니다.
        /// </summary>
        /// <param name="roomId">방 번호</param>
        /// <param name="roomTitle">방 제목</param>
        /// <param name="hostNickname">호스트의 닉네임</param>
        public void TransitionToRoomScene(string roomId, string roomTitle, string hostNickname)
        {
            Debug.Log($"[RoomSceneTransition] Transitioning to RoomScene. Room ID: {roomId}, Title: {roomTitle}, HostNickname: {hostNickname}");

            // 전역 RoomDataManager에 방 정보를 저장합니다.
            if (RoomDataManager.Instance != null)
            {
                RoomDataManager.Instance.SetRoomInfo(roomId, roomTitle, hostNickname);
            }
            else
            {
                Debug.LogWarning("[RoomSceneTransition] RoomDataManager 인스턴스가 없습니다.");
            }

            // 실제 RoomScene으로 씬 전환
            SceneManager.LoadScene("RoomScene");
        }

        public void ReturnToRoomListScene()
        {
            Debug.Log("[RoomSceneTransition] Returning to RoomListScene");
            SceneManager.LoadScene("RoomListScene");
        }

        public void ShowRoomCreationUI(GameObject roomCreationPanel)
        {
            if (roomCreationPanel != null)
                roomCreationPanel.SetActive(true);
        }
    }
}
