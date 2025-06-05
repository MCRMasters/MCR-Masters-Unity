using UnityEngine;

namespace MCRGame.Net
{
    public static class GameServerConfig
    {
        private static string webSocketBaseUrl = "";
        private static string WebSocketHttpsBaseUrl = "wss://mcrs.duckdns.org/game/api/v1/games/";

        /// <summary>
        /// 외부(게임 시작 API)에서 받은 websocket URL로 기본 URL을 업데이트합니다.
        /// </summary>
        /// <param name="newUrl">새로운 전체 WebSocket URL (game id 포함)</param>
        public static void UpdateWebSocketConfig(string newUrl)
        {
            webSocketBaseUrl = newUrl;
            Debug.Log("[GameServerConfig] Updated WebSocket base URL: " + webSocketBaseUrl);
        }

        public static void UpdateWebSocketConfigWithGameId(string gameId)
        {
            webSocketBaseUrl = $"{WebSocketHttpsBaseUrl}{gameId}";
            Debug.Log("[GameServerConfig] Updated WebSocket base URL: " + webSocketBaseUrl);
        }

        /// <summary>
        /// 현재 설정된 WebSocket 기본 URL을 반환합니다.
        /// </summary>
        public static string GetWebSocketUrl()
        {
            return webSocketBaseUrl;
        }
    }
}
