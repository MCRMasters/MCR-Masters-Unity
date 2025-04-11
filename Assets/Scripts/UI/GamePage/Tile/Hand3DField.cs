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

        private enum RequestType { Tsumo, Discard, InitFlowerTsumo }
        private struct HandRequest
        {
            public RequestType Type;
            public bool discardRightmost;
            public HandRequest(RequestType type, bool discardRightmost = false)
            {
                Type = type;
                this.discardRightmost = discardRightmost;
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
        /// 오른쪽 끝이 아닌 타일 중 랜덤을 버리고,
        /// 요청 큐가 모두 처리될 때까지 대기합니다.
        /// </summary>
        public IEnumerator RequestDiscardRandom()
        {
            EnqueueRequest(new HandRequest(RequestType.Discard, discardRightmost: false));
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
                // tsumo 타일 생성
                GameObject newTile = CreateWhiteTile();
                if (newTile != null)
                {
                    tsumoTile = newTile;
                    handTiles.Insert(0, newTile);
                    RepositionTiles(); // 즉시 재배치
                }
            }
            else if (req.Type == RequestType.Discard)
            {
                if (handTiles.Count == 0)
                    yield break;

                int idx;
                if (req.discardRightmost || handTiles.Count == 1)
                {
                    // 오른쪽 끝(리스트의 첫 번째 요소)을 제거
                    idx = 0;
                }
                else
                {
                    // 1~Count-1 사이 랜덤
                    idx = Random.Range(1, handTiles.Count);
                }

                // 제거 대상 오브젝트
                GameObject toRemove = handTiles[idx];

                // GameHand 데이터 반영은 외부에서 처리되었다고 가정(여기서는 UI만 제거)
                handTiles.RemoveAt(idx);
                if (toRemove != null)
                    Destroy(toRemove);

                // 남은 타일들 슬라이드 애니메이션으로 재배치
                yield return StartCoroutine(AnimateReposition());
            }
            else if (req.Type == RequestType.InitFlowerTsumo)
            {
                GameObject newTile = CreateWhiteTile();
                if (newTile != null)
                {
                    handTiles.Insert(0, newTile);
                    if (handTiles.Count == GameHand.FULL_HAND_SIZE)
                        tsumoTile = newTile;
                    else
                        tsumoTile = null;
                    RepositionTiles();
                }
            }
        }

        // --- AnimateReposition 재활용 (기존 코드) ---
        private IEnumerator AnimateReposition()
        {
            if (handTiles.Count == 0) yield break;

            // 초기/목표 위치 계산
            var initial = new Dictionary<GameObject, Vector3>();
            var target = new Dictionary<GameObject, Vector3>();
            for (int i = 0; i < handTiles.Count; i++)
            {
                var go = handTiles[i];
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
            // 최종 위치 확정
            foreach (var kv in target)
                kv.Key.transform.localPosition = kv.Value;
        }

        // --- 위치 계산 유틸들 (기존 코드) ---
        private Vector3 ComputeTargetLocalPositionForIndex(int index)
        {
            float offset = 0f;
            for (int i = 0; i < index; i++)
                offset += GetTileLocalWidth(handTiles[i]) + gap;
            float extra = (index == 0 && tsumoTile != null)
                ? GetTileLocalWidth(handTiles[0]) * 0.5f
                : 0f;
            return new Vector3(-(offset + extra), 0f, 0f);
        }

        private float GetTileLocalWidth(GameObject tile)
        {
            Renderer r = tile.GetComponent<Renderer>();
            if (r == null) return 1f;
            Bounds b = r.bounds;
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3[] corners = {
                new Vector3(b.min.x, b.min.y, b.min.z), new Vector3(b.min.x, b.min.y, b.max.z),
                new Vector3(b.min.x, b.max.y, b.min.z), new Vector3(b.min.x, b.max.y, b.max.z),
                new Vector3(b.max.x, b.min.y, b.min.z), new Vector3(b.max.x, b.min.y, b.max.z),
                new Vector3(b.max.x, b.max.y, b.min.z), new Vector3(b.max.x, b.max.y, b.max.z)
            };
            foreach (var c in corners)
            {
                Vector3 local = transform.InverseTransformPoint(c);
                min = Vector3.Min(min, local);
                max = Vector3.Max(max, local);
            }
            return max.x - min.x;
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

        // Hand3DField 클래스 내부에 추가
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
        // 초기 패 생성: 13개의 패 또는 13개 + tsumo 타일 (총 14개)를 애니메이션 없이 즉시 생성 및 배치
        /// <summary>
        /// 초기 패를 생성합니다.
        /// includeTsumo이 true이면 tsumo 타일을 포함하여 총 14개의 패, 그렇지 않으면 13개의 패를 즉시 생성 및 배치합니다.
        /// </summary>
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
                // 만약 tsumo 타일이 포함된다면, tsumo 타일은 오른쪽 끝(리스트의 첫 번째 요소)에 배치
                if (includeTsumo && i == 0)
                {
                    tsumoTile = CreateWhiteTile();
                    if (tsumoTile != null)
                        handTiles.Insert(0, tsumoTile);
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
