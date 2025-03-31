using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MCRGame
{
    public class EfuroButton : MonoBehaviour
    {
        [SerializeField] private Button efuroButton;    // Efuro 버튼
        [SerializeField] private FuroSpawner furoSpawner; // FuroSpawner 컴포넌트

        // 더미 데이터 리스트: 각 문자열은 타일 값과 수트를 나타냅니다.
        private List<string> dummyTileStrings = new List<string>
        {
            "234m",
            "123m",
            "456m",
            "789m",
            "147m"
        };

        // 현재 더미 데이터 인덱스
        private int currentIndex = 0;

        private void Start()
        {
            if (efuroButton != null)
            {
                efuroButton.onClick.AddListener(OnEfuroButtonClicked);
                Debug.Log("[EfuroButton] Button assigned and onClick listener added.");
            }
            else
            {
                Debug.LogWarning("[EfuroButton] Efuro button is not assigned!");
            }

            if (furoSpawner == null)
            {
                Debug.LogWarning("[EfuroButton] FuroSpawner is not assigned in Inspector!");
            }
        }

        /// <summary>
        /// Efuro 버튼 클릭 시, 더미 데이터 리스트에서 다음 데이터를 사용해 후로 영역을 생성합니다.
        /// 리스트의 마지막에 도달하면 인덱스를 0으로 재설정하여 반복합니다.
        /// </summary>
        private void OnEfuroButtonClicked()
        {
            Debug.Log("[EfuroButton] Efuro button clicked.");

            if (dummyTileStrings.Count == 0)
            {
                Debug.LogWarning("[EfuroButton] Dummy data list is empty.");
                return;
            }

            Debug.Log("[EfuroButton] Current dummyTileStrings count: " + dummyTileStrings.Count);
            Debug.Log("[EfuroButton] Current index: " + currentIndex);

            string tileString = dummyTileStrings[currentIndex];
            Debug.Log("[EfuroButton] Using dummy tile string: " + tileString);

            if (furoSpawner != null)
            {
                // 여기서 "chii", seat 0(East)로 호출 (테스트)
                furoSpawner.SpawnFuro("chii", 0, tileString);
                Debug.Log("[EfuroButton] SpawnFuro called with parameters: chii, 0, " + tileString);
            }
            else
            {
                Debug.LogError("[EfuroButton] FuroSpawner is not assigned!");
            }

            currentIndex++;
            if (currentIndex >= dummyTileStrings.Count)
            {
                currentIndex = 0;
                Debug.Log("[EfuroButton] Resetting dummy data index to 0.");
            }
        }
    }
}
