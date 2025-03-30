using UnityEngine;

namespace MCRGame
{
    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }

        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        public bool IsNewUser { get; private set; }

        // 새로 추가: 서버에서 가져온 유저 정보
        public string Uid { get; private set; }
        public string Nickname { get; private set; }
        public string Email { get; private set; }

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

        /// <summary>
        /// 서버에서 받아온 유저 정보를 저장
        /// </summary>
        public void SetUserData(string uid, string nickname, string email)
        {
            Uid = uid;
            Nickname = nickname;
            Email = email;
            Debug.Log($"[PlayerDataManager] 유저 정보 저장: {Nickname}, {Email}");
        }
    }
}