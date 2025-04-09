using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MCRGame.Common; // GameTile, GameAction, GameActionType, RelativeSeat, WinningConditions, CallBlockData 등

namespace MCRGame.UI
{
    public class GameHandManager : MonoBehaviour
    {
        [SerializeField] private GameObject baseTilePrefab;
        [SerializeField] private CallBlockField callBlockField;  // Inspector에서 할당 (UI상의 호출 블록 관리)
        [SerializeField] private DiscardManager discardManager;    // Inspector에서 DiscardManager를 할당받음

        private RectTransform haipaiRect;
        private List<GameObject> tileObjects;  // 생성된 타일 UI 오브젝트 목록
        private GameHand gameHand;             // 순수 데이터 및 로직을 관리하는 GameHand
        private GameObject tsumoTile;          // tsumo 타일 (보통 마지막 타일)

        // 외부에서 접근 가능한 프로퍼티
        public GameHand GameHand => gameHand;
        public CallBlockField CallBlockField => callBlockField;

        public const int FULL_HAND_SIZE = 14;

        // 폐기 요청들을 순차 처리하기 위한 큐 및 슬라이드 상태 플래그
        private Queue<DiscardRequest> discardQueue = new Queue<DiscardRequest>();
        private bool isSliding = false;
        // 애니메이션 지속시간 (HandField와 비슷하게)
        [SerializeField] private float slideDuration = 0.5f;
        [SerializeField] private float gap = 0.1f;


        // 폐기 요청 구조체: 인덱스와 tsumotile 여부 저장
        private struct DiscardRequest
        {
            public int index;
            public bool isTsumoTile;
            public DiscardRequest(int index, bool isTsumoTile)
            {
                this.index = index;
                this.isTsumoTile = isTsumoTile;
            }
        }

        void Awake()
        {
            // RectTransform 초기화 및 앵커/피벗 설정
            haipaiRect = GetComponent<RectTransform>();
            haipaiRect.anchorMin = new Vector2(0, 0.5f);
            haipaiRect.anchorMax = new Vector2(0, 0.5f);

            tileObjects = new List<GameObject>();
            tsumoTile = null;

            // GameHand 데이터 초기화 
            gameHand = new GameHand();
        }

        void Start()
        {
            InitTestHand();
        }

        // UI 오브젝트만 생성
        private GameObject AddTile(string tileName)
        {
            var newTile = Instantiate(baseTilePrefab, transform);
            var tm = newTile.GetComponent<TileManager>();
            if (tm != null) tm.SetTileName(tileName);
            var rt = newTile.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0, 0.5f);
                rt.anchorMax = new Vector2(0, 0.5f);
                rt.pivot = new Vector2(0, 0.5f);
            }
            tileObjects.Add(newTile);
            return newTile;
        }

        /// <summary>
        /// 실제로 tsumo(뽑기) 타일을 추가할 때 호출합니다.
        /// </summary>
        public void AddTsumo(GameTile tile)
        {
            // 1) GameHand 데이터에 tsumo 적용
            gameHand.ApplyTsumo(tile);

            // 2) UI 오브젝트 생성 & tsumoTile 지정
            var tileName = tile.ToCustomString();
            var newTileObj = AddTile(tileName);
            tsumoTile = newTileObj;

            // 3) 정렬(꽃 타일 등 특별처리 없으므로 즉시 배치)
            SortTileList();
            ImmediateReplaceTiles();
        }


        // ReplaceTiles() 함수는 즉시 배치하는 대신, 애니메이션 코루틴에서 타일의 새 위치를 계산합니다.
        private Vector2 GetTargetPosition(int visualIndex, float tileWidth)
        {
            // visualIndex는 discarded tile이 제거된 후 재배치할 순번
            return new Vector2(tileWidth * visualIndex, 0f);
        }

        // 테스트용 초기 핸드(haipai)를 생성합니다.
        void InitTestHand()
        {
            // NORMAL_TILE들 (꽃 타일 제외)을 GameTileExtensions.NormalTiles()로 가져옵니다.
            List<GameTile> normalTiles = new List<GameTile>(GameTileExtensions.NormalTiles());
            List<GameTile> tilesForData = new List<GameTile>();

            // 14장의 타일을 랜덤으로 선택하여 UI 오브젝트 생성 및 GameHand 데이터로 저장
            for (int i = 0; i < FULL_HAND_SIZE; ++i)
            {
                int randomIndex = UnityEngine.Random.Range(0, normalTiles.Count);
                GameTile randomTile = normalTiles[randomIndex];

                // UI 오브젝트 생성: randomTile의 문자열은 ToCustomString()으로 변환하여 사용
                AddTile(randomTile.ToCustomString());

                // GameHand 데이터 저장용 리스트에 추가
                tilesForData.Add(randomTile);
            }

            // TsumoTile은 보통 마지막 타일로 설정
            tsumoTile = tileObjects[tileObjects.Count - 1];

            // 정렬 후 즉시 배치 (초기화 전)
            SortTileList();
            ImmediateReplaceTiles();

            // GameHand 데이터를 생성하여 내부 타일 정보를 업데이트
            gameHand = GameHand.CreateFromTiles(tilesForData);
        }

        // 타일 UI 오브젝트 목록을 정렬합니다.
        void SortTileList()
        {
            tileObjects = tileObjects.OrderBy(child =>
            {
                // 이름의 앞 2글자를 기준으로 정렬 (예제 정렬 방식; 필요에 따라 변경)
                string namePart = child.name.Substring(0, 2);
                if (namePart == "0f")
                    return (2, string.Empty);
                string reversedString = new string(namePart.Reverse().ToArray());
                return (1, reversedString);
            }).ToList();
        }

        // 초기 배치는 즉시 적용 (애니메이션 없이)
        void ImmediateReplaceTiles()
        {
            int tsumoTileIndex = 0;
            int count = 0;
            for (int i = 0; i < tileObjects.Count; ++i)
            {
                if (tileObjects[i] == tsumoTile)
                {
                    tsumoTileIndex = i;
                    continue;
                }
                RectTransform tileRect = tileObjects[i].GetComponent<RectTransform>();
                if (tileRect != null)
                {
                    tileRect.anchoredPosition = new Vector2(tileRect.rect.width * count, 0);
                }
                count++;
            }
            if (tsumoTile != null)
            {
                RectTransform tsumoRect = tileObjects[tsumoTileIndex].GetComponent<RectTransform>();
                if (tsumoRect != null)
                {
                    tsumoRect.anchoredPosition = new Vector2(tsumoRect.rect.width * count + tsumoRect.rect.width * 0.2f, 0);
                }
            }
        }


        /// <summary>
        /// CallBlockData 기반으로 호출(Chi/Pon/Kan)을 처리합니다.
        /// </summary>
        public void ApplyCall(CallBlockData cbData)
        {
            // 1) 데이터 업데이트
            gameHand.ApplyCall(cbData);
            // 2) UI에 CallBlock 추가
            callBlockField.AddCallBlock(cbData);
            // 3) UI 핸드에서 제거된 타일들 애니메이션 처리
            StartCoroutine(ProcessCallUI(cbData));
        }
        private IEnumerator ProcessCallUI(CallBlockData cbData)
        {
            // 1) 제거할 GameTile 목록 계산
            List<GameTile> removeTiles = new List<GameTile>();
            switch (cbData.Type)
            {
                case CallBlockType.CHII:
                    for (int i = 0; i < 3; i++)
                        if (i != cbData.SourceTileIndex)
                            removeTiles.Add((GameTile)((int)cbData.FirstTile + i));
                    break;
                case CallBlockType.PUNG:
                    // PUNG의 경우 동일한 타일 2개 제거
                    removeTiles.Add(cbData.FirstTile);
                    removeTiles.Add(cbData.FirstTile);
                    break;
                case CallBlockType.DAIMIN_KONG:
                    // 3개 제거
                    removeTiles.Add(cbData.FirstTile);
                    removeTiles.Add(cbData.FirstTile);
                    removeTiles.Add(cbData.FirstTile);
                    break;
                case CallBlockType.AN_KONG:
                    // 4개 제거
                    removeTiles.Add(cbData.FirstTile);
                    removeTiles.Add(cbData.FirstTile);
                    removeTiles.Add(cbData.FirstTile);
                    removeTiles.Add(cbData.FirstTile);
                    break;
                case CallBlockType.SHOMIN_KONG:
                    removeTiles.Add(cbData.FirstTile);
                    break;
            }

            // 2) removeTiles에 있는 각 타일마다, tileObjects에서 해당 타일(이름이 같은 항목)을 찾아서 제거
            foreach (var gt in removeTiles)
            {
                string name = gt.ToCustomString();
                int idx = tileObjects.FindIndex(go => go.name == name);
                if (idx >= 0)
                {
                    GameObject go = tileObjects[idx];
                    tileObjects.RemoveAt(idx);
                    Destroy(go);
                }
            }

            // 3) 남은 tileObjects를 애니메이션으로 재배치
            yield return StartCoroutine(AnimateReposition());
        }
        private IEnumerator AnimateReposition()
        {
            if (tileObjects.Count == 0) yield break;

            // 기준 타일 너비
            var firstRect = tileObjects[0].GetComponent<RectTransform>();
            float tileWidth = firstRect != null ? firstRect.rect.width : 1f;

            // 초기/목표 위치 계산
            var initialPos = new Dictionary<GameObject, Vector2>();
            var targetPos = new Dictionary<GameObject, Vector2>();

            for (int i = 0; i < tileObjects.Count; i++)
            {
                var go = tileObjects[i];
                var rt = go.GetComponent<RectTransform>();
                if (rt == null) continue;

                initialPos[go] = rt.anchoredPosition;

                float extraGap = (tsumoTile != null && go == tsumoTile && i == tileObjects.Count - 1)
                    ? tileWidth * 0.2f
                    : 0f;

                targetPos[go] = new Vector2(i * (tileWidth + gap) + extraGap, 0f);
            }

            // SmoothStep 이징 애니메이션
            float elapsed = 0f;
            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / slideDuration);
                float easeT = Mathf.SmoothStep(0f, 1f, t);

                foreach (var kv in targetPos)
                {
                    var go = kv.Key;
                    var rt = go.GetComponent<RectTransform>();
                    if (rt == null) continue;
                    rt.anchoredPosition = Vector2.Lerp(initialPos[go], kv.Value, easeT);
                }

                yield return null;
            }

            // 최종 위치 확정
            foreach (var kv in targetPos)
            {
                var go = kv.Key;
                var rt = go.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = kv.Value;
            }
        }

        /// <summary>
        /// TileManager로부터 호출(discard) 요청이 들어오면, 해당 타일은 애니메이션 큐에 등록되어,
        /// 폐기 후 부드러운 슬라이드 애니메이션으로 위치를 재배치하고, 폐기된 타일은 Destroy 처리합니다.
        /// </summary>
        public void DiscardTile(TileManager tileManager)
        {
            if (tileManager == null)
            {
                Debug.LogError("DiscardTile: tileManager가 null입니다.");
                return;
            }
            // tileManager.gameObject.name은 ToCustomString() 결과값 (예: "1m")
            string customName = tileManager.gameObject.name;
            if (GameTileExtensions.TryParseCustom(customName, out GameTile tileValue))
            {
                try
                {
                    // 게임 로직 데이터 업데이트: GameHand의 ApplyDiscard 호출
                    gameHand.ApplyDiscard(tileValue);

                    // 필요 시, DiscardManager를 통해 UI상의 폐기도 수행
                    if (discardManager != null)
                    {
                        discardManager.DiscardTile(RelativeSeat.SELF, tileValue);
                    }

                    // 큐에 해당 타일 폐기 요청 등록 (해당 타일의 인덱스를 찾음)
                    int index = tileObjects.IndexOf(tileManager.gameObject);
                    if (index < 0)
                    {
                        Debug.LogError("DiscardTile: 타일 오브젝트를 찾을 수 없습니다.");
                        return;
                    }
                    bool isTsumo = (tileManager.gameObject == tsumoTile);
                    discardQueue.Enqueue(new DiscardRequest(index, isTsumo));

                    // 큐가 비어있지 않으면 슬라이드 코루틴 시작
                    if (!isSliding)
                    {
                        StartCoroutine(ProcessDiscardQueue());
                    }

                    Debug.Log($"DiscardTile: {tileValue} ({customName})를 SELF 위치로 폐기 요청 등록.");
                }
                catch (Exception ex)
                {
                    Debug.LogError("DiscardTile 오류: " + ex.Message);
                }
            }
            else
            {
                Debug.LogError($"DiscardTile: '{customName}' 문자열을 GameTile로 변환하는데 실패했습니다.");
            }
        }

        // 큐에 쌓인 폐기 요청들을 순차 처리하는 코루틴
        private IEnumerator ProcessDiscardQueue()
        {
            isSliding = true;
            while (discardQueue.Count > 0)
            {
                DiscardRequest request = discardQueue.Dequeue();
                yield return StartCoroutine(ProcessDiscardRequest(request));
            }
            isSliding = false;
        }

        // 개별 폐기 요청 처리 코루틴: 타일 리스트에서 해당 타일 제거 후, 나머지 타일의 위치를 애니메이션으로 이동
        private IEnumerator ProcessDiscardRequest(DiscardRequest request)
        {
            // 먼저, 해당 인덱스의 타일이 리스트에 남아 있다면 Destroy 처리
            if (request.index >= 0 && request.index < tileObjects.Count)
            {
                GameObject discardedTile = tileObjects[request.index];
                // 삭제된 타일은 즉시 리스트에서 제거
                tileObjects.RemoveAt(request.index);
                if (discardedTile != null)
                {
                    Destroy(discardedTile);
                }
                // tsumoTile의 경우, 참조도 초기화
                if (request.isTsumoTile)
                {
                    tsumoTile = null;
                }
            }

            // 재배열 대상 : 현재 tileObjects에 남은 모든 타일들(순서대로 배치)
            if (tileObjects.Count == 0)
                yield break;

            RectTransform firstRect = tileObjects[0].GetComponent<RectTransform>();
            float tileWidth = (firstRect != null) ? firstRect.rect.width : 1f;

            var initialPositions = new Dictionary<GameObject, Vector2>();
            var targetPositions = new Dictionary<GameObject, Vector2>();

            for (int i = 0; i < tileObjects.Count; i++)
            {
                var tileObj = tileObjects[i];
                var rect = tileObj.GetComponent<RectTransform>();
                if (rect == null) continue;

                initialPositions[tileObj] = rect.anchoredPosition;

                float extraGap = (tsumoTile != null && tileObj == tsumoTile && i == tileObjects.Count - 1)
                    ? tileWidth * 0.2f
                    : 0f;

                targetPositions[tileObj] = new Vector2(i * (tileWidth + gap) + extraGap, 0f);
            }

            float elapsed = 0f;
            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / slideDuration);
                // SmoothStep을 써서 0→1 구간에서 가속→감속 이징
                float easeT = Mathf.SmoothStep(0f, 1f, t);

                foreach (var kvp in targetPositions)
                {
                    var tileObj = kvp.Key;
                    if (tileObj == null) continue;
                    var rect = tileObj.GetComponent<RectTransform>();
                    if (rect == null) continue;
                    rect.anchoredPosition = Vector2.Lerp(initialPositions[tileObj], kvp.Value, easeT);
                }

                yield return null;
            }

            // 최종 위치 확정
            foreach (var kvp in targetPositions)
            {
                var tileObj = kvp.Key;
                var rect = tileObj.GetComponent<RectTransform>();
                if (rect != null)
                    rect.anchoredPosition = kvp.Value;
            }
        }
    }
}
