using System;
using Newtonsoft.Json;

namespace MCRGame.Net
{
    // 서버에서 받는 기본 메시지 포맷
    public class WSMessage
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("action")]
        public WSActionType Action { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
    }

    // Python의 WebSocketMessage에 대응
    public class WSWebSocketMessage
    {
        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }
    }

    // Python의 WebSocketResponse에 대응
    public class WSWebSocketResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
    }

    // Python의 UserReadyData에 대응
    public class WSUserReadyData
    {
        [JsonProperty("user_uid")]
        public string UserUid { get; set; }

        [JsonProperty("is_ready")]
        public bool IsReady { get; set; }
    }

    // Python의 UserJoinedData에 대응
    public class WSUserJoinedData
    {
        [JsonProperty("user_uid")]
        public string UserUid { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("slot_index")]
        public int SlotIndex { get; set; }

        [JsonProperty("is_ready")]
        public bool IsReady { get; set; } = false;
    }

    // Python의 UserLeftData에 대응
    public class WSUserLeftData
    {
        [JsonProperty("user_uid")]
        public string UserUid { get; set; }
    }

    // Python의 GameStartedData / 기존 WSGameStartedData에 대응
    public class WSGameStartedData
    {
        [JsonProperty("game_url")]
        public string GameUrl { get; set; }
    }
}
