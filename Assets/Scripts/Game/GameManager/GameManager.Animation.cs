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

        public IEnumerator AnimateFlowerCount(RelativeSeat rel, int fromValue, int toValue, System.Action onComplete)
        {
            float duration = 0.1f;
            float elapsed = 0f;
            TextMeshProUGUI flowerTxt = flowerCountTexts[(int)rel];
            Transform container = flowerTxt.transform.parent;

            Vector3 originalScale = container.localScale;
            float popScale = 1.3f;

            setRelativeSeatFlowerUIActive(active: true, seat: rel);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                int val = fromValue + Mathf.RoundToInt(Mathf.SmoothStep(0, 1, t) * (toValue - fromValue));
                flowerTxt.text = "X" + val;

                float scaleFactor = 1 + (popScale - 1) * (4 * t * (1 - t));
                container.localScale = originalScale * scaleFactor;
                yield return null;
            }
            flowerTxt.text = "X" + toValue;
            container.localScale = originalScale;
            onComplete?.Invoke();
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
