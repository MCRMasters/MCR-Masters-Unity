using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MCRGame.Common;

namespace MCRGame.UI
{
    public class Hand3DField : MonoBehaviour
    {
        // 보유 타일. tsumotile도 이 리스트에서 관리됩니다.
        public List<GameObject> handTiles = new List<GameObject>();
        // tsumotile에 대한 별도 참조
        public GameObject tsumoTile;

        // 슬라이드 애니메이션 지속시간
        public float slideDuration = 0.5f;
        // 타일 간의 간격 (타일 폭에 대한 절대값)
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

        // 타일을 생성하는 기본 함수 (예제에서는 GameTile.Z5 사용)
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

        // 일반 타일 추가: handTiles 리스트의 앞쪽(오른쪽 끝)에 삽입하여 역순으로 관리
        public void AddTile()
        {
            GameObject tileObj = CreateWhiteTile();
            if (tileObj == null)
                return;
            handTiles.Insert(0, tileObj);
            RepositionTiles();
        }

        // tsumotile 설정: 기존 tsumotile이 있으면 제거한 후 새로 생성, handTiles 리스트 앞쪽에 삽입
        public void SetTsumoTile()
        {
            if (tsumoTile != null)
            {
                handTiles.Remove(tsumoTile);
                Destroy(tsumoTile);
            }
            tsumoTile = CreateWhiteTile();
            handTiles.Insert(0, tsumoTile);
            RepositionTiles();
        }

        /// <summary>
        /// 각 타일의 Renderer를 기준으로, 부모의 로컬 좌표로 변환한 후 타일의 실제 폭(local x방향)을 계산합니다.
        /// </summary>
        private float GetTileLocalWidth(GameObject tile)
        {
            Renderer rend = tile.GetComponent<Renderer>();
            if (rend == null)
                return 1f;
            Bounds b = rend.bounds;
            Vector3 overallMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 overallMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3[] corners = new Vector3[8];
            corners[0] = new Vector3(b.min.x, b.min.y, b.min.z);
            corners[1] = new Vector3(b.min.x, b.min.y, b.max.z);
            corners[2] = new Vector3(b.min.x, b.max.y, b.min.z);
            corners[3] = new Vector3(b.min.x, b.max.y, b.max.z);
            corners[4] = new Vector3(b.max.x, b.min.y, b.min.z);
            corners[5] = new Vector3(b.max.x, b.min.y, b.max.z);
            corners[6] = new Vector3(b.max.x, b.max.y, b.min.z);
            corners[7] = new Vector3(b.max.x, b.max.y, b.max.z);
            foreach (var c in corners)
            {
                // 부모의 로컬 좌표로 변환 (Hand3DField의 transform 사용)
                Vector3 local = transform.InverseTransformPoint(c);
                overallMin = Vector3.Min(overallMin, local);
                overallMax = Vector3.Max(overallMax, local);
            }
            return overallMax.x - overallMin.x;
        }

        /// <summary>
        /// 오른쪽(인덱스 0가 오른쪽 끝)부터 왼쪽으로 배치하기 위해,
        /// 인덱스 0부터 해당 타일까지의 누적 타일 폭과 gap을 계산하여 목표 로컬 위치를 반환합니다.
        /// tsumoTile이 오른쪽 끝(인덱스 0)일 경우 추가 gap을 적용합니다.
        /// </summary>
        private Vector3 ComputeTargetLocalPositionForIndex(int index)
        {
            float offset = 0f;
            for (int i = 0; i < index; i++)
            {
                float width = GetTileLocalWidth(handTiles[i]);
                offset += width + gap;
            }
            float extraGap = 0f;
            if (index == 0 && tsumoTile != null)
            {
                extraGap = GetTileLocalWidth(handTiles[0]) * 0.5f;
            }
            // 타일들이 오른쪽에서 시작하여 왼쪽으로 쌓이도록, x축 음수 방향으로 offset 적용
            return new Vector3(- (offset + extraGap), 0f, 0f);
        }

        /// <summary>
        /// 모든 타일의 최종 로컬 위치를 누적 누적 오프셋 방식으로 재배치합니다.
        /// </summary>
        private void RepositionTiles()
        {
            for (int i = 0; i < handTiles.Count; i++)
            {
                if (handTiles[i] == null)
                    continue;
                handTiles[i].transform.localPosition = ComputeTargetLocalPositionForIndex(i);
            }
        }

        /// <summary>
        /// 지정한 인덱스의 타일 혹은 tsumotile을 제거하고,
        /// 제거된 이후 남은 타일들을 위 ComputeTargetLocalPositionForIndex()를 사용하여 슬라이드 애니메이션으로 재배치합니다.
        /// tsumotile 제거 요청인 경우 리스트에서 제거 후 tsumoTile을 null로 초기화합니다.
        /// </summary>
        public void Discard(int index, bool isTsumoTile)
        {
            discardQueue.Enqueue(new DiscardRequest(index, isTsumoTile));
            if (!isSliding)
            {
                StartCoroutine(ProcessDiscardQueue());
            }
        }

        // 큐에 쌓인 discard 요청들을 순차적으로 처리하는 코루틴
        private IEnumerator ProcessDiscardQueue()
        {
            while (discardQueue.Count > 0)
            {
                DiscardRequest request = discardQueue.Dequeue();
                yield return StartCoroutine(ProcessDiscardRequest(request));
            }
        }

        // 개별 discard 요청을 처리하는 코루틴. 슬라이드 애니메이션 도중 각 타일의 목표 위치를 위의 계산 방식으로 재계산합니다.
        private IEnumerator ProcessDiscardRequest(DiscardRequest request)
        {
            isSliding = true;
            // tsumotile 제거 요청 처리
            if (request.isTsumoTile)
            {
                if (tsumoTile != null)
                {
                    int idx = handTiles.IndexOf(tsumoTile);
                    if (idx >= 0)
                    {
                        Dictionary<GameObject, Vector3> initialPositions = new Dictionary<GameObject, Vector3>();
                        Dictionary<GameObject, Vector3> targetPositions = new Dictionary<GameObject, Vector3>();
                        // 제거되는 tsumotile 이후의 타일들에 대해 재배치
                        for (int i = idx + 1; i < handTiles.Count; i++)
                        {
                            GameObject tile = handTiles[i];
                            if (tile == null) continue;
                            initialPositions[tile] = tile.transform.localPosition;
                            Vector3 newPos = ComputeTargetLocalPositionForIndex(i - 1);
                            targetPositions[tile] = newPos;
                        }

                        float elapsed = 0f;
                        while (elapsed < slideDuration)
                        {
                            elapsed += Time.deltaTime;
                            float t = Mathf.Clamp01(elapsed / slideDuration);
                            foreach (var kvp in targetPositions)
                            {
                                if (kvp.Key == null) continue;
                                kvp.Key.transform.localPosition = Vector3.Lerp(initialPositions[kvp.Key], kvp.Value, t);
                            }
                            yield return null;
                        }
                        foreach (var kvp in targetPositions)
                        {
                            if (kvp.Key != null)
                                kvp.Key.transform.localPosition = kvp.Value;
                        }
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
                    GameObject discardedTile = handTiles[request.index];
                    handTiles.RemoveAt(request.index);
                    if (discardedTile != null)
                        Destroy(discardedTile);
                }
                Dictionary<GameObject, Vector3> initialPositions = new Dictionary<GameObject, Vector3>();
                Dictionary<GameObject, Vector3> targetPositions = new Dictionary<GameObject, Vector3>();
                for (int i = request.index; i < handTiles.Count; i++)
                {
                    GameObject tile = handTiles[i];
                    if (tile == null) continue;
                    initialPositions[tile] = tile.transform.localPosition;
                    Vector3 newPos = ComputeTargetLocalPositionForIndex(i);
                    targetPositions[tile] = newPos;
                }
                float elapsedAnim = 0f;
                while (elapsedAnim < slideDuration)
                {
                    elapsedAnim += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsedAnim / slideDuration);
                    foreach (var kvp in targetPositions)
                    {
                        if (kvp.Key == null) continue;
                        kvp.Key.transform.localPosition = Vector3.Lerp(initialPositions[kvp.Key], kvp.Value, t);
                    }
                    yield return null;
                }
                foreach (var kvp in targetPositions)
                {
                    if (kvp.Key != null)
                        kvp.Key.transform.localPosition = kvp.Value;
                }
                tsumoTile = null;
            }
            isSliding = false;
        }
    }
}
