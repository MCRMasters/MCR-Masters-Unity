using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace MCRGame.UI
{
    public class WinningPopupTester : MonoBehaviour
    {
        // 인스펙터에서 할당할 요소
        [Header("UI References")]
        [SerializeField] private Canvas subCanvas;
        [SerializeField] private GameObject winningPopupPrefab;

        // 테스트용 데이터
        [Header("Test Data")]
        [SerializeField] private Sprite testCharacterSprite;
        [SerializeField] private string testNickname = "테스트플레이어";
        [SerializeField] private int testSingleScore = 8000;
        [SerializeField] private int testTotalScore = 32000;
        [SerializeField] private int testFlowerCount = 2;

        private GameObject _currentPopup;
        private string _testResult = "버튼을 눌러 테스트 실행";

        // OnGUI로 테스트 인터페이스 표시
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label("🔍 마작 팝업 테스트", new GUIStyle(GUI.skin.label) { fontSize = 20 });

            if (GUILayout.Button("1. 팝업 생성 테스트", GUILayout.Height(40)))
            {
                CreatePopupTest();
            }

            if (GUILayout.Button("2. 데이터 표시 테스트", GUILayout.Height(40)))
            {
                DataDisplayTest();
            }

            if (GUILayout.Button("3. 버튼 클릭 테스트", GUILayout.Height(40)))
            {
                ButtonClickTest();
            }

            GUILayout.Label($"결과: {_testResult}");
            GUILayout.EndArea();
        }

        // 테스트 1: 팝업 생성
        private void CreatePopupTest()
        {
            if (_currentPopup != null) Destroy(_currentPopup);

            _currentPopup = Instantiate(winningPopupPrefab);
            _testResult = _currentPopup != null ?
                "✅ 팝업 생성 성공!" : "❌ 팝업 생성 실패!";
        }

        // 테스트 2: 데이터 표시
        private void DataDisplayTest()
        {
            if (_currentPopup == null)
            {
                _testResult = "❌ 먼저 팝업을 생성하세요!";
                return;
            }

            var popup = _currentPopup.GetComponent<WinningScorePopup>();
            if (popup == null) popup = _currentPopup.AddComponent<WinningScorePopup>();

            popup.Initialize(new WinningScoreData
            {
                singleScore = testSingleScore,
                totalScore = testTotalScore,
                winnerNickname = testNickname,
                characterSprite = testCharacterSprite,
                flowerCount = testFlowerCount
            });

            _testResult = "✅ 데이터 표시 완료!";
        }

        // 테스트 3: 버튼 동작
        private void ButtonClickTest()
        {
            if (_currentPopup == null)
            {
                _testResult = "❌ 먼저 팝업을 생성하세요!";
                return;
            }

            var button = _currentPopup.GetComponentInChildren<Button>();
            if (button != null)
            {
                button.onClick.Invoke();
                _testResult = "✅ 버튼 클릭 성공! (팝업 제거됨)";
                _currentPopup = null;
            }
            else
            {
                _testResult = "❌ 버튼을 찾을 수 없음!";
            }
        }
    }

}
