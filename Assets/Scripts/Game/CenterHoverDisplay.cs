using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using MCRGame.UI;
using MCRGame.Common;
using MCRGame.Net;

namespace MCRGame.Game
{
    public class CenterScoreHoverDisplay :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler          // ← 추가
    {
        [Header("Score Texts (RelativeSeat 순서)")]
        [SerializeField] private TextMeshProUGUI scoreText_Self;
        [SerializeField] private TextMeshProUGUI scoreText_Shimo;
        [SerializeField] private TextMeshProUGUI scoreText_Toi;
        [SerializeField] private TextMeshProUGUI scoreText_Kami;

        [Header("Colors")]
        [SerializeField] private Color positiveColor = new(0x23 / 255f, 0xE6 / 255f, 0xA5 / 255f); // #23E6A5
        [SerializeField] private Color zeroColor = new(0xC0 / 255f, 0xC0 / 255f, 0xC0 / 255f); // #C0C0C0
        [SerializeField] private Color negativeColor = new(0xFF / 255f, 0x6A / 255f, 0x6A / 255f); // #FF6A6A

        private Dictionary<RelativeSeat, TextMeshProUGUI> seatToText;
        private bool hovering;
        private Coroutine holdRoutine;         // 클릭 홀드용
        private const float HOLD_SECONDS = 1f; // diff 유지 시간

        private void Awake()
        {
            seatToText = new()
            {
                { RelativeSeat.SELF,  scoreText_Self  },
                { RelativeSeat.SHIMO, scoreText_Shimo },
                { RelativeSeat.TOI,   scoreText_Toi   },
                { RelativeSeat.KAMI,  scoreText_Kami  },
            };
        }

        /* ───────────────── Pointer ───────────────── */

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovering = true;
            ShowDifference();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovering = false;
            // 클릭 홀드 중이 아니면 즉시 복구
            if (holdRoutine == null && GameManager.Instance != null)
                GameManager.Instance.UpdateScoreText();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // 좌클릭만 처리
            if (eventData.button != PointerEventData.InputButton.Left) return;

            ShowDifference();               // 클릭 시 한 번 더 갱신
            // 기존 코루틴이 돌고 있으면 초기화
            if (holdRoutine != null) StopCoroutine(holdRoutine);
            holdRoutine = StartCoroutine(HoldDiffThenRestore());
        }

        /* ─────────── Diff 표시 & 복구 ─────────── */

        private IEnumerator HoldDiffThenRestore()
        {
            yield return new WaitForSeconds(HOLD_SECONDS);
            holdRoutine = null;              // 코루틴 끝남 표식
            // 여전히 마우스가 올려져 있으면 diff 그대로 두고,
            // 빠져 있다면 원상 복구
            if (!hovering && GameManager.Instance != null)
                GameManager.Instance.UpdateScoreText();
        }
        private void ShowDifference()
        {
            var gm = GameManager.Instance;
            var pdm = PlayerDataManager.Instance;

            // ① GameManager/PlayerDataManager/Players 가 없으면 중단
            if (gm == null || gm.Players == null || pdm == null)
            {
                Debug.LogWarning("[CenterScoreHoverDisplay] GameManager 또는 PlayerDataManager.Instance 가 null 입니다.");
                return;
            }

            // ② seatToText 딕셔너리가 없으면 중단
            if (seatToText == null)
            {
                Debug.LogWarning("[CenterScoreHoverDisplay] seatToText 가 초기화되지 않았습니다.");
                return;
            }

            // ③ seatToPlayerIndex 매핑이 없으면 중단
            if (gm.seatToPlayerIndex == null)
            {
                Debug.LogWarning("[CenterScoreHoverDisplay] GameManager.seatToPlayerIndex 가 초기화되지 않았습니다.");
                return;
            }

            // (원래 로직)
            int selfScore = 0;
            var me = gm.Players.Find(p => p.Uid == pdm.Uid);
            if (me != null) selfScore = me.Score;

            foreach (var (rel, txt) in seatToText)
            {
                if (txt == null) continue;

                AbsoluteSeat abs = rel.ToAbsoluteSeat(gm.MySeat);
                if (!gm.seatToPlayerIndex.TryGetValue(abs, out int idx)
                    || idx < 0 || idx >= gm.Players.Count)
                {
                    txt.text = "--";
                    txt.color = zeroColor;
                    continue;
                }

                var player = gm.Players[idx];
                int diff = (rel == RelativeSeat.SELF)
                    ? 0
                    : player.Score - selfScore;
                ApplyStyle(txt, diff);
            }
        }

        private void ApplyStyle(TextMeshProUGUI txt, int value)
        {
            if (value > 0)
            {
                txt.text = "+" + value;
                txt.color = positiveColor;
            }
            else if (value < 0)
            {
                txt.text = value.ToString();
                txt.color = negativeColor;
            }
            else
            {
                txt.text = "+0";
                txt.color = zeroColor;
            }
        }
    }
}
