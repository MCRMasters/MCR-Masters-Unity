using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

using MCRGame.Net;      // GameWSMessage, GameWSActionType
using MCRGame.Common;
using UnityEngine.SceneManagement;   // GameEventType

namespace MCRGame.Game.Events
{
    /*──────────────────────────────────────────────*/
    /*  1. 핸들러 인터페이스                       */
    /*──────────────────────────────────────────────*/
    public interface IGameEventHandler
    {
        void Handle(GameEventType evt, JObject data);
    }

    /*──────────────────────────────────────────────*/
    /*  2. Dispatcher (Singleton, per-scene)        */
    /*──────────────────────────────────────────────*/

    [RequireComponent(typeof(CallBlockHandler))]
    [RequireComponent(typeof(TsumoHandler))]
    [RequireComponent(typeof(FlowerHandler))]
    [RequireComponent(typeof(DiscardHandler))]
    [RequireComponent(typeof(HuHandler))]
    [RequireComponent(typeof(DrawHandler))]
    public class GameEventDispatcher : MonoBehaviour
    {
        /*────────────── Singleton ──────────────*/
        public static GameEventDispatcher Instance { get; private set; }

        private void Awake()
        {
            // Singleton: 씬 내 중복 방지 (DontDestroyOnLoad 사용 X, 씬마다 새 인스턴스)
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            RegisterHandlers();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "GameScene")
            {
                ResetState();
                Debug.Log("[GameEventDispatcher] ResetState 호출");
            }
        }

        /// <summary>
        /// 씬 재진입 시 핸들러 테이블을 비우고 다시 등록합니다.
        /// </summary>
        private void ResetState()
        {
            _handlers.Clear();
            RegisterHandlers();
        }


        /*────────────── Handler Table ───────────*/
        private readonly Dictionary<GameEventType, IGameEventHandler> _handlers = new();

        private void RegisterHandlers()
        {
            foreach (var h in GetComponents<IGameEventHandler>())
            {
                switch (h)
                {
                    case CallBlockHandler cb:
                        foreach (var evt in new[] { GameEventType.CHII, GameEventType.PON, GameEventType.DAIMIN_KAN, GameEventType.SHOMIN_KAN, GameEventType.AN_KAN })
                            _handlers[evt] = cb;
                        break;
                    case TsumoHandler t:
                        _handlers[GameEventType.TSUMO] = t;
                        break;
                    case FlowerHandler f:
                        _handlers[GameEventType.FLOWER] = f;
                        break;
                    case DiscardHandler d:
                        _handlers[GameEventType.DISCARD] = d;
                        _handlers[GameEventType.ROBBING_KONG] = d;
                        break;
                    case HuHandler h1:
                        _handlers[GameEventType.HU] = h1;
                        break;
                    case DrawHandler dw:
                        _handlers[GameEventType.NEXT_ROUND_CONFIRM] = dw;
                        break;
                }
            }
        }

        /*────────────── WS Action <-> GameEvent ───────────*/
        private static readonly Dictionary<GameWSActionType, GameEventType> Map = new()
        {
            { GameWSActionType.PON,                 GameEventType.PON },
            { GameWSActionType.CHII,                GameEventType.CHII },
            { GameWSActionType.DAIMIN_KAN,          GameEventType.DAIMIN_KAN },
            { GameWSActionType.SHOMIN_KAN,          GameEventType.SHOMIN_KAN },
            { GameWSActionType.AN_KAN,              GameEventType.AN_KAN },

            { GameWSActionType.TSUMO,               GameEventType.TSUMO },
            { GameWSActionType.TSUMO_ACTIONS,       GameEventType.TSUMO },

            { GameWSActionType.FLOWER,              GameEventType.FLOWER },

            { GameWSActionType.DISCARD,             GameEventType.DISCARD },
            { GameWSActionType.DISCARD_ACTIONS,     GameEventType.DISCARD },
            { GameWSActionType.ROBBING_KONG_ACTIONS,GameEventType.ROBBING_KONG },

            { GameWSActionType.HU_HAND,             GameEventType.HU },
            { GameWSActionType.DRAW,                GameEventType.NEXT_ROUND_CONFIRM }, // DRAW 값 없음 → 임시 매핑
        };

        /*────────────── Public API ──────────────*/
        public void OnWSMessage(GameWSMessage msg)
        {
            if (!Map.TryGetValue(msg.Event, out var evt))
            {
                Debug.LogWarning($"[Dispatcher] 매핑되지 않은 WS Action: {msg.Event}");
                return;
            }
            if (_handlers.TryGetValue(evt, out var handler) && handler != null)
            {
                try
                {
                    handler.Handle(evt, msg.Data);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Dispatcher] {evt} 처리 중 예외: {ex}");
                }
            }
            else
            {
                Debug.LogWarning($"[Dispatcher] {evt} 담당 핸들러가 등록되지 않았습니다.");
            }
        }
    }
}
