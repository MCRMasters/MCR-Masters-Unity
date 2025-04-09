using System;
using UnityEngine;
using MCRGame.Game;

namespace MCRGame.Net
{
    public class RoomDataManager : MonoBehaviour
    {
        public static RoomDataManager Instance { get; private set; }

        public string RoomId { get; private set; }
        public string RoomTitle { get; private set; }

        public RoomUserData HostUser { get; private set; }
        public int HostSlotIndex { get; private set; }  // 호스트의 슬롯 인덱스 (0~3)

        // 총 플레이어 배열 (호스트 포함, 길이 4)
        public RoomUserData[] Players { get; private set; } = new RoomUserData[4];
        public int mySlotIndex = 0;

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

        public void SetRoomInfo(string roomId, string roomTitle, RoomUserData hostUser, int hostSlotIndex)
        {
            RoomId = roomId;
            RoomTitle = roomTitle;
            HostUser = hostUser;
            HostSlotIndex = hostSlotIndex;
            Players = new RoomUserData[4];
            Players[hostSlotIndex] = hostUser;
            Debug.Log($"[RoomDataManager] Room info set: {roomId}, {roomTitle}, Host: {hostUser.nickname} ({hostUser.uid}) at slot {hostSlotIndex}");
        }

        public void SetRoomInfo(string roomId, string roomTitle, RoomUserData hostUser, int hostSlotIndex, RoomUserData[] users)
        {
            RoomId = roomId;
            RoomTitle = roomTitle;
            HostUser = hostUser;
            HostSlotIndex = hostSlotIndex;
            Players = new RoomUserData[4];
            Players[hostSlotIndex] = hostUser;
            if (users != null)
            {
                foreach (var user in users)
                {
                    if (user == null)
                        continue;
                    if (user.slot_index == hostSlotIndex)
                        continue;
                    if (user.slot_index >= 0 && user.slot_index < Players.Length)
                    {
                        Players[user.slot_index] = user;
                    }
                    else
                    {
                        Debug.LogWarning($"[RoomDataManager] user {user.nickname}의 slot_index({user.slot_index})가 유효 범위를 벗어났습니다.");
                    }
                }
            }
            Debug.Log($"[RoomDataManager] Room info set: {roomId}, {roomTitle}, Host: {hostUser.nickname} ({hostUser.uid}) at slot {hostSlotIndex}, Users: {string.Join(", ", GetGuestNicknames())}");
        }

        private string[] GetGuestNicknames()
        {
            string[] names = new string[Players.Length - 1];
            int idx = 0;
            for (int slot = 0; slot < Players.Length; slot++)
            {
                if (slot == HostSlotIndex)
                    continue;
                names[idx++] = Players[slot] != null ? Players[slot].nickname : "Empty";
            }
            return names;
        }

        public bool IsHost(string myUid)
        {
            if (HostUser == null)
                return false;
            return HostUser.uid == myUid;
        }

        public void PrintPlayers()
        {
            Debug.Log("[RoomDataManager] 현재 Players 배열 상태:");
            for (int i = 0; i < Players.Length; i++)
            {
                if (Players[i] != null)
                {
                    Debug.Log($"  Slot {i}: {Players[i].nickname}, uid: {Players[i].uid}, isReady: {Players[i].isReady}, slot_index: {Players[i].slot_index}");
                }
                else
                {
                    Debug.Log($"  Slot {i}: Empty");
                }
            }
        }


        // 새로 추가: Players 배열에 사용자 정보를 추가하거나 업데이트합니다.
        public void AddOrUpdateUser(RoomUserData user)
        {
            if (user == null)
                return;
            if (user.slot_index >= 0 && user.slot_index < Players.Length)
            {
                Players[user.slot_index] = user;
                RoomManager roomManagerInstance = FindFirstObjectByType<RoomManager>();
                if (roomManagerInstance != null)
                {
                    roomManagerInstance.UpdatePlayerReadyState(user.uid, user.isReady);
                }
                Debug.Log($"[RoomDataManager] Updated user at slot {user.slot_index}: {user.nickname}");
            }
            else
            {
                Debug.LogWarning($"[RoomDataManager] Invalid slot index {user.slot_index} for user {user.nickname}");
            }
        }
    }
}
