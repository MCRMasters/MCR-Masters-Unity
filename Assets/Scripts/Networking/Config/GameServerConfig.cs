using System;
using UnityEngine;

namespace MCRGame.Net
{
    public static class GameServerConfig
    {
        public static string HttpBaseUrl = "http://localhost";
        public static string WebSocketBaseUrl = "ws://localhost";
        public static int Port = 8001;
        public static string ApiPrefix = "/api/v1";

        public static string GetHttpUrl(string endpoint)
        {
            return $"{HttpBaseUrl}:{Port}{ApiPrefix}{endpoint}";
        }

        public static string GetWebSocketUrl(string endpoint)
        {
            return $"{WebSocketBaseUrl}:{Port}{ApiPrefix}{endpoint}";
        }
        
        // API 호출 후 새 WebSocket URL을 받아오면 이 메서드를 호출하여 업데이트합니다.
        public static void UpdateWebSocketConfig(string newWebSocketUrl)
        {
            if (Uri.TryCreate(newWebSocketUrl, UriKind.Absolute, out Uri uri))
            {
                WebSocketBaseUrl = uri.Scheme + "://" + uri.Host;
                Port = uri.Port;
                ApiPrefix = uri.AbsolutePath.TrimEnd('/');
                Debug.Log("[GameServerConfig] Updated: " + WebSocketBaseUrl + ":" + Port + ApiPrefix);
            }
            else
            {
                Debug.LogError("[GameServerConfig] Invalid WebSocket URL: " + newWebSocketUrl);
            }
        }
    }
}
