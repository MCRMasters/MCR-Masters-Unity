using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MCRGame.Common;
using UnityEngine.UI; // GameTile, GameAction, GameActionType, RelativeSeat, WinningConditions, CallBlockData 등
using MCRGame.Game;
using UnityEngine.Tilemaps;

namespace MCRGame.UI
{
    public class GameHandManager : MonoBehaviour
    {
        [SerializeField] public GameObject baseTilePrefab;
        [SerializeField] private CallBlockField callBlockField;
        [SerializeField] private DiscardManager discardManager;

        public DiscardManager DiscardManager => discardManager;

        [Header("Hand Animation Settings")]
        [SerializeField] private float slideDuration = 0.5f;
        [SerializeField] private float gap = 0.1f;

        [Header("Tsumo Drop Settings")]      // <-- 추가
        [SerializeField] private float tsumoDropHeight = 50f;
        [SerializeField] private float tsumoDropDuration = 0.1f;
        [SerializeField] private float tsumoFadeDuration = 0.05f;

        private RectTransform haipaiRect;
        private List<GameObject> tileObjects;
        private GameHand gameHand;
        private GameObject tsumoTile;

        private Queue<DiscardRequest> discardQueue = new Queue<DiscardRequest>();


        // round가 끝나면 다음 round 초기화를 위해서 다시 false로 돌려놓아야 함
        public bool IsInitHandComplete = false;

        // 외부에서 접근 가능한 프로퍼티
        public GameHand GameHandPublic => gameHand;
        public CallBlockField CallBlockField => callBlockField;

        public const int FULL_HAND_SIZE = 14;


        // animation 중일 때 true
        public bool IsAnimating;

        // 호버는 애니메이션 중일 때만 막음
        public bool CanHover => !IsAnimating;


        private TileManager requestedDiscardTile;

        private bool isTileOpRunning = false;              // 🔒 모든 타일-변경(파괴·추가·재배치) 공통 락

        private IEnumerator WaitForTileOpDone()
        {
            while (isTileOpRunning)            // 다른 연산이 끝날 때까지 한 프레임씩 기다린다
                yield return null;
        }

        public void ResetPositionAll()
        {
            foreach (GameObject tileObj in tileObjects)
            {
                if (tileObj == null) continue;
                TileManager tileManager = tileObj.GetComponent<TileManager>();
                if (tileManager != null)
                {
                    tileManager.ResetPosition();
                }
            }
        }

        public IEnumerator RunExclusive(IEnumerator body)
        {
            // ❶ 이미 내가 락을 보유 중이면 추가 대기 없이 바로 실행
            if (isTileOpRunning)
            {
                yield return StartCoroutine(body);   // 중첩 실행
                yield break;
            }

            // ❷ 락이 비어 있으면 정상 절차
            yield return WaitForTileOpDone();        // (사실상 필요 없지만 안전용)
            isTileOpRunning = true;                  // 🔒
            bool prevCanClick = GameManager.Instance.CanClick;
            GameManager.Instance.CanClick = false;

            try
            {
                yield return StartCoroutine(body);   // 본-작업
            }
            finally
            {
                if (GameManager.Instance.CanClick == false)
                    GameManager.Instance.CanClick = prevCanClick;
                isTileOpRunning = false;             // 🔓
            }
        }



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
            haipaiRect = GetComponent<RectTransform>();
            // haipaiRect.anchorMin = new Vector2(0, 0.5f);
            // haipaiRect.anchorMax = new Vector2(0, 0.5f);

            tileObjects = new List<GameObject>();
            tsumoTile = null;
            gameHand = new GameHand();
            IsAnimating = false;
            requestedDiscardTile = null;
            isTileOpRunning = false;
        }


        /// <summary>
        /// GameManager.OnSceneLoaded 에서 호출하여
        /// 필수 컴포넌트들을 주입해줍니다.
        /// </summary>
        public void Initialize(
            CallBlockField callBlockFieldRef,
            DiscardManager discardManagerRef
        )
        {
            // 1) Inspector 필드가 비어 있으면 외부에서 받은 참조로 채워줌
            if (callBlockField == null)
                callBlockField = callBlockFieldRef;
            if (discardManager == null)
                discardManager = discardManagerRef;
        }

        public IEnumerator RequestDiscardRightmostTile()
        {
            TileManager tileManager = null;
            if (tsumoTile != null)
                tileManager = tsumoTile.GetComponent<TileManager>();
            if (tileManager == null)
            {
                for (int i = tileObjects.Count - 1; i >= 0; --i)
                {
                    if (tileObjects[i] == null) continue;
                    tileManager = tileObjects[i].GetComponent<TileManager>();
                    if (tileManager != null) break;
                }
            }
            if (tileManager != null)
            {
                RequestDiscard(tileManager);
            }
            yield break;
        }

        /// <summary>
        /// TileManager에서 호출: 서버 검증 요청
        /// </summary>
        public void RequestDiscard(TileManager tileManager)
        {
            if (!GameTileExtensions.TryParseCustom(tileManager.gameObject.name, out GameTile tile)) return;
            // 서버로 DISCARD 요청
            requestedDiscardTile = tileManager;
            GameManager.Instance.RequestDiscard(tile, tileManager.gameObject == tsumoTile);
        }

        /// <summary>
        /// 서버에서 discard 성공 응답이 오면 호출: 실제로 손패에서 제거
        /// </summary>
        public void ConfirmDiscard(GameTile tile)
        {
            if (requestedDiscardTile == null || requestedDiscardTile.gameObject.name != tile.ToCustomString())
            {
                for (int i = tileObjects.Count - 1; i >= 0; --i)
                {
                    if (tileObjects[i].gameObject.name == tile.ToCustomString())
                    {
                        DiscardTile(tileObjects[i].GetComponent<TileManager>());
                        requestedDiscardTile = null;
                        return;
                    }
                }
            }
            DiscardTile(requestedDiscardTile);
            requestedDiscardTile = null;
        }

        /// <summary>
        /// 기본 타일 오브젝트를 생성하여 반환합니다.
        /// </summary>
        /// <param name="tileName">타일 이름 (예: "1m")</param>
        /// <returns>생성된 타일 GameObject</returns>
        private GameObject AddTile(string tileName)
        {
            GameObject newTile = Instantiate(baseTilePrefab, transform);
            var tm = newTile.GetComponent<TileManager>();
            tm?.SetTileName(tileName);
            tm?.UpdateTransparent();

            var rt = newTile.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0, 0.5f);
                rt.anchorMax = new Vector2(0, 0.5f);
                rt.pivot = new Vector2(0, 0.5f);
            }
            var imageField = newTile.transform.Find("ImageField");
            if (imageField != null)
            {
                var img = imageField.GetComponent<Image>();
                if (img != null)
                    img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
            }
            tileObjects.Add(newTile);
            return newTile;
        }

        public void clear()
        {
            // --- 이제 callBlockField가 null이 아님! ---
            if (tileObjects != null)
                foreach (var t in tileObjects)
                    if (t != null) Destroy(t);

            gameHand.Clear();
            tileObjects.Clear();
            tsumoTile = null;

            // null-safe 호출
            callBlockField.InitializeCallBlockField();

            IsAnimating = true;
            ResetPositionAll();
        }

        public void ReloadInitHand(
            List<GameTile> rawTiles,
            List<CallBlockData> rawCallBlocks,
            GameTile? rawTsumoTile
        )
        {
            clear();
            gameHand = GameHand.CreateFromReload(rawTiles, rawCallBlocks, rawTsumoTile);

            callBlockField.ReloadCallBlockListImmediate(rawCallBlocks);


            foreach (var tile in rawTiles)
            {
                string tileName = tile.ToCustomString();
                var go = AddTile(tileName);

                var img = go.transform.Find("ImageField")?.GetComponent<Image>();
                if (img != null)
                    img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);

                if (rawTsumoTile.HasValue && tile == rawTsumoTile.Value && tsumoTile == null)
                {
                    tsumoTile = go;
                }
            }

            if (!rawTsumoTile.HasValue)
                tsumoTile = null;

            SortTileList();
            ImmediateReplaceTiles();
        }

        public IEnumerator InitHand(List<GameTile> initTiles, GameTile? receivedTsumoTile)
        {
            // 기존 타일 오브젝트 제거 및 초기화
            foreach (GameObject tileObj in tileObjects)
            {
                Destroy(tileObj);
            }
            tileObjects.Clear();
            tsumoTile = null;
            // GameHand 데이터 업데이트
            gameHand = GameHand.CreateFromTiles(initTiles);

            // 전달받은 손패 리스트 셔플 (Fisher-Yates 알고리즘)
            for (int i = 0; i < initTiles.Count; i++)
            {
                int randIndex = UnityEngine.Random.Range(i, initTiles.Count);
                GameTile temp = initTiles[i];
                initTiles[i] = initTiles[randIndex];
                initTiles[randIndex] = temp;
            }

            // 셔플된 손패를 기반으로 타일 오브젝트 생성
            foreach (GameTile tile in initTiles)
            {
                GameObject tileObj = AddTile(tile.ToCustomString());
                var imageField = tileObj.transform.Find("ImageField");
                if (imageField != null)
                {
                    var img = imageField.GetComponent<Image>();
                    if (img != null)
                        img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
                }
            }

            // ★ AnimateInitHand 을 큐에 등록하고 끝날 때까지 대기
            yield return RunExclusive(AnimateInitHand());

            yield return new WaitForSeconds(0.5f);
            if (receivedTsumoTile.HasValue)
            {
                // tsumo도 큐로 처리해도 좋지만, 기존처럼 바로 드롭
                yield return RunExclusive(AddTsumo(receivedTsumoTile.Value));
            }

            IsInitHandComplete = true;
            Debug.Log("GameHandManager: InitHand 완료.");
        }

        private IEnumerator AnimateInitHand()
        {
            IsAnimating = true;
            ResetPositionAll();
            List<GameObject> tileObjectsExcludeTsumo = new List<GameObject>(tileObjects);
            tileObjectsExcludeTsumo.Remove(tsumoTile);

            int count = tileObjectsExcludeTsumo.Count;
            if (count <= 0)
            {
                yield break;
            }

            // 1) ImageField/Image 컴포넌트를 한 번만 캐싱
            var tileImages = new List<Image>(count);
            for (int i = 0; i < count; i++)
            {
                var tileObj = tileObjectsExcludeTsumo[i];
                var imageField = tileObj.transform.Find("ImageField");
                if (imageField != null)
                {
                    var img = imageField.GetComponent<Image>();
                    tileImages.Add(img);
                    if (img != null)
                        img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
                }
                else
                {
                    tileImages.Add(null);
                }
            }

            // 2) 위치와 finalPositions 계산
            RectTransform firstRT = tileObjectsExcludeTsumo[0].GetComponent<RectTransform>();
            float tileWidth = firstRT != null ? firstRT.rect.width : 100f;
            var finalPositions = new Dictionary<GameObject, Vector2>();
            for (int i = 0; i < count; i++)
                finalPositions[tileObjectsExcludeTsumo[i]] = new Vector2(i * (tileWidth + gap), 0f);

            int groupSize = 4;
            int numGroups = (count - 1) / groupSize + 1;
            float dropHeight = 300f;
            float duration = 0.2f;

            // 3) 초기 투명화 & 위치 세팅
            for (int i = 0; i < count; i++)
            {
                var rt = tileObjectsExcludeTsumo[i].GetComponent<RectTransform>();
                if (rt != null)
                    rt.anchoredPosition = finalPositions[tileObjectsExcludeTsumo[i]] + Vector2.up * dropHeight;
                var img = tileImages[i];
                if (img != null)
                    img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
            }

            // 4) 그룹별 애니메이션
            for (int g = 0; g < numGroups; g++)
            {
                int start = g * groupSize;
                int end = Mathf.Min(start + groupSize, count);

                // 매 프레임마다 아직 애니메이션되지 않은 (대기) 타일을 투명하게 유지
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    float ease = 1 - Mathf.Pow(1 - t, 2);

                    // 현재 그룹 타일의 위치 보간 및 alpha 업데이트
                    for (int i = start; i < end; i++)
                    {
                        var rt = tileObjectsExcludeTsumo[i].GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            var startPos = finalPositions[tileObjectsExcludeTsumo[i]] + Vector2.up * dropHeight;
                            var endPos = finalPositions[tileObjectsExcludeTsumo[i]];
                            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, ease);
                        }
                        var img = tileImages[i];
                        if (img != null)
                            img.color = new Color(img.color.r, img.color.g, img.color.b, t);
                    }
                    // 대기 타일은 매 프레임 α=0으로 설정
                    for (int i = end; i < count; i++)
                    {
                        var img = tileImages[i];
                        if (img != null)
                            img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
                    }
                    yield return null;
                }

                // 그룹 완료 시 위치 고정 및 alpha=1로 변경
                for (int i = start; i < end; i++)
                {
                    var rt = tileObjectsExcludeTsumo[i].GetComponent<RectTransform>();
                    if (rt != null)
                        rt.anchoredPosition = finalPositions[tileObjectsExcludeTsumo[i]];
                    var img = tileImages[i];
                    if (img != null)
                        img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
                }
                yield return new WaitForSeconds(0.1f);
            }

            SortTileList();
            yield return RunExclusive(AnimateReposition());
        }

        public IEnumerator AddInitFlowerTsumo(GameTile tile)
        {
            IsAnimating = true;
            ResetPositionAll();
            gameHand.ApplyTsumo(tile);

            string tileName = tile.ToCustomString();
            var newTileObj = AddTile(tileName);
            tsumoTile = newTileObj;
            yield return RunExclusive(AnimateTsumoDrop());

            if (gameHand.HandSize == GameHand.FULL_HAND_SIZE)
                tsumoTile = newTileObj;
            else
                tsumoTile = null;

            SortTileList();
            var prevSlideDuration = slideDuration;
            slideDuration = 0.1f;
            yield return RunExclusive(AnimateReposition());
            slideDuration = prevSlideDuration;
            IsAnimating = false;
        }

        public IEnumerator AddTsumo(GameTile tile)
        {
            // 1) 데이터에 추가
            gameHand.ApplyTsumo(tile);

            // 2) UI 생성
            string tileName = tile.ToCustomString();
            var newTileObj = AddTile(tileName);
            tsumoTile = newTileObj;

            // 3) 슬라이드 애니메이션 대신 드롭 애니메이션 시작
            yield return RunExclusive(AnimateTsumoDrop());
        }

        private IEnumerator AnimateTsumoDrop()
        {
            if (tsumoTile == null) yield break;

            // --- 1) 정렬 & 목표 위치 계산 ---
            // SortTileList();

            // 기준 타일 너비
            var firstRt = tileObjects[0].GetComponent<RectTransform>();
            float tileWidth = firstRt != null ? firstRt.rect.width : 1f;

            // 각 타일의 목표 anchoredPosition
            var targetPos = new Dictionary<GameObject, Vector2>();
            int idx = 0;
            foreach (var go in tileObjects)
            {
                if (go == tsumoTile) continue;
                targetPos[go] = new Vector2(idx * (tileWidth + gap), 0f);
                // 다른 타일은 즉시 배치
                var rt = go.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = targetPos[go];
                idx++;
            }
            // tsumo 위치: 마지막 + extra gap
            Vector2 tsumoTarget = new Vector2(
                idx * (tileWidth + gap) + tileWidth * 0.2f,
                0f
            );
            targetPos[tsumoTile] = tsumoTarget;

            // 2) 시작 위치 & 투명 세팅
            var tsumoRt = tsumoTile.GetComponent<RectTransform>();
            Vector2 startPos = tsumoTarget + Vector2.up * tsumoDropHeight;
            tsumoRt.anchoredPosition = startPos;

            var img = tsumoTile.GetComponentInChildren<Image>();
            Color origColor = img != null ? new Color(img.color.r, img.color.g, img.color.b, 1f) : Color.white;
            if (img != null)
                img.color = new Color(origColor.r, origColor.g, origColor.b, 0f);

            // 3) 물리 가속도 계산: y = y0 + 0.5 * a * t^2
            float duration = tsumoDropDuration;
            float y0 = startPos.y;
            float y1 = tsumoTarget.y;
            // a = 2*(y1 - y0)/t^2 로 하면 정확히 duration 후 y1 도달
            float a = 2f * (y1 - y0) / (duration * duration);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (elapsed > duration) elapsed = duration;

                // 가속 운동 공식
                float y = y0 + 0.5f * a * elapsed * elapsed;
                tsumoRt.anchoredPosition = new Vector2(tsumoTarget.x, y);

                // 페이드인 (기존 로직)
                if (img != null)
                {
                    float alpha = Mathf.Clamp01(elapsed / tsumoFadeDuration);
                    img.color = new Color(origColor.r, origColor.g, origColor.b, alpha);
                }

                yield return null;
            }

            // 4) 최종 보정
            tsumoRt.anchoredPosition = tsumoTarget;
            if (img != null)
                img.color = origColor;
        }


        // 타일 UI 오브젝트 목록을 정렬합니다.
        void SortTileList()
        {
            tileObjects = tileObjects.OrderBy(child =>
            {
                // 이름의 앞 2글자를 기준으로 정렬 (예제 정렬 방식; 필요에 따라 변경)
                string namePart = child.name.Substring(0, 2);
                string reversedString = new string(namePart.Reverse().ToArray());
                if (namePart[1] == 'f')
                    return (2, reversedString);
                return (1, reversedString);
            }).ToList();
        }

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

        public IEnumerator ApplyFlower(GameTile tile)
        {
            IsAnimating = true;
            ResetPositionAll();
            // 1) 이름으로 타일 오브젝트 찾기
            string tileName = tile.ToCustomString();
            int idx = tileObjects.FindIndex(go => go != null && go.name == tileName);
            if (idx < 0)
            {
                Debug.LogWarning($"[GameHandManager] '{tileName}' 타일을 찾을 수 없습니다.");
                yield break;
            }

            GameObject tileObj = tileObjects[idx];

            // 2) GameHand 데이터에서도 해당 타일 제거
            gameHand.ApplyDiscard(tile);

            // 3) UI 리스트에서 즉시 제거 및 오브젝트 파괴
            tileObjects.RemoveAt(idx);
            Destroy(tileObj);
            tsumoTile = null;
            SortTileList();

            var prevSlideDuration = slideDuration;
            slideDuration = 0.1f;
            // 4) 나머지 타일들 부드럽게 재배치

            yield return RunExclusive(AnimateReposition());
            slideDuration = prevSlideDuration;
            IsAnimating = false;
        }



        /// <summary>
        /// Chi/Pon/Kan 처리 후 UI 애니메이션을 큐로 등록하도록 수정
        /// </summary>
        public void ApplyCall(CallBlockData cbData)
        {
            // 1) 데이터 업데이트
            gameHand.ApplyCall(cbData);
            // 2) UI에 CallBlock 추가
            callBlockField.AddCallBlock(cbData);
            // 3) 처리 코루틴을 큐로 등록
            StartCoroutine(RunExclusive(ProcessCallUI(cbData)));
        }


        private IEnumerator ProcessCallUI(CallBlockData cbData)
        {
            Debug.Log($"[GameHandManager] ProcessCallUI 시작 → Type={cbData.Type}, FirstTile={cbData.FirstTile}");

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
            Debug.Log($"[GameHandManager] 제거할 타일 목록: {string.Join(", ", removeTiles)}");

            if (cbData.Type == CallBlockType.SHOMIN_KONG)
            {
                tsumoTile = null;
            }

            // 2) removeTiles에 있는 각 타일마다, tileObjects에서 해당 타일(이름이 같은 항목)을 찾아서 제거
            foreach (var gt in removeTiles)
            {
                string name = gt.ToCustomString();
                Debug.Log($"[GameHandManager] 처리 중 타일: {gt} → 오브젝트 이름('{name}') 찾기");
                int idx = tileObjects.FindIndex(go => go.name == name);
                if (idx >= 0)
                {
                    Debug.Log($"[GameHandManager] 타일 오브젝트 발견: index={idx}, name={name}");
                    GameObject go = tileObjects[idx];
                    if (go == tsumoTile)
                    {
                        Debug.Log("[GameHandManager] 이 타일은 tsumoTile 이므로 tsumoTile 레퍼런스 초기화");
                        tsumoTile = null;
                    }
                    tileObjects.RemoveAt(idx);
                    Destroy(go);
                    Debug.Log($"[GameHandManager] 타일 오브젝트 제거 완료: {name} (남은 객체 수: {tileObjects.Count})");
                }
                else
                {
                    Debug.LogWarning($"[GameHandManager] tileObjects에서 '{name}' 타일 오브젝트를 찾지 못함");
                }
            }

            // 3) 남은 tileObjects를 애니메이션으로 재배치
            Debug.Log("[GameHandManager] AnimateReposition 호출 전");
            yield return RunExclusive(AnimateReposition());
            Debug.Log($"[GameHandManager] ProcessCallUI 완료 → 최종 남은 타일 개수: {tileObjects.Count}");
        }


        private IEnumerator AnimateReposition()
        {
            bool nested = IsAnimating;
            IsAnimating = true;
            try
            {
                // 1) 리스트에서 파괴된 항목 제거
                tileObjects.RemoveAll(go => go == null);

                if (tileObjects.Count == 0) yield break;

                // 2) 기준 값 계산
                var firstRect = tileObjects[0].GetComponent<RectTransform>();
                float tileWidth = firstRect != null ? firstRect.rect.width : 1f;

                var initialPos = new Dictionary<GameObject, Vector2>();
                var targetPos = new Dictionary<GameObject, Vector2>();

                int idx = 0;
                foreach (var go in tileObjects)
                {
                    if (go == null) continue;                  // ← 추가
                    var rt = go.GetComponent<RectTransform>();
                    if (rt == null) continue;

                    initialPos[go] = rt.anchoredPosition;

                    if (go == tsumoTile) continue;

                    targetPos[go] = new Vector2(idx * (tileWidth + gap), 0f);
                    idx++;
                }

                if (tsumoTile != null && tsumoTile != null)   // null 중복체크
                {
                    var tsumoRect = tsumoTile.GetComponent<RectTransform>();
                    if (tsumoRect != null)
                    {
                        initialPos[tsumoTile] = tsumoRect.anchoredPosition;
                        targetPos[tsumoTile] =
                            new Vector2(idx * (tileWidth + gap) + tileWidth * 0.2f, 0f);
                    }
                }

                // 3) 이동 애니메이션
                float elapsed = 0f;
                while (elapsed < slideDuration)
                {
                    elapsed += Time.deltaTime;
                    float easeT = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);

                    foreach (var kv in targetPos)
                    {
                        var go = kv.Key;
                        if (go == null) continue;              // ← 추가
                        var rt = go.GetComponent<RectTransform>();
                        if (rt == null) continue;
                        rt.anchoredPosition = Vector2.Lerp(initialPos[go], kv.Value, easeT);
                    }
                    yield return null;
                }

                // 4) 최종 위치 스냅
                foreach (var kv in targetPos)
                {
                    if (kv.Key == null) continue;
                    var rt = kv.Key.GetComponent<RectTransform>();
                    if (rt != null) rt.anchoredPosition = kv.Value;
                }
            }
            finally
            {
                if (!nested) IsAnimating = false;
            }
        }


        /// <summary>
        /// 사용자 클릭에 따른 타일 폐기 요청 처리: 애니메이션 큐 등록으로 수정
        /// </summary>
        public void DiscardTile(TileManager tileManager)
        {
            if (tileManager == null)
            {
                Debug.LogError("DiscardTile: tileManager가 null입니다.");
                return;
            }
            string customName = tileManager.gameObject.name;
            if (!GameTileExtensions.TryParseCustom(customName, out GameTile tileValue))
            {
                Debug.LogError($"DiscardTile: '{customName}' 문자열을 GameTile로 변환 실패");
                return;
            }
            try
            {
                // 1) 데이터 업데이트
                gameHand.ApplyDiscard(tileValue);
                if (discardManager != null)
                    discardManager.DiscardTile(RelativeSeat.SELF, tileValue);

                // 2) 내부 큐에 요청 저장
                int index = tileObjects.IndexOf(tileManager.gameObject);
                bool isTsumo = (tileManager.gameObject == tsumoTile);
                discardQueue.Enqueue(new DiscardRequest(index, isTsumo));

                // ▶️ 애니메이션 처리 코루틴을 큐에 등록
                StartCoroutine(RunExclusive(ProcessDiscardQueue()));

                Debug.Log($"DiscardTile: {tileValue} ({customName}) 폐기 요청 등록.");
            }
            catch (Exception ex)
            {
                Debug.LogError("DiscardTile 오류: " + ex.Message);
            }
        }


        // 큐에 쌓인 폐기 요청들을 순차 처리하는 코루틴
        private IEnumerator ProcessDiscardQueue()
        {
            while (discardQueue.Count > 0)
            {
                DiscardRequest request = discardQueue.Dequeue();
                yield return RunExclusive(ProcessDiscardRequest(request));
            }
        }
        private IEnumerator ProcessDiscardRequest(DiscardRequest request)
        {
            bool nested = IsAnimating;   // 이미 애니메이션 중이었는지 기억
            IsAnimating = true;          // 무조건 true로 올려 둠

            try
            {
                ResetPositionAll();

                /* --- ① 버릴 타일 제거 --------------------- */
                if (request.index >= 0 && request.index < tileObjects.Count)
                {
                    var discarded = tileObjects[request.index];
                    tileObjects.RemoveAt(request.index);
                    if (discarded != null) Destroy(discarded);
                    tsumoTile = null;
                }
                tileObjects.RemoveAll(go => go == null);   // 파괴된 참조 정리
                SortTileList();

                if (tileObjects.Count == 0)
                    yield break;

                /* --- ② 위치 계산 -------------------------- */
                float tileWidth = 1f;
                var firstRT = tileObjects[0].GetComponent<RectTransform>();
                if (firstRT != null) tileWidth = firstRT.rect.width;

                var initialPos = new Dictionary<GameObject, Vector2>();
                var targetPos = new Dictionary<GameObject, Vector2>();

                for (int i = 0; i < tileObjects.Count; i++)
                {
                    var go = tileObjects[i];
                    if (go == null) continue;
                    var rt = go.GetComponent<RectTransform>();
                    if (rt == null) continue;

                    initialPos[go] = rt.anchoredPosition;
                    targetPos[go] = new Vector2(i * (tileWidth + gap), 0f);
                }

                /* --- ③ 슬라이드 애니메이션 ---------------- */
                float elapsed = 0f;
                while (elapsed < slideDuration)
                {
                    elapsed += Time.deltaTime;
                    float easeT = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);

                    foreach (var kv in targetPos)
                    {
                        var go = kv.Key;
                        if (go == null) continue;
                        var rt = go.GetComponent<RectTransform>();
                        if (rt == null) continue;
                        rt.anchoredPosition = Vector2.Lerp(initialPos[go], kv.Value, easeT);
                    }
                    yield return null;
                }

                foreach (var kv in targetPos)
                {
                    if (kv.Key == null) continue;
                    var rt = kv.Key.GetComponent<RectTransform>();
                    if (rt != null) rt.anchoredPosition = kv.Value;
                }

                /* --- ④ 후속 재배치 ------------------------ */
                yield return RunExclusive(AnimateReposition());
            }
            finally
            {
                // 내가 '최상위' 애니메이션이었다면 플래그 해제
                if (!nested) IsAnimating = false;
            }
        }
    }
}


