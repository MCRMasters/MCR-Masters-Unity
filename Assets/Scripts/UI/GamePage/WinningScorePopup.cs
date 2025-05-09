using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MCRGame.Common;
using MCRGame.Game;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using System;

namespace MCRGame.UI
{
    public class WinningScorePopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject TilePanel;
        [SerializeField] private TextMeshProUGUI singleScoreText;
        [SerializeField] private TextMeshProUGUI totalScoreText;
        [SerializeField] private TextMeshProUGUI winnerNicknameText;
        [SerializeField] private TextMeshProUGUI flowerCountText;
        [SerializeField] private Image characterImage;
        [SerializeField] private Image flowerImage;
        [SerializeField] private Button okButton;
        [SerializeField] private WinningHandDisplay winningHandDisplay;
        [SerializeField] private GameObject winningHandOrigin;
        [SerializeField] private GameObject yakuOrigin;
        [SerializeField] private TextMeshProUGUI scoreTextPrefab;
        [SerializeField] private GameObject yakuObjectPrefab;
        [SerializeField] private GameObject yakuPanel;

        private Sequence yakuAnimationSequence;

        void Awake()
        {
            okButton.gameObject.SetActive(false);
        }
        public void SetOKButtonActive(){
            okButton.gameObject.SetActive(true);
        }
        public Sequence Initialize(WinningScoreData scoreData)
        {
            // [기존 Initialize 내용은 동일]

            singleScoreText.text = $"{scoreData.singleScore:N0}";
            totalScoreText.text = $"{scoreData.totalScore:N0}";
            singleScoreText.alpha = 0;
            totalScoreText.alpha = 0;
            winningHandDisplay.ShowWinningHand(scoreData);
            // 승자 정보
            string nick = GameManager.Instance.Players[GameManager.Instance.seatToPlayerIndex[scoreData.winnerSeat]].Nickname;
            winnerNicknameText.text = nick;

            foreach(var p in GameManager.Instance.PlayerInfo){
                if (p.nickname == nick){
                    characterImage.sprite =  CharacterImageManager.Instance.get_character_sprite_by_code(p.current_character.code);
                    characterImage.color = new Color(255, 255, 255, 255);
                    break;
                }
            }

            // 확인 버튼 이벤트
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener(() => Destroy(gameObject)); // 팝업 닫기
                                                                     // 야쿠 점수 표시 (애니메이션 완료 후 점수 표시)
            return DisplayYakuScoresWithAnimation(
                yakuOrigin.GetComponent<RectTransform>(),
                yakuObjectPrefab,
                scoreData.yaku_score_list,
                () => DisplayScores(scoreData.singleScore, scoreData.totalScore) // Func<Sequence> 반환
            );
        }

        public Sequence DisplayYakuScoresWithAnimation(RectTransform panel, GameObject yakuObjectPrefab,
                                                List<YakuScore> yakuScores, Func<Sequence> onComplete)
        {
            // 기존 애니메이션 중지 및 정리
            if (yakuAnimationSequence != null && yakuAnimationSequence.IsActive())
            {
                yakuAnimationSequence.Kill();
            }

            foreach (Transform child in panel)
            {
                Destroy(child.gameObject);
            }

            if (panel == null || yakuObjectPrefab == null)
            {
                Debug.LogError("Panel or YakuItemPrefab is null!");
                return DOTween.Sequence();
            }

            yakuAnimationSequence = DOTween.Sequence();
            float startX = 50f;
            float startY = -50f;
            float animationDuration = 0.5f;
            float delayBetweenItems = 0.5f;

            // 패널 크기 계산
            float panelWidth = yakuPanel.GetComponent<RectTransform>().rect.width;
            int nOfRows = yakuScores.Count > 10 ? 5 : 4;
            int nOfColumns = yakuScores.Count > 8 ? 3 : 2;
            float yakuScale = panelWidth / nOfColumns / 550f;
            float yakuWidth = 500f * yakuScale;
            float yakuHeight = 100f * yakuScale;

            // 각 야쿠 항목 애니메이션 추가
            yakuScores.Sort((YakuScore a, YakuScore b) => { return a.CompareTo(b); });
            for (int i = 0; i < yakuScores.Count; i++)
            {
                int index = i; // 클로저 캡처 방지
                string name = Enum.GetName(typeof(Yaku), yakuScores[index].YakuId) ?? "";
                string score = yakuScores[index].Score.ToString();

                yakuAnimationSequence.AppendCallback(() =>
                {
                    GameObject itemObj = Instantiate(yakuObjectPrefab, panel);
                    itemObj.transform.localScale = Vector3.one * yakuScale;

                    RectTransform rt = itemObj.GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(-10f + yakuWidth * (index / nOfRows),
                                                     startY - yakuHeight * (index % nOfRows));

                    if (itemObj.TryGetComponent<YakuObject>(out var item))
                    {
                        item.SetYakuInfo(name, score);
                    }

                    // 애니메이션 적용
                    CanvasGroup cg = itemObj.GetComponent<CanvasGroup>();
                    if (cg == null) cg = itemObj.AddComponent<CanvasGroup>();
                    cg.alpha = 0;

                    rt.DOAnchorPosX(startX + yakuWidth * (index / nOfRows), animationDuration)
                    .SetEase(Ease.OutBack);
                    cg.DOFade(1, animationDuration);
                });

                yakuAnimationSequence.AppendInterval(delayBetweenItems);
            }

            // 모든 애니메이션 완료 후 콜백 실행
            yakuAnimationSequence.OnComplete(() =>
            {
                Sequence scoreSequence = onComplete?.Invoke();
                yakuAnimationSequence.Join(scoreSequence);
            });
            return yakuAnimationSequence;
        }

        private Sequence DisplayScores(int singleScore, int totalScore)
        {
            // 단일 점수와 총점 표시 (페이드인 + 스케일 애니메이션)
            Sequence scoreSequence = DOTween.Sequence();

            // 단일 점수 애니메이션
            singleScoreText.alpha = 0;
            singleScoreText.transform.localScale = Vector3.one * 1.5f;
            singleScoreText.text = $"{singleScore:N0}";

            scoreSequence.AppendInterval(0.5f);
            scoreSequence.Append(singleScoreText.DOFade(1, 0.8f).SetEase(Ease.InQuint));
            scoreSequence.Join(singleScoreText.transform.DOScale(1f, 0.6f).SetEase(Ease.InQuint));

            // 총점 애니메이션 (0.2초 딜레이 후 시작)
            totalScoreText.alpha = 0;
            totalScoreText.transform.localScale = Vector3.one * 1.5f;
            totalScoreText.text = $"{totalScore:N0}";

            scoreSequence.AppendInterval(0.1f);
            scoreSequence.Append(totalScoreText.DOFade(1, 0.8f).SetEase(Ease.InQuint));
            scoreSequence.Join(totalScoreText.transform.DOScale(1f, 0.8f).SetEase(Ease.InQuint));
            scoreSequence.AppendInterval(0.1f);

            Debug.Log("All scores displayed with animations");

            return scoreSequence;
        }

        // [나머지 기존 메서드들은 동일하게 유지]
    }
}