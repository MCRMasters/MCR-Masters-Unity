using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;


namespace MCRGame.Net
{
    /// <summary>
    /// 방 참가(Join) API 호출을 전담하는 매니저
    /// </summary>
    public class JoinRoomManager : MonoBehaviour
    {
        // 예: "http://localhost:8000/api/v1/room"
        [SerializeField] private string baseRoomUrl = "http://localhost:8000/api/v1/room";

        /// <summary>
        /// 외부(예: RoomItem)에서 호출되는 메서드.
        /// roomId(문자열)를 받아 API를 호출하고, 성공/실패 여부를 콜백으로 알림.
        /// </summary>
        public void JoinRoom(string roomId, Action<bool> callback = null)
        {
            StartCoroutine(JoinRoomRequest(roomId, callback));
        }

        /// <summary>
        /// 실제 POST 요청을 보내 방에 참가합니다.
        /// </summary>
        private IEnumerator JoinRoomRequest(string roomId, Action<bool> callback)
        {
            // roomId를 int로 변환
            if (!int.TryParse(roomId, out int roomNumber))
            {
                Debug.LogError("[JoinRoomManager] Invalid roomId (cannot parse to int).");
                callback?.Invoke(false);
                yield break;
            }

            // 최종 요청 URL: /api/v1/room/{room_number}/join
            string joinUrl = $"{baseRoomUrl}/{roomNumber}/join";
            Debug.Log($"[JoinRoomManager] Attempting to join room at: {joinUrl}");

            using (UnityWebRequest request = new UnityWebRequest(joinUrl, "POST"))
            {
                // API 스펙상 별도 바디 없이 전송
                request.uploadHandler = new UploadHandlerRaw(new byte[0]);
                request.downloadHandler = new DownloadHandlerBuffer();

                // 헤더 설정: Content-Type, 인증 토큰(Bearer)
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[JoinRoomManager] Successfully joined room {roomId}. Response: {request.downloadHandler.text}");
                    callback?.Invoke(true);
                }
                else
                {
                    Debug.LogError($"[JoinRoomManager] Failed to join room {roomId}. Error: {request.error}");
                    callback?.Invoke(false);
                }
            }
        }
    }
}