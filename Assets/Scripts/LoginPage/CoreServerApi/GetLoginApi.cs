using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

namespace MCRGame
{
    public class GetLoginApi : MonoBehaviour
    {
        private string backendLoginUrl = "http://localhost:8000/api/v1/auth/login/google";

        /// <summary>
        /// 백엔드에서 auth_url을 받아오는 GET 요청을 수행합니다.
        /// </summary>
        /// <param name="onSuccess">성공 시 auth_url을 반환하는 콜백</param>
        /// <param name="onError">실패 시 에러 메시지를 반환하는 콜백</param>
        public IEnumerator RequestGoogleAuthUrl(Action<string> onSuccess, Action<string> onError)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(backendLoginUrl))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string json = www.downloadHandler.text;
                    AuthUrlResponse authData = JsonUtility.FromJson<AuthUrlResponse>(json);
                    onSuccess?.Invoke(authData.auth_url);
                }
                else
                {
                    onError?.Invoke(www.error);
                }
            }
        }

        [System.Serializable]
        private class AuthUrlResponse
        {
            public string auth_url;
        }
    }
}