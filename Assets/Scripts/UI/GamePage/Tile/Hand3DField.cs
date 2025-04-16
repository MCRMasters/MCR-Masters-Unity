using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MCRGame.Common;
using MCRGame.Game;

namespace MCRGame.UI
{
    public class Hand3DField : MonoBehaviour
    {
        public List<GameObject> handTiles = new List<GameObject>();
        public GameObject tsumoTile;

        public float slideDuration = 0.5f;
        public float gap = 0.1f;

        private enum RequestType { Tsumo, Discard, DiscardMultiple, InitFlowerTsumo }
        private struct HandRequest
        {
            public RequestType Type;
            public bool discardRightmost;
            public int discardCount;
            public HandRequest(RequestType type, bool discardRightmost = false, int discardCount = 1)
            {
                Type = type;
                this.discardRightmost = discardRightmost;
                this.discardCount = discardCount;
            }
        }

        private Queue<HandRequest> requestQueue = new Queue<HandRequest>();
        private bool isProcessing = false;

        // --- 퍼블릭 API: 큐에 요청 등록 ---
        public IEnumerator RequestTsumo()
        {
            EnqueueRequest(new HandRequest(RequestType.Tsumo));
            yield return new WaitUntil(() => !isProcessing && requestQueue.Count == 0);
        }

        public IEnumerator RequestInitFlowerTsumo()
        {
            EnqueueRequest(new HandRequest(RequestType.InitFlowerTsumo));
            yield return new WaitUntil(() => !isProcessing && requestQueue.Count == 0);
        }

        public IEnumerator RequestDiscardRightmost()
        {
            EnqueueRequest(new HandRequest(RequestType.Discard, discardRightmost: true));
            yield return new WaitUntil(() => !isProcessing && requestQueue.Count == 0);
        }

        /// <summary>
        /// 오른쪽 끝이 아닌 단일 타일을 버리는 요청입니다.
        /// </summary>
        public IEnumerator RequestDiscardRandom()
        {
            EnqueueRequest(new HandRequest(RequestType.Discard, discardRightmost: false));
            yield return new WaitUntil(() => !isProcessing && requestQueue.Count == 0);
        }

        /// <summary>
        /// 여러 개의 랜덤한 타일을 한 번에 버리는 요청입니다.
        /// 단, tsumoTile(마지막 요소)는 제외하고 처리합니다.
        /// </summary>
        public IEnumerator RequestDiscardMultiple(int count)
        {
            EnqueueRequest(new HandRequest(RequestType.DiscardMultiple, discardCount: count));
            yield return new WaitUntil(() => !isProcessing && requestQueue.Count == 0);
        }

        private void EnqueueRequest(HandRequest req)
        {
            requestQueue.Enqueue(req);
            if (!isProcessing)
                StartCoroutine(ProcessRequestQueue());
        }

        // --- 큐 처리 코루틴 ---
        private IEnumerator ProcessRequestQueue()
        {
            isProcessing = true;
            while (requestQueue.Count > 0)
            {
                HandRequest req = requestQueue.Dequeue();
                yield return StartCoroutine(HandleRequest(req));
            }
            isProcessing = false;
        }

        // --- 요청별 실제 처리 ---
        private IEnumerator HandleRequest(HandRequest req)
        {
            if (req.Type == RequestType.Tsumo)
            {
                // tsumo 타일 생성 --> 리스트의 마지막에 추가
                GameObject newTile = CreateWhiteTile();
                if (newTile != null)
                {
                    tsumoTile = newTile;
                    handTiles.Add(newTile);
                    RepositionTiles(); // 즉시 재배치
                }
            }
            else if (req.Type == RequestType.Discard)
            {
                if (handTiles.Count == 0)
                    yield break;

                int idx;
                // 오른쪽 끝(리스트의 마지막 요소) 제거
                if (req.discardRightmost || handTiles.Count == 1)
                {
                    idx = handTiles.Count - 1;
                }
                else
                {
                    // 마지막 요소를 제외한 인덱스 중 랜덤 선택
                    idx = Random.Range(0, handTiles.Count - 1);
                }

                GameObject toRemove = handTiles[idx];

                // UI만 제거 (데이터 업데이트는 외부에서 처리한다고 가정)
                handTiles.RemoveAt(idx);
                if (toRemove != null)
                    Destroy(toRemove);
                tsumoTile = null;
                yield return StartCoroutine(AnimateReposition());
            }
            else if (req.Type == RequestType.DiscardMultiple)
            {
                // 여러 개의 타일 삭제 요청 처리: tsumoTile(마지막 요소)는 후보에서 제외
                if (handTiles.Count == 0)
                    yield break;

                List<int> candidateIndices = new List<int>();
                if (handTiles.Count > 1 && tsumoTile != null)
                {
                    // tsumoTile을 제외한 후보 인덱스 수집
                    for (int i = 0; i < handTiles.Count - 1; i++)
                    {
                        candidateIndices.Add(i);
                    }
                }
                else
                {
                    for (int i = 0; i < handTiles.Count; i++)
                    {
                        candidateIndices.Add(i);
                    }
                }

                int discardCount = Mathf.Min(req.discardCount, candidateIndices.Count);

                // 후보 인덱스 무작위 섞기 (Fisher-Yates)
                for (int i = 0; i < candidateIndices.Count; i++)
                {
                    int randIndex = Random.Range(i, candidateIndices.Count);
                    int temp = candidateIndices[i];
                    candidateIndices[i] = candidateIndices[randIndex];
                    candidateIndices[randIndex] = temp;
                }
                // 처음 discardCount개 선택 후 내림차순 정렬
                List<int> indicesToDiscard = candidateIndices.GetRange(0, discardCount);
                indicesToDiscard.Sort((a, b) => b.CompareTo(a));

                foreach (int idx in indicesToDiscard)
                {
                    GameObject toRemove = handTiles[idx];
                    handTiles.RemoveAt(idx);
                    if (toRemove != null)
                        Destroy(toRemove);
                }
                yield return StartCoroutine(AnimateReposition());
            }
            else if (req.Type == RequestType.InitFlowerTsumo)
            {
                GameObject newTile = CreateWhiteTile();
                if (newTile != null)
                {
                    // tsumo 타일은 항상 마지막 인덱스로 추가
                    handTiles.Add(newTile);
                    tsumoTile = (handTiles.Count == GameHand.FULL_HAND_SIZE) ? newTile : null;
                    RepositionTiles();
                }
            }
        }

        // --- AnimateReposition (기존 코드) ---
        private IEnumerator AnimateReposition()
        {
            if (handTiles.Count == 0) yield break;

            var initial = new Dictionary<GameObject, Vector3>();
            var target = new Dictionary<GameObject, Vector3>();
            for (int i = 0; i < handTiles.Count; i++)
            {
                GameObject go = handTiles[i];
                initial[go] = go.transform.localPosition;
                target[go] = ComputeTargetLocalPositionForIndex(i);
            }

            float elapsed = 0f;
            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / slideDuration);
                foreach (var kv in target)
                {
                    kv.Key.transform.localPosition = Vector3.Lerp(initial[kv.Key], kv.Value, t);
                }
                yield return null;
            }
            foreach (var kv in target)
                kv.Key.transform.localPosition = kv.Value;
        }

        // ComputeTargetLocalPositionForIndex는 원래 코드를 유지
        private Vector3 ComputeTargetLocalPositionForIndex(int index)
        {
            float width = GetTileLocalWidth(handTiles[0]);
            float offset = index * (width + gap);

            float extra = (index == handTiles.Count - 1 && tsumoTile != null)
                ? width * 0.5f
                : 0f;
            return new Vector3(-(offset + extra), 0f, 0f);
        }

        // 타일의 로컬 너비를 계산하는 유틸리티 (원래 코드에 따른 구현)
        private float GetTileLocalWidth(GameObject tile)
        {
            Renderer rend = tile.GetComponent<Renderer>();
            if (rend == null)
                return 0f;
            (Vector3 min, Vector3 max) = GetLocalBounds(tile);
            return max.x - min.x;
        }

        // 타일 오브젝트의 local bounds를 구하는 유틸리티 메서드
        private (Vector3 min, Vector3 max) GetLocalBounds(GameObject obj)
        {
            var rend = obj.GetComponent<Renderer>();
            if (rend == null)
                return (Vector3.zero, Vector3.zero);
            Bounds b = rend.bounds;

            Vector3 overallMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 overallMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3[] corners = new Vector3[8]
            {
                new Vector3(b.min.x, b.min.y, b.min.z),
                new Vector3(b.min.x, b.min.y, b.max.z),
                new Vector3(b.min.x, b.max.y, b.min.z),
                new Vector3(b.min.x, b.max.y, b.max.z),
                new Vector3(b.max.x, b.min.y, b.min.z),
                new Vector3(b.max.x, b.min.y, b.max.z),
                new Vector3(b.max.x, b.max.y, b.min.z),
                new Vector3(b.max.x, b.max.y, b.max.z),
            };

            foreach (var c in corners)
            {
                Vector3 localCorner = transform.InverseTransformPoint(c);
                overallMin = Vector3.Min(overallMin, localCorner);
                overallMax = Vector3.Max(overallMax, localCorner);
            }
            return (overallMin, overallMax);
        }

        // --- 기본 타일 생성 (기존 코드) ---
        private GameObject CreateWhiteTile()
        {
            if (Tile3DManager.Instance == null)
            {
                Debug.LogError("Tile3DManager 인스턴스가 없습니다.");
                return null;
            }
            GameTile white = GameTile.Z5;
            return Tile3DManager.Instance.Make3DTile(white.ToCustomString(), transform);
        }

        private GameObject CreateRealTile(GameTile tile)
        {
            if (Tile3DManager.Instance == null)
            {
                Debug.LogError("Tile3DManager 인스턴스가 없습니다.");
                return null;
            }
            return Tile3DManager.Instance.Make3DTile(tile.ToCustomString(), transform);
        }

        /// <summary>
        /// handTiles 리스트에 담긴 모든 타일을 즉시 재배치합니다.
        /// </summary>
        private void RepositionTiles()
        {
            for (int i = 0; i < handTiles.Count; i++)
            {
                if (handTiles[i] == null) continue;
                handTiles[i].transform.localPosition = ComputeTargetLocalPositionForIndex(i);
            }
        }

        public void clear()
        {
            foreach (var tile in handTiles)
            {
                if (tile != null)
                    Destroy(tile);
            }
            handTiles.Clear();
            tsumoTile = null;
        }

        public void MakeRealHand(GameTile? tsumoTileValue, List<GameTile> originalHandTiles)
        {
            // 기존 타일 모두 제거
            clear();

            // originalHandTiles의 복사본 생성
            List<GameTile> tilesForRealHand = new List<GameTile>(originalHandTiles);
            // tsumoTile과 동일한 타일 한 개를 제거하여 일반 핸드에서 제외
            if (tsumoTileValue.HasValue && tilesForRealHand.Contains((GameTile)tsumoTileValue))
            {
                tilesForRealHand.Remove((GameTile)tsumoTileValue);
            }

            // 일반 핸드 타일 생성 (실제 타일 생성 메서드 사용)
            foreach (var tile in tilesForRealHand)
            {
                GameObject tileObj = CreateRealTile(tile); // 기존 CreateRealTile 메서드 사용
                if (tileObj != null)
                {
                    handTiles.Add(tileObj);
                }
            }
            // 일반 타일들 즉시 재배치 (애니메이션 없이)
            RepositionTiles();

            if (tsumoTileValue.HasValue)
            {
                // 마지막에 tsumoTile 생성 및 추가
                GameObject tsumoTileObj = CreateRealTile((GameTile)tsumoTileValue);
                if (tsumoTileObj != null)
                {
                    handTiles.Add(tsumoTileObj);
                    tsumoTile = tsumoTileObj;
                }
            }
            // tsumoTile에 extra gap가 적용되도록 재배치 (ComputeTargetLocalPositionForIndex에서 extra 처리)
            RepositionTiles();
        }

        // ---------------------------
        // 초기 패 생성: 13개 또는 13개 + tsumo (총 14개)를 즉시 생성 및 배치 (tsumo는 마지막 요소)
        public void InitHand(bool includeTsumo)
        {
            // 기존 타일 삭제
            foreach (var tile in handTiles)
            {
                if (tile != null)
                    Destroy(tile);
            }
            handTiles.Clear();
            tsumoTile = null;

            int totalTiles = includeTsumo ? 14 : 13;
            for (int i = 0; i < totalTiles; i++)
            {
                // tsumo 타일은 마지막 인덱스에 배치
                if (includeTsumo && i == totalTiles - 1)
                {
                    tsumoTile = CreateWhiteTile();
                    if (tsumoTile != null)
                        handTiles.Add(tsumoTile);
                }
                else
                {
                    GameObject tile = CreateWhiteTile();
                    if (tile != null)
                        handTiles.Add(tile);
                }
            }
            // 즉시 재배치 (애니메이션 없이)
            RepositionTiles();
        }
        private IEnumerator AnimateTileRotation(GameObject tile, float baseDuration, int handScore)
        {
            if (tile == null) yield break;

            // --- 1) 회전 파트 (기존) ---
            Quaternion startRot = tile.transform.localRotation;
            Quaternion targetRot = Quaternion.Euler(-90f, startRot.eulerAngles.y, startRot.eulerAngles.z);
            float rotationDuration = baseDuration / (1f + handScore / 10f);

            float elapsed = 0f;
            while (elapsed < rotationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / rotationDuration);
                float easedT = t * t;
                float angle = Mathf.Lerp(0f, -90f, easedT);
                tile.transform.localRotation = Quaternion.Euler(angle, startRot.eulerAngles.y, startRot.eulerAngles.z);
                yield return null;
            }
            tile.transform.localRotation = targetRot;

            // --- 2) 튕김 파트 (y축 + z축) ---
            Vector3 startPos = tile.transform.localPosition;
            float bounceDuration = baseDuration * Random.Range(0.9f, 1.1f);
            float amplitudeY = Mathf.Max(1f, handScore);
            float frequency = Mathf.PI * 4f * Random.Range(0.8f, 1.2f);
            float dampingY = 3f * Random.Range(0.8f, 1.2f);
            float dampingZ = 2f * Random.Range(0.8f, 1.2f); // z축 감쇠 속도 (클수록 빨리 줄어듬)

            float prevY = 0f;
            float currentZOffset = 0f;

            elapsed = 0f;
            while (elapsed < bounceDuration)
            {
                elapsed += Time.deltaTime;
                float tt = Mathf.Clamp01(elapsed / bounceDuration);

                // y축 튕김: 댐핑된 사인파
                float y = amplitudeY * Mathf.Exp(-dampingY * tt) * Mathf.Abs(Mathf.Sin(frequency * tt));

                // 바닥에 닿았을 때(prevY > 0 && y <= 0)마다 새로운 Z 오프셋 생성
                if (prevY > 0f && y <= 0f)
                {
                    // 기본 Z 오프셋 크기: y축 amplitude의 10~30%
                    float baseZ = amplitudeY * Random.Range(0.2f, 0.5f);
                    // 랜덤 방향
                    float sign = Random.value < 0.5f ? -1f : 1f;
                    // 감쇠 적용: 진행률 tt에 따라 줄어들게
                    currentZOffset = baseZ * sign * Mathf.Exp(-dampingZ * tt);
                }

                // 위치 적용
                tile.transform.localPosition = startPos + new Vector3(0f, y, currentZOffset);

                prevY = y;
                yield return null;
            }

            // 최종 고정: 원위치로 복귀
            tile.transform.localPosition = startPos;
            tile.transform.localRotation = targetRot;
        }


        // EaseOutBounce 이징 함수 (로버트 페너 공식 참고)
        private float EaseOutBounce(float t)
        {
            if (t < (1f / 2.75f))
            {
                return 7.5625f * t * t;
            }
            else if (t < (2f / 2.75f))
            {
                t -= 1.5f / 2.75f;
                return 7.5625f * t * t + 0.75f;
            }
            else if (t < (2.5f / 2.75f))
            {
                t -= 2.25f / 2.75f;
                return 7.5625f * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / 2.75f;
                return 7.5625f * t * t + 0.984375f;
            }
        }
        // ★ 추가: 개별 타일 애니메이션 완료 후 완료 콜백을 호출하는 래퍼 코루틴
        private IEnumerator AnimateTileRotationWithCallback(GameObject tile, float duration, int handScore, System.Action onComplete)
        {
            yield return StartCoroutine(AnimateTileRotation(tile, duration, handScore));
            onComplete?.Invoke();
        }

        // ★ 수정: handTiles 전체를 도미노 효과로 순차적으로 시작하면서, 동시에 실행된 후 모든 애니메이션 종료 대기
        public IEnumerator AnimateAllTilesRotationDomino(float baseDuration, int handScore)
        {
            float delayBetweenTiles = baseDuration / 30f;
            Debug.Log("AnimateAllTilesRotationDomino 시작 - 총 타일 수: " + handTiles.Count + ", 기본 지속 시간: " + baseDuration + ", 타일 간 딜레이: " + delayBetweenTiles);

            int count = handTiles.Count;
            // 각 타일의 완료 여부를 추적하기 위한 배열
            bool[] doneFlags = new bool[count];

            // 각 타일에 대해 도미노 효과 시작 (순차적으로 시작되지만, 이전 코루틴을 기다리지 않고 바로 다음 코루틴을 시작)
            for (int i = 0; i < count; i++)
            {
                int index = i;  // 지역 변수로 복사
                GameObject tile = handTiles[index];
                if (tile != null)
                {
                    Debug.Log("AnimateTileRotation 시작 (domino): " + tile.name + " (Index " + index + ")");
                    StartCoroutine(AnimateTileRotationWithCallback(tile, baseDuration, handScore, () =>
                    {
                        Debug.Log("AnimateTileRotation 완료: " + tile.name + " (Index " + index + ")");
                        doneFlags[index] = true;
                    }));
                }
                else
                {
                    Debug.Log("AnimateTileRotation 건너뛰기 - null 타일 발견 (Index " + index + ")");
                    doneFlags[index] = true;
                }
                // 타일 간 딜레이만큼 기다림 (이전 코루틴이 끝날 때까지 기다리지 않음)
                yield return new WaitForSeconds(delayBetweenTiles);
            }

            // 모든 타일 애니메이션 코루틴이 완료될 때까지 대기
            yield return new WaitUntil(() =>
            {
                for (int i = 0; i < count; i++)
                {
                    if (!doneFlags[i])
                        return false;
                }
                return true;
            });

            Debug.Log("AnimateAllTilesRotationDomino 완료");
        }

        // ★ 추가: 모든 타일의 회전을 초기화(0도로 리셋)
        public void ResetTileRotations()
        {
            foreach (var tile in handTiles)
            {
                if (tile != null)
                {
                    tile.transform.localRotation = Quaternion.identity;
                }
            }
        }

    }
}
