namespace MCRGame.Common
{
    public enum GameEventType
    {
        DISCARD = 0,
        TSUMO = 1,
        SHOMIN_KAN = 2,
        DAIMIN_KAN = 3,
        AN_KAN = 4,
        CHII = 5,
        PON = 6,
        FLOWER = 7,
        INIT_HAIPAI = 8,
        INIT_FLOWER = 9,
        HU = 10,
        ROBBING_KONG = 11,
        INIT_FLOWER_OK = 12
    }

    public static class GameEventTypeExtensions
    {
        public static GameEventType? NextEvent(this GameEventType evt)
        {
            switch (evt)
            {
                case GameEventType.TSUMO:
                case GameEventType.CHII:
                case GameEventType.PON:
                    return GameEventType.DISCARD;
                case GameEventType.SHOMIN_KAN:
                    return GameEventType.ROBBING_KONG;
                case GameEventType.INIT_HAIPAI:
                    return GameEventType.INIT_FLOWER;
                case GameEventType.DISCARD:
                case GameEventType.DAIMIN_KAN:
                case GameEventType.AN_KAN:
                case GameEventType.FLOWER:
                case GameEventType.ROBBING_KONG:
                case GameEventType.INIT_FLOWER:
                case GameEventType.INIT_FLOWER_OK:
                    return GameEventType.TSUMO;
                case GameEventType.HU:
                    return null;
                default:
                    return null;
            }
        }

        public static bool IsNextReplacement(this GameEventType evt)
        {
            return evt == GameEventType.DAIMIN_KAN ||
                   evt == GameEventType.AN_KAN ||
                   evt == GameEventType.FLOWER ||
                   evt == GameEventType.ROBBING_KONG;
        }

        public static bool IsNextDiscard(this GameEventType evt)
        {
            return evt == GameEventType.TSUMO ||
                   evt == GameEventType.PON ||
                   evt == GameEventType.CHII;
        }

        public static bool IsKong(this GameEventType evt)
        {
            return evt == GameEventType.ROBBING_KONG ||
                   evt == GameEventType.DAIMIN_KAN ||
                   evt == GameEventType.AN_KAN;
        }
    }

    public enum GameActionType
    {
        SKIP = 0,
        HU = 1,
        KAN = 2,
        PON = 3,
        CHII = 4,
        FLOWER = 5
    }

    public static class GameActionTypeExtensions
    {
        public static GameActionType? CreateFromGameEventType(this GameEventType eventType)
        {
            switch (eventType)
            {
                case GameEventType.SHOMIN_KAN:
                case GameEventType.DAIMIN_KAN:
                case GameEventType.AN_KAN:
                    return GameActionType.KAN;
                case GameEventType.CHII:
                    return GameActionType.CHII;
                case GameEventType.PON:
                    return GameActionType.PON;
                case GameEventType.FLOWER:
                    return GameActionType.FLOWER;
                case GameEventType.HU:
                    return GameActionType.HU;
                default:
                    return null;
            }
        }
    }

    public enum CallBlockType
    {
        CHII = 0,
        PUNG = 1,
        AN_KONG = 2,
        SHOMIN_KONG = 3,
        DAIMIN_KONG = 4
    }
}
