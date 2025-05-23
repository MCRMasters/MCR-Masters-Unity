using UnityEngine;
using UnityEngine.UI;
using MCRGame.Net;


namespace MCRGame.UI
{
    public class PlayerSlotUI : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Image readyIndicator;
        [SerializeField] private Image characterImage;
        [SerializeField] private Button addBotButton;

        private int slot_index;
        public string Uid { get; private set; }

        private void Awake()
        {
            // 클릭 리스너 등록
            addBotButton.onClick.AddListener(OnAddBotClicked);
        }

        private void OnAddBotClicked()
        {
            Debug.Log($"[PlayerSlotUI] AddBot clicked for slot {slot_index}");
            RoomService.Instance.AddBotToSlot(slot_index);
        }

        public void Setup(RoomUserInfo info, string hostUid, int index)
        {
            Uid = info.uid;
            slot_index = index;
            nameText.gameObject.SetActive(true);
            readyIndicator.gameObject.SetActive(true);
            characterImage.gameObject.SetActive(true);
            addBotButton.gameObject.SetActive(false);
            characterImage.sprite = CharacterImageManager.Instance.get_character_sprite_by_code(info.current_character.code);
            characterImage.color = new Color(255, 255, 255, 255);
            nameText.text = info.nickname + (info.uid == hostUid ? " (Host)" : "");
            SetReady(info.is_ready);
        }

        public void SetReady(bool ready)
        {
            readyIndicator.color = ready ? Color.green : Color.red;
        }

        public void SetEmpty(int index)
        {
            slot_index = index;
            nameText.gameObject.SetActive(false);
            readyIndicator.gameObject.SetActive(false);
            characterImage.gameObject.SetActive(false);
            addBotButton.gameObject.SetActive(true);
        }
    }
}

