using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

using MCRGame.Net;
using MCRGame.Common;
using MCRGame.Game.Events;   // ★ Dispatcher 네임스페이스

namespace MCRGame.Game
{
    /// <summary>
    /// WebSocket → 메인 쓰레드 전달용 큐 + 비-게임플레이(로비/점수) 메시지 처리.
    /// 실제 게임플레이 메시지는 GameEventDispatcher 로 넘긴다.
    /// </summary>
    public class GameMessageMediator : MonoBehaviour
    {
        /*──────────────────────────────*/
        /*  Debug helpers               */
        /*──────────────────────────────*/
        private const string LOG_TAG = "[GameMessageMediator]";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static bool IsDebug = true;      // 실행 중에 켜고 끌 수 있음
#else
        public static bool IsDebug = false;
#endif
        private static void D(string msg)
        { if (IsDebug) Debug.Log($"{LOG_TAG} {msg}"); }

        private static void W(string msg)
        { Debug.LogWarning($"{LOG_TAG} {msg}"); }

        /*──────────────────────────────*/
        /*  Singleton                   */
        /*──────────────────────────────*/
        public static GameMessageMediator Instance { get; private set; }

        /*──────────────────────────────*/
        /*  Fields                      */
        /*──────────────────────────────*/
        private readonly Queue<GameWSMessage> _messageQueue = new();
        private GameEventDispatcher _dispatcher;

        /// <summary>
        /// Dispatcher 로 넘길 WS ActionType 집합 (실제 플레이 이벤트).
        /// </summary>
        private static readonly HashSet<GameWSActionType> GameplayEvents = new()
        {
            GameWSActionType.PON,            GameWSActionType.CHII,
            GameWSActionType.DAIMIN_KAN,     GameWSActionType.SHOMIN_KAN,
            GameWSActionType.AN_KAN,         GameWSActionType.DISCARD,
            GameWSActionType.DISCARD_ACTIONS,GameWSActionType.ROBBING_KONG_ACTIONS,
            GameWSActionType.FLOWER,         GameWSActionType.TSUMO,
            GameWSActionType.TSUMO_ACTIONS,  GameWSActionType.DRAW,
            GameWSActionType.HU_HAND
        };

        /// <summary>모든 대기 메시지를 폐기하고 큐를 초기화</summary>
        private void ClearQueue() => _messageQueue.Clear();

        /*──────────────────────────────*/
        /*  Unity lifecycle             */
        /*──────────────────────────────*/
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                D("Duplicate instance → destroyed.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            D("Awake — singleton ready.");
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            D("OnDestroy — detached listeners.");
        }

        private void Start()
        {
            _dispatcher = GameEventDispatcher.Instance;
            if (_dispatcher == null) W("GameEventDispatcher not found on Start().");
            D("Start — dispatcher = " + (_dispatcher ? "OK" : "NULL"));
        }

        private void Update()
        {
            if (IsGameSceneReady()) ProcessQueue();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            D($"SceneLoaded → {scene.name}");
            if (scene.name == "RoomScene")
            {
                D("Entering RoomScene — clearing message queue.");
                ClearQueue();
                _dispatcher = null;
                return;
            }

            if (scene.name == "GameScene")
            {
                _dispatcher = GameEventDispatcher.Instance;
                D("GameScene ready. Dispatcher = " + (_dispatcher ? "OK" : "NULL"));
            }
        }

        /*──────────────────────────────*/
        /*  Public API                  */
        /*──────────────────────────────*/
        public void EnqueueMessage(GameWSMessage message)
        {
            if (message == null)
            {
                W("Tried to enqueue null message.");
                return;
            }
            _messageQueue.Enqueue(message);
            D($"Enqueue: {_messageQueue.Count} in queue.  ▶ {message.Event}");
        }

        /*──────────────────────────────*/
        /*  Internals                   */
        /*──────────────────────────────*/
        private bool IsGameSceneReady()
            => SceneManager.GetActiveScene().name == "GameScene"
               && GameManager.Instance != null
               && GameManager.Instance.IsSceneReady;

        private void ProcessQueue()
        {
            if (_messageQueue.Count == 0) return;
            D($"ProcessQueue — {_messageQueue.Count} pending");

            int processed = 0;
            while (_messageQueue.Count > 0)
            {
                var msg = _messageQueue.Dequeue();
                processed++;

                // ① Gameplay → Dispatcher
                if (_dispatcher != null && GameplayEvents.Contains(msg.Event))
                {
                    D($"▶ Gameplay event → {_dispatcher.name}: {msg.Event}");
                    _dispatcher.OnWSMessage(msg);
                    continue;
                }

                // ② Non-gameplay → Mediator
                D($"▶ Non-gameplay event: {msg.Event}");
                ProcessNonGameplayMessage(msg);
            }
            D($"ProcessQueue done. {processed} handled.");
        }

        private void ProcessNonGameplayMessage(GameWSMessage message)
        {
            switch (message.Event)
            {
                /*────────────────────────────*/
                /*  로비/게임 시작 관련        */
                /*────────────────────────────*/
                case GameWSActionType.CLIENT_GAME_START_INFO:
                    D("CLIENT_GAME_START_INFO");
                    OnClientGameStartInfo(message.Data);
                    break;

                case GameWSActionType.GAME_START_INFO:
                    D("GAME_START_INFO");
                    OnGameStartInfo(message.Data);
                    break;

                case GameWSActionType.INIT_EVENT:
                    D("INIT_EVENT");
                    OnInitEvent(message.Data);
                    break;

                /*────────────────────────────*/
                /*  이모티콘                   */
                /*────────────────────────────*/
                case GameWSActionType.EMOJI_BROADCAST:
                    D("EMOJI_BROADCAST");
                    OnEmojiBroadCast(message.Data);
                    break;

                /*────────────────────────────*/
                /*  게임 진행 보조             */
                /*────────────────────────────*/
                case GameWSActionType.RELOAD_DATA:
                    D("RELOAD_DATA");
                    GameManager.Instance.ReloadData(message.Data);
                    break;

                case GameWSActionType.UPDATE_ACTION_ID:
                    if (message.Data.TryGetValue("action_id", out JToken aidTok))
                    {
                        D($"UPDATE_ACTION_ID → {aidTok}");
                        GameManager.Instance.UpdateActionId(aidTok.ToObject<int>());
                    }
                    break;

                case GameWSActionType.SET_TIMER:
                    D("SET_TIMER");
                    GameManager.Instance.SetTimer(message.Data);
                    break;

                /*────────────────────────────*/
                /*  게임 종료                  */
                /*────────────────────────────*/
                case GameWSActionType.END_GAME:
                    D("END_GAME");
                    OnEndGame(message.Data);
                    break;

                /*────────────────────────────*/
                /*  ACK / ERR                 */
                /*────────────────────────────*/
                case GameWSActionType.SUCCESS:
                    D("SUCCESS → " + message.Data);
                    break;

                case GameWSActionType.ERROR:
                    W("ERROR → " + message.Data);
                    break;

                default:
                    W("Unhandled non-gameplay event: " + message.Event);
                    break;
            }
        }

        /*──────────────────────────────*/
        /*  Handlers – Non-Gameplay     */
        /*──────────────────────────────*/
        private void OnClientGameStartInfo(JObject data)
        {
            D("OnClientGameStartInfo");
            if (data.TryGetValue("players", out JToken token))
            {
                GameManager.Instance.PlayerInfo = token.ToObject<List<RoomUserInfo>>();
            }
        }

        private void OnGameStartInfo(JObject data)
        {
            D("OnGameStartInfo");
            var info = data.ToObject<GameStartInfoData>();
            if (info != null)
                GameManager.Instance.InitGame(info.players);
        }

        private void OnInitEvent(JObject data)
        {
            D("OnInitEvent — parsing tiles…");

            if (!data.TryGetValue("hand", out JToken handTok))
            {
                W("InitEvent: 'hand' token missing.");
                return;
            }

            var initTiles = handTok.ToObject<List<int>>()
                                   .Select(i => (GameTile)i)
                                   .ToList();
            D($"  • initial hand count = {initTiles.Count}");

            GameTile? tsumoTile = null;
            if (data.TryGetValue("tsumo_tile", out JToken tsumoTok) &&
                tsumoTok.Type != JTokenType.Null)
            {
                tsumoTile = (GameTile)tsumoTok.ToObject<int>();
                initTiles.Remove(tsumoTile.Value); // tsumoTile 은 핸드에서 제외
                D($"  • tsumoTile = {tsumoTile}");
            }

            if (data.TryGetValue("players_score", out JToken scoreTok))
            {
                var scores = scoreTok.ToObject<List<int>>();
                GameManager.Instance.UpdatePlayerScores(scores);
                D("  • players_score updated.");
            }

            if (!TryParseFlowerParams(
                    data,
                    out var newTiles,
                    out var appliedFlowers,
                    out var flowerCounts))
            {
                W("flower phase 파라미터 누락 → RELOAD + INIT_FLOWER_OK 전송");

                // reload 요청
                GameWS.Instance.SendGameEvent(
                    action: GameWSActionType.REQUEST_RELOAD,
                    payload: new());

                // init flower ok 전송
                GameWS.Instance.SendGameEvent(
                    GameWSActionType.GAME_EVENT,
                    new
                    {
                        event_type = (int)GameEventType.INIT_FLOWER_OK,
                        data = new Dictionary<string, object>()
                    }
                );
            }

            D("OnInitEvent — coroutine queued.");
            StartCoroutine(GameManager.Instance.InitHandCoroutine(
                tiles: initTiles,
                tsumoTile: tsumoTile,
                newTiles: newTiles,
                appliedFlowers: appliedFlowers,
                flowerCounts: flowerCounts));
        }

        private bool TryParseFlowerParams(
            JObject data,
            out List<GameTile> newTiles,
            out List<GameTile> appliedFlowers,
            out List<int> flowerCounts)
        {
            newTiles = appliedFlowers = null;
            flowerCounts = null;

            if (data.TryGetValue("new_tiles", out var t0)
                && data.TryGetValue("applied_flowers", out var t1)
                && data.TryGetValue("flower_count", out var t2))
            {
                newTiles        = t0.ToObject<List<int>>().Select(i => (GameTile)i).ToList();
                appliedFlowers  = t1.ToObject<List<GameTile>>();
                flowerCounts    = t2.ToObject<List<int>>();

                D($"  • Flower params: newTiles={newTiles.Count}, " +
                  $"appliedFlowers={appliedFlowers.Count}, counts={string.Join(",", flowerCounts)}");
                return true;
            }
            return false;
        }

        private void OnEmojiBroadCast(JObject data)
        {
            string emojiKey = "";
            AbsoluteSeat seat = AbsoluteSeat.EAST;

            if (data.TryGetValue("emoji_key", out JToken emojiTok))
                emojiKey = emojiTok.ToObject<string>();

            if (data.TryGetValue("seat", out JToken seatTok))
                seat = (AbsoluteSeat)seatTok.ToObject<int>();

            D($"OnEmojiBroadCast → key={emojiKey}, seat={seat}");
            GameManager.Instance.emojiPanelController.OnServerEmoji(
                emojiKey: emojiKey,
                seat: seat);
        }

        private void OnEndGame(JObject data)
        {
            D("OnEndGame");

            if (data.TryGetValue("players_score", out JToken scoreTok))
            {
                var scores = scoreTok.ToObject<List<int>>();
                GameManager.Instance.UpdatePlayerScores(scores);
                D("  • players_score updated.");
            }
            GameManager.Instance.EndScorePopup();
        }
    }
}
