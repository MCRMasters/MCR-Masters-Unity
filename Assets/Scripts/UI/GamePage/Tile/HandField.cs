using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MCRGame.Common;

namespace MCRGame.UI
{
    public class HandField : MonoBehaviour
    {
        // 보유 타일. tsumotile도 이 리스트에서 관리됩니다.
        public List<GameObject> handTiles = new List<GameObject>();
        // tsumotile에 대한 별도 참조. handTiles의 마지막 요소여야 함.
        public GameObject tsumoTile;

        // 슬라이드 애니메이션 지속시간
        public float slideDuration = 0.5f;
        // 타일 간의 간격 (타일 너비에 대한 절대값 혹은 비율로 조정 가능)
        public float gap = 0.1f;

        // discard 요청들을 순차적으로 처리하기 위한 큐
        private Queue<DiscardRequest> discardQueue = new Queue<DiscardRequest>();
        private bool isSliding = false;

        // discard 요청 구조체
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

        // 매개변수를 받지 않고 GameTile.Z5(white tile)를 생성하는 함수
        private GameObject CreateWhiteTile()
        {
            if (Tile3DManager.Instance == null)
            {
                Debug.LogError("Tile3DManager 인스턴스가 없습니다.");
                return null;
            }

            GameTile whiteTile = GameTile.Z5;
            GameObject tileObj = Tile3DManager.Instance.Make3DTile(whiteTile.ToCustomString(), transform);
            if (tileObj == null)
            {
                Debug.LogError($"타일 생성 실패: {whiteTile.ToCustomString()}");
            }
            return tileObj;
        }

        // 일반 타일 추가: handTiles 리스트에 추가
        public void AddTile()
        {
            GameObject tileObj = CreateWhiteTile();
            if (tileObj == null)
                return;

            handTiles.Add(tileObj);
            RepositionTiles();
        }

        // tsumotile 설정: 기존 tsumotile이 있다면 제거한 후 새로 생성하여 handTiles 리스트의 마지막에 추가
        public void SetTsumoTile()
        {
            if (tsumoTile != null)
            {
                handTiles.Remove(tsumoTile);
                Destroy(tsumoTile);
            }
            tsumoTile = CreateWhiteTile();
            handTiles.Add(tsumoTile);
            RepositionTiles();
        }

        // handTiles에 포함된 모든 타일을 재배치. 만약 마지막 타일이 tsumotile이면 추가 gap(타일 너비의 0.5f)을 적용.
        private void RepositionTiles()
        {
            float tileWidth = GetTileWidth();
            for (int i = 0; i < handTiles.Count; i++)
            {
                if (handTiles[i] == null)
                    continue;

                float extraGap = 0f;
                // 마지막 요소가 tsumoTile이면 extraGap 적용
                if (tsumoTile != null && handTiles[i] == tsumoTile && i == handTiles.Count - 1)
                {
                    extraGap = tileWidth * 0.5f;
                }
                Vector3 targetPos = new Vector3(i * (tileWidth + gap) + extraGap, 0f, 0f);
                handTiles[i].transform.localPosition = targetPos;
            }
        }

        // 첫 타일의 Renderer를 기준으로 타일 너비 계산 (없으면 기본 1)
        private float GetTileWidth()
        {
            if (handTiles.Count > 0 && handTiles[0] != null)
            {
                Renderer rend = handTiles[0].GetComponent<Renderer>();
                if (rend != null)
                    return rend.bounds.size.x;
            }
            return 1f;
        }

        /// <summary>
        /// 지정한 인덱스의 타일 혹은 tsumotile을 제거하고,
        /// 오른쪽에 위치한 타일들(handTiles에 포함된)이 slide하며 재배치되도록 함.
        /// tsumotile 제거 요청이면 handTiles에서 제거 후 tsumoTile을 null로 비움.
        /// 일반 타일 제거 요청이어도 tsumoTile 변수에는 null이 들어가야 하며,
        /// tsumotile의 레퍼런스는 삭제되지 않고 리스트에 남아 일반 타일로 관리됩니다.
        /// </summary>
        /// <param name="index">
        /// 일반 타일일 경우 handTiles 내의 인덱스,
        /// tsumotile 제거 요청인 경우에는 무시됨.
        /// </param>
        /// <param name="isTsumoTile">true이면 tsumotile 제거</param>
        public void Discard(int index, bool isTsumoTile)
        {
            discardQueue.Enqueue(new DiscardRequest(index, isTsumoTile));
            if (!isSliding)
            {
                StartCoroutine(ProcessDiscardQueue());
            }
        }

        // 큐에 쌓인 discard 요청들을 순차 처리하는 코루틴
        private IEnumerator ProcessDiscardQueue()
        {
            while (discardQueue.Count > 0)
            {
                DiscardRequest request = discardQueue.Dequeue();
                yield return StartCoroutine(ProcessDiscardRequest(request));
            }
        }

        // 개별 discard 요청 처리 코루틴
        private IEnumerator ProcessDiscardRequest(DiscardRequest request)
        {
            isSliding = true;
            float tileWidth = GetTileWidth();
            float displacement = tileWidth + gap;

            if (request.isTsumoTile)
            {
                // tsumotile 제거 요청 처리
                if (tsumoTile != null)
                {
                    int idx = handTiles.IndexOf(tsumoTile);
                    if (idx >= 0)
                    {
                        // tsumotile 제거 전, 오른쪽 타일들(없을 수 있음)의 slide 애니메이션 처리
                        Dictionary<GameObject, Vector3> initialPositions = new Dictionary<GameObject, Vector3>();
                        Dictionary<GameObject, Vector3> targetPositions = new Dictionary<GameObject, Vector3>();

                        // tsumotile은 제거 대상이므로, idx부터 끝까지 재배치
                        for (int i = idx + 1; i < handTiles.Count; i++)
                        {
                            GameObject tile = handTiles[i];
                            if (tile == null)
                                continue;
                            initialPositions[tile] = tile.transform.localPosition;
                            Vector3 newPos = new Vector3((i - 1) * displacement, 0f, 0f);
                            targetPositions[tile] = newPos;
                        }

                        float elapsed = 0f;
                        while (elapsed < slideDuration)
                        {
                            elapsed += Time.deltaTime;
                            float t = Mathf.Clamp01(elapsed / slideDuration);
                            foreach (var kvp in targetPositions)
                            {
                                GameObject tile = kvp.Key;
                                if (tile == null)
                                    continue;
                                Vector3 start = initialPositions[tile];
                                Vector3 end = kvp.Value;
                                tile.transform.localPosition = Vector3.Lerp(start, end, t);
                            }
                            yield return null;
                        }
                        foreach (var kvp in targetPositions)
                        {
                            if (kvp.Key != null)
                                kvp.Key.transform.localPosition = kvp.Value;
                        }

                        // 제거 대상인 tsumotile 제거 (리스트에서는 제거)
                        handTiles.RemoveAt(idx);
                        Destroy(tsumoTile);
                        tsumoTile = null;
                    }
                }
            }
            else
            {
                // 일반 타일 제거 요청 처리
                if (request.index < 0 || request.index >= handTiles.Count)
                {
                    Debug.LogWarning("Discard 인덱스 범위 초과");
                    isSliding = false;
                    yield break;
                }

                bool targetIsTsumo = (handTiles[request.index] == tsumoTile);
                if (!targetIsTsumo)
                {
                    // 일반 타일 제거: 해당 타일을 리스트에서 제거
                    GameObject discardedTile = handTiles[request.index];
                    handTiles.RemoveAt(request.index);
                    if (discardedTile != null)
                    {
                        Destroy(discardedTile);
                    }
                }
                // 슬라이드 애니메이션 처리 (제거 여부와 상관없이 request.index부터 재배치)
                Dictionary<GameObject, Vector3> initialPositions = new Dictionary<GameObject, Vector3>();
                Dictionary<GameObject, Vector3> targetPositions = new Dictionary<GameObject, Vector3>();

                for (int i = request.index; i < handTiles.Count; i++)
                {
                    GameObject tile = handTiles[i];
                    if (tile == null)
                        continue;

                    initialPositions[tile] = tile.transform.localPosition;
                    // tsumoTile 변수가 null이 된 상태라 extraGap 없이 재배치
                    Vector3 newPos = new Vector3(i * displacement, 0f, 0f);
                    targetPositions[tile] = newPos;
                }

                float elapsed = 0f;
                while (elapsed < slideDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / slideDuration);
                    foreach (var kvp in targetPositions)
                    {
                        GameObject tile = kvp.Key;
                        if (tile == null)
                            continue;
                        Vector3 start = initialPositions[tile];
                        Vector3 end = kvp.Value;
                        tile.transform.localPosition = Vector3.Lerp(start, end, t);
                    }
                    yield return null;
                }
                foreach (var kvp in targetPositions)
                {
                    if (kvp.Key != null)
                        kvp.Key.transform.localPosition = kvp.Value;
                }
                // 일반 타일 제거 요청 시, tsumoTile 변수는 null로 설정 (tsumotile 객체는 리스트에 그대로 남음)
                tsumoTile = null;
            }
            isSliding = false;
        }
    }
}
