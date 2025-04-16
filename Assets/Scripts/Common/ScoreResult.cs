using System;
using System.Collections.Generic;


namespace MCRGame.Common
{
    [Serializable]
    public class ScoreResult
    {
        public int total_score { get; set; }
        public List<YakuScore> yaku_score_list;

        public override string ToString()
        {
            return $"Total: {total_score}, yaku_score_list: {string.Join(",", yaku_score_list)}";
        }
    }

    public class YakuScore
    {
        public Yaku YakuId { get; set; }
        public int Score { get; set; }

        public YakuScore(int yid, int score)
        {
            this.YakuId = (Yaku)yid;
            this.Score = score;
        }
    }
}
