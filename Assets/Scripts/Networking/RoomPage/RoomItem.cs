using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using MCRGame.Net; // RoomDataManager, PlayerDataManager 등 포함

namespace MCRGame.Net
{
    // AvailableRoomResponse 모델에 맞춰 UI를 업데이트합니다.
    public class RoomItem : MonoBehaviour
    {
        public Text titleText;   // 방 제목 UI 요소
        public Text infoText;    // 방 정보(UI 예: "현재인원/최대인원 | Host: 호스트닉네임")
        public Text idText;      // 방 번호 (문자열)

        // 내부적으로 AvailableRoomResponse 데이터를 보관
        private AvailableRoomResponse roomData;

        // RoomApiManager 참조 (API 호출 담당)
        public RoomApiManager roomApiManager;

        /// <summary>
        /// AvailableRoomResponse 데이터를 받아 RoomItem UI를 초기화합니다.
        /// </summary>
        public void Setup(AvailableRoomResponse roomResponse)
        {
            roomData = roomResponse;

            if (idText != null)
                idText.text = roomResponse.room_number.ToString();
            if (titleText != null)
                titleText.text = roomResponse.name;
            if (infoText != null)
                infoText.text = $"{roomResponse.current_users}/{roomResponse.max_users} | Host: {roomResponse.host_nickname}";
        }

        public void OnClickRoom()
        {
            Debug.Log($"[RoomItem] Clicked room: {roomData.room_number} - {roomData.name}");

            if (roomApiManager != null)
            {
                // roomData.room_number를 문자열로 변환하여 API 호출
                StartCoroutine(roomApiManager.JoinRoom(roomData.room_number.ToString(), (bool success) =>
                {
                    if (success)
                    {
                        Debug.Log("[RoomItem] 방 참가 API 호출 성공.");

                        // 추가 검증: roomData에 필수 정보가 모두 있는지 확인
                        if (roomData == null ||
                            string.IsNullOrEmpty(roomData.name) ||
                            string.IsNullOrEmpty(roomData.host_nickname))
                        {
                            Debug.LogError("[RoomItem] 방 정보가 유효하지 않습니다. 방 참가 후 검증 실패.");
                            return;
                        }

                        // 추가 검증: 방에 참가한 유저 정보(예: 게스트 닉네임)가 유효한지 체크 (옵션)
                        if (roomData.users == null || roomData.users.Length == 0)
                        {
                            Debug.LogWarning("[RoomItem] 참가한 유저 정보가 비어 있습니다.");
                            // 필요에 따라 기본값이나 추가 API 호출로 보완할 수 있습니다.
                        }

                        // RoomDataManager에 방 정보 저장 (게스트 닉네임 배열은 각 RoomUserResponse의 닉네임으로 설정)
                        string[] guestNicknames = roomData.users != null
                            ? roomData.users.Select(u => u.nickname).ToArray()
                            : new string[3]; // 기본 길이 3의 배열
                        RoomDataManager.Instance.SetRoomInfo(
                            roomData.room_number.ToString(),
                            roomData.name,
                            roomData.host_nickname,
                            guestNicknames
                        );

                        Debug.Log("[RoomItem] 방 정보 검증 완료. RoomScene으로 전환합니다.");
                        // RoomScene으로 씬 전환
                        SceneManager.LoadScene("RoomScene");
                    }
                    else
                    {
                        Debug.LogError($"[RoomItem] Failed to join room {roomData.room_number}. Cannot transition to room UI.");
                    }
                }));
            }
            else
            {
                Debug.LogWarning("[RoomItem] roomApiManager가 할당되어 있지 않습니다.");
            }
        }
    }
}
