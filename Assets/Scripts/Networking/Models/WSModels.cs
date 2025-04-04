using System;
using Newtonsoft.Json;

namespace MCRGame.Net
{
    // 클라이언트가 서버로부터 수신하는 이벤트 메시지 모델입니다.
    public class WSMessage
    {
        public string status;    // "success" 또는 "error"
        public WSActionType action;
        public object data;
        public string error;
        public string timestamp;
    }

    // 서버의 GameStartedData에 대응
    public class WSGameStartedData
    {
        public string game_url;
    }

    // 서버의 UserReadyData에 대응
    public class WSUserReadyData
    {
        public Guid user_id;
        public string nickname;
        public bool is_ready;
    }

    // 서버의 UserJoinedData에 대응
    public class WSUserJoinedData
    {
        public Guid user_id;
        public string nickname;
        public bool is_ready;  // 기본값 false
    }

    // 서버의 UserLeftData에 대응
    public class WSUserLeftData
    {
        public Guid user_id;
    }

    // 클라이언트가 서버로 보낼 요청 메시지 모델
    public class WSRequest
    {
        public string action;
        public object data;
    }
}
