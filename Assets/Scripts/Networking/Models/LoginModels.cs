using System.Collections.Generic;

namespace MCRGame.Net
{
    [System.Serializable]
    public class AuthUrlResponse
    {
        public string auth_url;
        public string session_id;
    }

    [System.Serializable]
    public class TokenResponse
    {
        public string access_token;
        public string refresh_token;
        public bool is_new_user;
        public string token_type;
    }

    /// <summary>
    /// 서버에서 /api/v1/user/me 응답으로 내려주는 JSON 모델
    /// {"uid":"string","nickname":"string","email":"string"}
    /// </summary>
    [System.Serializable]
    public class UserMeResponse
    {
        public string uid;
        public string nickname;
        public string email;
        public CharacterResponse current_character;
        public List<CharacterResponse> owned_characters;
    }

    [System.Serializable]
    public class CharacterResponse
    {
        public string code;
        public string name;
    }

    [System.Serializable]
    public class IsPlayingResponse
    {
        public string message;
        public IsPlayingData data;
    }

    [System.Serializable]
    public class IsPlayingData
    {
        public bool in_playing_room;
        public string game_id;
    }

}