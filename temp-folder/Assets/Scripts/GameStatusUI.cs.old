using UnityEngine;
using TMPro;
using Mirror;
using Unity.VisualScripting;

public class GameStatusUI : MonoBehaviour
{
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI playerNameText; // 플레이어 이름을 표시할 Text 추가
    public TextMeshProUGUI[] enemyScoresText;
    public TextMeshProUGUI[] enemyNamesText; // 적 이름을 표시할 Text 배열 추가
    public TextMeshProUGUI leftTilesText;
    public TextMeshProUGUI roundWindText;
    public TextMeshProUGUI[] enemySeatWindsText;
    public TextMeshProUGUI playerSeatWindText;
    public TextMeshProUGUI TimeLeftText;

    private PlayerManager localPlayerManager;

    public bool IsUpdated = false;

    public SpriteLightEffect[] spriteLightEffect;

    void Start()
    {

    }

    public void Initialize()
    {
        // Local PlayerManager 찾기
        if (localPlayerManager == null)
        {
            localPlayerManager = NetworkClient.connection.identity?.GetComponent<PlayerManager>();
        }
        if (localPlayerManager == null)
        {
            //Debug.LogError("Local PlayerManager not found.");
            return;
        }


        // 초기화: TextMeshPro 배열 크기 검사
        if (enemyScoresText.Length < 3 || enemySeatWindsText.Length < 3 || enemyNamesText.Length < 3)
        {
            //Debug.LogError("Enemy TextMeshProUGUI array lengths must be at least 3.");
            return;
        }

        IsUpdated = true;
    }


    void Update()
    {
        if (!IsUpdated)
        {
            return;
        }
        if (localPlayerManager == null) return;

        // Player의 CurrentScore 표시
        playerScoreText.text = $"{(localPlayerManager.playerStatus.CurrentScore >= 0 ? "+" : "")}{localPlayerManager.playerStatus.CurrentScore}";


        // Player의 SeatWind 표시
        playerSeatWindText.text = $"{localPlayerManager.playerStatus.SeatWind.ToString().Substring(0,1)}";

        playerNameText.text = $"{localPlayerManager.PlayerName}";

        

        // Enemy들의 CurrentScore, SeatWind 및 이름 표시
        var enemies = Object.FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (enemy != localPlayerManager)
            {
                int relativeIndex = localPlayerManager.GetRelativeIndex(enemy.PlayerIndex);
                //Debug.Log($"[GameStatusUI.Update] PlayerIndex: {localPlayerManager.PlayerIndex}, enemy name {enemy.PlayerName}, enemy index: {enemy.PlayerIndex}, relativeIndex: {relativeIndex}, enemy wind: {enemy.playerStatus.SeatWind.ToString()}");
                if (relativeIndex == -1)
                {
                    continue;
                }
                // 적의 점수 표시
                if (enemy.PlayerTurnForLight)
                {
                    spriteLightEffect[relativeIndex].gameObject.SetActive(true);
                    spriteLightEffect[relativeIndex].BlinkEffectOn();
                }
                else
                {
                    spriteLightEffect[relativeIndex].gameObject.SetActive(false);
                }
                enemyScoresText[relativeIndex].text = $"{(enemy.playerStatus.CurrentScore >= 0 ? "+" : "")}{enemy.playerStatus.CurrentScore}";

                // 적의 SeatWind 표시
                enemySeatWindsText[relativeIndex].text = $"{enemy.playerStatus.SeatWind.ToString().Substring(0, 1)}";

                // 적의 이름 표시
                enemyNamesText[relativeIndex].text = $"{enemy.PlayerName}";
            }
            else
            {
                if (enemy.PlayerTurnForLight)
                {
                    spriteLightEffect[3].gameObject.SetActive(true);
                    spriteLightEffect[3].BlinkEffectOn();
                }
                else
                {
                    spriteLightEffect[3].gameObject.SetActive(false);
                }
            }
        }

        // 남은 타일 정보 표시
        int tilesLeft = localPlayerManager.GetTilesLeft();
        leftTilesText.text = $"{tilesLeft}";

        // RoundWind와 CurrentRound 표시
        string roundWindName = localPlayerManager.playerStatus.RoundWind.ToString();
        int RoundIndex = localPlayerManager.GetRoundIndex(); ;
        roundWindText.text = $"{roundWindName.Substring(0,1)}{RoundIndex}";

        // 남은 시간 표시 및 색상 변경
        UpdateRemainingTime();
    }

    private void UpdateRemainingTime()
    {
        float remainingTime = localPlayerManager.getRemainingTime();
        int timeToDisplay = Mathf.FloorToInt(remainingTime); // 남은 시간의 내림 정수
        TimeLeftText.text = $"{timeToDisplay}";

        // 색상 설정
        Color textColor;
        if (remainingTime <= 0)
        {
            textColor = new Color(0, 0, 0, 0); // 투명
        }
        else if (remainingTime > 0 && remainingTime <= 5)
        {
            textColor = Color.red; // 빨간색
        }
        else
        {
            textColor = Color.black; // 검은색
        }

        // Material 색상 덮어쓰기
        Material material = TimeLeftText.fontMaterial;
        material.SetColor(ShaderUtilities.ID_FaceColor, textColor);
        TimeLeftText.fontMaterial = material;
    }

}
