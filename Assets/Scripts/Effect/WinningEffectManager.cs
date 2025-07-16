using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace MCRGame.Effect
{
    /// <summary>
    /// Utility manager used to spawn and play winning visual effects.
    /// </summary>
    public class WinningEffectManager : MonoBehaviour
    {
        public static WinningEffectManager Instance { get; private set; }
        private readonly Dictionary<string, GameObject> effectPrefabs = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllEffects();
        }
        private void LoadAllEffects()
        {
            GameObject[] prefabs = Resources.LoadAll<GameObject>("Effects/WinningEffects");
            foreach (var prefab in prefabs)
            {
                if (!effectPrefabs.ContainsKey(prefab.name))
                    effectPrefabs[prefab.name] = prefab;
            }
        }

        /// <summary>
        /// Instantiate <paramref name="effectPrefab"/> and play its effect.
        /// Returns the sequence so callers can chain extra behaviour.
        /// </summary>
        public Sequence PlayEffect(GameObject effectPrefab, Vector3 position, Quaternion rotation)
        {
            if (effectPrefab == null)
            {
                Debug.LogWarning("WinningEffectManager.PlayEffect: prefab is null");
                return DOTween.Sequence();
            }

            var go = Instantiate(effectPrefab, position, rotation);
            if (!go.TryGetComponent<IWinningEffect>(out var effect))
                effect = go.GetComponentInChildren<IWinningEffect>();
            if (effect == null)
            {
                Debug.LogWarning("Winning effect component not found on prefab");
                return DOTween.Sequence();
            }

            return effect.PlayEffect();
        }

        /// <summary>
        /// Convenience overload to spawn the effect at a tile's transform.
        /// </summary>
        public Sequence PlayEffectAtTile(string effectName, GameObject tile)
        {
            if (!effectPrefabs.TryGetValue(effectName, out var effectPrefab))
            {
                Debug.LogWarning($"[WinningEffectManager] Effect prefab '{effectName}' not found.");
                return null;
            }
            var offset = new Vector3(0, 1f, 0);
            var pos = tile.transform.position + offset;
            if (tile == null || tile.transform == null)
                return DOTween.Sequence();
            return PlayEffect(effectPrefab, pos, tile.transform.rotation);
        }
    }
}
