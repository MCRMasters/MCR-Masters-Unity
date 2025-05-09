using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MCRGame.Net
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GameWSActionType
    {
        [EnumMember(Value = "client_game_start_info")]
        CLIENT_GAME_START_INFO,
        [EnumMember(Value = "game_event")]
        GAME_EVENT,
        [EnumMember(Value = "return_action")]
        RETURN_ACTION,
        [EnumMember(Value = "reload_data")]
        RELOAD_DATA,
        [EnumMember(Value = "emoji_send")]
        EMOJI_SEND,
        [EnumMember(Value = "emoji_broadcast")]
        EMOJI_BROADCAST,
        [EnumMember(Value = "game_start_info")]
        GAME_START_INFO,
        [EnumMember(Value = "init_event")]
        INIT_EVENT,
        [EnumMember(Value = "init_flower_replacement")]
        INIT_FLOWER_REPLACEMENT,
        [EnumMember(Value = "haipai_hand")]
        HAIPAI_HAND,
        [EnumMember(Value = "tsumo_actions")]
        TSUMO_ACTIONS,
        [EnumMember(Value = "discard_actions")]
        DISCARD_ACTIONS,
        [EnumMember(Value = "robbing_kong_actions")]
        ROBBING_KONG_ACTIONS,
        [EnumMember(Value = "discard")]
        DISCARD,
        [EnumMember(Value = "tsumo")]
        TSUMO,
        [EnumMember(Value = "chii")]
        CHII,
        [EnumMember(Value = "pon")]
        PON,
        [EnumMember(Value = "daimin_kan")]
        DAIMIN_KAN,
        [EnumMember(Value = "shomin_kan")]
        SHOMIN_KAN,
        [EnumMember(Value = "an_kan")]
        AN_KAN,
        [EnumMember(Value = "flower")]
        FLOWER,
        [EnumMember(Value = "open_an_kan")]
        OPEN_AN_KAN,
        [EnumMember(Value = "hu_hand")]
        HU_HAND,
        [EnumMember(Value = "ping")]
        PING,
        [EnumMember(Value = "pong")]
        PONG,
        [EnumMember(Value = "user_joined")]
        USER_JOINED,
        [EnumMember(Value = "success")]
        SUCCESS,
        [EnumMember(Value = "error")]
        ERROR,
        [EnumMember(Value = "update_action_id")]
        UPDATE_ACTION_ID,
        [EnumMember(Value = "set_timer")]
        SET_TIMER,
        [EnumMember(Value = "draw")]
        DRAW,
        [EnumMember(Value = "end_game")]
        END_GAME,
    }
}
