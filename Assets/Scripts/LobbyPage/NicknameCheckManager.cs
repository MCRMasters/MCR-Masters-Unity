using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NicknameCheckManager : MonoBehaviour
{
    [Header("UI References")]
    public InputField nicknameInputField;      // 닉네임 입력창
    public Button nickNameCheckButton;         // "Check" 버튼

    // 더미 데이터 (서버 닉네임 목록이라 가정)
    // 나중엔 서버에 요청해서 같은값이 있는지 확인하는 api 필요
    private List<string> dummyNicknameList = new List<string>
    {
        "www", "aaa", "bbb", "ccc"
    };

    private void Start()
    {
        // 닉네임 체크 버튼에 리스너 연결
        if (nickNameCheckButton != null)
            nickNameCheckButton.onClick.AddListener(OnClickNicknameCheckButton);
    }

    /// <summary>
    /// "Check" 버튼을 눌렀을 때 호출되는 함수
    /// </summary>
    private void OnClickNicknameCheckButton()
    {
        string nickname = nicknameInputField.text.Trim();

        if (string.IsNullOrEmpty(nickname))
        {
            Debug.Log("닉네임을 입력해주세요!");
            return;
        }

        // 대소문자 구분 여부를 결정(소문자로 비교 예시)
        string lowerNick = nickname.ToLower();

        // 더미 데이터에 이미 있는지 확인
        if (dummyNicknameList.Contains(lowerNick))
        {
            Debug.Log($"중복된 닉네임입니다: {nickname}");
        }
        else
        {
            Debug.Log($"사용 가능한 닉네임입니다: {nickname}");
        }
    }
}
