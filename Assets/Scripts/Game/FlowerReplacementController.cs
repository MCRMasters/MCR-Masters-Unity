using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// GameManager 쪽에서 호출하는 진입점
        public IEnumerator StartFlowerReplacement(
            List<GameTile> newTiles,
            List<GameTile> appliedFlowers,
            List<int> flowerCounts)
        {
            yield return GameManager.Instance.GameHandManager.RunExclusive(FlowerReplacementCoroutine(newTiles: newTiles, appliedFlowers: appliedFlowers, flowerCounts: flowerCounts));
            GameManager.Instance.GameHandManager.IsAnimating = false;
            GameManager.Instance.CanClick = false;
        }

        private IEnumerator FlowerReplacementCoroutine(
            List<GameTile> newTiles,
            List<GameTile> appliedFlowers,
            List<int> flowerCounts)
        {
            GameManager gm = GameManager.Instance;
            if (gm == null)
                yield break;

            // (1) 캔버스 찾아서 부모로 지정
            GameObject canvas = GameObject.Find("Main 2D Canvas");
            Transform canvasTr = canvas != null ? canvas.transform : transform;

            // (2) FLOWER PHASE 효과 연출 (Fade-In)
            GameObject effectGO = null;
            if (flowerPhaseEffectPrefab != null)
            {
                effectGO = Instantiate(flowerPhaseEffectPrefab, canvasTr);
                var img = effectGO.GetComponentInChildren<Image>();
                img.raycastTarget = false;
                yield return StartCoroutine(FadeIn(img, 0.2f));
            }

            // (3) 좌석 순서대로 화패 교체
            AbsoluteSeat[] seats = {
                AbsoluteSeat.EAST,
                AbsoluteSeat.SOUTH,
                AbsoluteSeat.WEST,
                AbsoluteSeat.NORTH
            };

            foreach (var abs in seats)
            {
                int count = flowerCounts[(int)abs];
                RelativeSeat rel = RelativeSeatExtensions.CreateFromAbsoluteSeats(gm.MySeat, abs);

                for (int i = 0; i < count; i++)
                {
                    if (rel == RelativeSeat.SELF)
                    {
                        int prev = gm.flowerCountMap[rel];
                        int next = prev + 1;
                        bool animDone = false;
                        StartCoroutine(gm.AnimateFlowerCount(rel, prev, next, () => animDone = true));

                        yield return gm.GameHandManager
                            .RunExclusive(gm.GameHandManager.ApplyFlower(appliedFlowers[i]));
                        yield return new WaitUntil(() => animDone);
                        yield return gm.GameHandManager
                            .RunExclusive(gm.GameHandManager.AddInitFlowerTsumo(newTiles[i]));

                        gm.SetFlowerCount(rel, next);
                        gm.UpdateLeftTilesByDelta(-1);
                    }
                    else
                    {
                        int prev = gm.flowerCountMap[rel];
                        int next = prev + 1;

                        // 화패 카운트 애니메이션
                        yield return StartCoroutine(gm.AnimateFlowerCount(rel, prev, next, null));
                        // 상대 3D 애니메이션
                        yield return StartCoroutine(ProcessOpponentFlowerOperation(gm.playersHand3DFields[(int)rel], null));
                        // 업데이트
                        gm.SetFlowerCount(rel, next);
                        gm.UpdateLeftTilesByDelta(-1);
                    }

                    // 다음 교체 전 잠시 여유
                    yield return new WaitForSeconds(0.3f);
                }
            }

            // (4) FLOWER PHASE Fade-Out
            if (effectGO != null)
            {
                var img = effectGO.GetComponentInChildren<Image>();
                yield return StartCoroutine(FadeOut(img, 0.2f));
                Destroy(effectGO);
            }

            // (5) ROUND START 연출
            if (roundStartEffectPrefab != null)
            {
                var go = Instantiate(roundStartEffectPrefab, canvasTr);
                var img = go.GetComponentInChildren<Image>();
                img.raycastTarget = false;
                yield return StartCoroutine(FadeInAndOut(img, 0.2f, 0.7f));
                Destroy(go);
            }

            // (6) 서버에 INIT_FLOWER_OK 전송
            if (GameWS.Instance != null)
            {
                // 서버 OK 전송
                GameWS.Instance.SendGameEvent(GameWSActionType.GAME_EVENT,
                    new
                    {
                        event_type = (int)GameEventType.INIT_FLOWER_OK,
                        data = new Dictionary<string, object>()
                    }
                );
            }
            yield break;
        }

        /// <summary>
        /// 상대 3D 핸드 쪽 꽃 교체 애니메이션을 처리하고 완료 콜백을 호출합니다.
        /// </summary>
        private IEnumerator ProcessOpponentFlowerOperation(
            Hand3DField handField,
            Action onComplete)
        {
            yield return handField.RequestDiscardRandom();
            yield return handField.RequestInitFlowerTsumo();
            onComplete?.Invoke();
        }


        // 지정한 Image 컴포넌트가 fade in 효과로 나타나도록 처리 (fadeDuration 동안)
        private IEnumerator FadeIn(Image img, float fadeDuration)
        {
            Color origColor = img.color;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                img.color = new Color(origColor.r, origColor.g, origColor.b, t);
                yield return null;
            }
            img.color = new Color(origColor.r, origColor.g, origColor.b, 1f);
        }

        // 지정한 Image 컴포넌트가 fade out 효과로 사라지도록 처리 (fadeDuration 동안)
        private IEnumerator FadeOut(Image img, float fadeDuration)
        {
            Color origColor = img.color;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                img.color = new Color(origColor.r, origColor.g, origColor.b, 1 - t);
                yield return null;
            }
            img.color = new Color(origColor.r, origColor.g, origColor.b, 0f);
        }


        /// <summary>
        /// Image 컴포넌트에 대해 FadeIn 후 일정 시간 유지, FadeOut 애니메이션을 수행합니다.
        /// </summary>
        private IEnumerator FadeInAndOut(Image img, float fadeDuration, float displayDuration)
        {
            Color origColor = img.color;
            // Fade In
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                img.color = new Color(origColor.r, origColor.g, origColor.b, t);
                yield return null;
            }
            yield return new WaitForSeconds(displayDuration);
            // Fade Out
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                img.color = new Color(origColor.r, origColor.g, origColor.b, 1 - t);
                yield return null;
            }
        }
    }
}
