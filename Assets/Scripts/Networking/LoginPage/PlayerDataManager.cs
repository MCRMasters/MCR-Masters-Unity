using System.Collections.Generic;
using UnityEngine;

namespace MCRGame.Net
{
    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }

        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        public bool IsNewUser { get; set; }

        // 새로 추가: 서버에서 가져온 유저 정보
        public string Uid { get; private set; } // 고유 사용자 ID (문자열)
        public string Nickname { get; private set; }
        public string Email { get; private set; }

        // 유저가 보유한 캐릭터 종류 리스트
        public List<CharacterResponse> OwnedCharacters { get; private set; } = new List<CharacterResponse>();
        // 현재 설정된(선택된) 캐릭터
        public string CurrentCharacter { get; private set; }

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
            Debug.Log($"[PlayerDataManager] AccessToken: {AccessToken}");
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

        /// <summary>
        /// 유저의 보유 캐릭터 목록과 현재 선택 캐릭터를 설정
        /// </summary>
        public void SetCharacterData(List<CharacterResponse> owned, string current)
        {
            OwnedCharacters = owned;
            CurrentCharacter = current;
            Debug.Log($"[PlayerDataManager] 보유 캐릭터: {OwnedCharacters}");
            Debug.Log($"[PlayerDataManager] 현재 캐릭터: {CurrentCharacter}");
        }

        public void SetCurrentCharacter(string toChange)
        {
            if (CurrentCharacter == toChange) return;
            bool flag = false;
            foreach (var c in OwnedCharacters){
                if (c.code == toChange){
                    flag = true;
                    break;
                }
            }
            if (!flag) {
                Debug.Log("no such character");
                return;
            }
            CurrentCharacter = toChange;
        }
    }
}
