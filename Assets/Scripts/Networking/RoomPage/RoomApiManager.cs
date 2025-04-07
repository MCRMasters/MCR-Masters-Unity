using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using MCRGame.Net;

namespace MCRGame.Net
{
    /// <summary>
    /// RoomApiManager는 방 관련 API 호출(방 목록 조회, 방 참가, 방 생성)을 일원화합니다.
    /// PlayerDataManager에 저장된 토큰을 사용하여 인증 헤더를 설정합니다.
    /// </summary>
    public class RoomApiManager : MonoBehaviour
    {
        // CoreServerConfig를 사용하여 기본 방 URL 구성 (예: http://localhost:8000/api/v1/room)
        private string baseRoomUrl = CoreServerConfig.GetHttpUrl("/room");

        /// <summary>
        /// 서버에서 방 목록을 가져옵니다.
        /// 서버 응답이 JSON 배열 형태라면, JsonUtility의 제한을 위해 래퍼 객체로 감싸서 파싱합니다.
        /// </summary>
        /// <param name="onSuccess">성공 시 AvailableRoomResponseList 반환</param>
        /// <param name="onError">실패 시 오류 메시지 반환</param>
        public IEnumerator FetchRooms(Action<AvailableRoomResponseList> onSuccess, Action<string> onError)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(baseRoomUrl))
            {
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log("[RoomApiManager] Fetched rooms: " + jsonResponse);

                    // 서버 응답이 JSON 배열일 경우, JsonUtility로 파싱하려면 래퍼 객체로 감쌉니다.
                    string wrappedJson = "{\"rooms\":" + jsonResponse + "}";
                    AvailableRoomResponseList roomList = JsonUtility.FromJson<AvailableRoomResponseList>(wrappedJson);

                    onSuccess?.Invoke(roomList);
                }
                else
                {
                    Debug.LogError("[RoomApiManager] FetchRooms failed: " + request.error);
                    onError?.Invoke(request.error);
                }
            }
        }

        /// <summary>
        /// 지정된 방 ID로 방 참가 API를 호출합니다.
        /// roomId는 문자열(내부적으로 int로 변환)이며, 성공 시 RoomResponse 객체를 callback으로 반환합니다.
        /// </summary>
        /// <param name="roomId">방 ID (문자열)</param>
        /// <param name="callback">성공 시 RoomResponse를 반환, 실패하면 null</param>
        public IEnumerator JoinRoom(string roomId, Action<RoomResponse> callback)
        {
            if (!int.TryParse(roomId, out int roomNumber))
            {
                Debug.LogError("[RoomApiManager] Invalid roomId (cannot parse to int).");
                callback?.Invoke(null);
                yield break;
            }

            // 최종 API 엔드포인트: /api/v1/room/{roomNumber}/join
            string joinUrl = $"{baseRoomUrl}/{roomNumber}/join";
            Debug.Log($"[RoomApiManager] Joining room at: {joinUrl}");

            using (UnityWebRequest request = new UnityWebRequest(joinUrl, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(new byte[0]);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[RoomApiManager] Successfully joined room {roomId}. Response: {request.downloadHandler.text}");
                    RoomResponse response = JsonUtility.FromJson<RoomResponse>(request.downloadHandler.text);
                    callback?.Invoke(response);
                }
                else
                {
                    Debug.LogError($"[RoomApiManager] Failed to join room {roomId}. Error: {request.error}");
                    callback?.Invoke(null);
                }
            }
        }

        /// <summary>
        /// 방 생성 API를 호출하여 새 방을 생성합니다.
        /// 생성 성공 시, CreateRoomResponse 객체를 반환합니다.
        /// </summary>
        /// <param name="onSuccess">성공 시 CreateRoomResponse 반환</param>
        /// <param name="onError">실패 시 오류 메시지 반환</param>
        public IEnumerator CreateRoom(Action<CreateRoomResponse> onSuccess, Action<string> onError)
        {
            using (UnityWebRequest request = new UnityWebRequest(baseRoomUrl, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(new byte[0]);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("[RoomApiManager] Room created successfully: " + request.downloadHandler.text);
                    CreateRoomResponse response = JsonUtility.FromJson<CreateRoomResponse>(request.downloadHandler.text);
                    onSuccess?.Invoke(response);
                }
                else
                {
                    Debug.LogError("[RoomApiManager] Room creation failed: " + request.error);
                    onError?.Invoke(request.error);
                }
            }
        }
    }
}
