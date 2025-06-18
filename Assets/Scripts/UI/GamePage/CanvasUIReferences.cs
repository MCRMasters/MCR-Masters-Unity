using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MCRGame.UI
{
    /// <summary>
    /// Main 2D Canvas Prefab 내부의 모든 Scene-오브젝트 참조를
    /// Inspector 에서 할당해두고, 런타임에 GameManager에서 한 번에 바인딩하기 위한 스크립트입니다.
    /// </summary>
    public class CanvasUIReferences : MonoBehaviour
    {
        [Header("Player 2D Hand Manager")]
        public GameHandManager gameHandManager;

        [Header("Player Timer")]
        public TextMeshProUGUI TimerText;           // PlayerTimer → TimerText

        [Header("Player Hand (2D)")]
        public GameObject PlayerHand;               // PlayerHand 컨테이너

        [Header("Action Button Panel")]
        public RectTransform ActionButtonPanel;     // ActionButtonPanel

        [Header("Profile UI (SELF→SHIMO→TOI→KAMI)")]
        public Image[] ProfileImages = new Image[4];        // Profile_Self/ProfileImage_Self 등
        public Image[] ProfileFrameImages = new Image[4];   // Profile_Self/ProfileFrame_Self 등
        public TextMeshProUGUI[] NicknameTexts = new TextMeshProUGUI[4];     // Profile_Self/Nickname_Self 등
        public Image[] FlowerImages = new Image[4];         // Profile_Self/Flower_Self/FlowerImage_Self 등
        public TextMeshProUGUI[] FlowerCountTexts = new TextMeshProUGUI[4];  // Profile_Self/Flower_Self/FlowerCount_Self 등

        [Header("Tenpai Assist")]
        public GameObject TenpaiAssistPanel;        // TenpaiAssistPanel

        [Header("Settings")]
        public GameObject SettingPanelRoot;         // SettingPanelRoot
        public Button SettingButton;                // SettingButton

        [Header("Emoji UI")]
        public Button EmojiOpenButton;              // EmojiRoot/EmojiOpenButton
        public GameObject EmojiPanel;               // EmojiRoot/EmojiPanel

        [Header("Assist Button")]
        public Button AssistButton;                 // AssistButton

        [Header("Navigation Panel")]
        public GameObject NavigationPanel;          // NavigationPanel
    }
}
