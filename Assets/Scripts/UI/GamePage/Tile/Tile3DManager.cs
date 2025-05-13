using System.Collections.Generic;
using UnityEngine;

namespace MCRGame.UI
{
    public class Tile3DManager : MonoBehaviour
    {
        public static Tile3DManager Instance { get; private set; }

        [Header("Prefabs & Paths")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private string tileFrontMatPath = "Materials/3dTileFront";

        [Header("Default Side/Back Materials")]
        [SerializeField] private Material defaultSideMaterial;   // Inspector에 드래그
        [SerializeField] private Material defaultBackMaterial;   // Inspector에 드래그

        private Dictionary<string, Material> tileFrontMaterials;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (tilePrefab == null)
                Debug.LogError("Tile3DManager: 타일 프리팹이 설정되지 않았습니다.");

            if (defaultSideMaterial == null || defaultBackMaterial == null)
                Debug.LogError("Tile3DManager: 기본 Side/Back Material이 할당되지 않았습니다.");

            var mats = Resources.LoadAll<Material>(tileFrontMatPath);
            if (mats == null || mats.Length == 0)
                Debug.LogError($"Tile3DManager: 앞면 Material들을 '{tileFrontMatPath}'에서 찾을 수 없습니다.");
            else
            {
                tileFrontMaterials = new Dictionary<string, Material>();
                foreach (var mat in mats)
                {
                    if (!tileFrontMaterials.ContainsKey(mat.name))
                        tileFrontMaterials.Add(mat.name, mat);
                }
                Debug.Log($"Tile3DManager: {tileFrontMaterials.Count}개의 앞면 Material 로드 완료.");
            }
        }

        public GameObject Make3DTile(string tileName, Transform parent = null)
        {
            if (tilePrefab == null) return null;

            var go = Instantiate(tilePrefab, parent);
            go.name = tileName;

            // ─── 1) 루트 MeshRenderer 인스턴스화 ───
            var mr = go.GetComponent<MeshRenderer>();
            if (mr == null)
            {
                Debug.LogError("[Tile3DManager] MeshRenderer 누락");
                return go;
            }

            var mats = mr.materials;                     // ← 이 시점에 이미 ‘복사본’ 배열
            if (mats.Length < 3)
                Debug.LogWarning($"[Tile3DManager] subMesh가 3개가 아닙니다. materials.Length={mats.Length}");

            // 0: side, 1: back, 2: front  ── 전부 독립 인스턴스로 교체
            mats[0] = new Material(defaultSideMaterial);
            mats[1] = new Material(defaultBackMaterial);

            if (tileFrontMaterials != null &&
                tileFrontMaterials.TryGetValue(tileName, out var frontBase))
            {
                mats[2] = new Material(frontBase);
            }
            else
            {
                Debug.LogWarning($"[Tile3DManager] 앞면 Material 못 찾음: {tileName} (기본값 사용)");
                mats[2] = new Material(mats[2]);          // 원본이라도 복사본으로
            }

            mr.materials = mats;                         // 복사본 배열 지정

            // ─── 2) 자식 Renderer(그림자·테두리 등)도 모두 복사 ───
            foreach (var rend in go.GetComponentsInChildren<Renderer>())
            {
                if (rend == mr) continue;                // 루트는 이미 처리
                var childMats = rend.materials;          // 배열 + 소재 모두 복제
                for (int i = 0; i < childMats.Length; i++)
                    childMats[i] = new Material(childMats[i]);
                rend.materials = childMats;
            }

            return go;
        }

    }
}
