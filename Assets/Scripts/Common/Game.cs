using System;
using System.Collections.Generic;

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

    public class GameAction : IComparable<GameAction>
    {
        public GameActionType Type { get; set; }
        public RelativeSeat SeatPriority { get; set; }
        public GameTile Tile { get; set; }

        public int CompareTo(GameAction other)
        {
            // 간단한 정렬 예시: 타입, 좌석 우선순위, 타일 순으로 비교
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

    [Serializable]
    public class CallBlockData
    {
        public CallBlockType Type { get; set; }
        public GameTile FirstTile { get; set; }
        public RelativeSeat SourceSeat { get; set; }
        public int SourceTileIndex { get; set; }

        public CallBlockData() { }

        public CallBlockData(CallBlockType type, GameTile firstTile, RelativeSeat sourceSeat, int sourceTileIndex)
        {
            Type = type;
            FirstTile = firstTile;
            SourceSeat = sourceSeat;
            SourceTileIndex = sourceTileIndex;
        }
    }
}
