using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace MCRGame.UI
{
    public class WinningScorePopup : MonoBehaviour
    {
        // 참조할 UI 요소들 (인스펙터에서 할당)
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI singleScoreText;
        [SerializeField] private TextMeshProUGUI totalScoreText;
        [SerializeField] private TextMeshProUGUI winnerNicknameText;
        [SerializeField] private TextMeshProUGUI flowerCountText;
        [SerializeField] private Image characterImage;
        [SerializeField] private Image flowerImage;
        [SerializeField] private Button okButton;

        // 팝업 초기화 메서드
        public void Initialize(WinningScoreData scoreData)
        {
            // 점수 표시
            singleScoreText.text = $"{scoreData.singleScore:N0}";
            totalScoreText.text = $"{scoreData.totalScore:N0}";

            // 승자 정보
            winnerNicknameText.text = scoreData.winnerNickname;
            characterImage.sprite = scoreData.characterSprite;

            // 꽃 패 개수 (있을 경우만 표시)
            flowerCountText.text = scoreData.flowerCount > 0 ? scoreData.flowerCount.ToString() : "";
            flowerImage.gameObject.SetActive(scoreData.flowerCount > 0);

            // 확인 버튼 이벤트
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener(() => Destroy(gameObject)); // 팝업 닫기
        }
    }

    // 점수 데이터 전용 클래스 (구조체로도 가능)
    [System.Serializable]
    public class WinningScoreData
    {
        public int singleScore;
        public int totalScore;
        public string winnerNickname;
        public Sprite characterSprite;
        public int flowerCount;
    }
}
