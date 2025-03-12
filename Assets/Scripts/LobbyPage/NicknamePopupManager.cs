using UnityEngine;
using UnityEngine.UI;

public class NicknamePopupManager : MonoBehaviour
{
    [Header("닉네임 팝업")]
    public GameObject nickNamePopup;       // NickNamePopup 오브젝트
    public InputField nicknameInputField;  // 팝업 안의 InputField
    public Button nameMakeButton;          // 팝업 안의 "Make!" 버튼

    [Header("테스트 버튼")]
    public Button nicknameButton;          // 기존 UI에 만든 [테스트용] 버튼

    private void Start()
    {
        // 씬이 로드되면 기존 닉네임 유무를 확인해 팝업 열지 결정
        //CheckNicknameOnStart();

        // 테스트용 버튼(닉네임 버튼) 이벤트 연결
        if (nicknameButton != null)
            nicknameButton.onClick.AddListener(OnClickTestNicknameButton);

        // 팝업 내부 "Make!" 버튼 이벤트 연결
        if (nameMakeButton != null)
            nameMakeButton.onClick.AddListener(OnClickNameMakeButton);
    }

    /// <summary>
    /// 씬 로드 시 닉네임이 있는지 확인해서 팝업 표시/비표시
    /// 
    /// </summary>
    ///
    /*
    private void CheckNicknameOnStart()
    {
    
    // 닉네임이 없으면 팝업 열기
    추후에 서버측통신해서 닉네임쿼리를 받아서 처리해야함.
    // 닉네임이 있으면 팝업 닫기
    
    }
    */

    /// <summary>
    /// [테스트용] NicknameButton 클릭 시 팝업 열기
    /// </summary>
    private void OnClickTestNicknameButton()
    {
        // 이미 닉네임이 있는 경우에도 강제로 팝업을 열어 테스트 가능
        nickNamePopup.SetActive(true);
    }

    /// <summary>
    /// 팝업 내부의 "Make!" 버튼 클릭 시 닉네임 서버 전송 및 팝업 닫기
    /// </summary>
    private void OnClickNameMakeButton()
    {
        string nickname = nicknameInputField.text.Trim();
        if (string.IsNullOrEmpty(nickname))
        {
            Debug.Log("닉네임을 입력해주세요!");
            return;
        }

        // 1) 서버에 닉네임 전송 (예: REST, WebSocket 등)
        //    ServerManager.Instance.SendNickname(nickname);

        // 2) 로컬에 닉네임 저장 (테스트용)
        PlayerPrefs.SetString("Nickname", nickname);
        PlayerPrefs.Save();

        // 3) 팝업 닫기
        nickNamePopup.SetActive(false);

        Debug.Log($"닉네임 설정 완료: {nickname}");
    }
}
