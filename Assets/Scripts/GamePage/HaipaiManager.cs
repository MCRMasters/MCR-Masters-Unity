using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MCRGame
{
    public class HaipaiManager : MonoBehaviour
    {
        [SerializeField] private GameObject baseTilePrefab;
        private RectTransform haipaiRect;
        List<GameObject> tiles;

        GameObject tsumoTile;
        
        void Awake()
        {
            haipaiRect = GetComponent<RectTransform>();
            haipaiRect.anchorMin = new Vector2(0, 0.5f);
            haipaiRect.anchorMax = new Vector2(0, 0.5f);
            tiles = new List<GameObject>();
            tsumoTile = null;
        }
        void Start()
        {
            initTestHaipai();
        }

        void addTile(string tileName)
        {
            GameObject newTile = Instantiate(baseTilePrefab, transform);
            TileManager tileManager = newTile.GetComponent<TileManager>();
            if (tileManager != null){
                tileManager.SetTileName(tileName);
            }
            RectTransform tileRect = newTile.GetComponent<RectTransform>();
            if (tileManager != null){
                tileRect.anchorMin = new Vector2(0, 0.5f);
                tileRect.anchorMax = new Vector2(0, 0.5f);
                tileRect.pivot = new Vector2(0, 0.5f);
            }
            tiles.Add(newTile);
        }

        void replaceTiles()
        {
            int tsumoTileIndex = 0;
            int count = 0;
            for (int i=0;i<tiles.Count; ++i){
                if (tiles[i] == tsumoTile){
                    tsumoTileIndex = i;
                    continue;
                }
                RectTransform tileRect = tiles[i].GetComponent<RectTransform>();
                if (tileRect != null){
                    tileRect.anchoredPosition = new Vector2(tileRect.rect.width * count, 0);
                }
                count += 1;
            }
            if (tsumoTile != null){
                RectTransform tileRect = tiles[tsumoTileIndex].GetComponent<RectTransform>();
                if (tileRect != null){
                    tileRect.anchoredPosition = new Vector2(tileRect.rect.width * count + tileRect.rect.width * 0.2f , 0);
                }
            }
        }

        void sortTileList()
        {
            tiles = tiles.Select(child => child.gameObject)
            .OrderBy(child =>
            {
                string namePart = child.name.Substring(0, 2);
                if (namePart == "0f")
                    return (2, string.Empty);
                string reversedString = string.Concat(namePart.Reverse());
                return (1, reversedString);
            })
            .ToList();
        }
        void initTestHaipai()
        {
            List<string> tileNames = Tile2DManager.Instance.tile_name_to_sprite.Keys.ToList();
            for(int i = 0; i < 14; ++i){
                int index = Random.Range(0, tileNames.Count);
                addTile(tileNames[index]);
            }
            tsumoTile = tiles[tiles.Count - 1];
            sortTileList();
            replaceTiles();
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}