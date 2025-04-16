using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


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

    [JsonConverter(typeof(YakuScoreConverter))]
    [Serializable]
    public class YakuScore
    {
        public Yaku YakuId { get; set; }
        public int Score { get; set; }

        public YakuScore() { }

        public YakuScore(int yid, int score)
        {
            YakuId = (Yaku)yid;
            Score = score;
        }

        public override string ToString()
        {
            return $"({YakuId}, {Score})";
        }
    }

    public class YakuScoreConverter : JsonConverter<YakuScore>
    {
        public override YakuScore ReadJson(JsonReader reader, Type objectType, YakuScore existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // JSON 배열 예: [yakuId, score]
            JArray jArray = JArray.Load(reader);
            int yakuId = jArray[0].ToObject<int>();
            int score = jArray[1].ToObject<int>();
            return new YakuScore(yakuId, score);
        }

        public override void WriteJson(JsonWriter writer, YakuScore value, JsonSerializer serializer)
        {
            // 배열 형태로 출력: [yakuId, score]
            JArray jArray = new JArray
            {
                (int)value.YakuId,
                value.Score
            };
            jArray.WriteTo(writer);
        }
    }
}
