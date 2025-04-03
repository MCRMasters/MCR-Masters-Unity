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
    }
}
