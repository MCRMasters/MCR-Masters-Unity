// GoogleLoginCallbackListener.cs
using System;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class GoogleLoginCallbackListener : MonoBehaviour
{
    private HttpListener httpListener;
    private static GoogleLoginCallbackListener instance;

    // 액세스 토큰, 리프레시 토큰을 받을 콜백
    private Action<string, string> callbackAction;

    private const string redirectUri = "http://localhost:5005/callback/";

    private void Awake()
    {
        // 싱글톤
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 정적 메서드: 다른 스크립트에서 바로 호출 가능
    /// </summary>
    public static void StartListening(Action<string, string> callback)
    {
        if (instance != null)
        {
            instance.StartListener(callback);
        }
        else
        {
            Debug.LogError("GoogleLoginCallbackListener가 씬에 없습니다!");
        }
    }

    private void StartListener(Action<string, string> callback)
    {
        callbackAction = callback;

        httpListener = new HttpListener();
        httpListener.Prefixes.Add(redirectUri);
        httpListener.Start();
        Debug.Log("✅ HTTP 리스너 시작: " + redirectUri);

        Task.Run(() => ListenLoop());
    }

    private async Task ListenLoop()
    {
        while (httpListener.IsListening)
        {
            var context = await httpListener.GetContextAsync();
            ProcessRequest(context);
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        string query = context.Request.Url.Query;
        // ?access_token=xxx&refresh_token=yyy

        var parsed = System.Web.HttpUtility.ParseQueryString(query);
        string accessToken = parsed["access_token"];
        string refreshToken = parsed["refresh_token"];

        if (!string.IsNullOrEmpty(accessToken))
        {
            callbackAction?.Invoke(accessToken, refreshToken);
        }

        // 브라우저 응답
        string responseText = "<html><body><h2>Login complete. You can close this window.</h2></body></html>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseText);
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.OutputStream.Close();
    }

    private void OnDestroy()
    {
        if (httpListener != null && httpListener.IsListening)
        {
            httpListener.Stop();
            httpListener.Close();
        }
    }
}
