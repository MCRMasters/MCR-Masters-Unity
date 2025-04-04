using System;

namespace MCRGame.Net
{
    [Serializable]
    public class RoomUserResponse
    {
        public string nickname;
        public bool is_ready;
    }

    [Serializable]
    public class AvailableRoomResponse
    {
        public string name;
        public int room_number;
        public int max_users;
        public int current_users;
        public string host_nickname;
        public RoomUserResponse[] users;
    }

    [Serializable]
    public class AvailableRoomResponseList
    {
        public AvailableRoomResponse[] rooms;
    }

    [Serializable]
    public class CreateRoomResponse
    {
        public string name;
        public int room_number;
        public string message;
    }

    [Serializable]
    public class RoomData
    {
        public string roomId;
        public string roomTitle;
        public string roomInfo;
    }
}
