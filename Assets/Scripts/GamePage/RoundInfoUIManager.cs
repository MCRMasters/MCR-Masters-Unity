using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RoundInfoUIManager : MonoBehaviour
{
    [Header("국 정보 및 남은 패")]
    [SerializeField] private Text textRoundInfo;       // 현재 국 정보 표시 (예: E1)
    [SerializeField] private Text textTilesRemaining;    // 남은 패 수 표시 (예: 130)

    [Header("좌석 라벨 Text 컴포넌트")]
    [SerializeField] private Text textSeatE;
    [SerializeField] private Text textSeatS;
    [SerializeField] private Text textSeatW;
    [SerializeField] private Text textSeatN;

    [Header("좌석 점수 Text 컴포넌트")]
    [SerializeField] private Text textScoreE;
    [SerializeField] private Text textScoreS;
    [SerializeField] private Text textScoreW;
    [SerializeField] private Text textScoreN;

    // UI Text 컴포넌트를 좌석별로 관리하는 Dictionary
    private Dictionary<PlayerSeat, Text> seatLabelDict = new Dictionary<PlayerSeat, Text>();
    private Dictionary<PlayerSeat, Text> seatScoreDict = new Dictionary<PlayerSeat, Text>();

    // 서버에서 받아올 데이터를 모의하는 Dictionary (좌석 라벨 및 점수)
    private Dictionary<PlayerSeat, string> seatLabels = new Dictionary<PlayerSeat, string>();
    private Dictionary<PlayerSeat, int> seatScores = new Dictionary<PlayerSeat, int>();

    // 남은 패와 현재 국 정보 (서버에서 받아올 값)
    private int tilesRemaining = 130;   // 남은 패 130개 고정
    private string currentRound = "E1"; // 예시 국 정보

    private void Start()
    {
        // UI 컴포넌트를 Dictionary에 등록
        seatLabelDict[PlayerSeat.E] = textSeatE;
        seatLabelDict[PlayerSeat.S] = textSeatS;
        seatLabelDict[PlayerSeat.W] = textSeatW;
        seatLabelDict[PlayerSeat.N] = textSeatN;

        seatScoreDict[PlayerSeat.E] = textScoreE;
        seatScoreDict[PlayerSeat.S] = textScoreS;
        seatScoreDict[PlayerSeat.W] = textScoreW;
        seatScoreDict[PlayerSeat.N] = textScoreN;

        // Mock 데이터 초기화
        seatLabels[PlayerSeat.E] = "E";
        seatLabels[PlayerSeat.S] = "S";
        seatLabels[PlayerSeat.W] = "W";
        seatLabels[PlayerSeat.N] = "N";

        // 각 좌석 점수는 10,000 ~ 40,000 사이의 랜덤 값으로 설정 (여기서는 예시로 1 ~ 400 사용)
        seatScores[PlayerSeat.E] = Random.Range(1, 400);
        seatScores[PlayerSeat.S] = Random.Range(1, 400);
        seatScores[PlayerSeat.W] = Random.Range(1, 400);
        seatScores[PlayerSeat.N] = Random.Range(1, 400);

        UpdateUI();
    }

    /// <summary>
    /// UI를 업데이트하는 메서드 (국 정보, 남은 패, 좌석 라벨, 좌석 점수를 갱신)
    /// </summary>
    private void UpdateUI()
    {
        if (textRoundInfo != null)
            textRoundInfo.text = $"Round: {currentRound}";

        if (textTilesRemaining != null)
            textTilesRemaining.text = $"Tiles Remaining: {tilesRemaining}";

        // 좌석 라벨 업데이트 using TryGetValue
        foreach (var seat in seatLabelDict.Keys)
        {
            if (seatLabelDict[seat] != null && seatLabels.TryGetValue(seat, out string label))
            {
                seatLabelDict[seat].text = label;
            }
        }

        // 좌석 점수 업데이트 using TryGetValue
        foreach (var seat in seatScoreDict.Keys)
        {
            if (seatScoreDict[seat] != null && seatScores.TryGetValue(seat, out int score))
            {
                seatScoreDict[seat].text = score.ToString();
            }
        }
    }

    // 아래의 Set 메서드들은 서버와 연동 시 호출되어 데이터를 갱신할 때 사용할 수 있습니다.
    public void SetRoundInfo(string roundInfo)
    {
        currentRound = roundInfo;
        UpdateUI();
    }

    public void SetTilesRemaining(int count)
    {
        tilesRemaining = count;
        UpdateUI();
    }

    public void SetSeatLabel(PlayerSeat seat, string label)
    {
        seatLabels[seat] = label;
        UpdateUI();
    }

    public void SetSeatScore(PlayerSeat seat, int score)
    {
        seatScores[seat] = score;
        UpdateUI();
    }
}
