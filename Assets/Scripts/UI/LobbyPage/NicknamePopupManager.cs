using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.InputSystem;  // ← 추가
using System.Collections;
using MCRGame.Net;

namespace MCRGame.UI
{
    public class NicknamePopupManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject popupPanel;          // 팝업 전체 Panel (풀스크린 덮개 역할)
        [SerializeField] private InputField nicknameInput;       // 새 닉네임 입력창
        [SerializeField] private Text currentNicknameText;       // 로비에 표시중인 닉네임 텍스트
        [SerializeField] private Button confirmButton;           // 변경 확인 버튼
        [SerializeField] private Text errorText;                 // 에러 메시지 표시용(Text)

        [Header("Dependencies")]
        [SerializeField] private PutNicknameApi putNicknameApi;  // 이전에 구현한 PUT API 컴포넌트

        private Image blockerImage;   // 투명 이미지로 raycast 차단
        private bool isNicknameSet = false;

        private void Awake()
        {
            // 버튼 리스너
            confirmButton.onClick.AddListener(OnConfirmClicked);

            // blocker 세팅: popupPanel에 Image 컴포넌트가 있어야 풀스크린 클릭을 가로챌 수 있음
            blockerImage = popupPanel.GetComponent<Image>();
            if (blockerImage == null)
                blockerImage = popupPanel.AddComponent<Image>();
            blockerImage.color = new Color(0, 0, 0, 0);  // 완전 투명
            blockerImage.raycastTarget = false;          // 기본은 차단 OFF
        }

        private void Start()
        {
            // new user인 경우 팝업 띄우기
            if (PlayerDataManager.Instance != null
                && (PlayerDataManager.Instance.IsNewUser || PlayerDataManager.Instance.Nickname == "")
                && !isNicknameSet)
            {
                ShowPopup();
            }
        }

        private void Update()
        {
            if (!popupPanel.activeSelf)
                return;

            // 팝업이 열린 상태면 blocker로 뒤쪽 클릭 막기
            blockerImage.raycastTarget = true;

            // Enter 키 눌렀을 때 확인 버튼 실행
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                confirmButton.onClick.Invoke();
            }
        }

        /// <summary>
        /// 팝업 보이기
        /// </summary>
        private void ShowPopup()
        {
            popupPanel.SetActive(true);
            // popupWindow만 상호작용 원하면, popupWindow 아래 자식들에만 RaycastTarget=true
            // (InputField, Button 등)
        }

        /// <summary>
        /// 팝업 닫기
        /// </summary>
        public void ClosePopup()
        {
            // blocker도 해제
            PlayerDataManager.Instance.IsNewUser = false;
            blockerImage.raycastTarget = false;
            popupPanel.SetActive(false);
        }

        private void OnConfirmClicked()
        {
            string newNick = nicknameInput.text.Trim();
            if (string.IsNullOrEmpty(newNick))
            {
                errorText.text = "닉네임을 입력해주세요.";
                return;
            }
            // PUT /user/me/nickname
            StartCoroutine(putNicknameApi.UpdateNickname(
                newNick,
                PlayerDataManager.Instance.AccessToken,
                onSuccess: _ => StartCoroutine(FetchUserMe()),
                onError: err => errorText.text = $"업데이트 실패: {err}"
            ));
            // 닫기 전에 입력 잠시 비활성화하고 싶다면 여기서 처리
            ClosePopup();
        }

        private IEnumerator FetchUserMe()
        {
            string url = CoreServerConfig.GetHttpUrl("/user/me");
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");
                www.certificateHandler = new BypassCertificateHandler();
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    UserMeResponse userData = JsonUtility.FromJson<UserMeResponse>(www.downloadHandler.text);
                    PlayerDataManager.Instance.SetUserData(userData.uid, userData.nickname, userData.email);
                    currentNicknameText.text = userData.nickname;
                    isNicknameSet = true;
                    ClosePopup();
                }
                else
                {
                    errorText.text = $"정보 조회 실패: {www.error}";
                }
            }
        }
    }
}
