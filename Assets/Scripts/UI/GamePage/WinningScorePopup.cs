using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MCRGame.Common;
using MCRGame.Game;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using Unity.VisualScripting;

namespace MCRGame.UI
{
    public class WinningScorePopup : MonoBehaviour
    {
        // 참조할 UI 요소들 (인스펙터에서 할당)
        [Header("UI References")]
        [SerializeField] private GameObject TilePanel;
        [SerializeField] private GameObject TsumoPanel;
        [SerializeField] private TextMeshProUGUI winningTilesText;
        [SerializeField] private TextMeshProUGUI callBlocksText;
        [SerializeField] private TextMeshProUGUI singleScoreText;
        [SerializeField] private TextMeshProUGUI totalScoreText;
        [SerializeField] private TextMeshProUGUI winnerNicknameText;
        [SerializeField] private TextMeshProUGUI flowerCountText;
        [SerializeField] private Image characterImage;
        [SerializeField] private Image flowerImage;
        [SerializeField] private Button okButton;

        [SerializeField] private GameObject scorePannel;

        [SerializeField] private TextMeshProUGUI scoreTextPrefab;

        // 팝업 초기화 메서드
        public void Initialize(WinningScoreData scoreData)
        {
            // 점수 표시
            singleScoreText.text = $"{scoreData.singleScore:N0}";
            totalScoreText.text = $"{scoreData.totalScore:N0}";
            winningTilesText.text = ConvertHandTilesToString(scoreData.handTiles);
            callBlocksText.text = ConvertCallBlocksToString(scoreData.callBlocks);
            // 승자 정보
            //winnerNicknameText.text = GameManager.Instance.Players[(int)scoreData.winnerSeat].Nickname;
            //characterImage.sprite = scoreData.characterSprite;

            DisplayYakuScores(scorePannel.GetComponent<RectTransform>(), scoreTextPrefab, scoreData.yaku_score_list);

            // 꽃 패 개수 (있을 경우만 표시)
            flowerCountText.text = scoreData.flowerCount > 0 ? scoreData.flowerCount.ToString() : "";
            flowerImage.gameObject.SetActive(scoreData.flowerCount > 0);

            // 확인 버튼 이벤트
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener(() => Destroy(gameObject)); // 팝업 닫기
        }

        public string ConvertHandTilesToString(List<GameTile> handTiles)
        {
            if (handTiles == null || handTiles.Count == 0)
            {
                return "Empty hand";
            }
            // Sort tiles by their numeric value
            var sortedTiles = handTiles.OrderBy(tile => (int)tile).ToList();
            StringBuilder sb = new StringBuilder();
            string currentPrefix = "";
            foreach (GameTile tile in sortedTiles)
            {
                string tileName = Enum.GetName(typeof(GameTile), tile);
                if (string.IsNullOrEmpty(tileName))
                {
                    continue;
                }
                string prefix = tileName.Substring(0, 1); // Get first character (M/P/S/Z/F)
                string value = tileName.Substring(1);    // Get remaining characters
                if (currentPrefix != prefix)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(" ");
                    }
                    sb.Append(prefix);
                    currentPrefix = prefix;
                }
                sb.Append(value);
            }
            return sb.ToString();
        }

        public string ConvertCallBlocksToString(List<CallBlockData> callBlocks)
        {
            if (callBlocks == null || callBlocks.Count == 0)
            {
                return "No call blocks";
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Call Blocks:");
            for (int i = 0; i < callBlocks.Count; i++)
            {
                var block = callBlocks[i];
                sb.Append(block.ToString());
                sb.Append(" ");
            }
            return sb.ToString();
        }
        // 사용 예시:
        // DisplayYakuScores(scorePanel, scoreTextPrefab, yakuScoreList);
        public static void DisplayYakuScores(RectTransform panel, TMP_Text textPrefab, List<YakuScore> yakuScores)
        {
            float spacing = 30f;
            float startOffset = 20f;
            // 기존 텍스트 모두 삭제
            foreach (Transform child in panel)
            {
                if (child != panel && child.GetComponent<TMP_Text>())
                    Destroy(child.gameObject);
            }

            if (panel == null || textPrefab == null)
            {
                Debug.LogError("Panel or TextPrefab is null!");
                return;
            }

            float currentY = -startOffset;

            // 헤더 생성
            TMP_Text header = Instantiate(textPrefab, panel);
            header.text = "<size=100%><b>역 점수</b></size>";
            header.alignment = TextAlignmentOptions.Center;
            header.rectTransform.anchoredPosition = new Vector2(100, currentY);
            currentY -= spacing;

            // 각 야쿠 점수 표시
            foreach (YakuScore yakuInfo in yakuScores)
            {
                Yaku yakuId = yakuInfo.YakuId;
                int score = yakuInfo.Score;
                TMP_Text entry = Instantiate(textPrefab, panel);
                entry.text = $"<b>{Enum.GetName(typeof(Yaku), yakuId)}</b>: {score.ToString("N0")}점";
                entry.rectTransform.anchoredPosition = new Vector2(0, currentY);
                currentY -= spacing;
            }

            // 총점 표시 (구분선 추가)
            TMP_Text divider = Instantiate(textPrefab, panel);
            divider.text = "<color=#AAAAAA>---------------------</color>";
            divider.rectTransform.anchoredPosition = new Vector2(0, currentY);
        }

    }
}
