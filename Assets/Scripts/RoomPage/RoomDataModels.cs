using System;

[Serializable]
public class RoomUserResponse {
    public string nickname;
    public bool is_ready;
}

[Serializable]
public class AvailableRoomResponse {
    public string name;
    public int room_number;
    public int max_users;
    public int current_users;
    public string host_nickname;
    public RoomUserResponse[] users;
}

[Serializable]
public class AvailableRoomResponseList {
    public AvailableRoomResponse[] rooms;
}

[System.Serializable]
public class RoomData
{
    public string roomId;      // room_number를 문자열로 사용
    public string roomTitle;   // 방 제목
    public string roomInfo;    // 호스트/인원 정보
}