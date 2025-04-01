using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace MCRGame.UI
{
    [System.Serializable]
    public class ButtonDataWrapper
    {
        public string[] buttons;
    }

    [System.Serializable]
    public class ButtonSpriteMapping
    {
        public string actionType;
        public Sprite sprite;
    }

    public class ButtonManager : MonoBehaviour
    {
        [Header("UI Button References (총 6개)")]
        [SerializeField] private List<Button> actionButtons;

        [Header("Button Sprite Mappings")]
        [SerializeField] private List<ButtonSpriteMapping> spriteMappings;

        [Header("테스트용 버튼 (더미데이터 호출)")]
        [SerializeField] private Button testDummyButton;

        // 우선순위 사전
        private Dictionary<string, int> priorityDict = new Dictionary<string, int> {
            {"skip", 0},
            {"hu", 1},
            {"kan", 2},
            {"pong", 3},
            {"chii", 4},
            {"flower", 5}
        };

        private Dictionary<string, Sprite> mappingDict;

        // 더미 JSON 리스트 (스크립트 내부에 정의, 총 5개)
        private List<string> dummyJsonList = new List<string>
        {
            "{\"buttons\": [\"skip\", \"pong\"]}",
            "{\"buttons\": [\"hu\", \"flower\", \"skip\"]}",
            "{\"buttons\": [\"kan\", \"pong\", \"skip\", \"flower\"]}",
            "{\"buttons\": [\"chii\", \"kan\", \"pong\"]}",
            "{\"buttons\": [\"hu\", \"kan\", \"pong\", \"chii\", \"skip\"]}"
        };

        private void Awake()
        {
            // spriteMappings를 기반으로 sprite dictionary 생성 (소문자 키)
            mappingDict = new Dictionary<string, Sprite>();
            foreach (var mapping in spriteMappings)
            {
                string key = mapping.actionType.ToLower();
                if (!mappingDict.ContainsKey(key))
                {
                    mappingDict.Add(key, mapping.sprite);
                }
            }

            // 모든 액션 버튼은 기본적으로 안 보이게 초기화
            foreach (var btn in actionButtons)
            {
                btn.gameObject.SetActive(false);
                btn.interactable = false;
                btn.onClick.RemoveAllListeners();
            }

            // 테스트용 버튼이 연결되어 있다면 리스너 등록
            if (testDummyButton != null)
            {
                testDummyButton.onClick.RemoveAllListeners();
                testDummyButton.onClick.AddListener(TestUpdateButtons);
            }
        }

        /// <summary>
        /// JSON 데이터를 받아 버튼 UI를 업데이트합니다.
        /// </summary>
        public void UpdateButtonsFromJson(string jsonData)
        {
            Debug.Log("Received JSON: " + jsonData);
            ButtonDataWrapper data = JsonUtility.FromJson<ButtonDataWrapper>(jsonData);
            if (data == null || data.buttons == null)
            {
                Debug.LogError("Invalid JSON data.");
                return;
            }

            // 모든 액션을 소문자로 변환 후 우선순위에 따라 정렬
            List<string> receivedActions = data.buttons.Select(act => act.ToLower()).ToList();
            receivedActions = receivedActions.OrderBy(act =>
                priorityDict.ContainsKey(act) ? priorityDict[act] : int.MaxValue
            ).ToList();

            Debug.Log("Sorted actions: " + string.Join(", ", receivedActions));

            // 버튼들을 receivedActions의 개수에 맞춰 갱신 (나머지는 숨김)
            for (int i = 0; i < actionButtons.Count; i++)
            {
                Button btn = actionButtons[i];

                if (i < receivedActions.Count)
                {
                    string action = receivedActions[i];
                    btn.gameObject.SetActive(true);
                    btn.interactable = true;

                    Image img = btn.GetComponent<Image>();
                    if (img != null && mappingDict.TryGetValue(action, out Sprite sp))
                    {
                        img.sprite = sp;
                    }
                    else
                    {
                        Debug.LogWarning($"No sprite mapping found for action: {action}");
                    }

                    btn.onClick.RemoveAllListeners();
                    // 클릭 시 해당 액션을 처리하고 모든 버튼을 비활성화
                    btn.onClick.AddListener(() => OnActionButtonClicked(action));
                }
                else
                {
                    btn.gameObject.SetActive(false);
                    btn.interactable = false;
                    btn.onClick.RemoveAllListeners();
                }
            }
        }

        /// <summary>
        /// 버튼 클릭 시 호출되는 이벤트 핸들러
        /// </summary>
        private void OnActionButtonClicked(string action)
        {
            Debug.Log("Action button clicked: " + action);
            // TODO: 실제 액션 로직 구현

            // 액션 수행 후 모든 액션 버튼을 비활성화 (숨김)
            HideAllActionButtons();
        }

        /// <summary>
        /// 모든 액션 버튼을 비활성화합니다.
        /// </summary>
        private void HideAllActionButtons()
        {
            foreach (var btn in actionButtons)
            {
                btn.gameObject.SetActive(false);
                btn.interactable = false;
                btn.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// 내부 더미 JSON 데이터 중 랜덤한 데이터를 사용하여 버튼을 업데이트합니다.
        /// (테스트용 버튼을 통해 호출됩니다.)
        /// </summary>
        public void TestUpdateButtons()
        {
            if (dummyJsonList.Count > 0)
            {
                int randomIndex = Random.Range(0, dummyJsonList.Count);
                string dummyJson = dummyJsonList[randomIndex];
                Debug.Log("Randomly selected dummy JSON index: " + randomIndex);
                UpdateButtonsFromJson(dummyJson);
            }
            else
            {
                Debug.LogWarning("dummyJsonList가 비어 있습니다.");
            }
        }
    }
}
