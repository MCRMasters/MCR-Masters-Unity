using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;


namespace MCRGame.Net
{
    public class PutNicknameApi : MonoBehaviour
    {
        private string putNicknameUrl = CoreServerConfig.GetHttpUrl("/user/me/nickname");

        /// <summary>
        /// 닉네임 업데이트 PUT 요청을 보냅니다.
        /// </summary>
        /// <param name="nickname">업데이트할 닉네임</param>
        /// <param name="accessToken">인증에 사용할 액세스 토큰</param>
        /// <param name="onSuccess">성공 시 호출되는 콜백 (응답 문자열 전달)</param>
        /// <param name="onError">실패 시 호출되는 콜백 (에러 메시지 전달)</param>
        public IEnumerator UpdateNickname(string nickname, string accessToken, Action<string> onSuccess = null, Action<string> onError = null)
        {
            string jsonBody = $"{{\"nickname\":\"{nickname}\"}}";
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

            using (UnityWebRequest request = new UnityWebRequest(putNicknameUrl, "PUT"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
                request.certificateHandler = new BypassCertificateHandler();

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    onSuccess?.Invoke(request.downloadHandler.text);
                }
                else
                {
                    onError?.Invoke(request.error);
                }
            }
        }
    }
}