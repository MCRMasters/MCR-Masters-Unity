namespace MCRGame.Net
{
    public static class CoreServerConfig
    {
        // public static string HttpBaseUrl = "http://localhost";
        // public static string WebSocketBaseUrl = "ws://localhost";
        public static string HttpBaseUrl = "http://mcrs.duckdns.org";
        public static string WebSocketBaseUrl = "ws://mcrs.duckdns.org";
        
        public static string HttpsBaseUrl = "https://mcrs.duckdns.org/core";
        public static string WebSocketHttpsBaseUrl = "wss://mcrs.duckdns.org/core";
        
        
        
        public static int Port = 8000;
        
        public static string ApiPrefix = "/api/v1";
        
        public static string GetHttpUrl(string endpoint)
        {
            // return $"{HttpBaseUrl}:{Port}{ApiPrefix}{endpoint}";
            return $"{HttpsBaseUrl}{ApiPrefix}{endpoint}";
        }
        
        

        public static string GetWebSocketUrl(string endpoint)
        {
            // return $"{WebSocketBaseUrl}:{Port}{ApiPrefix}{endpoint}";
            return $"{WebSocketHttpsBaseUrl}{ApiPrefix}{endpoint}";
        }
    }
}
