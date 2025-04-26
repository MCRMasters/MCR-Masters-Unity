using System;
using System.Collections.Generic;
using MCRGame.Common;

namespace MCRGame.Common
{
    [Serializable]
    public class WinningScoreData
    {
        public List<GameTile> handTiles;
        public List<CallBlockData> callBlocks;
        public int singleScore;
        public int totalScore;

        public List<YakuScore> yaku_score_list;
        public AbsoluteSeat winnerSeat;
        public int flowerCount;
        public GameTile winningTile;

        public WinningScoreData(List<GameTile> handTiles, List<CallBlockData> callBlocks, int singleScore, int totalScore, List<YakuScore> yaku_score_list, AbsoluteSeat winnerSeat, int flowerCount, GameTile winning_tile)
        {
            this.handTiles = handTiles;
            this.callBlocks = callBlocks;
            this.singleScore = singleScore;
            this.totalScore = totalScore;
            this.yaku_score_list = yaku_score_list;
            this.winnerSeat = winnerSeat;
            this.flowerCount = flowerCount;
            this.winningTile = winning_tile;
        }
    }
}
