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

    public class Action : IComparable<Action>
    {
        public ActionType Type { get; set; }
        public RelativeSeat SeatPriority { get; set; }
        public GameTile Tile { get; set; }

        public int CompareTo(Action other)
        {
            // 간단한 정렬 예시: 타입, 좌석 우선순위, 타일 순으로 비교
            int cmp = Type.CompareTo(other.Type);
            if (cmp != 0) return cmp;
            cmp = SeatPriority.CompareTo(other.SeatPriority);
            if (cmp != 0) return cmp;
            return Tile.CompareTo(other.Tile);
        }

        public static Action CreateFromGameEvent(GameEvent gameEvent, AbsoluteSeat currentPlayerSeat)
        {
            GameTile tile = GameTile.F0;
            if (gameEvent.Data != null && gameEvent.Data.ContainsKey("tile"))
            {
                tile = (GameTile)gameEvent.Data["tile"];
            }
            ActionType? actionType = gameEvent.EventType.CreateFromGameEventType();
            if (actionType == null)
                throw new ArgumentException("action type is None.");

            return new Action
            {
                Type = (ActionType)actionType,
                SeatPriority = RelativeSeatExtensions.CreateFromAbsoluteSeats((int)currentPlayerSeat, (int)gameEvent.PlayerSeat),
                Tile = tile
            };
        }
    }
}
