namespace MCRGame.Common
{
    public class WinningConditions
    {
        public GameTile? WinningTile { get; set; }
        public bool IsDiscarded { get; set; }
        public bool IsLastTileInTheGame { get; set; }
        public bool IsLastTileOfItsKind { get; set; }
        public bool IsReplacementTile { get; set; }
        public bool IsRobbingTheKong { get; set; }

        public WinningConditions() { }
        public WinningConditions(
            GameTile? winningTile,
            bool isDiscarded,
            bool isLastTileInTheGame,
            bool isLastTileOfItsKind,
            bool isReplacementTile,
            bool isRobbingTheKong)
        {
            WinningTile = winningTile;
            IsDiscarded = isDiscarded;
            IsLastTileInTheGame = isLastTileInTheGame;
            IsLastTileOfItsKind = isLastTileOfItsKind;
            IsReplacementTile = isReplacementTile;
            IsRobbingTheKong = isRobbingTheKong;
        }

        public static WinningConditions CreateDefaultConditions()
        {
            return new WinningConditions
            {
                WinningTile = null,
                IsDiscarded = false,
                IsLastTileInTheGame = false,
                IsLastTileOfItsKind = false,
                IsReplacementTile = false,
                IsRobbingTheKong = false
            };
        }
    }
}
