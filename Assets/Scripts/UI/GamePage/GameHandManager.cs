using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MCRGame.Common;
using UnityEngine.UI; // GameTile, GameAction, GameActionType, RelativeSeat, WinningConditions, CallBlockData 등
using MCRGame.Game;

namespace MCRGame.UI
{
    public class GameHandManager : MonoBehaviour
    {
        [SerializeField] private GameObject baseTilePrefab;
        [SerializeField] private CallBlockField callBlockField;
        [SerializeField] private DiscardManager discardManager;

        [Header("Hand Animation Settings")]
        [SerializeField] private float slideDuration = 0.5f;
        [SerializeField] private float gap = 0.1f;

        [Header("Tsumo Drop Settings")]      // <-- 추가
        [SerializeField] private float tsumoDropHeight = 50f;
        [SerializeField] private float tsumoDropDuration = 0.2f;
        [SerializeField] private float tsumoFadeDuration = 0.15f;

        private RectTransform haipaiRect;
        private List<GameObject> tileObjects;
        private GameHand gameHand;
        private GameObject tsumoTile;

        private Queue<DiscardRequest> discardQueue = new Queue<DiscardRequest>();
        private bool isSliding = false;


        // 외부에서 접근 가능한 프로퍼티
        public GameHand GameHand => gameHand;
        public CallBlockField CallBlockField => callBlockField;

        public const int FULL_HAND_SIZE = 14;


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
            haipaiRect = GetComponent<RectTransform>();
            haipaiRect.anchorMin = new Vector2(0, 0.5f);
            haipaiRect.anchorMax = new Vector2(0, 0.5f);

            tileObjects = new List<GameObject>();
            tsumoTile = null;
            gameHand = new GameHand();
        }

        void Start()
        {
            // 초기 테스트 핸드 대신, INIT_EVENT에 따른 InitHand가 호출됩니다.
            // InitTestHand(); // 기존 테스트 코드는 주석 처리합니다.
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
        /// INIT_EVENT로 전달받은 손패 데이터를 이용하여 타일 오브젝트들을 생성 및 초기화한 후,
        /// 4장씩 그룹으로 위에서 아래로 떨어지는 애니메이션을 실행하고,
        /// 모든 그룹 애니메이션 종료 후 AnimateReposition()을 호출하여 최종 정렬합니다.
        /// </summary>
        /// <param name="initTiles">초기 손패에 해당하는 GameTile 리스트</param>
        public void InitHand(List<GameTile> initTiles)
        {
            // 기존 타일 오브젝트 제거
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
                AddTile(tile.ToCustomString());
            }

            // 떨어지는 애니메이션 실행 (그룹 단위: 4장씩)
            StartCoroutine(AnimateInitHand());
        }

        /// <summary>
        /// 타일 오브젝트들을 4장씩 그룹으로 떨어뜨리는 애니메이션을 실행합니다.
        /// 각 그룹은 상단에서 목표 위치까지 가속도 효과와 fade in 효과를 적용하여 떨어집니다.
        /// 모든 그룹 애니메이션 완료 후 정렬된 손패로 AnimateReposition() 호출합니다.
        /// </summary>
        private IEnumerator AnimateInitHand()
        {
            if (tileObjects.Count != GameHand.FULL_HAND_SIZE - 1)
            {
                yield break;
            }

            // 첫 타일의 RectTransform을 통해 타일 너비 계산
            RectTransform firstRT = tileObjects[0].GetComponent<RectTransform>();
            float tileWidth = firstRT != null ? firstRT.rect.width : 100f;

            // 최종 목표 위치 계산 (순서대로 수평 배치)
            Dictionary<GameObject, Vector2> finalPositions = new Dictionary<GameObject, Vector2>();
            for (int i = 0; i < tileObjects.Count; i++)
            {
                Vector2 pos = new Vector2(i * (tileWidth + gap), 0f);
                finalPositions[tileObjects[i]] = pos;
            }

            int groupSize = 4;
            int numGroups = Mathf.CeilToInt(tileObjects.Count / (float)groupSize);
            float dropHeight = 300f;   // 타일들이 시작할 Y 오프셋 (위쪽)
            float duration = 0.1f;     // 각 그룹 애니메이션 지속시간

            for (int group = 0; group < numGroups; group++)
            {
                int startIdx = group * groupSize;
                int endIdx = Mathf.Min((group + 1) * groupSize, tileObjects.Count);

                // 각 그룹의 타일들에 대해 시작 위치와 fade 초기화
                Dictionary<GameObject, Color> tileOriginalColors = new Dictionary<GameObject, Color>();
                for (int i = startIdx; i < endIdx; i++)
                {
                    GameObject tileObj = tileObjects[i];
                    RectTransform rt = tileObj.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchoredPosition = finalPositions[tileObj] + new Vector2(0f, dropHeight);
                    }
                    // Fade 초기화: 이미지의 알파를 0으로 설정
                    Image img = tileObj.GetComponentInChildren<Image>();
                    if (img != null)
                    {
                        tileOriginalColors[tileObj] = img.color;
                        img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
                    }
                }

                // 해당 그룹 타일 애니메이션: 이동과 동시에 fade in 진행 (easing: 1 - (1-t)^2)
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    float ease = 1 - Mathf.Pow(1 - t, 2);
                    float alpha = t; // fade in 진행 (drop과 동시에 진행하므로 t로 결정)
                    for (int i = startIdx; i < endIdx; i++)
                    {
                        GameObject tileObj = tileObjects[i];
                        RectTransform rt = tileObj.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            Vector2 startPos = finalPositions[tileObj] + new Vector2(0f, dropHeight);
                            Vector2 endPos = finalPositions[tileObj];
                            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, ease);
                        }
                        // fade in 업데이트
                        Image img = tileObj.GetComponentInChildren<Image>();
                        if (img != null)
                        {
                            Color origColor = tileOriginalColors[tileObj];
                            img.color = new Color(origColor.r, origColor.g, origColor.b, alpha);
                        }
                    }
                    yield return null;
                }

                // 그룹 애니메이션 완료 후 각 타일을 최종 위치에 고정하고, 이미지 알파를 원래 색상으로 복구
                for (int i = startIdx; i < endIdx; i++)
                {
                    GameObject tileObj = tileObjects[i];
                    RectTransform rt = tileObj.GetComponent<RectTransform>();
                    if (rt != null)
                        rt.anchoredPosition = finalPositions[tileObj];
                    Image img = tileObj.GetComponentInChildren<Image>();
                    if (img != null)
                    {
                        Color origColor = tileOriginalColors[tileObj];
                        img.color = new Color(origColor.r, origColor.g, origColor.b, 1f);
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
 
            SortTileList();

            // 모든 그룹 애니메이션 후 최종 정렬 애니메이션 실행
            yield return StartCoroutine(AnimateReposition());
            yield break;
        }



        public void AddTsumo(GameTile tile)
        {
            // 1) 데이터에 추가
            gameHand.ApplyTsumo(tile);

            // 2) UI 생성
            string tileName = tile.ToCustomString();
            var newTileObj = AddTile(tileName);
            tsumoTile = newTileObj;

            // 3) 슬라이드 애니메이션 대신 드롭 애니메이션 시작
            StartCoroutine(AnimateTsumoDrop());
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
            Color origColor = img != null ? img.color : Color.white;
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

                targetPos[go] = new Vector2(i * (tileWidth + gap), 0f);
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
                // tsumoTile 참조 초기화
                tsumoTile = null;
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

                targetPositions[tileObj] = new Vector2(i * (tileWidth + gap), 0f);
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


