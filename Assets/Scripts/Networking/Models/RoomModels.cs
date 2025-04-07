using System;

namespace MCRGame.Net
{
    [Serializable]
    public class RoomUserResponse
    {
        public string uid;
        public string nickname;
        public bool is_ready;
        public int slot_index; // 추가된 필드
    }

    [Serializable]
    public class AvailableRoomResponse
    {
        public string name;
        public int room_number;
        public int max_users;
        public int current_users;
        public string host_uid;
        public string host_nickname;
        public RoomUserResponse[] users;
    }

    [Serializable]
    public class AvailableRoomResponseList
    {
        public AvailableRoomResponse[] rooms;
    }

    [Serializable]
    public class RoomResponse
    {
        public string name;
        public int room_number;
        public int slot_index;
    }

    [Serializable]
    public class CreateRoomResponse
    {
        public string name;
        public int room_number;
        public int slot_index;
    }

    [Serializable]
    public class RoomData
    {
        public string roomId;
        public string roomTitle;
        public string roomInfo;
    }

    [Serializable]
    public class RoomUserData
    {
        public string uid;
        public string nickname;
        public int slot_index;
        public bool isReady;
    }
}
