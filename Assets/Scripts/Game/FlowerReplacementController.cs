using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using MCRGame.Common;
using MCRGame.UI;
using MCRGame.Net;


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
            yield return GameManager.Instance.GameHandManager.RunExclusive(FlowerReplacementCoroutine(newTiles:newTiles, appliedFlowers:appliedFlowers, flowerCounts:flowerCounts));
            GameManager.Instance.GameHandManager.IsAnimating = false;
            GameManager.Instance.CanClick = false;
        }

        private IEnumerator FlowerReplacementCoroutine(
            List<GameTile> newTiles,
            List<GameTile> appliedFlowers,
            List<int> flowerCounts)
        {
            GameManager gm = GameManager.Instance;
            if (gm == null) yield break;

            // ---------------- FLOWER PHASE 들어가기 ----------------
            GameObject canvas = GameObject.Find("Main 2D Canvas");
            Transform canvasTr = canvas != null ? canvas.transform : transform;

            // 0) “FLOWER PHASE” 연출
            Image flowerPhaseImg = null;
            if (flowerPhaseEffectPrefab != null)
            {
                var go = Instantiate(flowerPhaseEffectPrefab, canvasTr);
                flowerPhaseImg = go.GetComponent<Image>();
                flowerPhaseImg.raycastTarget = false;
                yield return FadeIn(flowerPhaseImg, .2f);
            }

            // 1) 좌석 순서 반복
            AbsoluteSeat[] seats = { AbsoluteSeat.EAST, AbsoluteSeat.SOUTH,
                                     AbsoluteSeat.WEST, AbsoluteSeat.NORTH };

            foreach (var abs in seats)
            {
                int cnt = flowerCounts[(int)abs];
                RelativeSeat rel =
                    RelativeSeatExtensions.CreateFromAbsoluteSeats(gm.MySeat, abs);

                for (int i = 0; i < cnt; i++)
                {
                    yield return HandleOneFlower(rel, i, gm,
                                                 newTiles, appliedFlowers);
                    gm.UpdateLeftTilesByDelta(-1);
                }
                yield return new WaitForSeconds(0.3f);
            }

            // 2) FLOWER PHASE fade-out
            if (flowerPhaseImg != null)
            {
                yield return FadeOut(flowerPhaseImg, .2f);
                Destroy(flowerPhaseImg.gameObject);
            }
            // 3) ROUND START 연출
            if (roundStartEffectPrefab != null)
            {
                var go = Instantiate(roundStartEffectPrefab, canvasTr);
                var img = go.GetComponent<Image>();
                img.raycastTarget = false;
                yield return FadeInAndOut(img, .2f, .7f);
                Destroy(go);
            }

            // 4) 서버 OK 전송
            GameWS.Instance?.SendGameEvent(
                GameWSActionType.GAME_EVENT,
                new
                {
                    event_type = (int)GameEventType.INIT_FLOWER_OK,
                    data = new Dictionary<string, object>()
                });

            yield break;
        }

        // ───── 헬퍼 메서드들 (GameManager 쪽 코드 그대로) ─────
        private IEnumerator HandleOneFlower(
            RelativeSeat rel, int index, GameManager gm,
            List<GameTile> newTiles, List<GameTile> appliedFlowers)
        {
            if (rel == RelativeSeat.SELF)
            {
                yield return gm.GameHandManager
                               .RunExclusive(gm.GameHandManager.ApplyFlower(
                                                 appliedFlowers[index]));
                yield return gm.GameHandManager
                               .RunExclusive(gm.GameHandManager.AddInitFlowerTsumo(
                                                 newTiles[index]));
            }
            else
            {
                Hand3DField field = gm.playersHand3DFields[(int)rel];
                yield return field.RequestDiscardRandom();
                yield return field.RequestInitFlowerTsumo();
            }

            // 꽃 카운트 UI 애니메이션
            int prev = gm.flowerCountMap[rel];
            int next = prev + 1;
            yield return StartCoroutine(gm.AnimateFlowerCount(
                rel, prev, next, null));
            gm.SetFlowerCount(rel, next);
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
