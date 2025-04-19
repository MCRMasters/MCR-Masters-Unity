using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using System;
using MCRGame.Net;
using MCRGame.Common;
using System.Linq;

namespace MCRGame.Game
{
    public class GameMessageMediator : MonoBehaviour
    {
        public static GameMessageMediator Instance { get; private set; }
        // WebSocket에서 수신한 메시지를 저장할 큐
        private Queue<GameWSMessage> messageQueue = new Queue<GameWSMessage>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// WebSocket에서 수신한 메시지를 큐에 저장합니다.
        /// </summary>
        /// <param name="message">수신 메시지</param>
        public void EnqueueMessage(GameWSMessage message)
        {
            if (message == null)
            {
                Debug.LogWarning("[GameMessageMediator] null 메시지를 큐에 추가하려 했습니다.");
                return;
            }
            messageQueue.Enqueue(message);
        }

        private void Update()
        {
            // 현재 씬이 "GameScene"이고 GameManager 인스턴스가 존재할 경우 메시지 큐를 처리합니다.
            if (IsGameSceneReady())
            {
                ProcessQueue();
            }
        }

        /// <summary>
        /// 현재 활성화된 씬이 GameScene이고 GameManager가 준비되어 있는지 확인합니다.
        /// </summary>
        /// <returns>조건이 충족되면 true</returns>
        private bool IsGameSceneReady()
        {
            return SceneManager.GetActiveScene().name == "GameScene" && GameManager.Instance != null;
        }

        /// <summary>
        /// 큐에 저장된 메시지를 순차적으로 처리합니다.
        /// </summary>
        private void ProcessQueue()
        {
            while (messageQueue.Count > 0)
            {
                GameWSMessage message = messageQueue.Dequeue();
                ProcessMessage(message);
            }
        }

        /// <summary>
        /// 개별 메시지를 이벤트 타입에 따라 처리하고, 필요한 경우 GameManager에 전달합니다.
        /// </summary>
        /// <param name="message">WebSocket 메시지</param>
        private void ProcessMessage(GameWSMessage message)
        {
            switch (message.Event)
            {
                case GameWSActionType.INIT_EVENT:
                    Debug.Log("[GameMessageMediator] Init event received.");

                    // hand 파싱
                    if (message.Data.TryGetValue("hand", out JToken handToken))
                    {
                        var handInts = handToken.ToObject<List<int>>();
                        var initTiles = handInts.Select(i => (GameTile)i).ToList();

                        // tsumo_tile 파싱 (null 가능)
                        GameTile? tsumoTile = null;
                        if (message.Data.TryGetValue("tsumo_tile", out JToken tsumoToken)
                            && tsumoToken.Type != JTokenType.Null)
                        {
                            int tsumoInt = tsumoToken.ToObject<int>();
                            tsumoTile = (GameTile)tsumoInt;
                            Debug.Log($"[GameMessageMediator] Tsumo tile: {tsumoTile}");

                            // tsumoTile이 있으면 initTiles에서 제거
                            if (tsumoTile.HasValue)
                            {
                                bool removed = initTiles.Remove(tsumoTile.Value);
                                if (!removed)
                                {
                                    Debug.LogWarning($"[GameMessageMediator] initTiles에 {tsumoTile.Value}가 없어 제거하지 못했습니다.");
                                }
                            }
                        }

                        // players_score 파싱 (추가된 부분)
                        if (message.Data.TryGetValue("players_score", out JToken scoreToken))
                        {
                            var playersScores = scoreToken.ToObject<List<int>>();
                            Debug.Log($"[GameMessageMediator] Players scores: {string.Join(", ", playersScores)}");
                            GameManager.Instance.UpdatePlayerScores(playersScores);
                        }
                        else
                        {
                            Debug.LogWarning("[GameMessageMediator] INIT_EVENT 메시지에 players_score 필드가 없습니다.");
                        }

                        GameManager.Instance.InitHandFromMessage(initTiles, tsumoTile);
                    }
                    break;

                case GameWSActionType.GAME_START_INFO:
                    Debug.Log("[GameMessageMediator] UPDATE_ACTION_ID event received.");
                    Debug.Log("[GameMessageMediator] Data: " + message.Data.ToString());
                    GameStartInfoData startInfo = message.Data.ToObject<GameStartInfoData>();
                    if (startInfo != null)
                    {
                        Debug.Log("[GameMessageMediator] Updating GameManager with game start info.");
                        GameManager.Instance.InitGame(startInfo.players);
                    }
                    break;


                case GameWSActionType.UPDATE_ACTION_ID:
                    Debug.Log("[GameMessageMediator] GAME_START_INFO event received.");
                    Debug.Log("[GameMessageMediator] Data: " + message.Data.ToString());
                    if (message.Data.TryGetValue("action_id", out JToken actionIdToken))
                    {
                        int actionId = actionIdToken.ToObject<int>();
                        GameManager.Instance.UpdateActionId(actionId);
                    }
                    else
                    {
                        Debug.LogWarning("[GameMessageMediator] UPDATE_ACTION_ID: applied_flowers 키가 없습니다.");
                    }
                    break;
                case GameWSActionType.SET_TIMER:
                    Debug.Log("[GameMessageMediator] set timer event received.");
                    GameManager.Instance.SetTimer(message.Data);
                    break;
                case GameWSActionType.TSUMO_ACTIONS:
                    Debug.Log("[GameMessageMediator] Tsumo actions received.");
                    // message.Data는 JObject이므로 바로 넘겨줌
                    GameManager.Instance.ProcessTsumoActions(message.Data);
                    break;
                case GameWSActionType.DISCARD_ACTIONS:
                case GameWSActionType.ROBBING_KONG_ACTIONS:
                    Debug.Log($"[GameMessageMediator] {message.Event} received.");
                    GameManager.Instance.ProcessDiscardActions(message.Data);
                    break;
                case GameWSActionType.DISCARD:
                    Debug.Log("[GameMessageMediator] Discard Confirmed.");
                    GameManager.Instance.ConfirmDiscard(message.Data);
                    break;
                case GameWSActionType.FLOWER:
                    Debug.Log("[GameMessageMediator] Flower Confirmed.");
                    StartCoroutine(GameManager.Instance.ConfirmFlower(message.Data));
                    break;
                case GameWSActionType.PON:
                case GameWSActionType.CHII:
                case GameWSActionType.DAIMIN_KAN:
                case GameWSActionType.SHOMIN_KAN:
                case GameWSActionType.AN_KAN:
                    Debug.Log($"[GameMessageMediator] {message.Event} received");
                    GameManager.Instance.ConfirmCallBlock(message.Data);
                    break;
                case GameWSActionType.TSUMO:
                    Debug.Log("[GameMessageMediator] broadcasted enemy tsumo");
                    GameManager.Instance.ConfirmTsumo(message.Data);
                    break;
                case GameWSActionType.INIT_FLOWER_REPLACEMENT:
                    Debug.Log("[GameMessageMediator] INIT_FLOWER_REPLACEMENT event received.");

                    List<GameTile> newTiles = null;
                    List<GameTile> appliedFlowers = null;
                    List<int> flowerCounts = null;

                    if (message.Data.TryGetValue("new_tiles", out JToken tilesToken))
                    {
                        List<int> newTilesInts = tilesToken.ToObject<List<int>>();
                        newTiles = newTilesInts.Select(i => (GameTile)i).ToList();
                        Debug.Log("[GameMessageMediator] New flower replacement tiles: " + string.Join(", ", newTiles));
                    }
                    else
                    {
                        Debug.LogWarning("[GameMessageMediator] INIT_FLOWER_REPLACEMENT: new_tiles 키가 없습니다.");
                    }

                    if (message.Data.TryGetValue("applied_flowers", out JToken appliedFlowersToken))
                    {
                        appliedFlowers = appliedFlowersToken.ToObject<List<GameTile>>();
                        Debug.Log("[GameMessageMediator] Applied flower tiles: " + string.Join(", ", appliedFlowers));
                    }
                    else
                    {
                        Debug.LogWarning("[GameMessageMediator] INIT_FLOWER_REPLACEMENT: applied_flowers 키가 없습니다.");
                    }

                    if (message.Data.TryGetValue("flower_count", out JToken countToken))
                    {
                        flowerCounts = countToken.ToObject<List<int>>();
                        Debug.Log("[GameMessageMediator] Flower counts for each hand: " + string.Join(", ", flowerCounts));
                    }
                    else
                    {
                        Debug.LogWarning("[GameMessageMediator] INIT_FLOWER_REPLACEMENT: flower_count 키가 없습니다.");
                    }

                    if (newTiles != null && appliedFlowers != null && flowerCounts != null)
                    {
                        // GameManager의 화패 교체 이벤트 시작 (코루틴 내부에서 전체 이벤트 연출 진행)
                        StartCoroutine(GameManager.Instance.StartFlowerReplacement(newTiles, appliedFlowers, flowerCounts));
                    }
                    break;

                case GameWSActionType.SUCCESS:
                    Debug.Log("[GameMessageMediator] Success event received.");
                    Debug.Log("[GameMessageMediator] Success data: " + message.Data.ToString());
                    // 필요 시 성공 메시지를 UI 업데이트 등으로 전달
                    break;

                case GameWSActionType.ERROR:
                    Debug.Log("[GameMessageMediator] Error event received.");
                    Debug.Log("[GameMessageMediator] Error data: " + message.Data.ToString());
                    // 필요 시 에러 메시지 처리
                    break;
                case GameWSActionType.DRAW:
                    Debug.Log("[GameMessageMediator] DRAW event received");
                    List<List<GameTile>> anKanInfo2 = new List<List<GameTile>>();
                    if (message.Data.TryGetValue("an_kan_infos", out JToken anKanToken2))
                    {
                        var infoIntsList = anKanToken2.ToObject<List<List<int>>>();
                        foreach (var listInt in infoIntsList)
                        {
                            anKanInfo2.Add(listInt.Select(i => (GameTile)i).ToList());
                        }
                        Debug.Log($"[GameMessageMediator] an_kan_infos count: {anKanInfo2.Count}");
                    }
                    StartCoroutine(GameManager.Instance.ProcessDraw(anKanInfo2));
                    break;
                case GameWSActionType.HU_HAND:
                    Debug.Log("[GameMessageMediator] HU_HAND event received.");
                    try
                    {
                        // 핸드 타일 파싱
                        List<GameTile> handTiles = new List<GameTile>();
                        if (message.Data.TryGetValue("hand", out JToken winHandToken))
                        {
                            var handInts = winHandToken.ToObject<List<int>>();
                            handTiles = handInts.Select(i => (GameTile)i).ToList();
                            Debug.Log($"[GameMessageMediator] Hu hand tiles: {string.Join(", ", handTiles)}");
                        }

                        // 콜 블록 파싱
                        List<CallBlockData> callBlocks = new List<CallBlockData>();
                        if (message.Data.TryGetValue("call_blocks", out JToken blocksToken))
                        {
                            callBlocks = blocksToken.ToObject<List<CallBlockData>>();
                            Debug.Log($"[GameMessageMediator] Call blocks count: {callBlocks.Count}");
                        }

                        // 점수 결과 파싱
                        ScoreResult scoreResult = null;
                        if (message.Data.TryGetValue("score_result", out JToken scoreToken))
                        {
                            scoreResult = scoreToken.ToObject<ScoreResult>();
                            Debug.Log($"[GameMessageMediator] Score result: {scoreResult}");
                        }

                        // 플레이어 좌석 파싱
                        AbsoluteSeat playerSeat = 0;
                        if (message.Data.TryGetValue("player_seat", out JToken seatToken))
                        {
                            playerSeat = seatToken.ToObject<AbsoluteSeat>();
                            Debug.Log($"[GameMessageMediator] Hu player seat: {playerSeat}");
                        }

                        AbsoluteSeat currentPlayerSeat = 0;
                        if (message.Data.TryGetValue("current_player_seat", out JToken currSeatToken))
                        {
                            currentPlayerSeat = currSeatToken.ToObject<AbsoluteSeat>();
                            Debug.Log($"[GameMessageMediator] Current player seat: {currentPlayerSeat}");
                        }

                        int flowerCount = 0;
                        if (message.Data.TryGetValue("flower_count", out JToken flowerCountToken))
                        {
                            flowerCount = flowerCountToken.ToObject<int>();
                            Debug.Log($"[GameMessageMediator] flower count: {flowerCount}");
                        }

                        // tsumo tile 파싱
                        GameTile? tsumoTile = null;
                        if (message.Data.TryGetValue("tsumo_tile", out JToken tsumoTileToken) && tsumoTileToken.Type != JTokenType.Null)
                        {
                            int? tsumoInt = tsumoTileToken.ToObject<int?>();
                            if (tsumoInt.HasValue)
                            {
                                tsumoTile = (GameTile)tsumoInt.Value;
                                Debug.Log($"[GameMessageMediator] Hu tsumo tile: {tsumoTile}");
                            }
                            else
                            {
                                tsumoTile = null;
                                Debug.Log("[GameMessageMediator] tsumo_tile 값이 null입니다.");
                            }
                        }
                        else
                        {
                            tsumoTile = null;
                            Debug.Log("[GameMessageMediator] tsumo_tile 키가 없거나 null입니다.");
                        }
                        GameTile winningTile = GameTile.F0;
                        if (message.Data.TryGetValue("winning_tile", out JToken winningTileToken))
                        {
                            int winningInt = winningTileToken.ToObject<int>();
                            winningTile = (GameTile)winningInt;
                            Debug.Log($"[GameMessageMediator] Hu winning tile: {winningTile}");
                        }
                        
                        // an_kan_infos 파싱
                        List<List<GameTile>> anKanInfos = new List<List<GameTile>>();
                        if (message.Data.TryGetValue("an_kan_infos", out JToken anKanToken))
                        {
                            var infoIntsList = anKanToken.ToObject<List<List<int>>>();
                            foreach (var listInt in infoIntsList)
                            {
                                anKanInfos.Add(listInt.Select(i => (GameTile)i).ToList());
                            }
                            Debug.Log($"[GameMessageMediator] an_kan_infos count: {anKanInfos.Count}");
                        }

                        // GameManager로 전달 (필요한 모든 정보 포함)
                        if (handTiles != null && callBlocks != null && scoreResult != null)
                        {
                            StartCoroutine(GameManager.Instance.ProcessHuHand(
                                handTiles,
                                callBlocks,
                                scoreResult,
                                playerSeat,
                                currentPlayerSeat,
                                flowerCount,
                                tsumoTile,
                                anKanInfos,
                                winningTile
                            ));
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[GameMessageMediator] HU_HAND 처리 오류: {ex.Message}");
                    }
                    break;


                default:
                    Debug.Log("[GameMessageMediator] Unhandled event: " + message.Event);
                    break;
            }
        }
    }


}