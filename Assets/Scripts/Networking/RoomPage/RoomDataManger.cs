using UnityEngine;

namespace MCRGame.Net
{
    public class RoomDataManager : MonoBehaviour
    {
        public static RoomDataManager Instance { get; private set; }

        public string RoomId { get; private set; }
        public string RoomTitle { get; private set; }
        public string HostNickname { get; private set; }
        // 기본 게스트 닉네임 배열 길이를 3으로 설정 (게스트 3명)
        public string[] GuestNicknames { get; private set; } = new string[3];

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // 씬 전환 시 유지
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 3개의 인자를 받아 방 정보를 저장합니다.
        /// 게스트 닉네임 배열은 기본 길이 3의 빈 배열로 설정합니다.
        /// </summary>
        public void SetRoomInfo(string roomId, string roomTitle, string hostNickname)
        {
            RoomId = roomId;
            RoomTitle = roomTitle;
            HostNickname = hostNickname;
            GuestNicknames = new string[3]; // 길이 3의 빈 배열
            Debug.Log($"[RoomDataManager] Room info set: {roomId}, {roomTitle}, HostNickname: {hostNickname}");
        }

        /// <summary>
        /// 4개의 인자를 받아 방 정보를 저장합니다.
        /// guestNicknames 배열의 길이가 3이 아니면, 길이 3으로 재설정합니다.
        /// </summary>
        public void SetRoomInfo(string roomId, string roomTitle, string hostNickname, string[] guestNicknames)
        {
            RoomId = roomId;
            RoomTitle = roomTitle;
            HostNickname = hostNickname;
            if (guestNicknames.Length == 3)
            {
                GuestNicknames = guestNicknames;
            }
            else
            {
                // guestNicknames 길이가 3이 아니면, 길이 3으로 재설정 (필요 시 배열 값을 복사할 수 있음)
                GuestNicknames = new string[3];
                for (int i = 0; i < Mathf.Min(guestNicknames.Length, 3); i++)
                {
                    GuestNicknames[i] = guestNicknames[i];
                }
            }
            Debug.Log($"[RoomDataManager] Room info set: {roomId}, {roomTitle}, HostNickname: {hostNickname}, GuestNicknames: {string.Join(", ", GuestNicknames)}");
        }
    }
}
