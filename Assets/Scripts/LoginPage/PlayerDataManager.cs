using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    public string AccessToken { get; private set; }
    public string RefreshToken { get; private set; }
    public bool IsNewUser { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetTokenData(string accessToken, string refreshToken, bool isNewUser)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        IsNewUser = isNewUser;
        Debug.Log("[PlayerDataManager] 토큰 저장 완료");
    }
}
