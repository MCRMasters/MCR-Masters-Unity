using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScoreTransferManager : MonoBehaviour
{
    [Header("점수 패널 전체 (---UI---ScoreTransfer)")]
    [SerializeField] private GameObject scoreTransferPanel; // 전체 점수 패널을 제어

    [Header("닉네임 텍스트")]
    [SerializeField] private Text eNickNameText;
    [SerializeField] private Text wNickNameText;
    [SerializeField] private Text sNickNameText;
    [SerializeField] private Text nNickNameText;

    [Header("점수 텍스트")]
    [SerializeField] private Text eScoreText;
    [SerializeField] private Text wScoreText;
    [SerializeField] private Text sScoreText;
    [SerializeField] private Text nScoreText;

    [Header("플레이어 프로필 이미지")]
    [SerializeField] private Image eProfileImage;
    [SerializeField] private Image wProfileImage;
    [SerializeField] private Image sProfileImage;
    [SerializeField] private Image nProfileImage;

    [Header("프로필 이미지 리스트 (인스펙터에서 설정)")]
    [SerializeField] private Sprite[] profileImages;

    [Header("점수 패널 표시 버튼")]
    [SerializeField] private Button toggleButton;

    // 닉네임 (목업 데이터)
    private string[] fruitNames = { "Apple", "Banana", "Cherry", "Grape", "Mango", "Peach", "Plum", "Melon" };

    private void Start()
    {
        // ---UI---ScoreTransfer 오브젝트를 비활성화
        if (scoreTransferPanel != null)
        {
            scoreTransferPanel.SetActive(false);
        }

        // 버튼 클릭 이벤트 추가
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleScorePanel);
        }

        // 플레이어 데이터 설정
        AssignRandomPlayerData();
    }

    /// <summary>
    /// ---UI---ScoreTransfer 오브젝트의 활성화/비활성화
    /// </summary>
    private void ToggleScorePanel()
    {
        if (scoreTransferPanel != null)
        {
            scoreTransferPanel.SetActive(!scoreTransferPanel.activeSelf);
        }
    }

    /// <summary>
    /// 4명의 플레이어 정보를 랜덤으로 설정
    /// 현재는 목업 데이터이지만, 나중에는 실제 게임 데이터에서 가져올 수 있음
    /// </summary>
    private void AssignRandomPlayerData()
    {
        // 닉네임 (랜덤 과일 이름 사용)
        eNickNameText.text = GetRandomFruitName();
        wNickNameText.text = GetRandomFruitName();
        sNickNameText.text = GetRandomFruitName();
        nNickNameText.text = GetRandomFruitName();

        // 점수 (0 ~ 99999 사이의 랜덤 값)
        eScoreText.text = GetRandomScore();
        wScoreText.text = GetRandomScore();
        sScoreText.text = GetRandomScore();
        nScoreText.text = GetRandomScore();

        // 프로필 이미지 설정 (인스펙터에서 지정된 이미지 중에서 선택)
        if (profileImages.Length >= 4)
        {
            eProfileImage.sprite = profileImages[0];
            wProfileImage.sprite = profileImages[1];
            sProfileImage.sprite = profileImages[2];
            nProfileImage.sprite = profileImages[3];
        }
    }

    /// <summary>
    /// 무작위 과일 닉네임 반환
    /// </summary>
    private string GetRandomFruitName()
    {
        return fruitNames[Random.Range(0, fruitNames.Length)];
    }

    /// <summary>
    /// 무작위 점수 반환 (0 ~ 99999)
    /// </summary>
    private string GetRandomScore()
    {
        return Random.Range(0, 100000).ToString();
    }
}
