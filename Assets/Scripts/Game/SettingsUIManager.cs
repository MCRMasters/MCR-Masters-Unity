using UnityEngine;
using UnityEngine.UI;
using MCRGame.Audio;
using MCRGame.Game;
using MCRGame.UI;

namespace MCRGame.Game
{
    public class SettingsUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button closeButton;

        [Header("Prefab & Container")]
        [Tooltip("Scroll-Rect > Content로 할당")]
        [SerializeField] private Transform contentContainer;
        [SerializeField] private GameObject voiceContentPrefab;       // Action 볼륨 슬라이더 프리팹
        [SerializeField] private GameObject sfxContentPrefab;         // Discard 볼륨 슬라이더 프리팹
        [SerializeField] private GameObject rightTsumoContentPrefab; // 우클릭 쯔모기리 토글 프리팹
        [SerializeField] private GameObject autoHuDefaultContentPrefab;
        [SerializeField] private GameObject autoFlowerDefaultContentPrefab;

        private Toggle autoHuDefaultToggle;
        private Toggle autoFlowerDefaultToggle;
        private Slider actionVolumeSlider;
        private Slider discardVolumeSlider;
        private Toggle rightClickTsumogiriToggle;

        private const string PREF_AUTO_HU_DEFAULT = "AutoHuDefault";
        private const string PREF_AUTO_FLOWER_DEFAULT = "AutoFlowerDefault";
        private const string PREF_ACTION_VOL = "ActionVolume";
        private const string PREF_DISCARD_VOL = "DiscardVolume";
        private const string PREF_RIGHT_CLICK = "RightClickTsumogiri";



        /// <summary>
        /// GameManager.OnSceneLoaded 에서 인스턴스화 직후 한 번만 호출하세요.
        /// </summary>
        public void Initialize(SettingsUIReferences refs)
        {
            // 1) refs → 내부 필드 복사
            settingsButton = refs.SettingsButton;
            settingsPanel = refs.SettingsPanel;
            closeButton = refs.CloseButton;
            contentContainer = refs.ContentContainer;
            voiceContentPrefab = refs.VoiceContentPrefab;
            sfxContentPrefab = refs.SFXContentPrefab;
            rightTsumoContentPrefab = refs.RightTsumoContentPrefab;
            autoHuDefaultContentPrefab = refs.AutoHuDefaultContentPrefab;
            autoFlowerDefaultContentPrefab = refs.AutoFlowerDefaultContentPrefab;

            // 2) UI rows 생성
            PopulateContentRows();

            // 3) Start() 로 하던 나머지 초기화
            settingsButton.onClick.AddListener(OpenPanel);
            closeButton.onClick.AddListener(ClosePanel);
            settingsPanel.SetActive(false);

            bool huDefault = PlayerPrefs.GetInt(PREF_AUTO_HU_DEFAULT, 0) == 1;
            bool flowerDefault = PlayerPrefs.GetInt(PREF_AUTO_FLOWER_DEFAULT, 1) == 1;
            autoHuDefaultToggle.isOn = huDefault;
            autoFlowerDefaultToggle.isOn = flowerDefault;
            GameManager.Instance.IsAutoHuDefault = huDefault;
            GameManager.Instance.IsAutoFlowerDefault = flowerDefault;
            autoHuDefaultToggle.onValueChanged.AddListener(OnAutoHuDefaultChanged);
            autoFlowerDefaultToggle.onValueChanged.AddListener(OnAutoFlowerDefaultChanged);

            float aVol = PlayerPrefs.GetFloat(PREF_ACTION_VOL, 1f);
            float dVol = PlayerPrefs.GetFloat(PREF_DISCARD_VOL, 0.2f);
            bool rightOn = PlayerPrefs.GetInt(PREF_RIGHT_CLICK, 0) == 1;
            actionVolumeSlider.value = aVol;
            discardVolumeSlider.value = dVol;
            rightClickTsumogiriToggle.isOn = rightOn;
            actionVolumeSlider.onValueChanged.AddListener(OnActionVolumeChanged);
            discardVolumeSlider.onValueChanged.AddListener(OnDiscardVolumeChanged);
            rightClickTsumogiriToggle.onValueChanged.AddListener(OnRightClickToggled);

            ApplyActionVolume(aVol);
            ApplyDiscardVolume(dVol);
            GameManager.Instance.IsRightClickTsumogiri = rightOn;
        }

        private void PopulateContentRows()
        {
            // 1) Action 볼륨
            var voiceGO = Instantiate(voiceContentPrefab, contentContainer);
            actionVolumeSlider = voiceGO.GetComponentInChildren<Slider>();

            // 2) Discard 볼륨
            var sfxGO = Instantiate(sfxContentPrefab, contentContainer);
            discardVolumeSlider = sfxGO.GetComponentInChildren<Slider>();

            // 3) 우클릭 쯔모기리 토글 생성 여부 결정
            // - 모바일 네이티브(iOS/Android) 또는
            // - WebGL 빌드인데 '핸드헬드' 디바이스(모바일 브라우저)인 경우에는 생성하지 않음
            bool skipRightToggle =
                Application.isMobilePlatform
                || (Application.platform == RuntimePlatform.WebGLPlayer
                    && SystemInfo.deviceType == DeviceType.Handheld);

            if (!skipRightToggle)
            {
                var rightGO = Instantiate(rightTsumoContentPrefab, contentContainer);
                rightClickTsumogiriToggle = rightGO.GetComponentInChildren<Toggle>();
            }
            else
            {
                // 모바일(WebGL 모바일 브라우저 포함)일 땐 더미 Toggle 생성(널 방지용)
                rightClickTsumogiriToggle = new GameObject("DummyToggle")
                    .AddComponent<Toggle>();
            }

            // 4) 자동 후(default false)
            var huGO = Instantiate(autoHuDefaultContentPrefab, contentContainer);
            autoHuDefaultToggle = huGO.GetComponentInChildren<Toggle>();

            // 5) 자동 꽃(default true)
            var flGO = Instantiate(autoFlowerDefaultContentPrefab, contentContainer);
            autoFlowerDefaultToggle = flGO.GetComponentInChildren<Toggle>();
        }


        private void OpenPanel() => settingsPanel.SetActive(!settingsPanel.activeSelf);
        private void ClosePanel() => settingsPanel.SetActive(false);

        // 자동 후 변경
        private void OnAutoHuDefaultChanged(bool on)
        {
            GameManager.Instance.IsAutoHuDefault = on;
            PlayerPrefs.SetInt(PREF_AUTO_HU_DEFAULT, on ? 1 : 0);
        }

        // 자동 꽃 변경
        private void OnAutoFlowerDefaultChanged(bool on)
        {
            GameManager.Instance.IsAutoFlowerDefault = on;
            PlayerPrefs.SetInt(PREF_AUTO_FLOWER_DEFAULT, on ? 1 : 0);
        }

        private void OnActionVolumeChanged(float v)
        {
            ApplyActionVolume(v);
            PlayerPrefs.SetFloat(PREF_ACTION_VOL, v);
        }

        private void OnDiscardVolumeChanged(float v)
        {
            ApplyDiscardVolume(v);
            PlayerPrefs.SetFloat(PREF_DISCARD_VOL, v);
        }

        private void OnRightClickToggled(bool on)
        {
            GameManager.Instance.IsRightClickTsumogiri = on;
            PlayerPrefs.SetInt(PREF_RIGHT_CLICK, on ? 1 : 0);
        }

        private void ApplyActionVolume(float v)
        {
            if (ActionAudioManager.Instance != null)
                ActionAudioManager.Instance.Volume = v;
        }

        private void ApplyDiscardVolume(float v)
        {
            if (DiscardSoundManager.Instance != null)
                DiscardSoundManager.Instance.Volume = v;
        }
    }
}
