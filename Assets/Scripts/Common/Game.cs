using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MCRGame.Common
{
    public class GameEvent
    {
        public GameEventType EventType { get; set; }
        public AbsoluteSeat PlayerSeat { get; set; }
        public int ActionId { get; set; }
        public Dictionary<string, object> Data { get; set; }

        public GameEvent()
        {
            Data = new Dictionary<string, object>();
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class GameAction : IComparable<GameAction>
    {
        [JsonProperty("type")]
        public GameActionType Type { get; set; }

        [JsonProperty("seat_priority")]
        public RelativeSeat SeatPriority { get; set; }

        [JsonProperty("tile")]
        public GameTile Tile { get; set; }

        public int CompareTo(GameAction other)
        {
            int cmp = Type.CompareTo(other.Type);
            if (cmp != 0) return cmp;
            cmp = SeatPriority.CompareTo(other.SeatPriority);
            if (cmp != 0) return cmp;
            return Tile.CompareTo(other.Tile);
        }


        public static GameAction CreateFromGameEvent(GameEvent gameEvent, AbsoluteSeat currentPlayerSeat)
        {
            GameTile tile = GameTile.F0;
            if (gameEvent.Data != null && gameEvent.Data.ContainsKey("tile"))
            {
                tile = (GameTile)gameEvent.Data["tile"];
            }
            GameActionType? actionType = gameEvent.EventType.CreateFromGameEventType();
            if (actionType == null)
                throw new ArgumentException("action type is None.");

            return new GameAction
            {
                Type = (GameActionType)actionType,
                SeatPriority = RelativeSeatExtensions.CreateFromAbsoluteSeats(currentPlayerSeat, gameEvent.PlayerSeat),
                Tile = tile
            };
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class CallBlockData
    {
        [JsonProperty("type")]
        public CallBlockType Type { get; set; }

        [JsonProperty("first_tile")]
        public GameTile FirstTile { get; set; }

        [JsonProperty("source_seat")]
        public RelativeSeat SourceSeat { get; set; }

        [JsonProperty("source_tile_index")]
        public int SourceTileIndex { get; set; }

        public CallBlockData() { }

        public CallBlockData(CallBlockType type, GameTile firstTile, RelativeSeat sourceSeat, int sourceTileIndex)
        {
            Type = type;
            FirstTile = firstTile;
            SourceSeat = sourceSeat;
            SourceTileIndex = sourceTileIndex;
        }

        public override string ToString()
        {
            string ret = "";
            switch (Type){
                case CallBlockType.CHII:
                    ret += Enum.GetName(typeof(GameTile), FirstTile);
                    ret += Enum.GetName(typeof(GameTile), FirstTile + 1);
                    ret += Enum.GetName(typeof(GameTile), FirstTile + 2);
                    break;
                case CallBlockType.PUNG:
                    ret += Enum.GetName(typeof(GameTile), FirstTile);
                    ret += Enum.GetName(typeof(GameTile), FirstTile);
                    ret += Enum.GetName(typeof(GameTile), FirstTile);
                    break;
                case CallBlockType.AN_KONG:
                case CallBlockType.SHOMIN_KONG:
                case CallBlockType.DAIMIN_KONG:
                    ret += Enum.GetName(typeof(GameTile), FirstTile);
                    ret += Enum.GetName(typeof(GameTile), FirstTile);
                    ret += Enum.GetName(typeof(GameTile), FirstTile);
                    ret += Enum.GetName(typeof(GameTile), FirstTile);
                    break;
                default:
                    break;
            }
            return $"{Enum.GetName(typeof(CallBlockType), Type)} {ret}";
        }
    }
}
