using UnityEngine;

namespace MCRGame
{
    [System.Serializable]
    public class Tile3DMapping
    {
        public string suit;   // 예: "m", "s", "p", "z"
        public int value;     // 만/삭/통 1~9, 자패 1~7 등
        public GameObject prefab3D; // 3D 버림패 프리팹
    }
}