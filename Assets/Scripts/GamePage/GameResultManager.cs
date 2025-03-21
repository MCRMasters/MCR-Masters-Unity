using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameResultManager : MonoBehaviour
{
    [Header("게임 결과 패널 (초기 비활성화)")]
    [SerializeField] private GameObject gameResultPanel;

    [Header("1등 UI 요소")]
    [SerializeField] private Image firstProfileImage;
    [SerializeField] private Text firstNickName;
    [SerializeField] private Text firstScore;

    [Header("2등 UI 요소")]
    [SerializeField] private Image secondProfileImage;
    [SerializeField] private Text secondNickName;
    [SerializeField] private Text secondScore;

    [Header("3등 UI 요소")]
    [SerializeField] private Image thirdProfileImage;
    [SerializeField] private Text thirdNickName;
    [SerializeField] private Text thirdScore;

    [Header("4등 UI 요소")]
    [SerializeField] private Image fourthProfileImage;
    [SerializeField] private Text fourthNickName;
    [SerializeField] private Text fourthScore;

    [Header("프로필 이미지 리스트 (인스펙터에서 설정)")]
    [SerializeField] private Sprite[] profileImages;

    [Header("테스트 데이터 버튼")]
    [SerializeField] private Button testButton;

    private void Start()
    {
        // 게임 결과 패널 비활성화
        if (gameResultPanel != null)
        {
            gameResultPanel.SetActive(false);
        }

        // 테스트 버튼으로 목업 데이터 확인 가능
        if (testButton != null)
        {
            testButton.onClick.AddListener(ShowMockData);
        }
    }

    /// <summary>
    /// 서버에서 받은 데이터를 적용하는 메서드
    /// </summary>
    public void SetGameResult(List<PlayerResultData> playerResults)
    {
        if (playerResults == null || playerResults.Count < 4) return;

        // UI 업데이트
        ApplyPlayerResult(playerResults[0], firstProfileImage, firstNickName, firstScore);
        ApplyPlayerResult(playerResults[1], secondProfileImage, secondNickName, secondScore);
        ApplyPlayerResult(playerResults[2], thirdProfileImage, thirdNickName, thirdScore);
        ApplyPlayerResult(playerResults[3], fourthProfileImage, fourthNickName, fourthScore);

        // 패널 활성화
        gameResultPanel.SetActive(true);
    }

    /// <summary>
    /// 개별 플레이어 데이터를 UI에 적용하는 메서드
    /// </summary>
    private void ApplyPlayerResult(PlayerResultData data, Image profile, Text nameText, Text scoreText)
    {
        if (profile != null && data.profileIndex >= 0 && data.profileIndex < profileImages.Length)
        {
            profile.sprite = profileImages[data.profileIndex];
        }

        if (nameText != null)
        {
            nameText.text = data.nickName;
        }

        if (scoreText != null)
        {
            scoreText.text = data.score.ToString();
        }
    }

    /// <summary>
    /// 테스트용 데이터 (랜덤 닉네임 및 점수 적용)
    /// </summary>
    private void ShowMockData()
    {
        List<PlayerResultData> mockData = new List<PlayerResultData>();

        string[] fruitNames = { "Apple", "Banana", "Cherry", "Grape", "Mango", "Peach" };

        for (int i = 0; i < 4; i++)
        {
            PlayerResultData data = new PlayerResultData
            {
                nickName = fruitNames[Random.Range(0, fruitNames.Length)],
                score = Random.Range(1000, 50000),
                profileIndex = i % profileImages.Length // 인덱스 순환
            };
            mockData.Add(data);
        }

        SetGameResult(mockData);
    }
}

/// <summary>
/// 플레이어의 게임 결과 데이터를 저장하는 구조체
/// </summary>
[System.Serializable]
public struct PlayerResultData
{
    public string nickName;
    public int score;
    public int profileIndex; // 프로필 이미지 인덱스
}
