using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using NativeWebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using MCRGame.Common;
using MCRGame.Game;


namespace MCRGame.Net
{
    public class RoomService : MonoBehaviour
    {
        public static RoomService Instance { get; private set; }

        private string httpBaseUrl = CoreServerConfig.GetHttpUrl("/room");
        private string wsBaseUrl = CoreServerConfig.GetWebSocketUrl("/ws/room");

        private WebSocket websocket;

        // ▶ 이벤트
        public event Action<List<RoomInfo>> OnRoomListReceived;
        public event Action<RoomJoinedInfo> OnRoomJoined;
        public event Action<string, List<RoomUserInfo>> OnRoomUsersUpdated; // hostUid, users
        public event Action<string> OnUserJoined;          // userUid
        public event Action<string> OnUserLeft;            // userUid
        public event Action<string, bool> OnUserReadyChanged;    // userUid, isReady
        public event Action OnGameStarted;

        public int CurrentRoomNumber { get; private set; }
        public string CurrentRoomTitle { get; private set; }
        public string HostUid { get; private set; }
        public int HostSlotIndex { get; private set; }
        public List<RoomUserInfo> Players { get; private set; } = new List<RoomUserInfo>();
        public int MySlotIndex { get; private set; }

        private bool hasPendingReady = false;
        private bool pendingReadyState = false;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #region ▶ HTTP API

        public void FetchRooms() => StartCoroutine(FetchRoomsCoroutine());
        private IEnumerator FetchRoomsCoroutine()
        {
            using var req = UnityWebRequest.Get(httpBaseUrl);
            req.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[RoomService] FetchRooms 실패: {req.error}");
                yield break;
            }
            var jarr = JArray.Parse(req.downloadHandler.text);
            var rooms = jarr.ToObject<List<RoomInfo>>();
            OnRoomListReceived?.Invoke(rooms);
        }

        public void CreateRoom() => StartCoroutine(CreateRoomCoroutine());

        public void JoinRoom(int roomNumber)
        {
            CurrentRoomNumber = roomNumber;
            StartCoroutine(JoinRoomCoroutine(roomNumber));
        }
        private IEnumerator CreateRoomCoroutine()
        {
            using var req = new UnityWebRequest(httpBaseUrl, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Array.Empty<byte>()),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[RoomService] CreateRoom 실패: {req.error}");
                yield break;
            }
            var info = JsonConvert.DeserializeObject<RoomJoinedInfo>(req.downloadHandler.text);

            // 상태 세팅
            CurrentRoomNumber = info.room_number;
            CurrentRoomTitle = info.name;
            MySlotIndex = HostSlotIndex = info.slot_index;
            HostUid = PlayerDataManager.Instance.Uid;

            OnRoomJoined?.Invoke(info);
            // → 먼저 사용자 목록 로드부터 완료하고
            yield return StartCoroutine(FetchRoomUsersCoroutine());
            // → 그 다음 WS 연결
            ConnectWebSocket(info.room_number);
        }

        private IEnumerator JoinRoomCoroutine(int roomNumber)
        {
            var url = $"{httpBaseUrl}/{roomNumber}/join";
            using var req = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Array.Empty<byte>()),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[RoomService] JoinRoom 실패: {req.error}");
                yield break;
            }
            var info = JsonConvert.DeserializeObject<RoomJoinedInfo>(req.downloadHandler.text);


            // 상태 세팅

            CurrentRoomNumber = info.room_number;
            CurrentRoomTitle = info.name;
            MySlotIndex = info.slot_index;
            OnRoomJoined?.Invoke(info);
            // → 먼저 HTTP로 기존 유저들 로드
            yield return StartCoroutine(FetchRoomUsersCoroutine());
            // → 이후에 WebSocket 연결
            ConnectWebSocket(roomNumber);
        }

        public void FetchRoomUsers() => StartCoroutine(FetchRoomUsersCoroutine());
        private IEnumerator FetchRoomUsersCoroutine()
        {
            var url = $"{httpBaseUrl}/{CurrentRoomNumber}/users";
            Debug.Log($"[RoomService] ▶ FetchRoomUsers start. URL={url}");

            using var req = UnityWebRequest.Get(url);
            req.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[RoomService] ❌ FetchRoomUsers failed: {req.error}");
                yield break;
            }

            var text = req.downloadHandler.text;
            Debug.Log($"[RoomService] ✔ FetchRoomUsers success. Response JSON: {text}");

            var resp = JsonConvert.DeserializeObject<RoomUsersResponse>(text);
            HostUid = resp.host_uid;
            Players = resp.users.ToList();
            HostSlotIndex = Players.FindIndex(u => u.uid == HostUid);

            Debug.Log($"[RoomService] ▶ Parsed Users: count={Players.Count}, HostUid={HostUid}, HostSlotIndex={HostSlotIndex}");
            for (int i = 0; i < Players.Count; i++)
            {
                var u = Players[i];
                Debug.Log($"[RoomService]   Slot[{i}]: uid={u.uid}, nickname={u.nickname}, isReady={u.is_ready}");
            }

            OnRoomUsersUpdated?.Invoke(HostUid, Players);
        }

        public void LeaveRoom()
        {
            Debug.Log($"[RoomService] ▶ LeaveRoom start. RoomNumber={CurrentRoomNumber}");
            DisconnectWebSocket();
            StartCoroutine(LeaveRoomCoroutine());
        }
        private IEnumerator LeaveRoomCoroutine()
        {
            var url = $"{httpBaseUrl}/{CurrentRoomNumber}/leave";
            Debug.Log($"[RoomService] ▶ LeaveRoom API call. URL={url}");

            using var req = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Array.Empty<byte>()),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[RoomService] ❌ LeaveRoom failed: {req.error}");
            }
            else
            {
                Debug.Log($"[RoomService] ✔ LeaveRoom success");
            }
        }



        public void SendReady(bool isReady)
        {
            // 1) 로컬 상태 업데이트 (기존)
            var me = Players.FirstOrDefault(p => p.slot_index == MySlotIndex);
            if (me != null)
            {
                me.is_ready = isReady;
                OnUserReadyChanged?.Invoke(me.uid, isReady);
                OnRoomUsersUpdated?.Invoke(HostUid, new List<RoomUserInfo>(Players));
            }

            // 2) WS로 Ready 메시지 전송 or 저장
            if (websocket?.State == WebSocketState.Open)
            {
                var msg = new JObject
                {
                    ["action"] = "ready",
                    ["data"] = new JObject { ["is_ready"] = isReady }
                }.ToString(Formatting.None);

                websocket.SendText(msg);
                Debug.Log($"[RoomService] Sent READY message: is_ready={isReady}");
            }
            else
            {
                // 저장해둔다
                hasPendingReady = true;
                pendingReadyState = isReady;
                Debug.LogWarning("[RoomService] WebSocket not open — pending ready saved");
            }
        }

        /// <summary>
        /// 호스트가 특정 슬롯에 봇을 추가 요청합니다.
        /// </summary>
        /// <param name="slotIndex">추가할 슬롯 인덱스</param>
        public void AddBotToSlot(int slotIndex)
        {
            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                // JSON 메시지 생성
                var msg = new JObject
                {
                    ["action"] = "add_bot",
                    ["data"] = new JObject
                    {
                        ["slot_index"] = slotIndex
                    }
                }.ToString();

                websocket.SendText(msg);
                Debug.Log($"[RoomService] Sent ADD_BOT message → slot_index={slotIndex}");
            }
            else
            {
                Debug.LogWarning("[RoomService] Cannot send ADD_BOT: WebSocket is not open");
            }
        }


        public void StartGame() => StartCoroutine(StartGameCoroutine());
        private IEnumerator StartGameCoroutine()
        {
            var url = $"{httpBaseUrl}/{CurrentRoomNumber}/game-start";
            using var req = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Array.Empty<byte>()),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
                Debug.Log("[RoomService] GameStarted");
            else
                Debug.LogError($"[RoomService] StartGame 실패: {req.error}");
        }

        #endregion

        #region ▶ WebSocket

        private async void ConnectWebSocket(int roomNumber)
        {
            DisconnectWebSocket();
            var token = PlayerDataManager.Instance.AccessToken;
            var url = $"{wsBaseUrl}/{roomNumber}?authorization={Uri.EscapeDataString(token)}";
            websocket = new WebSocket(url);

            websocket.OnOpen += () =>
            {
                Debug.Log("[RoomService] WS Connected");
                // 연결 직후에 저장된 pending ready 상태가 있으면 전송
                if (hasPendingReady)
                {
                    var pendingMsg = new JObject
                    {
                        ["action"] = "ready",
                        ["data"] = new JObject { ["is_ready"] = pendingReadyState }
                    }.ToString(Formatting.None);

                    websocket.SendText(pendingMsg);
                    Debug.Log($"[RoomService] Sent pending READY message: is_ready={pendingReadyState}");
                    hasPendingReady = false;
                }
            };

            websocket.OnError += e => Debug.LogError("[RoomService] WS Error: " + e);
            websocket.OnClose += e => Debug.LogWarning("[RoomService] WS Closed: " + e);
            websocket.OnMessage += bytes =>
            {
                var str = Encoding.UTF8.GetString(bytes);
                ProcessWebSocketMessage(str);
            };

            try { await websocket.Connect(); }
            catch (Exception ex) { Debug.LogError("[RoomService] WS Connect Failed: " + ex.Message); }
        }

        private void DisconnectWebSocket()
        {
            if (websocket != null)
            {
                websocket.Close();
                websocket = null;
            }
        }

        private void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            websocket?.DispatchMessageQueue();
#endif
        }


        public bool GetMyReadyState()
        {
            // mySlotIndex 는 JoinRoom 후 설정됩니다.
            // Players 리스트에서 내 슬롯을 찾아 준비 상태를 확인
            var me = Players.FirstOrDefault(p => p.slot_index == MySlotIndex);
            return me != null && me.is_ready;
        }

        public string GetMyUid()
        {
            var me = Players.FirstOrDefault(p => p.slot_index == MySlotIndex);
            return me != null ? me.uid : "";
        }

        private void ProcessWebSocketMessage(string message)
        {
            Debug.Log($"[RoomService] WS Msg Received: {message}");

            var j = JObject.Parse(message);
            var action = j["action"]?.Value<string>();
            var data = j["data"] as JObject;

            Debug.Log($"[RoomService] Action: {action}");

            switch (action)
            {
                case "user_joined":
                    {
                        Debug.Log("[RoomService] ▶ user_joined branch");
                        // 1) 데이터 파싱
                        var newUser = data.ToObject<RoomUserInfo>();
                        Debug.Log($"[RoomService] NewUser: {newUser.ToString()}");
                        Debug.Log($"[RoomService] NewUser: uid={newUser.uid}, slot={newUser.slot_index}, ready={newUser.is_ready}");

                        // 2) 기존 슬롯에 있으면 교체, 없으면 추가
                        var idx = Players.FindIndex(u => u.slot_index == newUser.slot_index);
                        if (idx >= 0)
                        {
                            Debug.Log($"[RoomService] Replacing existing user at slot {idx}");
                            Players[idx] = newUser;
                        }
                        else
                        {
                            Debug.Log($"[RoomService] Adding new user at slot {newUser.slot_index}");
                            Players.Add(newUser);
                        }

                        // 3) 혹시 호스트가 변경됐으면 갱신
                        if (newUser.uid == HostUid)
                        {
                            HostSlotIndex = newUser.slot_index;
                            Debug.Log($"[RoomService] HostSlotIndex updated to {HostSlotIndex}");
                        }

                        // 4) 이벤트 발행
                        Debug.Log($"[RoomService] Firing OnUserJoined({newUser.uid}) and OnRoomUsersUpdated");
                        OnUserJoined?.Invoke(newUser.uid);
                        OnRoomUsersUpdated?.Invoke(HostUid, new List<RoomUserInfo>(Players));
                        break;
                    }

                case "user_left":
                    {
                        Debug.Log("[RoomService] ▶ user_left branch");
                        // 1) UID 읽어서 리스트에서 제거
                        var uid = data["user_uid"].Value<string>();
                        Debug.Log($"[RoomService] Removing user uid={uid}");
                        var toRemove = Players.FirstOrDefault(u => u.uid == uid);
                        if (toRemove != null)
                        {
                            Players.Remove(toRemove);
                            Debug.Log($"[RoomService] Removed. Remaining count: {Players.Count}");
                        }
                        else
                        {
                            Debug.LogWarning($"[RoomService] Could not find user uid={uid} to remove");
                        }

                        // 2) 이벤트
                        Debug.Log($"[RoomService] Firing OnUserLeft({uid}) and OnRoomUsersUpdated");
                        OnUserLeft?.Invoke(uid);
                        OnRoomUsersUpdated?.Invoke(HostUid, new List<RoomUserInfo>(Players));
                        break;
                    }

                case "user_ready_changed":
                    {
                        Debug.Log("[RoomService] ▶ user_ready_changed branch");
                        // 1) UID, ready 상태 읽기
                        var uid = data["user_uid"].Value<string>();
                        var isReady = data["is_ready"].Value<bool>();
                        Debug.Log($"[RoomService] user_ready_changed: uid={uid}, isReady={isReady}");

                        // 2) 리스트에서 찾아서 상태 갱신
                        var user = Players.FirstOrDefault(u => u.uid == uid);
                        if (user != null)
                        {
                            user.is_ready = isReady;
                            Debug.Log($"[RoomService] Updated Players[{user.slot_index}].is_ready = {isReady}");
                        }
                        else
                        {
                            Debug.LogWarning($"[RoomService] Could not find user uid={uid} in Players to update ready state");
                        }

                        // 3) 이벤트
                        Debug.Log($"[RoomService] Firing OnUserReadyChanged({uid}, {isReady}) and OnRoomUsersUpdated");
                        OnUserReadyChanged?.Invoke(uid, isReady);
                        OnRoomUsersUpdated?.Invoke(HostUid, new List<RoomUserInfo>(Players));
                        break;
                    }

                case "user_list":
                    {
                        Debug.Log("[RoomService] ▶ user_list branch");
                        // 1) 전체 리스트 덮어쓰기
                        HostUid = j["data"]["host_uid"].Value<string>();
                        Debug.Log($"[RoomService] New HostUid = {HostUid}");
                        var arr = j["data"]["users"] as JArray;
                        Players = arr.ToObject<List<RoomUserInfo>>();
                        Debug.Log($"[RoomService] Players overwritten, count = {Players.Count}");

                        // 2) HostSlotIndex 갱신
                        HostSlotIndex = Players.FindIndex(u => u.uid == HostUid);
                        Debug.Log($"[RoomService] HostSlotIndex = {HostSlotIndex}");

                        // 3) 이벤트
                        Debug.Log($"[RoomService] Firing OnRoomUsersUpdated");
                        OnRoomUsersUpdated?.Invoke(HostUid, new List<RoomUserInfo>(Players));
                        break;
                    }

                case "game_started":
                    {
                        var wsMessage = new GameWSMessage
                        {
                            Event = GameWSActionType.CLIENT_GAME_START_INFO,
                            Data = new JObject
                            {
                                ["players"] = JArray.FromObject(Players)
                            }
                        };

                        GameMessageMediator.Instance.EnqueueMessage(wsMessage);
                        Debug.Log("[RoomService] ▶ game_started branch");
                        // Game 서버 URL 업데이트
                        var gameUrl = j["data"]["game_url"]?.Value<string>();
                        if (!string.IsNullOrEmpty(gameUrl))
                        {
                            GameServerConfig.UpdateWebSocketConfig(gameUrl);
                            Debug.Log($"[RoomService] GameServerConfig updated to {gameUrl}");
                        }
                        OnGameStarted?.Invoke();
                        break;
                    }

                default:
                    Debug.LogWarning($"[RoomService] Unknown action: {action}");
                    break;


            }
        }
        #endregion
    }
}