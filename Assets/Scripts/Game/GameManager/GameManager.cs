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
using UnityEngine.SceneManagement;


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
        public AbsoluteSeat ViewSeat { get; set; } = AbsoluteSeat.EAST;
        public AbsoluteSeat ReferenceSeat => IsSpectator ? ViewSeat : MySeat;
        public RelativeSeat CurrentTurnSeat { get; private set; }
        public Round CurrentRound { get; private set; }

        /* ---------- Post-Processing ---------- */
        [SerializeField] private GameObject bloomPrefab;   // 에디터에서 할당할 Bloom Prefab
        private GameObject bloomInstance;                  // 런타임에 생성된 인스턴스

        /* ---------- Manager refs ---------- */
        [SerializeField] private GameHandManager gameHandManager;
        public GameHandManager GameHandManager => gameHandManager;
        public GameHand GameHand => gameHandManager != null ? gameHandManager.GameHandPublic : null;
        [SerializeField] private GameObject discardManagerPrefab;
        [SerializeField] private GameObject emojiPanelPrefab;
        [SerializeField] private GameObject settingsUIPrefab;

        public DiscardManager discardManager;
        public EmojiPanelController emojiPanelController;
        public SettingsUIManager settingsUIManager;


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
        public bool IsSpectator = false;

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

        public bool IsSceneReady { get; private set; } = false;

        public GameTile? NowHoverTile = null;
        public TileManager NowHoverSource;

        public GameObject NowFocus3DTile = null;


        // ▶ 씬 전용 Prefab 할당 슬롯
        [Header("Scene Prefabs")]
        [SerializeField] private GameObject main2DCanvasPrefab;
        [SerializeField] private GameObject prefab3DField;

        // ▶ 런타임에 Instantiate 된 인스턴스
        public GameObject _canvasInstance;
        private GameObject _field3DInstance;



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
        // Store the default scale of each flower count container so we can reset it
        private Vector3[] flowerCountBaseScales = new Vector3[4];
        // Keep track of running flower count DOTween sequences per seat
        private Tween[] flowerCountTweens = new Tween[4];

        [SerializeField] private Sprite FlowerIcon_White;
        [SerializeField] private Sprite FlowerIcon_Yellow;
        [SerializeField] private Sprite FlowerIcon_Red;

        public Dictionary<RelativeSeat, int> flowerCountMap = new();

        [Header("Effect Prefabs")]
        [SerializeField] private GameObject flowerPhaseEffectPrefab;
        [SerializeField] private GameObject roundStartEffectPrefab;

        [Header("Winning Effect")]
        [SerializeField] private string winningEffectName = "WaterColumn";
        public string WinningEffectName => winningEffectName;

        [Header("Default Profile Frame/Image")]
        [SerializeField] private Sprite defaultFrameSprite;
        [SerializeField] private Sprite defaultProfileImageSprite;

        [Header("Tsumo Action UI")]
        private RectTransform actionButtonPanel;
        [SerializeField] private GameObject actionButtonPrefab;
        [SerializeField] private Sprite skipButtonSprite;
        [SerializeField] private Sprite chiiButtonSprite;
        [SerializeField] private Sprite ponButtonSprite;
        [SerializeField] private Sprite kanButtonSprite;
        [SerializeField] private Sprite huButtonSprite;
        [SerializeField] private Sprite flowerButtonSprite;
        [SerializeField] private GameObject backButtonPrefab;

        [Header("Timer UI")]
        private TextMeshProUGUI timerText;
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

            /* 씬 전환 시 파괴되지 않도록 설정 */
            DontDestroyOnLoad(gameObject);

            SetUIActive(false);
            ClearActionUI();
            isGameStarted = false;
        }
        private void Update()
        {
            // 현재 씬이 "GameScene"이 아닐 땐 타이머 업데이트 스킵
            if (SceneManager.GetActiveScene().name != "GameScene")
                return;

            UpdateTimerText();
        }

        // --- 씬 로드 콜백 등록/해제 ---
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
            if (scene.name != "GameScene")
            {
                IsSceneReady = false;
                return;
            }
            IsSceneReady = false;
            // ─── 1) 2D Canvas 먼저 ───────────────────────────────
            if (_canvasInstance != null) Destroy(_canvasInstance);
            _canvasInstance = Instantiate(main2DCanvasPrefab);

            var uiRefs = _canvasInstance.GetComponent<CanvasUIReferences>();
            if (uiRefs == null)
            {
                Debug.LogError("CanvasUIReferences 컴포넌트가 없습니다!");
                return;
            }

            // UI 전용 필드 바인딩 (Timer, ActionPanel, Profile 등)
            timerText = uiRefs.TimerText;
            actionButtonPanel = uiRefs.ActionButtonPanel;
            profileImages = uiRefs.ProfileImages;
            profileFrameImages = uiRefs.ProfileFrameImages;
            nicknameTexts = uiRefs.NicknameTexts;
            flowerImages = uiRefs.FlowerImages;
            flowerCountTexts = uiRefs.FlowerCountTexts;
            flowerCountBaseScales = new Vector3[flowerCountTexts.Length];
            flowerCountTweens = new Tween[flowerCountTexts.Length];
            for (int i = 0; i < flowerCountTexts.Length; i++)
            {
                var txt = flowerCountTexts[i];
                flowerCountBaseScales[i] = txt != null ? txt.transform.parent.localScale : Vector3.one;
            }

            // CanvasUIReferences 에 선언된 GameHandManager 할당
            gameHandManager = uiRefs.gameHandManager;
            if (gameHandManager == null)
            {
                Debug.LogError("CanvasUIReferences.gameHandManager 가 할당되지 않았습니다!");
            }

            // ─── 4) Settings, Emoji 등 기타 초기화 ───────────────────
            // 1-A) Settings UI Instantiate
            if (settingsUIPrefab != null)
            {
                var settingsGO = Instantiate(settingsUIPrefab, _canvasInstance.transform);
                settingsUIManager = settingsGO.GetComponent<SettingsUIManager>();
                var settingsRefs = _canvasInstance.GetComponent<SettingsUIReferences>();
                if (settingsUIManager != null && settingsRefs != null)
                    settingsUIManager.Initialize(settingsRefs);
            }
            // 1-B) Emoji Panel Instantiate
            if (emojiPanelPrefab != null)
            {
                var emojiGO = Instantiate(emojiPanelPrefab, _canvasInstance.transform);
                emojiPanelController = emojiGO.GetComponent<EmojiPanelController>();
                var emojiRefs = _canvasInstance.GetComponent<EmojiPanelReferences>();
                if (emojiPanelController != null && emojiRefs != null)
                    emojiPanelController.Initialize(emojiRefs);
            }
            // ─── 2) 3D Field 인스턴스화 ───────────────────────────────
            if (_field3DInstance != null) Destroy(_field3DInstance);
            _field3DInstance = Instantiate(prefab3DField);

            var fRefs = _field3DInstance.GetComponent<FieldReferences>();
            if (fRefs == null)
            {
                Debug.LogError("FieldReferences 가 없습니다!");
                return;
            }



            // 3D Field 전용 필드 바인딩 (TurnImages, WindTexts, ScoreTexts, Hand3DFields, CallBlockOrigins 등)
            for (int i = 0; i < 4; i++)
                BlinkTurnImages[i] = fRefs.TurnImages[i];

            windText_Self = fRefs.WindTexts[0];
            windText_Shimo = fRefs.WindTexts[1];
            windText_Toi = fRefs.WindTexts[2];
            windText_Kami = fRefs.WindTexts[3];

            scoreText_Self = fRefs.ScoreTexts[0];
            scoreText_Shimo = fRefs.ScoreTexts[1];
            scoreText_Toi = fRefs.ScoreTexts[2];
            scoreText_Kami = fRefs.ScoreTexts[3];

            currentRoundText = fRefs.RoundText;
            leftTilesText = fRefs.LeftTileText;

            BlinkTurnImages = fRefs.TurnImages;
            playersHand3DFields = fRefs.Hand3DFields;
            callBlockFields = fRefs.CallBlockOrigins
                                      .Select(t => t.GetComponent<CallBlockField>())
                                      .ToArray();

            // 2-A) DiscardManager Prefab Instantiate
            if (discardManagerPrefab != null)
            {
                var dmGO = Instantiate(discardManagerPrefab, _field3DInstance.transform);
                discardManager = dmGO.GetComponent<DiscardManager>();
            }
            else
            {
                // 3DField 안에 이미 붙어 있는 경우
                discardManager = _field3DInstance.GetComponentInChildren<DiscardManager>();
            }
            if (discardManager == null) Debug.LogError("DiscardManager 누락!");

            // DiscardManager 위치 세팅
            discardManager?.SetPositions(fRefs.DiscardPositions);

            // ─── 3) GameHandManager 초기화 ───────────────────────────────
            if (gameHandManager != null)
            {
                if (callBlockFields.Length == 0)
                {
                    Debug.LogError("callBlockFields 배열이 비어 있습니다!");
                }
                else
                {
                    gameHandManager.Initialize(
                        callBlockFieldRef: callBlockFields[0],
                        discardManagerRef: discardManager
                    );
                }
            }
            BindCameraResultAnimator();



            // ─── 4) Bloom 생성 ───────────────────────────────
            if (bloomPrefab != null)
            {
                bloomInstance = Instantiate(bloomPrefab);
            }
            // ─── 5) 상태 초기화 ───────────────────────────────
            ResetState();
            IsSceneReady = true;
            Debug.Log("[GameManager] Scene → Prefab 바인딩 & ResetState 완료");
        }

        /// <summary>
        /// 현재 씬에 존재하는 Main Camera에서 CameraResultAnimator를 찾아 바인딩
        /// </summary>
        private void BindCameraResultAnimator()
        {
            cameraResultAnimator = null;          // 이전 참조 무효화

            // ① 기본적인 찾기 – Unity 기본 태그를 썼다면
            var cam = Camera.main;

            // ② 혹시 커스텀 프리팹 안에 들어있다면
            if (cam == null && _field3DInstance != null)
                cam = _field3DInstance.GetComponentInChildren<Camera>();

            if (cam == null)
            {
                Debug.LogError("[GameManager] MainCamera를 찾지 못했습니다.");
                return;
            }

            cameraResultAnimator = cam.GetComponent<CameraResultAnimator>();
            if (cameraResultAnimator == null)
            {
                Debug.LogError("[GameManager] MainCamera에 CameraResultAnimator가 없습니다.");
                return;
            }

            // 필요하다면 여기서 이벤트 재-구독이나 초기화 메서드 호출
            // cameraResultAnimator.Initialize(...);
            Debug.Log("[GameManager] CameraResultAnimator 재바인딩 완료");
        }

        /// <summary>
        /// 씬 재진입할 때, Awake 이후 할당된 초기값과 
        /// 게임 진행 중 변경된 모든 내부 상태를 클리어합니다.
        /// </summary>
        private void ResetState()
        {
            // IsSceneReady = false;

            // (1) 돌고 있는 코루틴 중지
            StopAllCoroutines();
            // 화패 카운트 애니메이션이 중단되었다면 스케일을 원래값으로 돌려둔다
            ResetFlowerCountContainerScales();
            if (flowerCountTweens != null)
            {
                for (int i = 0; i < flowerCountTweens.Length; i++)
                {
                    flowerCountTweens[i]?.Kill();
                    flowerCountTweens[i] = null;
                }
            }

            // (2) 언어 및 플래그 리셋
            // currentLanguage = Language.Korean;
            IsFlowerConfirming = false;
            // IsRightClickTsumogiri = false;
            AutoHuFlag = IsAutoHuDefault;
            PreventCallFlag = false;
            AutoFlowerFlag = IsAutoFlowerDefault;
            TsumogiriFlag = false;
            isGameStarted = false;
            IsMyTurn = false;
            isInitHandDone = false;
            isActionUIActive = false;
            isAfterTsumoAction = false;
            CanClick = false;
            IsSpectator = false;

            // (3) 게임 상태 관련 컬렉션 및 값들 초기화
            Players = new List<Player>();
            PlayerInfo = new List<RoomUserInfo>();
            MySeat = default;
            ViewSeat = AbsoluteSeat.EAST;
            CurrentTurnSeat = default;
            CurrentRound = Round.E1;
            NowHoverTile = null;
            NowHoverSource = null;

            tenpaiAssistDict.Clear();
            NowTenpaiAssistList.Clear();
            flowerQueue.Clear();
            seatToPlayerIndex?.Clear();
            playerIndexToSeat?.Clear();
            playerUidToIndex?.Clear();

            prevBlinkSeat = -1;

            // (4) UI 리셋
            SetUIActive(false);
            ClearActionUI();
            InitializeFlowerUI();
            leftTilesText.text = "";
            currentRoundText.text = "";
            timerText.text = "";
        }

        /// <summary>
        /// 모든 화패 카운트 텍스트의 컨테이너 스케일을 기본값으로 되돌립니다.
        /// 애니메이션이 중단되거나 UI가 초기화될 때 호출합니다.
        /// </summary>
        public void ResetFlowerCountContainerScales()
        {
            if (flowerCountTexts == null) return;
            for (int i = 0; i < flowerCountTexts.Length; i++)
            {
                var txt = flowerCountTexts[i];
                if (txt == null) continue;
                Transform container = txt.transform.parent;
                if (container == null) continue;
                Vector3 baseScale = (flowerCountBaseScales != null && i < flowerCountBaseScales.Length)
                    ? flowerCountBaseScales[i]
                    : Vector3.one;
                container.localScale = baseScale;
            }
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

        /// <summary>
        /// ViewSeat 변경 시 상대 좌표 기반 데이터를 새 ViewSeat 기준으로 재구성합니다.
        /// </summary>
        public void ChangeViewSeat(AbsoluteSeat newViewSeat)
        {
            if (ViewSeat == newViewSeat) return;

            // 기존 기준 좌석과 새 기준 좌석 확보
            AbsoluteSeat oldRef = ReferenceSeat;
            ViewSeat = newViewSeat;
            AbsoluteSeat newRef = ReferenceSeat;

            // 현재 턴 좌석 재계산
            AbsoluteSeat absTurn = CurrentTurnSeat.ToAbsoluteSeat(oldRef);
            CurrentTurnSeat = RelativeSeatExtensions.CreateFromAbsoluteSeats(newRef, absTurn);
            if (IsSpectator)
            {
                IsMyTurn = false;
                CanClick = false;
            }

            // 화패 카운트 재배열
            var rotatedFlowerMap = new Dictionary<RelativeSeat, int>();
            foreach (var kv in flowerCountMap)
            {
                AbsoluteSeat abs = kv.Key.ToAbsoluteSeat(oldRef);
                RelativeSeat rel = RelativeSeatExtensions.CreateFromAbsoluteSeats(newRef, abs);
                rotatedFlowerMap[rel] = kv.Value;
            }
            flowerCountMap = rotatedFlowerMap;

            // UI 갱신
            InitializeProfileUI();
            UpdateSeatLabels();
            UpdateScoreText();
            UpdateFlowerCountText();
            ResetAllBlinkTurnEffects();
            UpdateCurrentTurnEffect();
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
