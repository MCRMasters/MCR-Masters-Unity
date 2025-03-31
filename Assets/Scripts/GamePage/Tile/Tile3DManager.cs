using System.Collections.Generic;
using UnityEngine;

namespace MCRGame
{
    public class Tile3DManager : MonoBehaviour
    {
        public static Tile3DManager Instance { get; private set; }
        
        // 3D 타일 프리팹이 저장된 Resources 내 경로
        // [SerializeField] private string tilePrefabPath = "Prefabs/Game/Tile/3dTile";
        [SerializeField] private GameObject tilePrefab;
        
        // 3dTileFront 폴더에 있는 타일 앞면 Material들을 로드할 경로
        [SerializeField] private string tileFrontMatPath = "Materials/3dTileFront";
        // tileFrontMaterials: key는 Material의 이름, value는 Material
        private Dictionary<string, Material> tileFrontMaterials;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Resources에서 3D 타일 프리팹 로드
            // tilePrefab = Resources.Load<GameObject>(tilePrefabPath);
            if (tilePrefab == null)
            {
                Debug.LogError($"Tile3DManager: 타일 프리팹이 설정되지 않았습니다.");
            }

            // Resources에서 3dTileFront 폴더 내의 모든 Material 로드
            Material[] mats = Resources.LoadAll<Material>(tileFrontMatPath);
            if (mats == null || mats.Length == 0)
            {
                Debug.LogError($"Tile3DManager: 타일 앞면 Material들을 경로 '{tileFrontMatPath}'에서 찾을 수 없습니다.");
            }
            else
            {
                tileFrontMaterials = new Dictionary<string, Material>();
                foreach (Material mat in mats)
                {
                    if (!tileFrontMaterials.ContainsKey(mat.name))
                    {
                        tileFrontMaterials.Add(mat.name, mat);
                    }
                    else
                    {
                        Debug.LogWarning($"Tile3DManager: 이미 '{mat.name}' Material이 등록되어 있습니다.");
                    }
                }
                Debug.Log($"Tile3DManager: {tileFrontMaterials.Count}개의 타일 앞면 Material을 로드했습니다.");
            }
        }

        /// <summary>
        /// 지정한 tileName에 해당하는 3D 타일 GameObject를 생성합니다.
        /// MeshRenderer의 첫 번째 Material(Front 슬롯)에 dictionary에서 해당 이름의 Material을 적용합니다.
        /// Optional parent가 지정되면 생성된 타일의 부모로 설정합니다.
        /// </summary>
        /// <param name="tileName">타일 이름 (예: "1m")</param>
        /// <param name="parent">생성된 GameObject의 부모 Transform (옵션)</param>
        /// <returns>생성된 3D 타일 GameObject (실패 시 null)</returns>
        public GameObject Make3DTile(string tileName, Transform parent = null)
        {
            if (tilePrefab == null)
            {
                Debug.LogError("Tile3DManager: 타일 프리팹이 누락되었습니다.");
                return null;
            }

            // 3D 타일 프리팹을 Instantiate (부모 지정 가능)
            GameObject tileInstance = Instantiate(tilePrefab, parent);
            tileInstance.name = tileName;

            MeshRenderer mr = tileInstance.GetComponent<MeshRenderer>();
            if (mr == null)
            {
                Debug.LogError("Tile3DManager: 타일 프리팹에 MeshRenderer 컴포넌트가 없습니다.");
                return tileInstance;
            }

            Material[] currentMats = mr.materials;
            if (currentMats == null || currentMats.Length < 1)
            {
                Debug.LogWarning("Tile3DManager: MeshRenderer에 Material이 없습니다.");
                return tileInstance;
            }

            // dictionary에서 tileName에 해당하는 Material 찾기
            Material baseMat = null;
            if (!tileFrontMaterials.TryGetValue(tileName, out baseMat))
            {
                Debug.LogWarning($"Tile3DManager: '{tileName}'에 해당하는 Material을 찾을 수 없습니다. 기본 Material을 사용합니다.");
                // 기본 Material이 없다면 currentMats[0]를 그대로 사용합니다.
                baseMat = currentMats[0];
            }

            // baseMat의 인스턴스를 새로 생성하여 Front 슬롯(Material[0])에 적용
            Material frontMatInstance = new Material(baseMat);
            currentMats[0] = frontMatInstance;
            mr.materials = currentMats;

            return tileInstance;
        }
    }
}
