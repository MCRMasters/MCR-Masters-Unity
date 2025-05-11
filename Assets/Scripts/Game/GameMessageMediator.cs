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
        public static GameMessageMediator Instance { get; private set; }

        /*──────────────────────────────────────────────*/
        /*  Fields                                      */
        /*──────────────────────────────────────────────*/

        private readonly Queue<GameWSMessage> _messageQueue = new();
        private GameEventDispatcher _dispatcher;

        /// <summary>
        /// Dispatcher 로 넘길 WS ActionType 집합 (실제 플레이 이벤트).
        /// </summary>
        private static readonly HashSet<GameWSActionType> GameplayEvents = new()
        {
            GameWSActionType.PON,
            GameWSActionType.CHII,
            GameWSActionType.DAIMIN_KAN,
            GameWSActionType.SHOMIN_KAN,
            GameWSActionType.AN_KAN,
            GameWSActionType.DISCARD,
            GameWSActionType.DISCARD_ACTIONS,
            GameWSActionType.ROBBING_KONG_ACTIONS,
            GameWSActionType.FLOWER,
            GameWSActionType.TSUMO,
            GameWSActionType.TSUMO_ACTIONS,
            GameWSActionType.DRAW,
            GameWSActionType.HU_HAND
        };

        /*──────────────────────────────────────────────*/
        /*  Unity lifecycle                             */
        /*──────────────────────────────────────────────*/

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "GameScene")
            {
                _dispatcher = null;
                return;
            }
            _dispatcher = GameEventDispatcher.Instance;
            if (_dispatcher == null)
                Debug.Log($"[GameMessageMediator] Dispatcher not found in scene {scene.name}");
        }

        private void Start()
        {

            _dispatcher = GameEventDispatcher.Instance;
            if (_dispatcher == null)
                Debug.Log("[GameMessageMediator] GameEventDispatcher not found in the scene.");
        }

        private void Update()
        {
            if (IsGameSceneReady())
                ProcessQueue();
        }

        /*──────────────────────────────────────────────*/
        /*  Public API                                  */
        /*──────────────────────────────────────────────*/

        public void EnqueueMessage(GameWSMessage message)
        {
            if (message == null)
            {
                Debug.LogWarning("[GameMessageMediator] Tried to enqueue null message.");
                return;
            }
            _messageQueue.Enqueue(message);
        }

        /*──────────────────────────────────────────────*/
        /*  Internals                                   */
        /*──────────────────────────────────────────────*/

        private bool IsGameSceneReady()
            => SceneManager.GetActiveScene().name == "GameScene" && GameManager.Instance != null;

        private void ProcessQueue()
        {
            while (_messageQueue.Count > 0)
            {
                var msg = _messageQueue.Dequeue();

                // ① 게임플레이 이벤트면 Dispatcher 에게 위임
                if (_dispatcher != null && GameplayEvents.Contains(msg.Event))
                {
                    _dispatcher.OnWSMessage(msg);
                    continue;   // Mediator 에서 추가 처리 안 함
                }

                // ② 그 외(로비/점수/세션)… Mediator 내부 처리
                ProcessNonGameplayMessage(msg);
            }
        }

        private void ProcessNonGameplayMessage(GameWSMessage message)
        {
            switch (message.Event)
            {
                /*──────────────────────────────────*/
                /*  로비/게임 시작 관련              */
                /*──────────────────────────────────*/
                case GameWSActionType.CLIENT_GAME_START_INFO:
                    OnClientGameStartInfo(message.Data);
                    break;

                case GameWSActionType.GAME_START_INFO:
                    OnGameStartInfo(message.Data);
                    break;

                case GameWSActionType.INIT_EVENT:
                    OnInitEvent(message.Data);
                    break;

                /*──────────────────────────────────*/
                /*  이모티콘                         */
                /*──────────────────────────────────*/

                case GameWSActionType.EMOJI_BROADCAST:
                    OnEmojiBroadCast(message.Data);
                    break;

                /*──────────────────────────────────*/
                /*  게임 진행 보조                   */
                /*──────────────────────────────────*/
                case GameWSActionType.RELOAD_DATA:
                    GameManager.Instance.ReloadData(message.Data);
                    break;

                case GameWSActionType.UPDATE_ACTION_ID:
                    if (message.Data.TryGetValue("action_id", out JToken aidTok))
                        GameManager.Instance.UpdateActionId(aidTok.ToObject<int>());
                    break;

                case GameWSActionType.SET_TIMER:
                    GameManager.Instance.SetTimer(message.Data);
                    break;

                /*──────────────────────────────────*/
                /*  게임 종료                        */
                /*──────────────────────────────────*/
                case GameWSActionType.END_GAME:
                    OnEndGame(message.Data);
                    break;

                /*──────────────────────────────────*/
                /*  기타 ACK/ERR                    */
                /*──────────────────────────────────*/
                case GameWSActionType.SUCCESS:
                    Debug.Log("[GameMessageMediator] SUCCESS → " + message.Data);
                    break;
                case GameWSActionType.ERROR:
                    Debug.LogWarning("[GameMessageMediator] ERROR → " + message.Data);
                    break;

                default:
                    Debug.Log("[GameMessageMediator] Unhandled (non-gameplay) event: " + message.Event);
                    break;
            }
        }

        /*──────────────────────────────────────────────*/
        /*  Handlers – Non-Gameplay                     */
        /*──────────────────────────────────────────────*/

        private void OnClientGameStartInfo(JObject data)
        {
            if (data.TryGetValue("players", out JToken token))
            {
                GameManager.Instance.PlayerInfo = token.ToObject<List<RoomUserInfo>>();
            }
        }

        private void OnGameStartInfo(JObject data)
        {
            var info = data.ToObject<GameStartInfoData>();
            if (info != null)
                GameManager.Instance.InitGame(info.players);
        }

        private void OnInitEvent(JObject data)
        {
            if (!data.TryGetValue("hand", out JToken handTok))
                return;

            var initTiles = handTok.ToObject<List<int>>().Select(i => (GameTile)i).ToList();

            GameTile? tsumoTile = null;
            if (data.TryGetValue("tsumo_tile", out JToken tsumoTok) && tsumoTok.Type != JTokenType.Null)
            {
                tsumoTile = (GameTile)tsumoTok.ToObject<int>();
                initTiles.Remove(tsumoTile.Value); // tsumoTile 은 핸드에서 제외
            }

            if (data.TryGetValue("players_score", out JToken scoreTok))
            {
                var scores = scoreTok.ToObject<List<int>>();
                GameManager.Instance.UpdatePlayerScores(scores);
            }

            if (!TryParseFlowerParams(data, out var newTiles, out var appliedFlowers, out var flowerCounts))
            {
                Debug.LogWarning("[GameMessageMediator] flower phase 파라미터 누락.");
                // reload 요청 후
                GameWS.Instance.SendGameEvent(action: GameWSActionType.REQUEST_RELOAD, payload: new());
                // init flower ok 보내기
                GameWS.Instance.SendGameEvent(
                    GameWSActionType.GAME_EVENT,
                    new
                    {
                        event_type = (int)GameEventType.INIT_FLOWER_OK,
                        data = new Dictionary<string, object>()
                    }
                );
            }

            StartCoroutine(GameManager.Instance.InitHandCoroutine(tiles:initTiles, tsumoTile:tsumoTile, newTiles:newTiles, appliedFlowers:appliedFlowers, flowerCounts:flowerCounts));
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
                newTiles = t0.ToObject<List<int>>().Select(i => (GameTile)i).ToList();
                appliedFlowers = t1.ToObject<List<GameTile>>();
                flowerCounts = t2.ToObject<List<int>>();
                return true;
            }
            return false;
        }

        private void OnEmojiBroadCast(JObject data)
        {
            string emojiKey = "";
            AbsoluteSeat seat = AbsoluteSeat.EAST;
            if (data.TryGetValue("emoji_key", out JToken emojiTok))
            {
                emojiKey = emojiTok.ToObject<string>();
            }
            if (data.TryGetValue("seat", out JToken seatTok))
            {
                seat = (AbsoluteSeat)seatTok.ToObject<int>();
            }
            GameManager.Instance.EmojiPanelController.OnServerEmoji(emojiKey: emojiKey, seat: seat);
        }

        private void OnEndGame(JObject data)
        {
            if (data.TryGetValue("players_score", out JToken scoreTok))
            {
                var scores = scoreTok.ToObject<List<int>>();
                GameManager.Instance.UpdatePlayerScores(scores);
            }
            GameManager.Instance.EndScorePopup();
        }
    }
}
