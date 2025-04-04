using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MCRGame.Net
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WSActionType
    {
        [EnumMember(Value = "ping")]
        PING,
        [EnumMember(Value = "join")]
        JOIN,
        [EnumMember(Value = "leave")]
        LEAVE,
        [EnumMember(Value = "ready")]
        READY,
        [EnumMember(Value = "pong")]
        PONG,
        [EnumMember(Value = "user_joined")]
        USER_JOINED,
        [EnumMember(Value = "user_left")]
        USER_LEFT,
        [EnumMember(Value = "user_ready_changed")]
        USER_READY_CHANGED,
        [EnumMember(Value = "game_started")]
        GAME_STARTED,
        [EnumMember(Value = "error")]
        ERROR,
    }
}
