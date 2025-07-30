using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Collections;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TMPro;

using MCRGame.Common;
using MCRGame.UI;
using MCRGame.Net;
using MCRGame.View;
using MCRGame.Audio;
using DG.Tweening;

namespace MCRGame.Game
{
    public partial class GameManager
    {
        /*────────────────────────────────────────────────────*/
        /*          🎞️  ANIMATION  /  RESULT  처리               */
        /*────────────────────────────────────────────────────*/
        #region 🎞️ ANIMATION / RESULT

        /* ───────── ①  Init-Hand 관련 ───────── */
        #region ▶ Init-Hand 코루틴

        public IEnumerator InitHandCoroutine(List<GameTile> tiles, GameTile? tsumoTile, List<GameTile> newTiles, List<GameTile> appliedFlowers, List<int> flowerCounts)
        {
            isInitHandDone = false;
            yield return StartCoroutine(InitHandFromMessage(tiles, tsumoTile));
            isInitHandDone = true;

            yield return StartCoroutine(
                gameHandManager.RunExclusive(
                    FlowerReplacementController.Instance
                        .StartFlowerReplacement(newTiles:newTiles, appliedFlowers:appliedFlowers, flowerCounts:flowerCounts)
                )
            );

            Debug.Log("[GameMessageMediator] InitHand complete. Processing any queued flower replacement messages.");
        }

        /// <summary>
        /// INIT_EVENT 로 받은 초기 패 적용
        /// </summary>
        public IEnumerator InitHandFromMessage(List<GameTile> initTiles, GameTile? tsumoTile)
        {
            InitRound();
            CanClick = false;
            gameHandManager.IsAnimating = true;
            Debug.Log("GameManager: Initializing hand with received data for SELF.");

            /* 1) 2D Hand */
            if (gameHandManager != null)
                yield return gameHandManager.RunExclusive(gameHandManager.InitHand(initTiles, tsumoTile));
            else
                Debug.LogWarning("GameHandManager 인스턴스가 없습니다.");

            /* 2) 각 플레이어 3D Hand */
            if (playersHand3DFields == null || playersHand3DFields.Length < MAX_PLAYERS)
            {
                Debug.LogError("playersHand3DFields 배열이 4개로 할당되어 있지 않습니다.");
                yield break;
            }
            for (int i = 0; i < playersHand3DFields.Length; i++)
            {
                var hand3D = playersHand3DFields[i];
                if (hand3D == null) continue;
                if (i == (int)RelativeSeat.SELF) continue;

                RelativeSeat rel = (RelativeSeat)i;
                AbsoluteSeat abs = rel.ToAbsoluteSeat(MySeat);

                bool includeTsumo = (abs == AbsoluteSeat.EAST);
                hand3D.InitHand(includeTsumo);
            }
        }

        #endregion


        /* ───────── ②  화패 카운트 애니메이션 ───────── */
        #region ▶ Flower Count Pop 애니메이션

        private const float FlowerAnimDuration = 0.2f;

        /// <summary>
        /// Plays a flower count pop animation using DOTween. Any existing animation
        /// for the seat is killed before starting the new one, and the count text
        /// is updated immediately.
        /// </summary>
        public Coroutine PlayFlowerCountAnimation(RelativeSeat rel, int fromValue, int toValue, System.Action onComplete = null)
        {
            return StartCoroutine(PlayFlowerCountAnimationRoutine(rel, toValue, onComplete));
        }

        private IEnumerator PlayFlowerCountAnimationRoutine(RelativeSeat rel, int toValue, System.Action onComplete)
        {
            int idx = (int)rel;
            if (flowerCountTexts == null || idx >= flowerCountTexts.Length) yield break;

            TextMeshProUGUI flowerTxt = flowerCountTexts[idx];
            if (flowerTxt == null) yield break;
            Transform container = flowerTxt.transform.parent;
            Vector3 baseScale = flowerCountBaseScales[idx];

            setRelativeSeatFlowerUIActive(true, rel);

            // Kill any existing tween and reset scale
            if (flowerCountTweens[idx] != null)
            {
                flowerCountTweens[idx].Kill();
                flowerCountTweens[idx] = null;
                container.localScale = baseScale;
                // ensure text shows the latest count after kill
                flowerTxt.text = $"X{flowerCountMap[rel]}";
            }

            float popScale = 1.3f;

            // Update text and flower image immediately before animating
            flowerTxt.text = "X" + toValue;
            if (flowerImages != null && idx < flowerImages.Length)
            {
                var img = flowerImages[idx];
                if (img != null)
                {
                    img.sprite = GetFlowerIconByCount(toValue);
                }
            }
            container.localScale = baseScale;

            Sequence seq = DOTween.Sequence();
            seq.Append(container.DOScale(baseScale * popScale, FlowerAnimDuration * 0.5f).SetEase(Ease.OutBack));
            seq.Append(container.DOScale(baseScale, FlowerAnimDuration * 0.5f).SetEase(Ease.InBack));
            seq.OnComplete(() =>
            {
                flowerCountTweens[idx] = null;
                onComplete?.Invoke();
            });

            flowerCountTweens[idx] = seq;
            yield return seq.WaitForCompletion();
        }

        #endregion


        /* ───────── ③  결과 팝업 / 연출 ───────── */
        #region ▶ Result Popup & Draw / Hu Hand

        public void EndScorePopup()
        {
            if (EndScorePopupPrefab == null)
            {
                Debug.LogError("ScorePopupPrefab이 할당되지 않았습니다.");
                return;
            }
            var popupGO = Instantiate(EndScorePopupPrefab);
            var mgr = popupGO.GetComponentInChildren<EndScorePopupManager>();
            if (mgr == null)
            {
                Debug.LogError("EndScorePopupManager를 찾을 수 없습니다.");
                return;
            }
            StartCoroutine(mgr.ShowScores(Players));
        }

        /*  ProcessDraw / ProcessHuHand / WaitAndProcessTsumo 등
            나머지 결과-연출 메서드는 기존 partial 파일에 그대로 유지됩니다. */

        #endregion

        #endregion /* 🎞️ ANIMATION / RESULT */
    }
}
