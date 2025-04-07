using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MCRGame.Net; // RoomDataManager, PlayerDataManager, RoomUserData 등 포함

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

            // 플레이어 데이터 매니저가 있다면 host uid를 가져옵니다.
            string hostUid = "";
            if (PlayerDataManager.Instance != null)
            {
                hostUid = PlayerDataManager.Instance.Uid;
            }
            else
            {
                Debug.LogWarning("[RoomSceneTransition] PlayerDataManager 인스턴스가 없습니다. host uid는 빈 문자열로 설정됩니다.");
            }

            // RoomUserData 객체 생성 (호스트) - host의 슬롯 인덱스는 0으로 설정
            RoomUserData hostUser = new RoomUserData
            {
                uid = hostUid,
                nickname = hostNickname,
                isReady = true, // 호스트는 기본적으로 준비 상태라고 가정
                slot_index = 0
            };

            // 전역 RoomDataManager에 방 정보를 저장합니다.
            if (RoomDataManager.Instance != null)
            {
                // host-only 상황이므로, host의 슬롯 인덱스 0을 함께 전달
                RoomDataManager.Instance.SetRoomInfo(roomId, roomTitle, hostUser, 0);
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
