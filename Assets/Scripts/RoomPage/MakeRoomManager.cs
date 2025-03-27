using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class MakeRoomManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button makeRoomButton; // Make 버튼

    // 이미 씬에 존재하는 LobbyRoomChange를 Inspector에서 할당
    [SerializeField] private LobbyRoomChange lobbyRoomChange;

    // 서버 URL (POST /api/v1/room)
    private string createRoomUrl = "http://0.0.0.0:8000/api/v1/room";

    private void Start()
    {
        if (makeRoomButton != null)
            makeRoomButton.onClick.AddListener(OnClickMakeRoom);
    }

    /// <summary>
    /// Make 버튼 클릭 시, 서버에 방 생성 요청 → 성공 시 UI에서 해당 방에 들어간 것처럼 전환
    /// </summary>
    private void OnClickMakeRoom()
    {
        StartCoroutine(SendPostRequest());
    }

    private IEnumerator SendPostRequest()
    {
        using (UnityWebRequest request = new UnityWebRequest(createRoomUrl, "POST"))
        {
            // 파라미터 없이 요청 (API가 자동 생성)
            request.uploadHandler = new UploadHandlerRaw(new byte[0]);
            request.downloadHandler = new DownloadHandlerBuffer();

            // 헤더: Content-Type, Authorization
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[MakeRoomManager] Room created successfully: " + request.downloadHandler.text);

                // 서버가 {"name":"새로운방","room_number":123,"message":"Room created"} 등으로 응답한다고 가정
                CreateRoomResponse response = JsonUtility.FromJson<CreateRoomResponse>(request.downloadHandler.text);

                // 이미 서버가 "이 사용자는 새로 만든 방의 호스트"로 처리하므로,
                // 굳이 JoinRoom API를 다시 호출할 필요 없음 → UI 전환만 하면 됨

                // 방 번호/제목 추출
                string roomId = response.room_number.ToString();
                string roomTitle = response.name;

                // LobbyRoomChange의 JoinRoom(...) 메서드 호출로
                // “방에 들어간 것과 동일한” UI 전환
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

// 서버 응답 파싱용 예시 클래스
[System.Serializable]
public class CreateRoomResponse
{
    public string name;         // 방 이름
    public int room_number;     // 방 번호
    public string message;      // "Room created" 등
}
