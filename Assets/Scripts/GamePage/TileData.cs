namespace MCRGame
{
    [System.Serializable]
    public class TileData
    {
        public string suit;  // "m", "s", "p", "z"
        public int value;    // 만/삭/통: 1~9, 자패: 1~7 (예시)

        public override string ToString()
        {
            return value.ToString() + suit;
        }
    }
}
