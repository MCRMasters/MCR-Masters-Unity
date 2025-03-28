using System;

namespace MCRMastersCore.Models
{
    [Serializable]
    public class RoomUserResponse
    {
        public string nickname { get; set; }
        public bool is_ready { get; set; }
    }

    [Serializable]
    public class AvailableRoomResponse
    {
        public string name { get; set; }
        public int room_number { get; set; }
        public int max_users { get; set; }
        public int current_users { get; set; }
        public string host_nickname { get; set; }
        public RoomUserResponse[] users { get; set; }
    }

    [Serializable]
    public class AvailableRoomResponseList
    {
        public AvailableRoomResponse[] rooms { get; set; }
    }

    [Serializable]
    public class RoomData
    {
        // room_number를 문자열로 사용하여 UI에 표시할 때 활용
        public string roomId { get; set; }
        public string roomTitle { get; set; }
        public string roomInfo { get; set; }
    }
}
