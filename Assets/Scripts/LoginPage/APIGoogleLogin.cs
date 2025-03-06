using UnityEngine;
using UnityEngine.Networking;
using System.Collections;


[System.Serializable]
public class AuthUrlResponse
{
    public string auth_url;
}

public class APIGoogleLogin : MonoBehaviour
{
    private string auth_request_server_url = "http://localhost:8000/api/v1/auth/login/google";

    public void start_google_login()
    {
        StartCoroutine(get_auth_url());
    }

    IEnumerator get_auth_url()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(auth_request_server_url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Failed to get Google Auth URL: " + request.error);
            }
            else
            {
                // 응답은 JSON 형태로 온다. 예: {"auth_url": "https://accounts.google.com/o/oauth2/v2/auth?..."}
                AuthUrlResponse response_data = JsonUtility.FromJson<AuthUrlResponse>(request.downloadHandler.text);
                if (!string.IsNullOrEmpty(response_data.auth_url))
                {
                    Application.OpenURL(response_data.auth_url);
                }
                else
                {
                    Debug.LogError("❌ auth_url is empty in the response.");
                }
            }
        }
    }
}

