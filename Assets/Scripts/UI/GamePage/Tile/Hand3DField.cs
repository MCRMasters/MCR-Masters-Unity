using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
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

        // --- 퍼블릭 API: 요청 등록 및 완료 대기 ---
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
            EnqueueRequest(new HandRequest(RequestType.Discard, true));
            yield return new WaitUntil(() => !isProcessing && requestQueue.Count == 0);
        }

        public IEnumerator RequestDiscardRandom()
        {
            EnqueueRequest(new HandRequest(RequestType.Discard, false));
            yield return new WaitUntil(() => !isProcessing && requestQueue.Count == 0);
        }

        public IEnumerator RequestDiscardMultiple(int count)
        {
            EnqueueRequest(new HandRequest(RequestType.DiscardMultiple, false, count));
            yield return new WaitUntil(() => !isProcessing && requestQueue.Count == 0);
        }

        // --- 내부: 요청 큐 처리 ---
        private void EnqueueRequest(HandRequest req)
        {
            requestQueue.Enqueue(req);
            if (!isProcessing)
                ProcessRequestQueue();
        }

        private void ProcessRequestQueue()
        {
            if (requestQueue.Count == 0)
            {
                isProcessing = false;
                return;
            }
            isProcessing = true;
            var req = requestQueue.Dequeue();
            HandleRequest(req)
                .OnComplete(() => ProcessRequestQueue())
                .Play();
        }

        // --- 요청별 처리: DOTween Sequence 반환 ---
        private Sequence HandleRequest(HandRequest req)
        {
            var seq = DOTween.Sequence();
            switch (req.Type)
            {
                case RequestType.Tsumo:
                    seq.AppendCallback(() =>
                    {
                        var tile = CreateWhiteTile();
                        if (tile != null)
                        {
                            tsumoTile = tile;
                            handTiles.Add(tile);
                            RepositionInstant();
                        }
                    });
                    break;
                case RequestType.InitFlowerTsumo:
                    seq.AppendCallback(() =>
                    {
                        var tile = CreateWhiteTile();
                        if (tile != null)
                        {
                            handTiles.Add(tile);
                            tsumoTile = (handTiles.Count == GameHand.FULL_HAND_SIZE) ? tile : null;
                            RepositionInstant();
                        }
                    });
                    break;
                case RequestType.Discard:
                    seq.Insert(0, HandleDiscard(req.discardRightmost));
                    break;
                case RequestType.DiscardMultiple:
                    seq.Insert(0, HandleDiscardMultiple(req.discardCount));
                    break;
            }
            return seq;
        }

        private Sequence HandleDiscard(bool rightmost)
        {
            var seq = DOTween.Sequence();
            if (handTiles.Count == 0) return seq;

            int idx = rightmost || handTiles.Count == 1
                ? handTiles.Count - 1
                : UnityEngine.Random.Range(0, handTiles.Count - 1);

            var toRemove = handTiles[idx];
            handTiles.RemoveAt(idx);
            tsumoTile = null;

            seq.AppendInterval(0.2f)
               .AppendCallback(() => Destroy(toRemove))
               .Append(RepositionTween(slideDuration));
            return seq;
        }

        private Sequence HandleDiscardMultiple(int count)
        {
            var seq = DOTween.Sequence();
            if (handTiles.Count == 0) return seq;

            var candidates = new List<int>();
            int limit = tsumoTile != null ? handTiles.Count - 1 : handTiles.Count;
            for (int i = 0; i < limit; i++) candidates.Add(i);
            int removeCount = Mathf.Min(count, candidates.Count);

            for (int i = 0; i < candidates.Count; i++)
            {
                int r = UnityEngine.Random.Range(i, candidates.Count);
                var tmp = candidates[i]; candidates[i] = candidates[r]; candidates[r] = tmp;
            }
            candidates.Sort((a, b) => b.CompareTo(a));

            seq.AppendInterval(0.2f)
               .AppendCallback(() =>
               {
                   for (int i = 0; i < removeCount; i++)
                   {
                       int idx = candidates[i];
                       Destroy(handTiles[idx]);
                       handTiles.RemoveAt(idx);
                   }
               })
               .Append(RepositionTween(slideDuration));

            return seq;
        }

        // --- 재배치 ---
        private void RepositionInstant()
        {
            for (int i = 0; i < handTiles.Count; i++)
                handTiles[i].transform.localPosition = ComputePosition(i);
        }

        private Tween RepositionTween(float duration)
        {
            var seq = DOTween.Sequence();
            for (int i = 0; i < handTiles.Count; i++)
            {
                var tile = handTiles[i];
                seq.Join(tile.transform.DOLocalMove(ComputePosition(i), duration).SetEase(Ease.InOutQuad));
            }
            return seq;
        }

        private Vector3 ComputePosition(int index)
        {
            if (handTiles.Count == 0) return Vector3.zero;
            float width = GetTileWidth(handTiles[0]);
            float offset = index * (width + gap);
            float extra = (index == handTiles.Count - 1 && tsumoTile != null)
                ? width * 0.5f : 0f;
            return new Vector3(-(offset + extra), 0f, 0f);
        }
        private float GetTileWidth(GameObject tile)
        {
            // Hand3DField 로컬에서 측정한 min/max 사용
            var (min, max) = GetBounds(tile);
            return max.x - min.x;
        }

        private (Vector3 min, Vector3 max) GetBounds(GameObject obj)
        {
            // 1) 자식 포함 모든 Renderer 수집
            var renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return (Vector3.zero, Vector3.zero);

            // 2) 월드 바운드 합치기
            Bounds combined = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                combined.Encapsulate(renderers[i].bounds);

            // 3) 8개 코너를 **Hand3DField 기준 로컬**로 변환
            Vector3[] corners = new Vector3[8]
            {
        new Vector3(combined.min.x, combined.min.y, combined.min.z),
        new Vector3(combined.min.x, combined.min.y, combined.max.z),
        new Vector3(combined.min.x, combined.max.y, combined.min.z),
        new Vector3(combined.min.x, combined.max.y, combined.max.z),
        new Vector3(combined.max.x, combined.min.y, combined.min.z),
        new Vector3(combined.max.x, combined.min.y, combined.max.z),
        new Vector3(combined.max.x, combined.max.y, combined.min.z),
        new Vector3(combined.max.x, combined.max.y, combined.max.z),
            };

            Vector3 localMin = Vector3.positiveInfinity;
            Vector3 localMax = Vector3.negativeInfinity;
            foreach (var worldCorner in corners)
            {
                // **핵심**: Hand3DField.transform 기준으로 변환해야 폭이 맞음
                Vector3 lc = transform.InverseTransformPoint(worldCorner);
                localMin = Vector3.Min(localMin, lc);
                localMax = Vector3.Max(localMax, lc);
            }
            return (localMin, localMax);
        }

        // --- 타일 생성 ---
        private GameObject CreateWhiteTile()
        {
            if (Tile3DManager.Instance == null)
            {
                Debug.LogError("Tile3DManager 인스턴스가 없습니다.");
                return null;
            }
            return Tile3DManager.Instance.Make3DTile(GameTile.Z5.ToCustomString(), transform);
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

        // --- Hand 초기화/리로드 ---
        public void InitHand(bool includeTsumo)
        {
            clear();
            int total = includeTsumo ? 14 : 13;
            for (int i = 0; i < total; i++)
            {
                if (includeTsumo && i == total - 1)
                {
                    tsumoTile = CreateWhiteTile();
                    if (tsumoTile != null) handTiles.Add(tsumoTile);
                }
                else
                {
                    var tile = CreateWhiteTile();
                    if (tile != null) handTiles.Add(tile);
                }
            }
            RepositionInstant();
        }

        public void ReloadInitHand(int handCount, bool includeTsumo)
        {
            clear();
            int total = handCount;
            for (int i = 0; i < total; i++)
            {
                if (includeTsumo && i == total - 1)
                {
                    tsumoTile = CreateWhiteTile();
                    if (tsumoTile != null) handTiles.Add(tsumoTile);
                }
                else
                {
                    var tile = CreateWhiteTile();
                    if (tile != null) handTiles.Add(tile);
                }
            }
            RepositionInstant();
        }

        // --- Real Hand 생성 ---
        public GameObject MakeRealHand(GameTile winningTile, List<GameTile> originalHandTiles, bool isTsumo)
        {
            clear();
            var tiles = new List<GameTile>(originalHandTiles);
            if (tiles.Contains(winningTile)) tiles.Remove(winningTile);
            GameObject go;
            foreach (var t in tiles)
            {
                go = CreateRealTile(t);
                if (go != null) handTiles.Add(go);
            }
            RepositionInstant();
            if (isTsumo)
            {
                go = CreateRealTile(winningTile);
                if (go != null)
                {
                    handTiles.Add(go);
                    tsumoTile = go;
                }
            }
            else
            {
                go = null;
            }
            RepositionInstant();
            return go;
        }

        public Sequence AnimateTileRotation(GameObject tile, float baseDuration, int handScore)
        {
            var seq = DOTween.Sequence();
            if (tile == null) return seq;

            // 1) 회전 애니메이션 (원래대로)
            var startRot = tile.transform.localRotation;
            var euler = startRot.eulerAngles;
            float rotDuration = baseDuration / (1f + handScore / 10f);
            seq.Append(
                tile.transform
                    .DOLocalRotate(new Vector3(-90f, euler.y, euler.z), rotDuration)
                    .SetEase(Ease.OutQuad)
            );

            // 2) 위치 애니메이션을 위한 준비
            Vector3 startPos = tile.transform.localPosition;
            float amp = Mathf.Max(1f, handScore);

            // ▷ 여기서 이동 크기를 조정할 스케일 값을 정의합니다.
            //    예: 0.1f → 원래 값의 10%만큼만 움직임
            float positionScale = 0.1f;

            float freq = Mathf.PI * 4f * UnityEngine.Random.Range(0.8f, 1.2f);
            float dampY = 3f * UnityEngine.Random.Range(0.8f, 1.2f);
            float dampZ = 2f * UnityEngine.Random.Range(0.8f, 1.2f);
            float duration = baseDuration * UnityEngine.Random.Range(0.9f, 1.1f);

            float prevY = 0f;
            float currZ = 0f;

            // 3) 커스텀 Tween으로 Y,Z 움직임을 계산
            seq.Append(
                DOTween.To(
                    () => 0f,
                    elapsedValue =>
                    {
                        float elapsed = elapsedValue;
                        float t = Mathf.Clamp01(elapsed / duration);

                        // 원래 y 계산(탄젠트 + 감쇠)
                        float rawY = amp * Mathf.Exp(-dampY * t) * Mathf.Abs(Mathf.Sin(freq * t));

                        // 이전 y가 양수였다가 지금 y가 0 이하로 바뀔 때만 Z 축 흔들림 계산
                        if (prevY > 0f && rawY <= 0f)
                        {
                            float baseZ = amp * UnityEngine.Random.Range(0.2f, 0.5f);
                            float sign = (UnityEngine.Random.value < 0.5f) ? -1f : 1f;
                            currZ = baseZ * sign * Mathf.Exp(-dampZ * t);
                        }

                        // ▷ Y와 Z를 서로 바꿔서 적용(원래는 new Vector3(0, rawY, currZ))
                        //    그리고 positionScale로 전체 크기를 조절
                        float moveY = currZ * positionScale; // 원래 Z 로직 → Y축으로
                        float moveZ = rawY * positionScale; // 원래 Y 로직 → Z축으로

                        tile.transform.localPosition = startPos + new Vector3(0f, moveY, moveZ);
                        prevY = rawY;
                    },
                    duration,
                    duration
                ).SetEase(Ease.Linear)
            );

            // 4) 마지막에 원복
            seq.AppendCallback(() =>
            {
                tile.transform.localPosition = startPos;
                tile.transform.localRotation = Quaternion.Euler(-90f, euler.y, euler.z);
            });

            return seq;
        }

        public Sequence AnimateTileRotationWithCallback(GameObject tile, float duration, int handScore, Action onComplete)
        {
            var seq = AnimateTileRotation(tile, duration, handScore);
            seq.AppendCallback(() => onComplete?.Invoke());
            return seq;
        }

        public Sequence AnimateAllTilesRotationDomino(float baseDuration, int handScore)
        {
            float delay = baseDuration / 30f;
            var master = DOTween.Sequence();

            for (int i = 0; i < handTiles.Count; i++)
            {
                int idx = i;
                var tile = handTiles[idx];
                var tileSeq = DOTween.Sequence()
                    .AppendInterval(idx * delay)
                    .Append(AnimateTileRotation(tile, baseDuration, handScore));
                master.Join(tileSeq);
            }

            return master;
        }

        public void ResetTileRotations()
        {
            foreach (var tile in handTiles)
            {
                if (tile != null)
                    tile.transform.localRotation = Quaternion.identity;
            }
        }

        // --- 클리어 ---
        public void clear()
        {
            foreach (var t in handTiles) if (t != null) Destroy(t);
            handTiles.Clear();
            requestQueue.Clear();
            tsumoTile = null;
        }
    }
}
