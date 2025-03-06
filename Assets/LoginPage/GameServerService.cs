using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public static class GameServerService
{
/* 
    public static IEnumerator AuthenticateWithGameServer(string gameServerAuthUrl, string accessToken)
    {
        Debug.Log("🔄 게임 서버 인증 요청 중...");

        using (UnityWebRequest request = new UnityWebRequest(gameServerAuthUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes($"{{\"access_token\":\"{accessToken}\"}}");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ 게임 서버 인증 성공!");
                WebSocketManager.Instance.ConnectWebSocket(accessToken);
            }
            else
            {
                Debug.LogError("❌ 게임 서버 인증 실패: " + request.error);
            }
        }
    }
*/
}
