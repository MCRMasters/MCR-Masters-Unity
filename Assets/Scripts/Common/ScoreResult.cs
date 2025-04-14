using System;
using System.Collections.Generic;

[Serializable]
public class ScoreResult
{
    public int total_score;
    public List<List<int>> yaku_score_list;

    public override string ToString()
    {
        return $"Total: {total_score}, yaku_score_list: {string.Join(",", yaku_score_list)}";
    }
}