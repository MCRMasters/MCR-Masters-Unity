using System.Collections.Generic;
using UnityEngine;
using MCRGame.Common;
using MCRGame.UI;
using MCRGame.Net;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Collections;

namespace MCRGame.Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        // 게임 관련 데이터
        public List<Player> Players { get; private set; }
        public AbsoluteSeat MySeat { get; private set; }
        public AbsoluteSeat CurrentTurnSeat { get; private set; }
        public Round CurrentRound { get; private set; }

        // Inspector에서 할당하는 GameHandManager 오브젝트를 통해 GameHand를 관리합니다.
        [SerializeField]
        private GameHandManager gameHandManager;
        public GameHand GameHand => gameHandManager != null ? gameHandManager.GameHand : null;

        public const int MAX_TILES = 144;
        public const int MAX_PLAYERS = 4;
        private int leftTiles;
        [SerializeField]
        private UnityEngine.UI.Text leftTilesText;

        [SerializeField]
        private UnityEngine.UI.Text currentRoundText;

        // 추가: Inspector에서 할당할 수 있는 4개의 Hand3DField 배열 (index 0~3 은 각 상대 좌석에 대응)
        [SerializeField]
        private Hand3DField[] playersHand3DFields;

        private Dictionary<AbsoluteSeat, int> seatToPlayerIndex;
        private Dictionary<int, AbsoluteSeat> playerIndexToSeat;
        private Dictionary<string, int> playerUidToIndex;


        [Header("Score Label Texts (RelativeSeat 순서)")]
        [SerializeField] private Text scoreText_Self;
        [SerializeField] private Text scoreText_Shimo;
        [SerializeField] private Text scoreText_Toi;
        [SerializeField] private Text scoreText_Kami;

        [Header("Score Colors")]
        [Tooltip("양의 점수일 때 텍스트 색상")]
        [SerializeField] private Color positiveScoreColor = new Color(0f, 0.0941f, 0.7373f); // #0018CB
        [Tooltip("0점일 때 텍스트 색상")]
        [SerializeField] private Color zeroScoreColor = Color.white;                  // #FFFFFF
        [Tooltip("음의 점수일 때 텍스트 색상")]
        [SerializeField] private Color negativeScoreColor = new Color(0.7804f, 0.7569f, 0.3186f); // #C7C151

        [Header("Wind Label Texts (RelativeSeat 순서)")]
        [SerializeField] private Text windText_Self;
        [SerializeField] private Text windText_Shimo;
        [SerializeField] private Text windText_Toi;
        [SerializeField] private Text windText_Kami;

        [Header("Wind Colors")]
        [Tooltip("동풍(E)일 때 텍스트 색상")]
        [SerializeField] private Color eastWindColor = new Color(0.7961f, 0f, 0f); // #CB0000
        [Tooltip("나머지 풍향일 때 텍스트 색상")]
        [SerializeField] private Color otherWindColor = Color.black;            // #000000

        [Header("Profile UI (SELF, SHIMO, TOI, KAMI 순서)")]
        [SerializeField] private Image[] profileImages = new Image[4];
        [SerializeField] private Image[] profileFrameImages = new Image[4];
        [SerializeField] private Text[] nicknameTexts = new Text[4];
        [SerializeField] private Image[] flowerImages = new Image[4];
        [SerializeField] private Text[] flowerCountTexts = new Text[4];

        [SerializeField] private Sprite FlowerIcon_White;
        [SerializeField] private Sprite FlowerIcon_Yellow;
        [SerializeField] private Sprite FlowerIcon_Red;

        private Dictionary<RelativeSeat, int> flowerCountMap = new();

        [Header("Effect Prefabs")]
        [SerializeField] private GameObject flowerPhaseEffectPrefab;
        [SerializeField] private GameObject roundStartEffectPrefab;


        // Inspector에서 할당할 기본 프레임
        [Header("Default Profile Frame/Image")]
        [SerializeField] private Sprite defaultFrameSprite;
        [SerializeField] private Sprite defaultProfileImageSprite;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            SetUIActive(false);
        }

        /// <summary>
        /// 게임 시작 전/후 UI 전체 활성 토글
        /// </summary>
        private void SetUIActive(bool active)
        {
            // 기존 텍스트 UI 토글…
            if (leftTilesText != null) leftTilesText.gameObject.SetActive(active);
            if (currentRoundText != null) currentRoundText.gameObject.SetActive(active);

            foreach (var txt in new[] { windText_Self, windText_Shimo, windText_Toi, windText_Kami })
                if (txt != null) txt.gameObject.SetActive(active);

            foreach (var txt in new[] { scoreText_Self, scoreText_Shimo, scoreText_Toi, scoreText_Kami })
                if (txt != null) txt.gameObject.SetActive(active);

            // ───────────────────────────────────────────────
            // Profile UI 토글
            foreach (var img in profileImages) if (img != null) img.gameObject.SetActive(active);
            foreach (var frame in profileFrameImages) if (frame != null) frame.gameObject.SetActive(active);
            foreach (var txt in nicknameTexts) if (txt != null) txt.gameObject.SetActive(active);
            foreach (var img in flowerImages) if (img != null) img.gameObject.SetActive(active);
            foreach (var txt in flowerCountTexts) if (txt != null) txt.gameObject.SetActive(active);
        }
        private void UpdateCurrentRoundUI()
        {
            if (currentRoundText != null)
                currentRoundText.text = CurrentRound.ToString();
            else
                Debug.LogWarning("currentRoundText UI가 할당되지 않았습니다.");
        }


        private void InitRound()
        {
            leftTiles = MAX_TILES - (GameHand.FULL_HAND_SIZE - 1) * MAX_PLAYERS;
            SetUIActive(true);
            UpdateLeftTiles(leftTiles);

            CurrentRound = Round.E1;
            UpdateCurrentRoundUI();
            InitSeatIndexMapping();

            UpdateSeatLabels();
            UpdateScoreText();
            InitializeProfileUI();
            InitializeFlowerUI();
        }


        private void InitializeFlowerUI()
        {
            flowerCountMap.Clear();
            foreach (RelativeSeat rel in Enum.GetValues(typeof(RelativeSeat)))
                flowerCountMap[rel] = 0;
            UpdateFlowerCountText();
        }

        public void SetFlowerCount(RelativeSeat rel, int count)
        {
            flowerCountMap[rel] = count;
            UpdateFlowerCountText();
        }

        public void UpdateFlowerCountText()
        {
            foreach (RelativeSeat rel in Enum.GetValues(typeof(RelativeSeat)))
            {
                int index = (int)rel;

                // 유효성 검사
                if (flowerImages.Length <= index || flowerCountTexts.Length <= index)
                    continue;

                var image = flowerImages[index];
                var text = flowerCountTexts[index];

                if (!flowerCountMap.TryGetValue(rel, out int count))
                    count = 0;

                if (count == 0)
                {
                    image?.gameObject.SetActive(false);
                    text?.gameObject.SetActive(false);
                    image.sprite = FlowerIcon_White;
                    text.text = "X0";
                }
                else
                {
                    image?.gameObject.SetActive(true);
                    text?.gameObject.SetActive(true);

                    if (count >= 1 && count <= 3)
                        image.sprite = FlowerIcon_White;
                    else if (count >= 4 && count <= 6)
                        image.sprite = FlowerIcon_Yellow;
                    else
                        image.sprite = FlowerIcon_Red;

                    text.text = $"X{count}";
                }
            }
        }

        private void setRelativeSeatFlowerUIActive(bool isActive, RelativeSeat seat)
        {
            int index = (int)seat;

            // 유효성 검사
            if (flowerImages.Length <= index || flowerCountTexts.Length <= index)
                return;

            var image = flowerImages[index];
            var text = flowerCountTexts[index];

            image?.gameObject.SetActive(isActive);
            text?.gameObject.SetActive(isActive);
        }

        private void InitializeProfileUI()
        {
            for (int i = 0; i < 4; i++)
            {
                var rel = (RelativeSeat)i;
                var abs = rel.ToAbsoluteSeat(MySeat);

                if (!seatToPlayerIndex.TryGetValue(abs, out int idx) || idx < 0 || idx >= Players.Count)
                    continue;

                var player = Players[idx];

                // 1) 닉네임
                if (nicknameTexts[i] != null)
                    nicknameTexts[i].text = player.Nickname;

                // 2) 프로필 이미지
                if (profileImages[i] != null)
                    profileImages[i].sprite = GetProfileImageSprite(player.Uid); // 필요시 구현

                // 3) 프로필 프레임
                if (profileFrameImages[i] != null)
                {
                    profileFrameImages[i].sprite = GetFrameSprite(player.Uid); // 필요시 구현
                    profileFrameImages[i].gameObject.SetActive(true);
                }

                // 4) 꽃 UI 초기화 (이미 InitializeFlowerUI 에서 처리됨)
            }
        }

        private Sprite GetProfileImageSprite(string uid)
        {
            return defaultProfileImageSprite;
        }

        // 플레이어별 프레임 스프라이트 반환 (예시: 모두 동일 프레임 사용하거나,
        // Player 데이터에 frame 정보가 있으면 거기서 가져오면 됩니다)
        private Sprite GetFrameSprite(string uid)
        {
            // TODO: uid별로 다른 프레임을 쓰려면 이곳에 로직 추가
            // 지금은 Inspector에서 미리 할당한 기본 프레임을 반환하도록 하겠습니다.
            return defaultFrameSprite;
        }


        /// <summary>
        /// Round와 wind 정보를 바탕으로 seat<->player index 매핑 초기화
        /// </summary>
        public void InitSeatIndexMapping()
        {
            int shift = CurrentRound.Number() - 1;
            string wind = CurrentRound.Wind();

            var baseMapping = wind switch
            {
                "E" => new Dictionary<AbsoluteSeat, int> {
                    { AbsoluteSeat.EAST, 0 }, { AbsoluteSeat.SOUTH, 1 },
                    { AbsoluteSeat.WEST, 2 }, { AbsoluteSeat.NORTH, 3 } },
                "S" => new Dictionary<AbsoluteSeat, int> {
                    { AbsoluteSeat.EAST, 1 }, { AbsoluteSeat.SOUTH, 0 },
                    { AbsoluteSeat.WEST, 3 }, { AbsoluteSeat.NORTH, 2 } },
                "W" => new Dictionary<AbsoluteSeat, int> {
                    { AbsoluteSeat.EAST, 2 }, { AbsoluteSeat.SOUTH, 3 },
                    { AbsoluteSeat.WEST, 1 }, { AbsoluteSeat.NORTH, 0 } },
                "N" => new Dictionary<AbsoluteSeat, int> {
                    { AbsoluteSeat.EAST, 3 }, { AbsoluteSeat.SOUTH, 2 },
                    { AbsoluteSeat.WEST, 0 }, { AbsoluteSeat.NORTH, 1 } },
                _ => throw new System.Exception("Invalid wind: " + wind),
            };

            seatToPlayerIndex = new Dictionary<AbsoluteSeat, int>();
            playerIndexToSeat = new Dictionary<int, AbsoluteSeat>();
            foreach (var kv in baseMapping)
            {
                int mapped = (kv.Value + shift) % MAX_PLAYERS;
                seatToPlayerIndex[kv.Key] = mapped;
                playerIndexToSeat[mapped] = kv.Key;
            }
            MySeat = playerIndexToSeat[playerUidToIndex[PlayerDataManager.Instance.Uid]];
        }

        public void UpdateLeftTiles(int newValue)
        {
            leftTiles = newValue;
            if (leftTilesText != null)
            {
                leftTilesText.text = leftTiles.ToString();
            }
            else
            {
                Debug.LogWarning("leftTilesText UI가 할당되지 않았습니다.");
            }
        }

        public void UpdateScoreText()
        {
            var scoreMap = new Dictionary<RelativeSeat, Text>
    {
        { RelativeSeat.SELF,  scoreText_Self  },
        { RelativeSeat.SHIMO, scoreText_Shimo },
        { RelativeSeat.TOI,   scoreText_Toi   },
        { RelativeSeat.KAMI,  scoreText_Kami  },
    };

            Debug.Log("[UpdateScoreText] 시작 — MySeat: " + MySeat);

            foreach (var kv in scoreMap)
            {
                var rel = kv.Key;
                var txt = kv.Value;
                if (txt == null)
                {
                    Debug.LogWarning($"[UpdateScoreText] 텍스트 컴포넌트 누락: {rel}");
                    continue;
                }

                // Relative → Absolute → Player index
                var absSeat = rel.ToAbsoluteSeat(MySeat);
                Debug.Log($"[UpdateScoreText] rel: {rel} → abs: {absSeat}");

                if (!seatToPlayerIndex.TryGetValue(absSeat, out int idx))
                {
                    Debug.LogWarning($"[UpdateScoreText] seatToPlayerIndex에 없음: {absSeat}");
                    continue;
                }
                if (idx < 0 || idx >= Players.Count)
                {
                    Debug.LogWarning($"[UpdateScoreText] 잘못된 플레이어 인덱스: {idx} (Players.Count={Players.Count})");
                    continue;
                }

                int score = Players[idx].Score;
                Debug.Log($"[UpdateScoreText] Players[{idx}].Score = {score}");

                if (score < 0)
                {
                    txt.text = score.ToString();
                    txt.color = negativeScoreColor;
                    Debug.Log($"[UpdateScoreText] 음수 분기: text='{txt.text}', color={negativeScoreColor}");
                }
                else if (score > 0)
                {
                    txt.text = "+" + score.ToString();
                    txt.color = positiveScoreColor;
                    Debug.Log($"[UpdateScoreText] 양수 분기: text='{txt.text}', color={positiveScoreColor}");
                }
                else // score == 0
                {
                    txt.text = "+0";
                    txt.color = zeroScoreColor;
                    Debug.Log($"[UpdateScoreText] 0점 분기: text='{txt.text}', color={zeroScoreColor}");
                }
            }

            Debug.Log("[UpdateScoreText] 완료");
        }


        public void UpdateSeatLabels()
        {
            var windMap = new Dictionary<RelativeSeat, Text>
            {
                { RelativeSeat.SELF,  windText_Self  },
                { RelativeSeat.SHIMO, windText_Shimo },
                { RelativeSeat.TOI,   windText_Toi   },
                { RelativeSeat.KAMI,  windText_Kami  },
            };

            AbsoluteSeat seat = MySeat;
            for (RelativeSeat rel = RelativeSeat.SELF; ; rel = rel.NextSeat())
            {
                if (windMap.TryGetValue(rel, out var txt) && txt != null)
                {
                    string label = seat.ToString().Substring(0, 1);
                    txt.text = label;
                    txt.color = (label == "E") ? eastWindColor : otherWindColor;
                }
                seat = seat.NextSeat();
                if (rel == RelativeSeat.KAMI) break;
            }
        }

        /// <summary>
        /// 서버로부터 전달된 플레이어 목록으로 게임을 초기화합니다.
        /// Players 리스트 세팅과, UID→인덱스 매핑을 만듭니다.
        /// </summary>
        public void InitGame(List<Player> players)
        {
            // 1) Deep copy: 새로운 Player 인스턴스 리스트 생성
            Players = players
                .Select(p => new Player(p.Uid, p.Nickname, p.Index, p.Score))
                .ToList();

            // 2) UID → index 매핑 초기화
            playerUidToIndex = Players.ToDictionary(p => p.Uid, p => p.Index);

            Debug.Log($"GameManager: Game initialized with {Players.Count} players.");
        }

        /// <summary>
        /// INIT_EVENT 메시지를 통해 받은 초기 손패 데이터를 SELF의 손패와
        /// 플레이어들의 3D 손패 필드에 반영합니다.
        /// </summary>
        /// <param name="initTiles">자신의 초기 손패 타일 데이터 리스트</param>
        /// <param name="tsumoTile">서버에서 받은 tsumotile (없으면 null)</param>
        public void InitHandFromMessage(List<GameTile> initTiles, GameTile? tsumoTile)
        {
            InitRound();

            Debug.Log("GameManager: Initializing hand with received data for SELF.");

            // 1) 2D 핸드(UI) 초기화
            if (gameHandManager != null)
            {
                // 기본 init (initTiles 수만큼 4장씩 떨어뜨림)
                StartCoroutine(gameHandManager.InitHand(initTiles, tsumoTile));

            }
            else
            {
                Debug.LogWarning("GameManager: GameHandManager 인스턴스가 없습니다.");
            }

            // 2) 3D 필드(상대방 포함) 초기화
            if (playersHand3DFields == null || playersHand3DFields.Length < MAX_PLAYERS)
            {
                Debug.LogError("playersHand3DFields 배열이 4개로 할당되어 있지 않습니다.");
                return;
            }
            for (int i = 0; i < playersHand3DFields.Length; i++)
            {
                Hand3DField hand3DField = playersHand3DFields[i];
                if (hand3DField == null)
                {
                    Debug.LogWarning($"Hand3DField가 배열의 index {i}에서 할당되지 않았습니다.");
                    continue;
                }

                // SELF(자기)는 이미 2D 핸드로 처리했으니 건너뛰기
                if (i == (int)RelativeSeat.SELF)
                    continue;

                // RelativeSeat → AbsoluteSeat 변환
                RelativeSeat rel = (RelativeSeat)i;
                AbsoluteSeat abs = rel.ToAbsoluteSeat(MySeat);

                // EAST면 tsumo 포함(14장), 아니면 13장만
                bool includeTsumo = (abs == AbsoluteSeat.EAST);
                hand3DField.InitHand(includeTsumo);
            }

        }

        public void StartFlowerReplacement(List<GameTile> newTiles, List<GameTile> appliedFlowers, List<int> flowerCounts)
        {
            StartCoroutine(FlowerReplacementCoroutine(newTiles, appliedFlowers, flowerCounts));
        }

        private IEnumerator FlowerReplacementCoroutine(List<GameTile> newTiles, List<GameTile> appliedFlowers, List<int> flowerCounts)
        {
            // GameHandManager의 InitHand 완료 여부 체크
            while (!gameHandManager.IsInitHandComplete)
            {
                yield return null;
            }
            if (MySeat != AbsoluteSeat.EAST)
            {
                yield return new WaitForSeconds(0.4f);
            }
            Debug.Log("FlowerReplacementCoroutine: InitHand 완료 확인. 꽃 교체 이벤트 시작.");
            yield return new WaitForSeconds(0.7f);

            // "Main 2D Canvas" 오브젝트를 찾아 그 자식으로 prefab을 Instantiate합니다.
            GameObject mainCanvasObject = GameObject.Find("Main 2D Canvas");
            Transform canvasTransform = mainCanvasObject != null ? mainCanvasObject.transform : transform;

            // 0) FLOWER PHASE 연출: fade in 해서 유지
            GameObject flowerEffect = null;
            Image flowerPhaseImage = null;
            if (flowerPhaseEffectPrefab != null)
            {
                flowerEffect = Instantiate(flowerPhaseEffectPrefab, canvasTransform);
                flowerPhaseImage = flowerEffect.GetComponent<Image>();
                if (flowerPhaseImage != null)
                {
                    // fade in (0.2초 동안 서서히 나타남)
                    yield return StartCoroutine(FadeIn(flowerPhaseImage, 0.2f));
                    // 이 후 flowerPhaseImage는 화면에 완전히 나타난 상태로 유지됩니다.
                }
            }
            else
            {
                Debug.LogWarning("flowerPhaseEffectPrefab이 할당되지 않았습니다.");
            }

            // 1) 좌석 순서: EAST, SOUTH, WEST, NORTH
            AbsoluteSeat[] seats = new AbsoluteSeat[] { AbsoluteSeat.EAST, AbsoluteSeat.SOUTH, AbsoluteSeat.WEST, AbsoluteSeat.NORTH };

            foreach (var absoluteSeat in seats)
            {
                int count = flowerCounts[(int)absoluteSeat];
                Debug.Log($"[FlowerReplacement] {absoluteSeat} 좌석 꽃 개수: {count}");

                RelativeSeat relativeSeat = RelativeSeatExtensions.CreateFromAbsoluteSeats(MySeat, absoluteSeat);
                if (relativeSeat == RelativeSeat.SELF)
                {
                    int currentFlowerCount = 0; // 초기 꽃 개수 (필요 시 기존 값 반영)
                    for (int i = 0; i < count; i++)
                    {
                        int previousCount = currentFlowerCount;
                        currentFlowerCount++;

                        bool operationDone = false;
                        bool animateDone = false;

                        // 동시에 꽃 카운트 애니메이션 실행
                        StartCoroutine(AnimateFlowerCount(relativeSeat, previousCount, currentFlowerCount, () => { animateDone = true; }));
                        // 두 작업(ApplyFlower와 AddInitFlowerTsumo)을 순차 실행하는 코루틴 실행
                        StartCoroutine(ProcessFlowerOperation(i, newTiles, appliedFlowers, () => { operationDone = true; }));

                        // 두 작업 모두 완료될 때까지 기다림
                        yield return new WaitUntil(() => operationDone && animateDone);
                    }
                    SetFlowerCount(relativeSeat, currentFlowerCount);
                }
                else
                {
                    // 상대의 경우: Hand3DField를 이용해 요청
                    Hand3DField handField = playersHand3DFields[(int)relativeSeat];
                    int currentFlowerCount = 0; // 상대 좌석의 초기 꽃 개수
                    for (int i = 0; i < count; i++)
                    {
                        int previousCount = currentFlowerCount;
                        currentFlowerCount++;

                        bool operationDone = false;
                        bool animateDone = false;

                        // 동시에 꽃 카운트 애니메이션 실행
                        StartCoroutine(AnimateFlowerCount(relativeSeat, previousCount, currentFlowerCount, () => { animateDone = true; }));
                        // Hand3DField의 RequestDiscardRandom과 RequestInitFlowerTsumo를 순차 실행하는 코루틴
                        StartCoroutine(ProcessOpponentFlowerOperation(handField, () => { operationDone = true; }));

                        yield return new WaitUntil(() => operationDone && animateDone);
                    }
                    SetFlowerCount(relativeSeat, currentFlowerCount);
                }
                yield return new WaitForSeconds(0.3f);
            }

            // 2) 모든 꽃 교체 완료 후, ROUND START 연출 직전에 FLOWER PHASE fade out 처리
            if (flowerPhaseImage != null)
            {
                yield return StartCoroutine(FadeOut(flowerPhaseImage, 0.2f));
                Destroy(flowerEffect);
            }
            yield return new WaitForSeconds(1f);
            // 3) 전체 화패 교체 이벤트 종료 후 ROUND START 연출
            if (roundStartEffectPrefab != null)
            {
                GameObject roundStartEffect = Instantiate(roundStartEffectPrefab, canvasTransform);
                Image roundStartImage = roundStartEffect.GetComponent<Image>();
                if (roundStartImage != null)
                {
                    yield return StartCoroutine(FadeInAndOut(roundStartImage, 0.2f, 0.7f));
                }
                Destroy(roundStartEffect);
            }
            else
            {
                Debug.LogWarning("roundStartEffectPrefab이 할당되지 않았습니다.");
            }

            Debug.Log("[FlowerReplacement] 꽃 교체 이벤트 완료.");
            yield break;
        }


        // SELF인 경우 두 작업(ApplyFlower와 AddInitFlowerTsumo)을 순차적으로 실행합니다.
        private IEnumerator ProcessFlowerOperation(int index, List<GameTile> newTiles, List<GameTile> appliedFlowers, Action onComplete)
        {
            yield return StartCoroutine(gameHandManager.ApplyFlower(appliedFlowers[index]));
            yield return StartCoroutine(gameHandManager.AddInitFlowerTsumo(newTiles[index]));
            onComplete?.Invoke();
        }

        // 상대의 경우, Hand3DField의 RequestDiscardRandom()과 RequestInitFlowerTsumo()를 순차적으로 실행합니다.
        private IEnumerator ProcessOpponentFlowerOperation(Hand3DField handField, Action onComplete)
        {
            yield return StartCoroutine(handField.RequestDiscardRandom());
            yield return StartCoroutine(handField.RequestInitFlowerTsumo());
            onComplete?.Invoke();
        }
        private IEnumerator AnimateFlowerCount(RelativeSeat rel, int fromValue, int toValue, Action onComplete)
        {
            float duration = 0.1f;
            float elapsed = 0f;
            // 해당 좌석의 꽃 카운트 텍스트를 가져옵니다.
            Text flowerCountText = flowerCountTexts[(int)rel];
            // 텍스트와 이미지가 동일한 부모 오브젝트에 있다고 가정합니다.
            Transform container = flowerCountText.transform.parent;

            // 부모 컨테이너의 원래 스케일을 저장합니다.
            Vector3 originalScale = container.localScale;
            // popScale : 중간에 도달할 최대 확대 비율 (예: 1.3배)
            float popScale = 1.3f;

            // UI 활성화 (필요 시)
            setRelativeSeatFlowerUIActive(isActive: true, seat: rel);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // 숫자 애니메이션: SmoothStep을 적용하여 fromValue에서 toValue로 보간
                float eased = Mathf.SmoothStep(0f, 1f, t);
                int displayValue = fromValue + Mathf.RoundToInt(eased * (toValue - fromValue));
                flowerCountText.text = "X" + displayValue.ToString();

                // 팝 효과: 4*t*(1-t)는 t=0과 t=1에서 0, t=0.5에서 최대값을 가지도록 함.
                float scaleFactor = 1 + (popScale - 1) * (4 * t * (1 - t));
                // 부모 컨테이너 스케일을 변경하면, 자식인 텍스트와 이미지 모두 영향을 받습니다.
                container.localScale = originalScale * scaleFactor;
                yield return null;
            }
            // 최종 값 적용 및 스케일 복귀
            flowerCountText.text = "X" + toValue.ToString();
            container.localScale = originalScale;
            onComplete?.Invoke();
        }

        // 지정한 Image 컴포넌트가 fade in 효과로 나타나도록 처리 (fadeDuration 동안)
        private IEnumerator FadeIn(Image img, float fadeDuration)
        {
            Color origColor = img.color;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                img.color = new Color(origColor.r, origColor.g, origColor.b, t);
                yield return null;
            }
            img.color = new Color(origColor.r, origColor.g, origColor.b, 1f);
        }

        // 지정한 Image 컴포넌트가 fade out 효과로 사라지도록 처리 (fadeDuration 동안)
        private IEnumerator FadeOut(Image img, float fadeDuration)
        {
            Color origColor = img.color;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                img.color = new Color(origColor.r, origColor.g, origColor.b, 1 - t);
                yield return null;
            }
            img.color = new Color(origColor.r, origColor.g, origColor.b, 0f);
        }


        /// <summary>
        /// Image 컴포넌트에 대해 FadeIn 후 일정 시간 유지, FadeOut 애니메이션을 수행합니다.
        /// </summary>
        private IEnumerator FadeInAndOut(Image img, float fadeDuration, float displayDuration)
        {
            Color origColor = img.color;
            // Fade In
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                img.color = new Color(origColor.r, origColor.g, origColor.b, t);
                yield return null;
            }
            yield return new WaitForSeconds(displayDuration);
            // Fade Out
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                img.color = new Color(origColor.r, origColor.g, origColor.b, 1 - t);
                yield return null;
            }
        }
    }
}
