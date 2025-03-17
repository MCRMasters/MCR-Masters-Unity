using UnityEngine;


[System.Serializable]
public class Tile2DMapping
{
    public string suit; // "m", "s", "p", "z"
    public int value;   // 1~9, 자패는 1~7 등
    public GameObject prefab2D;
}
