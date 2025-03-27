using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class RoomRefreshManager : MonoBehaviour
{
    // API 전송용 페이로드 클래스: API에서는 "name"과 "room_number" 두 값이 필요합니다.
    [System.Serializable]
    public class RoomPayload
    {
        public string name;
        public int room_number;
    }

    [Header("UI References")]
    [SerializeField] private Button refreshButton; // 테스트용 버튼

    // 서버 URL (POST /api/v1/room) 및 GET /api/v1/room
    private string createRoomUrl = "http://0.0.0.0:8000/api/v1/room";

    private void Start()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(OnClickRefresh);
    }

    /// <summary>
    /// 버튼 클릭 시, 하드코딩된 값으로 방 생성 API를 호출한 후 GET 요청으로 방 목록을 가져옵니다.
    /// </summary>
    private void OnClickRefresh()
    {
        StartCoroutine(CreateRoomAndGetRooms());
    }

    /// <summary>
    /// POST와 GET 요청을 순차적으로 실행하는 코루틴
    /// </summary>
    private IEnumerator CreateRoomAndGetRooms()
    {
        yield return StartCoroutine(CreateRoomRequest());
        yield return StartCoroutine(GetRoomsRequest());
    }

    /// <summary>
    /// 서버에 POST 요청으로 방을 생성합니다.
    /// JSON 바디로 { "name": "wooro", "room_number": 1557 } 값을 전송합니다.
    /// </summary>
    private IEnumerator CreateRoomRequest()
    {
        // 하드코딩된 값으로 페이로드 생성
        RoomPayload payload = new RoomPayload
        {
            name = "wooro",
            room_number = 1557
        };

        string jsonData = JsonUtility.ToJson(payload);
        Debug.Log("Sending JSON: " + jsonData);

        using (UnityWebRequest request = new UnityWebRequest(createRoomUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // 헤더 설정: JSON 타입과 인증 토큰 포함
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[RoomRefreshManager] POST 방 생성 성공: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("[RoomRefreshManager] POST 방 생성 실패: " + request.error);
            }
        }
    }

    /// <summary>
    /// 서버에 GET 요청으로 이용 가능한 방 목록을 가져옵니다.
    /// </summary>
    private IEnumerator GetRoomsRequest()
    {
        using (UnityWebRequest getRequest = UnityWebRequest.Get(createRoomUrl))
        {
            getRequest.SetRequestHeader("Content-Type", "application/json");
            getRequest.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");

            yield return getRequest.SendWebRequest();

            if (getRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[RoomRefreshManager] GET available rooms: " + getRequest.downloadHandler.text);
            }
            else
            {
                Debug.LogError("[RoomRefreshManager] GET request failed: " + getRequest.error);
            }
        }
    }
}
