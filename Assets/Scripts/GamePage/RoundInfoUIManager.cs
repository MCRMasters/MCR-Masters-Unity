using UnityEngine;
using UnityEngine.UI;

public class RoundInfoUIManager : MonoBehaviour
{
    [Header("UI Text Components")]
    [SerializeField] private Text textRoundInfo;
    [SerializeField] private Text textTilesRemaining;
    [SerializeField] private Text textWindE;
    [SerializeField] private Text textWindS;
    [SerializeField] private Text textWindW;
    [SerializeField] private Text textWindN;

    // 임시 변수 (서버에서 받을 예정)
    private int tilesRemaining = 99;
    private string currentRound = "E1";  // 예: 동1국

    private void Start()
    {
        // 초기 UI 업데이트
        UpdateUI();
    }

    /// <summary>
    /// 서버 정보 수신 후 갱신할 때 호출
    /// </summary>
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

    private void UpdateUI()
    {
        if (textRoundInfo != null)
            textRoundInfo.text = $"Round: {currentRound}";

        if (textTilesRemaining != null)
            textTilesRemaining.text = $"Tiles Remaining: {tilesRemaining}";

        // 필요하다면 자리바람(Seat Wind)와 점수도 업데이트
        // ex) textWindE.text = "E\nScore: 25000";
        //     textWindS.text = "S\nScore: 24000";
        // ...
    }
}
