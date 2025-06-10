using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TMPro;

using MCRGame.Common;
using MCRGame.UI;
using MCRGame.Net;
using MCRGame.View;
using MCRGame.Audio;
using DG.Tweening;


namespace MCRGame.Game
{
    public partial class GameManager : MonoBehaviour
    {
        /*──────────────────────────────────────────────────*/
        /*           ⚙ CORE : 필드 / Awake-Update / 유틸        */
        /*──────────────────────────────────────────────────*/
        #region ⚙ CORE

        /**************** ① Singleton ****************/
        public static GameManager Instance { get; private set; }


        /**************** ② 직렬화/공개 필드 ****************/
        #region ▶ Serialized & Public Fields

        /* ---------- 게임 데이터 ---------- */
        public List<Player> Players { get; private set; }
        public List<RoomUserInfo> PlayerInfo { get; set; }
        public AbsoluteSeat MySeat { get; private set; }
        public RelativeSeat CurrentTurnSeat { get; private set; }
        public Round CurrentRound { get; private set; }

        /* ---------- Manager refs ---------- */
        [SerializeField] private GameHandManager gameHandManager;
        public GameHandManager GameHandManager => gameHandManager;
        public GameHand GameHand => gameHandManager != null ? gameHandManager.GameHandPublic : null;

        [SerializeField] private DiscardManager discardManager;

        [SerializeField] private EmojiPanelController emojiPanelController;
        public EmojiPanelController EmojiPanelController => emojiPanelController;

        /* ---------- 글로벌 상태 플래그 ---------- */
        public Language currentLanguage = Language.Korean;
        public bool IsFlowerConfirming = false;
        public bool IsRightClickTsumogiri;

        public bool isGameStarted = false;
        public bool IsMyTurn;
        public bool isInitHandDone = false;
        public bool isActionUIActive = false;
        public bool isAfterTsumoAction = false;
        public bool CanClick = false;

        private bool autoHuFlag;
        public bool AutoHuFlag
        {
            get => autoHuFlag;
            set
            {
                if (autoHuFlag == value) return;
                autoHuFlag = value;
                OnAutoHuFlagChanged?.Invoke(autoHuFlag);
            }
        }
        public event Action<bool> OnAutoHuFlagChanged;

        private bool preventCallFlag;
        public bool PreventCallFlag
        {
            get => preventCallFlag;
            set
            {
                if (preventCallFlag == value) return;
                preventCallFlag = value;
                OnPreventCallFlagChanged?.Invoke(preventCallFlag);
            }
        }
        public event Action<bool> OnPreventCallFlagChanged;

        private bool autoFlowerFlag;
        public bool AutoFlowerFlag
        {
            get => autoFlowerFlag;
            set
            {
                if (autoFlowerFlag == value) return;
                autoFlowerFlag = value;
                OnAutoFlowerFlagChanged?.Invoke(autoFlowerFlag);
            }
        }
        public event Action<bool> OnAutoFlowerFlagChanged;
        private bool tsumogiriFlag;
        public bool TsumogiriFlag
        {
            get => tsumogiriFlag;
            set
            {
                if (tsumogiriFlag == value) return;
                tsumogiriFlag = value;
                OnTsumogiriFlagChanged?.Invoke(tsumogiriFlag);
            }
        }
        public event Action<bool> OnTsumogiriFlagChanged;


        // 자동 후(default false)
        public bool IsAutoHuDefault { get; set; } = false;
        // 자동 꽃(default true)
        public bool IsAutoFlowerDefault { get; set; } = true;


        public GameTile? NowHoverTile = null;
        public TileManager NowHoverSource;

        /* ---------- 타일/도움 dict ---------- */
        public Dictionary<GameTile, List<TenpaiAssistEntry>> tenpaiAssistDict = new();
        public List<TenpaiAssistEntry> NowTenpaiAssistList = new();
        // public List<GameWSMessage> pendingFlowerReplacement = new();
        public readonly Queue<GameWSMessage> flowerQueue = new Queue<GameWSMessage>();

        /* ---------- 좌석 매핑 ---------- */
        public Dictionary<AbsoluteSeat, int> seatToPlayerIndex;
        private Dictionary<int, AbsoluteSeat> playerIndexToSeat;
        private Dictionary<string, int> playerUidToIndex;

        /* ---------- Hand & CallBlock Field ---------- */
        [SerializeField] public Hand3DField[] playersHand3DFields;
        [SerializeField] private CallBlockField[] callBlockFields;

        public CallBlockField[] CallBlockFields => callBlockFields;

        /* ---------- UI Refs ---------- */
        [SerializeField] private TextMeshProUGUI leftTilesText;
        [SerializeField] private TextMeshProUGUI currentRoundText;

        [Header("Camera")][SerializeField] private CameraResultAnimator cameraResultAnimator;

        [Header("Score Label Texts (RelativeSeat 순서)")]
        [SerializeField] private TextMeshProUGUI scoreText_Self;
        [SerializeField] private TextMeshProUGUI scoreText_Shimo;
        [SerializeField] private TextMeshProUGUI scoreText_Toi;
        [SerializeField] private TextMeshProUGUI scoreText_Kami;

        [Header("Score Colors")]
        [SerializeField] private Color positiveScoreColor = new(0x5F / 255f, 0xD8 / 255f, 0xA2 / 255f);
        [SerializeField] private Color zeroScoreColor = new(0xB0 / 255f, 0xB0 / 255f, 0xB0 / 255f);
        [SerializeField] private Color negativeScoreColor = new(0xE2 / 255f, 0x78 / 255f, 0x78 / 255f);
        public Color PositiveScoreColor => positiveScoreColor;
        public Color ZeroScoreColor => zeroScoreColor;
        public Color NegativeScoreColor => negativeScoreColor;

        [Header("Wind Label Texts (RelativeSeat 순서)")]
        [SerializeField] private TextMeshProUGUI windText_Self;
        [SerializeField] private TextMeshProUGUI windText_Shimo;
        [SerializeField] private TextMeshProUGUI windText_Toi;
        [SerializeField] private TextMeshProUGUI windText_Kami;
        [Header("Wind Colors")]
        [SerializeField] private Color eastWindColor = new(0.7961f, 0f, 0f);
        [SerializeField] private Color otherWindColor = Color.black;

        [Header("Profile UI (SELF, SHIMO, TOI, KAMI)")]
        [SerializeField] private Image[] profileImages = new Image[4];
        [SerializeField] private Image[] profileFrameImages = new Image[4];
        [SerializeField] private Image[] BlinkTurnImages = new Image[4];
        [SerializeField] private TextMeshProUGUI[] nicknameTexts = new TextMeshProUGUI[4];
        [SerializeField] private Image[] flowerImages = new Image[4];
        [SerializeField] private TextMeshProUGUI[] flowerCountTexts = new TextMeshProUGUI[4];

        [SerializeField] private Sprite FlowerIcon_White;
        [SerializeField] private Sprite FlowerIcon_Yellow;
        [SerializeField] private Sprite FlowerIcon_Red;

        public Dictionary<RelativeSeat, int> flowerCountMap = new();

        [Header("Effect Prefabs")]
        [SerializeField] private GameObject flowerPhaseEffectPrefab;
        [SerializeField] private GameObject roundStartEffectPrefab;

        [Header("Default Profile Frame/Image")]
        [SerializeField] private Sprite defaultFrameSprite;
        [SerializeField] private Sprite defaultProfileImageSprite;

        [Header("Tsumo Action UI")]
        [SerializeField] private RectTransform actionButtonPanel;
        [SerializeField] private GameObject actionButtonPrefab;
        [SerializeField] private Sprite skipButtonSprite;
        [SerializeField] private Sprite chiiButtonSprite;
        [SerializeField] private Sprite ponButtonSprite;
        [SerializeField] private Sprite kanButtonSprite;
        [SerializeField] private Sprite huButtonSprite;
        [SerializeField] private Sprite flowerButtonSprite;
        [SerializeField] private GameObject backButtonPrefab;

        [Header("Timer UI")]
        [SerializeField] private TextMeshProUGUI timerText;
        private float remainingTime;
        private int currentActionId;

        [SerializeField] private GameObject EndScorePopupPrefab;

        private GameObject additionalChoicesContainer;
        private int prevBlinkSeat = -1;

        /* ---------- 상수 ---------- */
        public const int MAX_TILES = 144;
        public const int MAX_PLAYERS = 4;
        private int leftTiles;

        #endregion /* ▶ Serialized & Public Fields */


        /**************** ③ Unity Lifecycle ****************/
        #region ▶ Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            SetUIActive(false);
            ClearActionUI();
            isGameStarted = false;
        }

        private void Update()
        {
            UpdateTimerText();
        }

        #endregion


        /**************** ④ Helper 메서드 ****************/
        #region ▶ Helpers

        private void moveTurn(RelativeSeat seat)
        {
            if (seat == RelativeSeat.SELF) { IsMyTurn = true; CanClick = true; }
            else { IsMyTurn = false; CanClick = false; }
            CurrentTurnSeat = seat;
            UpdateCurrentTurnEffect();
            Debug.Log($"Current turn: {CurrentTurnSeat}");
        }

        private Dictionary<GameTile, List<TenpaiAssistEntry>> BuildTenpaiAssistDict(JObject outer)
        {
            var dict = new Dictionary<GameTile, List<TenpaiAssistEntry>>();
            foreach (var discardProp in outer.Properties())
            {
                GameTile discardTile = (GameTile)int.Parse(discardProp.Name);
                var inner = (JObject)discardProp.Value;

                var list = new List<TenpaiAssistEntry>();
                foreach (var tenpaiProp in inner.Properties())
                {
                    GameTile tenpaiTile = (GameTile)int.Parse(tenpaiProp.Name);
                    var arr = (JArray)tenpaiProp.Value;
                    list.Add(new TenpaiAssistEntry
                    {
                        TenpaiTile = tenpaiTile,
                        TsumoResult = arr[0].ToObject<ScoreResult>(),
                        DiscardResult = arr[1].ToObject<ScoreResult>()
                    });
                }
                dict[discardTile] = list;
            }
            return dict;
        }

        #endregion


        /**************** ⑤ Deal Table (상수) ****************/
        private static readonly AbsoluteSeat[][] DEAL_TABLE =
        {
            new[]{ AbsoluteSeat.EAST,  AbsoluteSeat.SOUTH, AbsoluteSeat.WEST,  AbsoluteSeat.NORTH }, //1
            new[]{ AbsoluteSeat.NORTH, AbsoluteSeat.EAST,  AbsoluteSeat.SOUTH, AbsoluteSeat.WEST  }, //2
            new[]{ AbsoluteSeat.WEST,  AbsoluteSeat.NORTH, AbsoluteSeat.EAST,  AbsoluteSeat.SOUTH }, //3
            new[]{ AbsoluteSeat.SOUTH, AbsoluteSeat.WEST,  AbsoluteSeat.NORTH, AbsoluteSeat.EAST  }, //4
            new[]{ AbsoluteSeat.SOUTH, AbsoluteSeat.EAST,  AbsoluteSeat.NORTH, AbsoluteSeat.WEST  }, //5
            new[]{ AbsoluteSeat.EAST,  AbsoluteSeat.NORTH, AbsoluteSeat.WEST,  AbsoluteSeat.SOUTH }, //6
            new[]{ AbsoluteSeat.NORTH, AbsoluteSeat.WEST,  AbsoluteSeat.SOUTH, AbsoluteSeat.EAST  }, //7
            new[]{ AbsoluteSeat.WEST,  AbsoluteSeat.SOUTH, AbsoluteSeat.EAST,  AbsoluteSeat.NORTH }, //8
            new[]{ AbsoluteSeat.NORTH, AbsoluteSeat.WEST,  AbsoluteSeat.EAST,  AbsoluteSeat.SOUTH }, //9
            new[]{ AbsoluteSeat.WEST,  AbsoluteSeat.SOUTH, AbsoluteSeat.NORTH, AbsoluteSeat.EAST  }, //10
            new[]{ AbsoluteSeat.SOUTH, AbsoluteSeat.EAST,  AbsoluteSeat.WEST,  AbsoluteSeat.NORTH }, //11
            new[]{ AbsoluteSeat.EAST,  AbsoluteSeat.NORTH, AbsoluteSeat.SOUTH, AbsoluteSeat.WEST  }, //12
            new[]{ AbsoluteSeat.WEST,  AbsoluteSeat.NORTH, AbsoluteSeat.SOUTH, AbsoluteSeat.EAST  }, //13
            new[]{ AbsoluteSeat.SOUTH, AbsoluteSeat.WEST,  AbsoluteSeat.EAST,  AbsoluteSeat.NORTH }, //14
            new[]{ AbsoluteSeat.EAST,  AbsoluteSeat.SOUTH, AbsoluteSeat.NORTH, AbsoluteSeat.WEST  }, //15
            new[]{ AbsoluteSeat.NORTH, AbsoluteSeat.EAST,  AbsoluteSeat.WEST,  AbsoluteSeat.SOUTH }  //16
        };

        #endregion /* ⚙ CORE */
    }
}
