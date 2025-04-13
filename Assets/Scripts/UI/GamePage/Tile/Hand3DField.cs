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
            // 기존의 discardRightmost: 단일 폐기 요청에서만 사용됨
            public bool discardRightmost;
            // DiscardMultiple 요청 시 삭제할 타일 개수
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
    }
}
