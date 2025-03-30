using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NicknamePopupManager : MonoBehaviour
{
    [Header("닉네임 팝업")]
    public GameObject nickNamePopup;       // NickNamePopup 오브젝트
    public InputField nicknameInputField;  // 팝업 내 InputField
    public Button nameMakeButton;          // 팝업 내 "Make!" 버튼

    [Header("테스트 버튼")]
    public Button nicknameButton;          // 테스트용 버튼

    [Header("API 관리")]
    public PutNicknameApi putNicknameApi;  // PutNicknameApi 스크립트를 에디터에서 할당

    private void Start()
    {
        // 테스트 버튼 이벤트 등록 (팝업 강제 오픈)
        if (nicknameButton != null)
            nicknameButton.onClick.AddListener(OnClickTestNicknameButton);

        // 팝업 내 "Make!" 버튼 이벤트 등록 (닉네임 업데이트)
        if (nameMakeButton != null)
            nameMakeButton.onClick.AddListener(OnClickNameMakeButton);

        // PlayerDataManager의 닉네임이 빈 문자열이거나 정확히 ':'일 경우 팝업을 자동으로 띄웁니다.
        if (PlayerDataManager.Instance != null &&
            (string.IsNullOrEmpty(PlayerDataManager.Instance.Nickname) || PlayerDataManager.Instance.Nickname == ":"))
        {
            nickNamePopup.SetActive(true);
        }
    }

    /// <summary>
    /// 테스트 버튼 클릭 시 닉네임 팝업을 표시합니다.
    /// </summary>
    private void OnClickTestNicknameButton()
    {
        nickNamePopup.SetActive(true);
    }

    /// <summary>
    /// 팝업 내 "Make!" 버튼 클릭 시 입력된 닉네임을 서버에 업데이트합니다.
    /// </summary>
    private void OnClickNameMakeButton()
    {
        string nickname = nicknameInputField.text.Trim();
        if (string.IsNullOrEmpty(nickname))
        {
            Debug.Log("닉네임을 입력해주세요!");
            return;
        }

        // PutNicknameApi의 UpdateNickname 코루틴 호출
        StartCoroutine(putNicknameApi.UpdateNickname(nickname, PlayerDataManager.Instance.AccessToken,
            onSuccess: (response) =>
            {
                Debug.Log("[NicknamePopupManager] 닉네임 업데이트 성공: " + response);
                // 로컬에 닉네임 저장
                PlayerPrefs.SetString("Nickname", nickname);
                PlayerPrefs.Save();
                // 팝업 닫기
                nickNamePopup.SetActive(false);
                Debug.Log($"닉네임 설정 완료: {nickname}");
            },
            onError: (error) =>
            {
                Debug.LogError("[NicknamePopupManager] 닉네임 업데이트 실패: " + error);
            }));
    }
}
