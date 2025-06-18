using UnityEngine;
using UnityEngine.UI;

namespace MCRGame.UI
{
    /// <summary>
    /// Settings UI Prefab 루트에 붙여서
    /// Inspector에서 모든 UI & Prefab 레퍼런스를 할당해두는 바인더 스크립트
    /// </summary>
    public class SettingsUIReferences : MonoBehaviour
    {
        [Header("UI References")]
        public Button   SettingsButton;               // Settings 버튼
        public GameObject SettingsPanel;              // 설정 패널 전체
        public Button   CloseButton;                  // 패널 닫기 버튼

        [Header("Prefab & Container")]
        [Tooltip("Scroll-Rect > Content 를 할당")]
        public Transform ContentContainer;            // 스크롤 영역의 Content
        public GameObject VoiceContentPrefab;         // Action 볼륨 슬라이더 프리팹
        public GameObject SFXContentPrefab;           // Discard 볼륨 슬라이더 프리팹
        public GameObject RightTsumoContentPrefab;   // 우클릭 쯔모기리 토글 프리팹
        public GameObject AutoHuDefaultContentPrefab;// 자동 Hu Default 토글 프리팹
        public GameObject AutoFlowerDefaultContentPrefab; // 자동 Flower Default 토글 프리팹
    }
}
