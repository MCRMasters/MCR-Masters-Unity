using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace MCRGame.Net
{
    // 서버로부터 수신하는 메시지 모델을 GameWSActionType enum 기반으로 변경합니다.
    [Serializable]
    public class GameWSMessage
    {
        [JsonProperty("event")]
        public GameWSActionType Event { get; set; }

        [JsonProperty("data")]
        public JObject Data { get; set; }
    }
}