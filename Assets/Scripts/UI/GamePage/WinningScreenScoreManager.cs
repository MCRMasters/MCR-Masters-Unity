using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MCRGame.UI
{
    public class WinningScreenScoreManager : MonoBehaviour
    {
        [Header("승리 화면 루트 패널")]
        [SerializeField] private GameObject winningScreenRoot;

        [Header("타일 패널 (13개 타일)")]
        [SerializeField] private Transform tilePanel;

        [Header("쯔모 패널 (1개 타일)")]
        [SerializeField] private Transform tsumoPanel;

        [Header("역(役) 정보 표시 패널")]
        [SerializeField] private Transform scorePanel;

        [Header("사용할 폰트 (선택 사항)")]
        [SerializeField] private Font existingFont;

        [Header("단일 점수 UI")]
        [SerializeField] private Text singleScoreText;

        [Header("승리 쯔모 프리팹 (랜덤)")]
        [SerializeField] private Transform winnerTsumoParent;

        [Header("기타 UI 요소")]
        [SerializeField] private Text totalScoreText;
        [SerializeField] private Sprite[] characterSprites;
        [SerializeField] private Image characterImage;
        [SerializeField] private Text winnerNickName;
        [SerializeField] private Text flowerCountText;

        [Header("버튼들")]
        [SerializeField] private Button okButton;
        [SerializeField] private Button testButton;

        // 목업 데이터
        private string[] possibleYaku = { "리치", "도라1", "멘젠칭후", "이페코", "혼일색", "백", "발", "중" };
        private string[] possibleNames = { "Apple", "Banana", "Cherry", "Grape", "Mango", "Peach" };
        private string[] suits = { "m", "p", "s" }; // 만(m), 삭(p), 통(s)

        private void Start()
        {
            if (okButton != null)
                okButton.onClick.AddListener(HideWinningScreen);

            if (testButton != null)
                testButton.onClick.AddListener(ShowRandomData);

            if (winningScreenRoot != null)
                winningScreenRoot.SetActive(false);
        }
        //이곳이 next 누르는 버튼입니다 현재는 단순히 결과창을 닫는데 패산을 초기화하거나 국을 리셋하는 api를 보내야합니다
        private void HideWinningScreen()
        {
            if (winningScreenRoot != null)
                winningScreenRoot.SetActive(false);
        }

        public void ShowRandomData()
        {
            if (winningScreenRoot != null)
                winningScreenRoot.SetActive(true);

            // 13개 타일 생성 (TileLoader 활용)
            GenerateRandomTiles();

            // 역(役) 정보 표시
            DisplayRandomYakuAsTexts();

            // 단일 점수 설정
            if (singleScoreText != null)
            {
                int singleScore = Random.Range(10, 70);
                singleScoreText.text = singleScore.ToString();
            }

            // 승리 쯔모 프리팹 생성
            GenerateRandomWinnerTsumo();

            // 총점 설정
            if (totalScoreText != null)
            {
                int randScore = Random.Range(10, 201);
                totalScoreText.text = $"Total Score: {randScore}";
            }

            // 캐릭터 이미지, 닉네임, 화패 개수 랜덤 설정
            if (characterImage != null && characterSprites != null && characterSprites.Length > 0)
            {
                int randIndex = Random.Range(0, characterSprites.Length);
                characterImage.sprite = characterSprites[randIndex];
            }
            if (winnerNickName != null)
            {
                winnerNickName.text = possibleNames[Random.Range(0, possibleNames.Length)];
            }
            if (flowerCountText != null)
            {
                int flowerCount = Random.Range(0, 10);
                flowerCountText.text = $"Flower: {flowerCount}";
            }

            // 쯔모 패널에 1개 타일 추가
            GenerateSingleTsumoTile();
        }

        /// <summary>
        /// TilePanel에 13개의 무작위 2D 타일 (만/삭/통, 1~9) 생성
        /// </summary>
        private void GenerateRandomTiles()
        {
            if (tilePanel == null) return;

            // 기존 자식 오브젝트 제거
            for (int i = tilePanel.childCount - 1; i >= 0; i--)
            {
                Destroy(tilePanel.GetChild(i).gameObject);
            }

            // 13개 타일 생성
            for (int i = 0; i < 13; i++)
            {
                // 무작위 문양 선택 (만, 삭, 통 중 하나)
                string randomSuit = suits[Random.Range(0, suits.Length)];
                // 무작위 숫자 선택 (1~9)
                int randomValue = Random.Range(1, 10);

                // TileLoader에서 2D 타일 프리팹 가져오기
                GameObject tilePrefab = TileLoader.Instance.Get2DPrefab(randomSuit, randomValue);
                if (tilePrefab != null)
                {
                    Instantiate(tilePrefab, tilePanel, false);
                }
            }
        }

        /// <summary>
        /// ScorePanel 내부에 5개의 역(役) 정보를 Text로 표시
        /// </summary>
        private void DisplayRandomYakuAsTexts()
        {
            if (scorePanel == null) return;

            // 기존 "YakuText_"로 시작하는 오브젝트 삭제
            for (int i = scorePanel.childCount - 1; i >= 0; i--)
            {
                Transform child = scorePanel.GetChild(i);
                if (child.name.StartsWith("YakuText_"))
                {
                    Destroy(child.gameObject);
                }
            }

            // 5개의 무작위 역(役) 생성
            for (int i = 0; i < 5; i++)
            {
                string yaku = possibleYaku[Random.Range(0, possibleYaku.Length)];

                GameObject textObj = new GameObject("YakuText_" + i);
                textObj.transform.SetParent(scorePanel, false);

                Text yakuText = textObj.AddComponent<Text>();
                Font fontToUse = (existingFont != null) ? existingFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
                yakuText.font = fontToUse;
                yakuText.fontSize = 24;
                yakuText.color = Color.black;
                yakuText.alignment = TextAnchor.MiddleLeft;
                yakuText.text = yaku;

                RectTransform rt = textObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(200, 30);
            }
        }

        /// <summary>
        /// 승리한 플레이어의 쯔모 패 (WinnerTsumo) 랜덤 생성
        /// </summary>
        private void GenerateRandomWinnerTsumo()
        {
            if (winnerTsumoParent == null) return;

            // 기존 오브젝트 제거
            for (int i = winnerTsumoParent.childCount - 1; i >= 0; i--)
            {
                Destroy(winnerTsumoParent.GetChild(i).gameObject);
            }

            // 무작위 문양 선택 (만, 삭, 통 중 하나)
            string randomSuit = suits[Random.Range(0, suits.Length)];
            // 무작위 숫자 선택 (1~9)
            int randomValue = Random.Range(1, 10);

            // TileLoader에서 2D 타일 프리팹 가져오기
            GameObject tilePrefab = TileLoader.Instance.Get2DPrefab(randomSuit, randomValue);
            if (tilePrefab != null)
            {
                Instantiate(tilePrefab, winnerTsumoParent, false);
            }
        }

        /// <summary>
        /// 쯔모 패널(TsumoPanel)에 1개의 무작위 2D 타일 생성
        /// </summary>
        private void GenerateSingleTsumoTile()
        {
            if (tsumoPanel == null) return;

            // 기존 타일 제거
            for (int i = tsumoPanel.childCount - 1; i >= 0; i--)
            {
                Destroy(tsumoPanel.GetChild(i).gameObject);
            }

            // 무작위 문양 선택 (만, 삭, 통 중 하나)
            string randomSuit = suits[Random.Range(0, suits.Length)];
            // 무작위 숫자 선택 (1~9)
            int randomValue = Random.Range(1, 10);

            // TileLoader에서 2D 타일 프리팹 가져오기
            GameObject tilePrefab = TileLoader.Instance.Get2DPrefab(randomSuit, randomValue);
            if (tilePrefab != null)
            {
                Instantiate(tilePrefab, tsumoPanel, false);
            }
        }
    }
}