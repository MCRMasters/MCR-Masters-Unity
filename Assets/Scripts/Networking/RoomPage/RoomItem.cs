using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MCRGame.Net;

namespace MCRGame.UI
{
    // AvailableRoomResponse 모델에 맞춰 UI를 업데이트합니다.
    public class RoomItem : MonoBehaviour
    {
        public Text titleText;   // 방 제목 UI 요소
        public Text infoText;    // 방 정보 (예: "현재인원/최대인원 | Host: 호스트닉네임")
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
                StartCoroutine(roomApiManager.JoinRoom(roomData.room_number.ToString(), (RoomResponse joinResponse) =>
                {
                    if (joinResponse != null)
                    {
                        Debug.Log("[RoomItem] 방 참가 API 호출 성공.");

                        // 필수 정보 검증
                        if (roomData == null ||
                            string.IsNullOrEmpty(roomData.name) ||
                            string.IsNullOrEmpty(roomData.host_nickname) ||
                            string.IsNullOrEmpty(roomData.host_uid))
                        {
                            Debug.LogError("[RoomItem] 방 정보가 유효하지 않습니다. 방 참가 후 검증 실패.");
                            return;
                        }

                        // AvailableRoomResponse의 users 배열에서 host 정보를 찾습니다.
                        RoomUserData hostUser;
                        var hostResponse = roomData.users.FirstOrDefault(u => u.uid == roomData.host_uid);
                        if (hostResponse != null)
                        {
                            hostUser = new RoomUserData
                            {
                                uid = hostResponse.uid,
                                nickname = hostResponse.nickname,
                                isReady = true,
                                slot_index = hostResponse.slot_index
                            };
                        }
                        else
                        {
                            hostUser = new RoomUserData
                            {
                                uid = roomData.host_uid,
                                nickname = roomData.host_nickname,
                                isReady = true,
                                slot_index = 0
                            };
                        }
                        
                        RoomUserData[] allUsers = new RoomUserData[4];
                        foreach (var u in roomData.users)
                        {
                            int index = u.slot_index;
                            if (index >= 0 && index < allUsers.Length)
                            {
                                allUsers[index] = new RoomUserData
                                {
                                    uid = u.uid,
                                    nickname = u.nickname,
                                    isReady = u.is_ready ? true : u.uid == hostUser.uid,
                                    slot_index = u.slot_index
                                };
                            }
                            else
                            {
                                Debug.LogWarning($"[RoomItem] 사용자 {u.nickname}의 slot_index({u.slot_index})가 유효 범위를 벗어났습니다.");
                            }
                        }


                        // 디버깅: 전체 사용자 목록 출력
                        Debug.Log("[RoomItem] 전체 사용자 목록 디버깅:");
                        foreach (var user in allUsers)
                        {
                            if (user != null){
                            Debug.Log($"[RoomItem] 사용자: {user.nickname}, uid: {user.uid}, slot_index: {user.slot_index}, isReady{user.isReady}");
                        }else{
                            Debug.Log("null");
                        }}

                        // 전역 RoomDataManager에 방 정보 저장 (host 정보와 슬롯 인덱스, 전체 사용자 목록 전달)
                        RoomDataManager.Instance.SetRoomInfo(
                            roomData.room_number.ToString(),
                            roomData.name,
                            hostUser,
                            hostUser.slot_index,
                            allUsers
                        );

                        // JoinRoom API 응답에서 전달된 slot_index로 mySlotIndex 업데이트
                        RoomDataManager.Instance.mySlotIndex = joinResponse.slot_index;

                        // 현재 플레이어 정보를 Players 배열의 해당 슬롯에 ready false 상태로 저장
                        RoomUserData currentPlayer = new RoomUserData
                        {
                            uid = PlayerDataManager.Instance.Uid,
                            nickname = PlayerDataManager.Instance.Nickname,
                            slot_index = joinResponse.slot_index,
                            isReady = false
                        };
                        RoomDataManager.Instance.Players[joinResponse.slot_index] = currentPlayer;

                        Debug.Log($"[RoomItem] 현재 플레이어 정보 저장: {currentPlayer.nickname}, slot_index: {currentPlayer.slot_index}, isReady: {currentPlayer.isReady}");

                        Debug.Log("[RoomItem] 방 정보 검증 완료. RoomScene으로 전환합니다.");
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
