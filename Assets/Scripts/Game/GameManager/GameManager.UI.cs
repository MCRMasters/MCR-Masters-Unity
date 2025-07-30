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
        /*──────────────────────────────────────────────*/
        /*                UI  (partial)                 */
        /*──────────────────────────────────────────────*/

        #region ⏳ Timer

        private void UpdateTimerText()
        {
            if (!timerText.IsActive()) return;

            if (remainingTime > 0f)
            {
                remainingTime -= Time.deltaTime;
                int sec = Mathf.Max(0, Mathf.FloorToInt(remainingTime));
                timerText.text = sec.ToString();
                if (remainingTime <= 0f) timerText.text = "0";
            }
        }

        #endregion
        #region 🔄 턴 이펙트

        private void UpdateCurrentTurnEffect()
        {
            int curIdx = (int)CurrentTurnSeat;

            if (prevBlinkSeat >= 0 && prevBlinkSeat < BlinkTurnImages.Length)
                BlinkTurnImages[prevBlinkSeat].GetComponent<BlinkTurnEffect>()?.BlinkEffectOff();

            if (curIdx >= 0 && curIdx < BlinkTurnImages.Length)
                BlinkTurnImages[curIdx].GetComponent<BlinkTurnEffect>()?.BlinkEffectOn();

            prevBlinkSeat = curIdx;
        }

        public void ResetAllBlinkTurnEffects()
        {
            foreach (var img in BlinkTurnImages)
                img?.GetComponent<BlinkTurnEffect>()?.BlinkEffectOff();

            prevBlinkSeat = -1;
        }

        #endregion
        #region 🎲 액션 패널 (버튼 / 추가 선택지)

        /* — ClearActionUI · ClearActionButtons — */
        private void ClearActionButtons()
        {
            isActionUIActive = false;
            foreach (Transform c in actionButtonPanel) Destroy(c.gameObject);
            if (additionalChoicesContainer) Destroy(additionalChoicesContainer);
        }

        private void ClearActionUI()
        {
            isAfterTsumoAction = false;
            if (actionButtonPanel == null) return;
            ClearActionButtons();
            if (timerText) timerText.gameObject.SetActive(false);
        }

        #endregion
        #region 📋 게임 전체 UI 토글 & 라운드 표시

        private void SetUIActive(bool active)
        {
            if (leftTilesText)   leftTilesText.gameObject.SetActive(active);
            if (currentRoundText) currentRoundText.gameObject.SetActive(active);

            foreach (var t in new[] { windText_Self, windText_Shimo, windText_Toi, windText_Kami })   t?.gameObject.SetActive(active);
            foreach (var t in new[] { scoreText_Self, scoreText_Shimo, scoreText_Toi, scoreText_Kami }) t?.gameObject.SetActive(active);
            foreach (var i in profileImages)  i?.gameObject.SetActive(active);
            foreach (var i in profileFrameImages) i?.gameObject.SetActive(active);
            foreach (var t in nicknameTexts)  t?.gameObject.SetActive(active);
            foreach (var i in flowerImages)   i?.gameObject.SetActive(active);
            foreach (var t in flowerCountTexts) t?.gameObject.SetActive(active);
        }

        private void UpdateCurrentRoundUI()
        {
            if (currentRoundText) currentRoundText.text = CurrentRound.ToLocalizedString();
            else Debug.LogWarning("currentRoundText UI가 할당되지 않았습니다.");
        }

        #endregion
        #region 🌸 화패 UI

        private void InitializeFlowerUI()
        {
            flowerCountMap.Clear();
            foreach (RelativeSeat rel in Enum.GetValues(typeof(RelativeSeat)))
                flowerCountMap[rel] = 0;
            UpdateFlowerCountText();
        }

        public void SetFlowerCount(RelativeSeat rel,int count)
        {
            flowerCountMap[rel] = count;
            UpdateFlowerCountText();
        }

        public void UpdateFlowerCountText()
        {
            foreach (RelativeSeat rel in Enum.GetValues(typeof(RelativeSeat)))
            {
                int i = (int)rel;
                if (i >= flowerImages.Length || i >= flowerCountTexts.Length) continue;

                var img = flowerImages[i];
                var txt = flowerCountTexts[i];
                int cnt = flowerCountMap.GetValueOrDefault(rel,0);
                if (txt != null)
                    txt.transform.parent.localScale = flowerCountBaseScales[i];

                if (cnt==0)
                {
                    img?.gameObject.SetActive(false);
                    txt?.gameObject.SetActive(false);
                    img.sprite = FlowerIcon_White;
                    txt.text = "X0";
                }
                else
                {
                    img?.gameObject.SetActive(true);
                    txt?.gameObject.SetActive(true);

                    img.sprite = GetFlowerIconByCount(cnt);
                    txt.text = $"X{cnt}";
                }
            }
        }

        /// <summary>
        /// Returns the appropriate flower icon sprite for the given count.
        /// </summary>
        private Sprite GetFlowerIconByCount(int count)
        {
            if (count <= 3) return FlowerIcon_White;
            if (count <= 6) return FlowerIcon_Yellow;
            return FlowerIcon_Red;
        }

        private void setRelativeSeatFlowerUIActive(bool active,RelativeSeat seat)
        {
            int idx = (int)seat;
            if (idx>=flowerImages.Length||idx>=flowerCountTexts.Length) return;
            flowerImages[idx]?.gameObject.SetActive(active);
            flowerCountTexts[idx]?.gameObject.SetActive(active);
        }

        #endregion
        #region 🙍‍♂️ 프로필 / 닉네임 / 프레임

        private void InitializeProfileUI()
        {
            for (int i=0;i<4;i++)
            {
                RelativeSeat rel = (RelativeSeat)i;
                AbsoluteSeat abs = rel.ToAbsoluteSeat(MySeat);

                if (!seatToPlayerIndex.TryGetValue(abs,out int pIdx) || pIdx<0 || pIdx>=Players.Count) continue;
                var player = Players[pIdx];

                nicknameTexts[i]?.SetText(player.Nickname);
                if (profileImages[i]) profileImages[i].sprite = GetProfileImageSprite(player.Uid);
                if (profileFrameImages[i])
                {
                    profileFrameImages[i].sprite = GetFrameSprite(player.Uid);
                    profileFrameImages[i].gameObject.SetActive(true);
                }
            }
        }

        private Sprite GetProfileImageSprite(string uid)
        {
            var ru = PlayerInfo.FirstOrDefault(p=>p.uid==uid);
            return ru != null ? CharacterImageManager.Instance.get_character_pfp_by_code(ru.current_character.code)
                              : defaultProfileImageSprite;
        }
        private Sprite GetFrameSprite(string uid) => defaultFrameSprite; // 커스텀 시 수정

        #endregion
        #region 🀄 남은패 / 점수 / 자리 라벨

        /* ── 남은 패 ──────────────────────────────*/
        public void UpdateLeftTilesByDelta(int delta) => UpdateLeftTiles(leftTiles+delta);
        public void UpdateLeftTiles(int v)
        {
            leftTiles = v;
            if (leftTilesText) leftTilesText.text = v.ToString();
            else Debug.LogWarning("leftTilesText UI가 할당되지 않았습니다.");
        }

        /* ── 점수 라벨 ────────────────────────────*/
        public void UpdateScoreText()
        {
            var map = new Dictionary<RelativeSeat,TextMeshProUGUI>
            {
                {RelativeSeat.SELF,scoreText_Self},
                {RelativeSeat.SHIMO,scoreText_Shimo},
                {RelativeSeat.TOI,scoreText_Toi},
                {RelativeSeat.KAMI,scoreText_Kami}
            };

            foreach (var (rel,txt) in map)
            {
                if (!txt) continue;
                var abs = rel.ToAbsoluteSeat(MySeat);
                if (!seatToPlayerIndex.TryGetValue(abs,out int idx) || idx<0 || idx>=Players.Count) continue;

                int s = Players[idx].Score;
                txt.text  = s>0? $"+{s}" : s.ToString();
                txt.color = s switch { >0 => positiveScoreColor, <0 => negativeScoreColor, _ => zeroScoreColor };
            }
        }

        /* ── 자리 라벨 ───────────────────────────*/
        public void UpdateSeatLabels()
        {
            var map = new Dictionary<RelativeSeat,TextMeshProUGUI>
            {
                {RelativeSeat.SELF,windText_Self},
                {RelativeSeat.SHIMO,windText_Shimo},
                {RelativeSeat.TOI,windText_Toi},
                {RelativeSeat.KAMI,windText_Kami}
            };

            AbsoluteSeat seat = MySeat;
            for (RelativeSeat rel = RelativeSeat.SELF;; rel = rel.NextSeat())
            {
                if (map.TryGetValue(rel,out var t) && t)
                {
                    string l = seat.ToLocalizedString();
                    t.text  = l;
                    t.color = seat == AbsoluteSeat.EAST? eastWindColor : otherWindColor;
                }
                seat = seat.NextSeat();
                if (rel==RelativeSeat.KAMI) break;
            }
        }

        #endregion
    }
}
