using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RoundInfoUIManager : MonoBehaviour
{
    [Header("UI Text Components")]
    [SerializeField] private Text textRoundInfo;       // 현재 국 정보
    [SerializeField] private Text textTilesRemaining;  // 남은 패

    [Header("Seat Label Text")]
    [SerializeField] private Text textSeatE;
    [SerializeField] private Text textSeatS;
    [SerializeField] private Text textSeatW;
    [SerializeField] private Text textSeatN;

    [Header("Seat Score Text")]
    [SerializeField] private Text textScoreE;
    [SerializeField] private Text textScoreS;
    [SerializeField] private Text textScoreW;
    [SerializeField] private Text textScoreN;

    // 임시 데이터
    private int tilesRemaining = 130;   // 남은 패 130개 고정
    private string currentRound = "E1"; // 예: 동1국

    // 자리와 점수를 각각 Dictionary로 관리
    private Dictionary<PlayerSeat, string> seatLabels = new Dictionary<PlayerSeat, string>();
    private Dictionary<PlayerSeat, int> seatScores = new Dictionary<PlayerSeat, int>();

    private void Start()
    {
        // Mock(더미) 데이터 초기화
        tilesRemaining = 130;      // 고정
        currentRound = "E1";       // 예시 국 정보

        // 좌석 라벨
        seatLabels[PlayerSeat.E] = "E"; // 실제로는 "동" 등으로 쓸 수도 있음
        seatLabels[PlayerSeat.S] = "S";
        seatLabels[PlayerSeat.W] = "W";
        seatLabels[PlayerSeat.N] = "N";

        // 각 좌석에 임의 점수
        seatScores[PlayerSeat.E] = Random.Range(1, 400);
        seatScores[PlayerSeat.S] = Random.Range(1, 400);
        seatScores[PlayerSeat.W] = Random.Range(1, 400);
        seatScores[PlayerSeat.N] = Random.Range(1, 400);

        UpdateUI();
    }

    /// <summary>
    /// 국 정보(E1, E2...) 변경 시
    /// </summary>
    public void SetRoundInfo(string roundInfo)
    {
        currentRound = roundInfo;
        UpdateUI();
    }

    /// <summary>
    /// 남은 패 갱신 시
    /// </summary>
    public void SetTilesRemaining(int count)
    {
        tilesRemaining = count;
        UpdateUI();
    }

    /// <summary>
    /// 좌석 라벨(문자열) 설정
    /// </summary>
    public void SetSeatLabel(PlayerSeat seat, string label)
    {
        seatLabels[seat] = label;
        UpdateUI();
    }

    /// <summary>
    /// 좌석 점수(int) 설정
    /// </summary>
    public void SetSeatScore(PlayerSeat seat, int score)
    {
        seatScores[seat] = score;
        UpdateUI();
    }

    private void UpdateUI()
    {
        // 국 정보
        if (textRoundInfo != null)
            textRoundInfo.text = $"Round: {currentRound}";

        // 남은 패
        if (textTilesRemaining != null)
            textTilesRemaining.text = $"Tiles Remaining: {tilesRemaining}";

        // --- 동(E) ---
        if (textSeatE != null && seatLabels.ContainsKey(PlayerSeat.E))
            textSeatE.text = seatLabels[PlayerSeat.E];
        if (textScoreE != null && seatScores.ContainsKey(PlayerSeat.E))
            textScoreE.text = seatScores[PlayerSeat.E].ToString();

        // --- 남(S) ---
        if (textSeatS != null && seatLabels.ContainsKey(PlayerSeat.S))
            textSeatS.text = seatLabels[PlayerSeat.S];
        if (textScoreS != null && seatScores.ContainsKey(PlayerSeat.S))
            textScoreS.text = seatScores[PlayerSeat.S].ToString();

        // --- 서(W) ---
        if (textSeatW != null && seatLabels.ContainsKey(PlayerSeat.W))
            textSeatW.text = seatLabels[PlayerSeat.W];
        if (textScoreW != null && seatScores.ContainsKey(PlayerSeat.W))
            textScoreW.text = seatScores[PlayerSeat.W].ToString();

        // --- 북(N) ---
        if (textSeatN != null && seatLabels.ContainsKey(PlayerSeat.N))
            textSeatN.text = seatLabels[PlayerSeat.N];
        if (textScoreN != null && seatScores.ContainsKey(PlayerSeat.N))
            textScoreN.text = seatScores[PlayerSeat.N].ToString();
    }
}

