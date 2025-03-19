using UnityEngine;
using System.Collections.Generic;

public class TileLoader : MonoBehaviour
{
    private Dictionary<(string suit, int value), GameObject> tiles2D = new();
    private Dictionary<(string suit, int value), GameObject> tiles3D = new();

    private static TileLoader instance;
    public static TileLoader Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("TileLoader");
                instance = go.AddComponent<TileLoader>();
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAll();
    }

    private void LoadAll()
    {
        GameObject[] all = Resources.LoadAll<GameObject>("GamePrefabs");

        foreach (var prefab in all)
        {
            string name = prefab.name.ToLower(); // ex: "1m2d", "3p3d"
            if (name.Length < 3) continue;

            if (!int.TryParse(name.Substring(0, 1), out int value)) continue;
            string suit = name.Substring(1, 1);
            string dim = name.Substring(2); // "2d" or "3d"

            var key = (suit, value);
            if (dim == "2d")
            {
                if (!tiles2D.ContainsKey(key)) tiles2D[key] = prefab;
            }
            else if (dim == "3d")
            {
                if (!tiles3D.ContainsKey(key)) tiles3D[key] = prefab;
            }
        }
    }

    public GameObject Get2DPrefab(string suit, int value)
    {
        tiles2D.TryGetValue((suit.ToLower(), value), out var prefab);
        return prefab;
    }

    public GameObject Get3DPrefab(string suit, int value)
    {
        tiles3D.TryGetValue((suit.ToLower(), value), out var prefab);
        return prefab;
    }
}
