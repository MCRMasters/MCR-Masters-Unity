using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MCRGame.Net
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GameWSActionType
    {
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
        HU_HAND
    }
}