using System.Collections.Generic;
using UnityEngine;

namespace MCRGame
{
    public class Tile2DManager : MonoBehaviour
    {
        public static Tile2DManager Instance { get; private set; }

        private string tile_images_path = "Images/TileImages";

        public Dictionary<string, Sprite> tile_name_to_sprite;

        [SerializeField] public GameObject baseTilePrefab;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Sprite[] sprites = Resources.LoadAll<Sprite>(tile_images_path);
            tile_name_to_sprite = new Dictionary<string, Sprite>();

            foreach (Sprite sprite in sprites)
            {
                if (!tile_name_to_sprite.ContainsKey(sprite.name))
                {
                    tile_name_to_sprite.Add(sprite.name, sprite);
                }
            }

            Debug.Log($"[TileImageManager] {tile_name_to_sprite.Count}개의 스프라이트를 로드했습니다.");
        }

        public Sprite get_sprite_by_name(string name)
        {
            if (tile_name_to_sprite.TryGetValue(name, out Sprite sprite))
            {
                return sprite;
            }
            Debug.LogWarning($"[TileImageManager] '{name}' 스프라이트를 찾을 수 없습니다.");
            return null;
        }
    }
}
