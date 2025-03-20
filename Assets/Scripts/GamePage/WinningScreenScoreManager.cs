using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WinningScreenScoreManager : MonoBehaviour
{
    /// <summary>
    /// 승리화면 전체(---UI---WinningScreen-ScorePanel)를 참조
    /// </summary>
    [Header("Score Panel Root (---UI---WinningScreen-ScorePanel)")]
    [SerializeField] private GameObject winningScreenRoot;

    /// <summary>
    /// 13개의 타일을 넣을 부모(WinningHandTile/TilePanel)
    /// </summary>
    [Header("Tile Panel")]
    [SerializeField] private Transform tilePanel;
    [SerializeField] private GameObject tilePrefab2D;
    [SerializeField] private Sprite[] tileSprites;

    /// <summary>
    /// 역 정보 5줄 표시용 텍스트, 총점 텍스트
    /// </summary>
    [Header("Score Panel Texts")]
    [SerializeField] private Text singleScoreText;
    [SerializeField] private Text totalScoreText;

    /// <summary>
    /// 캐릭터 이미지, 닉네임, 화패 수 텍스트
    /// </summary>
    [Header("Other UI Elements")]
    [SerializeField] private Image characterImage;
    [SerializeField] private Text winnerNickName;
    [SerializeField] private Text flowerCountText;

    /// <summary>
    /// OK 버튼, TEST 버튼
    /// </summary>
    [Header("Buttons")]
    [SerializeField] private Button okButton;
    [SerializeField] private Button testButton;

    // 역(役) 정보 후보 (목업용)
    private string[] possibleYaku = { "리치", "도라1", "멘젠칭후", "이페코", "혼일색", "백", "발", "중" };
    // 닉네임 후보 (목업용)
    private string[] possibleNames = { "Apple", "Banana", "Cherry", "Grape", "Mango", "Peach" };

    private void Start()
    {
        // OK 버튼을 누르면 전체 승리화면 비활성화
        if (okButton != null)
            okButton.onClick.AddListener(HideWinningScreen);

        // TEST 버튼을 누르면 목업 데이터로 화면 표시
        if (testButton != null)
            testButton.onClick.AddListener(ShowRandomData);

        // 시작 시 전체 화면 비활성화
        if (winningScreenRoot != null)
            winningScreenRoot.SetActive(false);
    }

    /// <summary>
    /// OK 버튼 클릭 시, 승리화면 전체(---UI---WinningScreen-ScorePanel) 비활성화
    /// </summary>
    private void HideWinningScreen()
    {
        // 승리화면을 비활성화
        if (winningScreenRoot != null)
            winningScreenRoot.SetActive(false);
    }

    /// <summary>
    /// TEST 버튼 클릭 시, 승리화면 활성화 + 무작위 데이터 세팅
    /// </summary>
    public void ShowRandomData()
    {
        // 승리화면 활성화
        if (winningScreenRoot != null)
            winningScreenRoot.SetActive(true);

        // 13개 타일 생성
        GenerateRandomTiles();

        // 역 정보 5개 표시
        DisplayRandomYaku();

        // 총점 무작위
        if (totalScoreText != null)
        {
            int randScore = Random.Range(1000, 20001);
            totalScoreText.text = $"Total Score: {randScore}";
        }

        // 캐릭터 이미지, 닉네임, 화패 수 무작위
        if (characterImage != null && tileSprites.Length > 0)
        {
            characterImage.sprite = tileSprites[Random.Range(0, tileSprites.Length)];
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
    }

    /// <summary>
    /// TilePanel 내부에 13개의 타일 프리팹을 무작위로 생성
    /// </summary>
    private void GenerateRandomTiles()
    {
        if (tilePanel == null) return;

        // 기존 타일 제거
        for (int i = tilePanel.childCount - 1; i >= 0; i--)
        {
            Destroy(tilePanel.GetChild(i).gameObject);
        }

        // 13개 생성
        for (int i = 0; i < 13; i++)
        {
            GameObject tileObj = Instantiate(tilePrefab2D, tilePanel);
            Image img = tileObj.GetComponent<Image>();
            if (img != null && tileSprites.Length > 0)
            {
                img.sprite = tileSprites[Random.Range(0, tileSprites.Length)];
            }
        }
    }

    /// <summary>
    /// 5개의 역(役) 정보를 무작위로 골라서 singleScoreText에 표시
    /// </summary>
    private void DisplayRandomYaku()
    {
        if (singleScoreText == null) return;

        List<string> yakuList = new List<string>();
        for (int i = 0; i < 5; i++)
        {
            string yaku = possibleYaku[Random.Range(0, possibleYaku.Length)];
            yakuList.Add(yaku);
        }

        // 줄바꿈으로 연결
        singleScoreText.text = string.Join("\n", yakuList);
    }
}
