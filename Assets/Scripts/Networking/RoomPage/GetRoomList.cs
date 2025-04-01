using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;


namespace MCRGame.Net
{
    /// <summary>
    /// 서버로부터 방 목록을 가져오는 GET 요청 전담 스크립트
    /// </summary>
    public class GetRoomList : MonoBehaviour
    {
        // 서버 URL (GET /api/v1/room)
        [SerializeField] private string getRoomsUrl = "http://localhost:8000/api/v1/room";

        /// <summary>
        /// 서버에서 방 목록을 가져옵니다.
        /// onSuccess: 요청 성공 시 AvailableRoomResponseList를 전달
        /// onError: 요청 실패 시 에러 메시지를 전달
        /// </summary>
        public IEnumerator FetchRooms(Action<AvailableRoomResponseList> onSuccess, Action<string> onError)
        {
            using (UnityWebRequest getRequest = UnityWebRequest.Get(getRoomsUrl))
            {
                getRequest.SetRequestHeader("Content-Type", "application/json");
                getRequest.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");

                yield return getRequest.SendWebRequest();

                if (getRequest.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = getRequest.downloadHandler.text;
                    Debug.Log("[GetRoomList] GET available rooms: " + jsonResponse);

                    // JsonUtility로 배열 파싱 시 래퍼 객체로 감싸야 함
                    string wrappedJson = "{\"rooms\":" + jsonResponse + "}";
                    AvailableRoomResponseList roomList = JsonUtility.FromJson<AvailableRoomResponseList>(wrappedJson);

                    onSuccess?.Invoke(roomList);
                }
                else
                {
                    Debug.LogError("[GetRoomList] GET request failed: " + getRequest.error);
                    onError?.Invoke(getRequest.error);
                }
            }
        }
    }
}