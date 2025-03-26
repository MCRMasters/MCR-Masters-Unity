using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class NicknameCheckManager : MonoBehaviour
{
    [Header("UI References")]
    public InputField nicknameInputField; // 닉네임 입력창
    public Button nickNameCheckButton;    // "Check" or "Make!" 버튼

    // 서버 URL (필요 시 실제 도메인/포트로 수정)
    private string putNicknameUrl = "http://localhost:8000/api/v1/user/me/nickname";

    private void Start()
    {
        if (nickNameCheckButton != null)
            nickNameCheckButton.onClick.AddListener(OnClickNicknameCheckButton);
    }

    /// <summary>
    /// "Make!" 버튼(또는 "Check")을 눌렀을 때 호출되는 함수
    /// </summary>
    private void OnClickNicknameCheckButton()
    {
        string nickname = nicknameInputField.text.Trim();

        if (string.IsNullOrEmpty(nickname))
        {
            Debug.Log("닉네임을 입력해주세요!");
            return;
        }

        // PUT 요청을 보내 닉네임 업데이트
        StartCoroutine(PutNicknameToServer(nickname));
    }

    /// <summary>
    /// 서버에 PUT 요청을 보내 닉네임을 업데이트하는 코루틴
    /// </summary>
    private IEnumerator PutNicknameToServer(string newNickname)
    {
        // JSON 데이터 생성
        string jsonBody = $"{{\"nickname\":\"{newNickname}\"}}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        // UnityWebRequest 설정
        using (UnityWebRequest request = new UnityWebRequest(putNicknameUrl, "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // 만약 인증 토큰이 필요하다면 (로그인 시스템과 연동 시)
            // request.SetRequestHeader("Authorization", "Bearer " + PlayerDataManager.Instance.AccessToken);

            // 요청 전송
            yield return request.SendWebRequest();

            // 응답 처리
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[NicknameCheckManager] 닉네임 업데이트 성공: {request.downloadHandler.text}");
                // 서버가 {"message":"..."} 형태로 응답한다면 파싱 가능
                // 닉네임 업데이트 후 로컬 변수나 UI 갱신도 가능
            }
            else
            {
                Debug.LogError($"[NicknameCheckManager] 닉네임 업데이트 실패: {request.error}");
            }
        }
    }
}
