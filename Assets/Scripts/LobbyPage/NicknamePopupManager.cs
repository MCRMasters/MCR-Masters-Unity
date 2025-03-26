using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class NicknamePopupManager : MonoBehaviour
{
    [Header("닉네임 팝업")]
    public GameObject nickNamePopup;       // NickNamePopup 오브젝트
    public InputField nicknameInputField;  // 팝업 안의 InputField
    public Button nameMakeButton;          // 팝업 안의 "Make!" 버튼

    [Header("테스트 버튼")]
    public Button nicknameButton;          // 기존 UI에 만든 [테스트용] 버튼

    // 서버에 닉네임 업데이트 PUT 요청을 보낼 URL (실제 주소로 변경 필요)
    private string putNicknameUrl = "http://localhost:8000/api/v1/user/me/nickname";

    private void Start()
    {
        // 테스트 버튼 클릭 시 팝업을 강제로 열 수 있음
        if (nicknameButton != null)
            nicknameButton.onClick.AddListener(OnClickTestNicknameButton);

        // 팝업 내 "Make!" 버튼 클릭 시 PUT 요청을 보내도록 이벤트 연결
        if (nameMakeButton != null)
            nameMakeButton.onClick.AddListener(OnClickNameMakeButton);
    }

    /// <summary>
    /// 테스트용 버튼 클릭 시 닉네임 팝업을 표시합니다.
    /// </summary>
    private void OnClickTestNicknameButton()
    {
        nickNamePopup.SetActive(true);
    }

    /// <summary>
    /// 팝업 내 "Make!" 버튼 클릭 시 인풋 필드의 닉네임을 서버에 PUT 요청으로 전송합니다.
    /// </summary>
    private void OnClickNameMakeButton()
    {
        string nickname = nicknameInputField.text.Trim();
        if (string.IsNullOrEmpty(nickname))
        {
            Debug.Log("닉네임을 입력해주세요!");
            return;
        }

        StartCoroutine(UpdateNickname(nickname));
    }

    /// <summary>
    /// 서버에 PUT 요청을 보내 닉네임을 업데이트합니다.
    /// 요청 본문은 {"nickname": "입력값"} 형태의 JSON입니다.
    /// </summary>
    private IEnumerator UpdateNickname(string nickname)
    {
        string jsonBody = $"{{\"nickname\":\"{nickname}\"}}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(putNicknameUrl, "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Authorization 헤더 추가: PlayerDataManager에 저장된 액세스 토큰 사용
            request.SetRequestHeader("Authorization", $"Bearer {PlayerDataManager.Instance.AccessToken}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[NicknamePopupManager] 닉네임 업데이트 성공: " + request.downloadHandler.text);

                // 테스트용: 로컬에 닉네임 저장
                PlayerPrefs.SetString("Nickname", nickname);
                PlayerPrefs.Save();

                // 팝업 닫기
                nickNamePopup.SetActive(false);

                Debug.Log($"닉네임 설정 완료: {nickname}");
            }
            else
            {
                Debug.LogError("[NicknamePopupManager] 닉네임 업데이트 실패: " + request.error);
            }
        }
    }

}
