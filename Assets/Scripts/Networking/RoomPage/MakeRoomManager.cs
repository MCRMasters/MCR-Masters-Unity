using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using MCRGame.UI;

namespace MCRGame.Net
{
    public class MakeRoomManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button makeRoomButton; // Make 버튼

        // 이미 씬에 존재하는 LobbyRoomChange를 Inspector에서 할당
        [SerializeField] private LobbyRoomChange lobbyRoomChange;

        // 서버 URL (POST /api/v1/room)
        private string createRoomUrl = "http://localhost:8000/api/v1/room";

        private void Start()
        {
            if (makeRoomButton != null)
                makeRoomButton.onClick.AddListener(OnClickMakeRoom);
        }

        /// <summary>
        /// Make 버튼 클릭 시, 서버에 방 생성 요청 → 성공 시 UI 전환 (이미 호스트로 등록됨)
        /// </summary>
        private void OnClickMakeRoom()
        {
            StartCoroutine(SendPostRequest());
        }

        private IEnumerator SendPostRequest()
        {
            using (UnityWebRequest request = new UnityWebRequest(createRoomUrl, "POST"))
            {
                // 파라미터 없이 요청 (서버가 자동으로 방을 생성하고, 생성자에게 방에 참가시킵니다)
                request.uploadHandler = new UploadHandlerRaw(new byte[0]);
                request.downloadHandler = new DownloadHandlerBuffer();

                // 헤더 설정: Content-Type 및 인증 토큰
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("[MakeRoomManager] Room created successfully: " + request.downloadHandler.text);

                    // 서버 응답 예시:
                    // {"name":"새로운방","room_number":123,"message":"Room created"}
                    CreateRoomResponse response = JsonUtility.FromJson<CreateRoomResponse>(request.downloadHandler.text);

                    // 이미 서버에서 생성 시 호스트로 등록하므로, 바로 UI 전환
                    string roomId = response.room_number.ToString();
                    string roomTitle = response.name;

                    if (lobbyRoomChange != null)
                    {
                        lobbyRoomChange.JoinRoom(roomId, roomTitle);
                    }
                    else
                    {
                        Debug.LogWarning("[MakeRoomManager] lobbyRoomChange is not assigned!");
                    }
                }
                else
                {
                    Debug.LogError("[MakeRoomManager] Room creation failed: " + request.error);
                }
            }
        }
    }

    [System.Serializable]
    public class CreateRoomResponse
    {
        public string name;         // 방 이름
        public int room_number;     // 방 번호
        public string message;      // 예: "Room created"
    }
}