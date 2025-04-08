using System;

namespace MCRGame.Common
{
    [Serializable]
    public class Player
    {
        public string Uid { get; set; }
        public string Nickname { get; set; }
        public int Index { get; set; }
        public int Score { get; set; }

        // 기본 생성자 (직렬화를 위해 필요할 수 있음)
        public Player() { }

        // 생성자를 통한 초기화
        public Player(string uid, string nickname, int index, int score)
        {
            Uid = uid;
            Nickname = nickname;
            Index = index;
            Score = score;
        }

        public override string ToString()
        {
            return $"Player [Uid: {Uid}, Nickname: {Nickname}, Index: {Index}, Score: {Score}]";
        }
    }
}
