using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using System;
using MCRGame.Net;
using MCRGame.Common;

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
                    if (message.Data["hand"] != null)
                    {
                        Debug.Log("[GameMessageMediator] Hand: " + message.Data["hand"].ToString());
                        List<int> handInts = message.Data["hand"].ToObject<List<int>>();
                        List<GameTile> initTiles = new List<GameTile>();
                        foreach (int i in handInts)
                        {
                            // int 값을 GameTile 열거형으로 캐스팅
                            initTiles.Add((GameTile)i);
                        }
                        // GameManager를 통해 손 초기화 처리
                        GameManager.Instance.InitHandFromMessage(initTiles);
                    }
                    break;


                case GameWSActionType.GAME_START_INFO:
                    Debug.Log("[GameMessageMediator] GAME_START_INFO event received.");
                    Debug.Log("[GameMessageMediator] Data: " + message.Data.ToString());
                    GameStartInfoData startInfo = message.Data.ToObject<GameStartInfoData>();
                    if (startInfo != null)
                    {
                        Debug.Log("[GameMessageMediator] Updating GameManager with game start info.");
                        GameManager.Instance.SetPlayers(startInfo.players);
                    }
                    break;

                case GameWSActionType.DISCARD:
                    Debug.Log("[GameMessageMediator] Discard event received.");
                    break;

                case GameWSActionType.TSUMO_ACTIONS:
                    Debug.Log("[GameMessageMediator] Tsumo actions received.");
                    break;

                case GameWSActionType.INIT_FLOWER_REPLACEMENT:
                    Debug.Log("[GameMessageMediator] INIT_FLOWER_REPLACEMENT event received.");
                    if (message.Data.TryGetValue("new_tiles", out JToken tilesToken))
                    {
                        List<int> newTiles = tilesToken.ToObject<List<int>>();
                        Debug.Log("[GameMessageMediator] New flower replacement tiles: " + string.Join(", ", newTiles));
                    }
                    else
                    {
                        Debug.LogWarning("[GameMessageMediator] INIT_FLOWER_REPLACEMENT: new_tiles 키가 없습니다.");
                    }
                    if (message.Data.TryGetValue("applied_flowers", out JToken appliedFlowersToken))
                    {
                        List<GameTile> appliedFlowers = appliedFlowersToken.ToObject<List<GameTile>>();
                        Debug.Log("[GameMessageMediator] Applied flower tiles: " + string.Join(", ", appliedFlowers));
                    }
                    else
                    {
                        Debug.LogWarning("[GameMessageMediator] INIT_FLOWER_REPLACEMENT: applied_flowers 키가 없습니다.");
                    }
                    if (message.Data.TryGetValue("flower_count", out JToken countToken))
                    {
                        List<GameTile> flowerCount = countToken.ToObject<List<GameTile>>();
                        Debug.Log("[GameMessageMediator] Flower counts for each hand: " + string.Join(", ", flowerCount));
                    }
                    else
                    {
                        Debug.LogWarning("[GameMessageMediator] INIT_FLOWER_REPLACEMENT: flower_count 키가 없습니다.");
                    }
                    break;

                default:
                    Debug.Log("[GameMessageMediator] Unhandled event: " + message.Event);
                    break;
            }
        }
    }
}
