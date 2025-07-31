using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using MCRGame.Common;
using MCRGame.UI;
using MCRGame.Net;
using System;

namespace MCRGame.Game
{
    public class FlowerReplacementController : MonoBehaviour
    {
        public static FlowerReplacementController Instance { get; private set; }

        [Header("Effect Prefabs")]
        [SerializeField] private GameObject flowerPhaseEffectPrefab;
        [SerializeField] private GameObject roundStartEffectPrefab;

        /*──────────────────────────────────────────────*/
        /*  Life-cycle                                  */
        /*──────────────────────────────────────────────*/
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Debug.Log("[FR] Awake");
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "GameScene")
            {
                ResetState();
                Debug.Log("[FR] OnSceneLoaded → ResetState()");
            }
        }

        private void ResetState()
        {
            Debug.Log("[FR] ResetState : StopAllCoroutines");
            StopAllCoroutines();
            // 진행 중이던 화패 카운트 애니메이션의 스케일을 원래대로 복구
            GameManager.Instance?.ResetFlowerCountContainerScales();
        }

        /*──────────────────────────────────────────────*/
        /*  Public Entry                                */
        /*──────────────────────────────────────────────*/
        public IEnumerator StartFlowerReplacement(
            List<GameTile> newTiles,
            List<GameTile> appliedFlowers,
            List<int> flowerCounts)
        {
            Debug.Log($"[FR] ▶ StartFlowerReplacement  new={newTiles.Count}, " +
                      $"applied={appliedFlowers.Count}, counts=({string.Join(",", flowerCounts)})");

            yield return GameManager.Instance.GameHandManager
                .RunExclusive(FlowerReplacementCoroutine(newTiles, appliedFlowers, flowerCounts));

            Debug.Log("[FR] ◀ StartFlowerReplacement finished");

            GameManager.Instance.GameHandManager.IsAnimating = false;
            GameManager.Instance.CanClick = false;
        }

        /*──────────────────────────────────────────────*/
        /*  Core Coroutine                              */
        /*──────────────────────────────────────────────*/
        private IEnumerator FlowerReplacementCoroutine(
            List<GameTile> newTiles,
            List<GameTile> appliedFlowers,
            List<int> flowerCounts)
        {
            Debug.Log("[FR]   FlowerReplacementCoroutine BEGIN");

            GameManager gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.LogError("[FR] GameManager.Instance is null - abort");
                yield break;
            }

            /* 1) 캔버스 찾기 */
            GameObject canvas = gm._canvasInstance;
            Transform canvasTr = canvas != null ? canvas.transform : transform;

            /* 2) PHASE 효과 */
            GameObject effectGO = null;
            if (flowerPhaseEffectPrefab != null)
            {
                effectGO = Instantiate(flowerPhaseEffectPrefab, canvasTr);
                Debug.Log("[FR]   Flower phase effect instanced");
                var img = effectGO.GetComponentInChildren<Image>();
                img.raycastTarget = false;
                yield return StartCoroutine(FadeIn(img, 0.2f));
            }

            /* 3) 교체 루프 */
            AbsoluteSeat[] seats = { AbsoluteSeat.EAST, AbsoluteSeat.SOUTH, AbsoluteSeat.WEST, AbsoluteSeat.NORTH };
            for (int s = 0; s < seats.Length; ++s)
            {
                var abs = seats[s];
                int cnt = flowerCounts[(int)abs];
                if (cnt == 0)
                {
                    Debug.Log($"[FR]   Seat {abs} → skip (0)");
                    continue;
                }

                Debug.Log($"[FR]   Seat {abs} → {cnt} flower(s)");
                RelativeSeat rel = RelativeSeatExtensions.CreateFromAbsoluteSeats(gm.MySeat, abs);

                for (int i = 0; i < cnt; ++i)
                {
                    Debug.Log($"[FR]     Seat {abs}/{rel}  step {i + 1}/{cnt}");

                    if (rel == RelativeSeat.SELF)
                    {
                        Debug.Log("[FR]       SELF animation");
                        int prev = gm.flowerCountMap[rel];
                        int next = prev + 1;
                        gm.flowerCountMap[rel] = next; // update map immediately so later iterations see correct value
                        bool animDone = false;

                        gm.PlayFlowerCountAnimation(rel, prev, next, () => animDone = true);

                        yield return gm.GameHandManager
                            .RunExclusive(gm.GameHandManager.ApplyFlowerSequence(appliedFlowers[i]));
                        yield return new WaitUntil(() => animDone);
                        yield return gm.GameHandManager
                            .RunExclusive(gm.GameHandManager.AddInitFlowerTsumoSequence(newTiles[i]));

                        gm.SetFlowerCount(rel, gm.flowerCountMap[rel]);
                        gm.UpdateLeftTilesByDelta(-1);
                    }
                    else
                    {
                        Debug.Log("[FR]       OPPONENT animation");
                        int prev = gm.flowerCountMap[rel];
                        int next = prev + 1;
                        gm.flowerCountMap[rel] = next;

                        yield return gm.PlayFlowerCountAnimation(rel, prev, next, null);
                        yield return StartCoroutine(ProcessOpponentFlowerOperation(
                            gm.playersHand3DFields[(int)rel], null));

                        gm.SetFlowerCount(rel, gm.flowerCountMap[rel]);
                        gm.UpdateLeftTilesByDelta(-1);
                    }

                    yield return new WaitForSeconds(0.3f);
                }
            }

            /* 4) 효과 Fade-out */
            if (effectGO != null)
            {
                Debug.Log("[FR]   Flower phase effect fade-out");
                var img = effectGO.GetComponentInChildren<Image>();
                yield return StartCoroutine(FadeOut(img, 0.2f));
                Destroy(effectGO);
            }

            /* 5) ROUND START 효과 */
            if (roundStartEffectPrefab != null)
            {
                Debug.Log("[FR]   RoundStart effect");
                var go = Instantiate(roundStartEffectPrefab, canvasTr);
                var img = go.GetComponentInChildren<Image>();
                img.raycastTarget = false;
                yield return StartCoroutine(FadeInAndOut(img, 0.2f, 0.7f));
                Destroy(go);
            }

            /* 6) 서버 OK 전송 */
            if (GameWS.Instance != null)
            {
                Debug.Log("[FR]   Send INIT_FLOWER_OK");
                GameWS.Instance.SendGameEvent(
                    GameWSActionType.GAME_EVENT,
                    new
                    {
                        event_type = (int)GameEventType.INIT_FLOWER_OK,
                        data = new Dictionary<string, object>()
                    });
            }
            else
            {
                Debug.LogWarning("[FR]   GameWS.Instance is null – OK not sent");
            }

            Debug.Log("[FR]   FlowerReplacementCoroutine END");
        }

        /*──────────────────────────────────────────────*/
        /*  Helper Coroutines                           */
        /*──────────────────────────────────────────────*/
        private IEnumerator ProcessOpponentFlowerOperation(
            Hand3DField handField,
            Action onComplete)
        {
            Debug.Log("[FR]       Opponent discard + tsumo");
            yield return handField.RequestDiscardRandom();
            yield return handField.RequestInitFlowerTsumo();
            onComplete?.Invoke();
        }

        // Fade helpers with simple logs (원하면 로그 더 추가)
        private IEnumerator FadeIn(Image img, float d)
        {
            Color c = img.color; float t = 0;
            while (t < d)
            {
                t += Time.deltaTime;
                img.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(t / d));
                yield return null;
            }
        }

        private IEnumerator FadeOut(Image img, float d)
        {
            Color c = img.color; float t = 0;
            while (t < d)
            {
                t += Time.deltaTime;
                img.color = new Color(c.r, c.g, c.b, 1 - Mathf.Clamp01(t / d));
                yield return null;
            }
        }

        private IEnumerator FadeInAndOut(Image img, float d, float hold)
        {
            yield return FadeIn(img, d);
            yield return new WaitForSeconds(hold);
            yield return FadeOut(img, d);
        }
    }
}
