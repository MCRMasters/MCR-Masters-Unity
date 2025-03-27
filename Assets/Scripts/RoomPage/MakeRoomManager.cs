using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class MakeRoomManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button makeRoomButton; // Make 버튼

    // 서버 URL (POST /api/v1/room)
    private string createRoomUrl = "http://0.0.0.0:8000/api/v1/room";

    private void Start()
    {
        if (makeRoomButton != null)
            makeRoomButton.onClick.AddListener(OnClickMakeRoom);
    }

    /// <summary>
    /// Make 버튼 클릭 시, 파라미터 없이 POST 요청을 보내 방 생성을 요청합니다.
    /// </summary>
    private void OnClickMakeRoom()
    {
        StartCoroutine(SendPostRequest());
    }

    /// <summary>
    /// 빈 바디로 POST 요청을 보내, 서버가 자동으로 방을 생성하도록 합니다.
    /// </summary>
    private IEnumerator SendPostRequest()
    {
        using (UnityWebRequest request = new UnityWebRequest(createRoomUrl, "POST"))
        {
            // 빈 바디 전송: API 스펙 상 파라미터 없이 요청하는 경우
            request.uploadHandler = new UploadHandlerRaw(new byte[0]);
            request.downloadHandler = new DownloadHandlerBuffer();

            // 헤더 설정: Content-Type 및 인증 헤더 추가
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[MakeRoomManager] Room created successfully: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("[MakeRoomManager] Room creation failed: " + request.error);
            }
        }
    }
}
