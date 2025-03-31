using UnityEngine;
using UnityEditor;

public class CreateMeshRendererPrefab
{
    [MenuItem("Assets/Create MeshRenderer Prefab from Selected Mesh", false, 201)]
    public static void CreatePrefabFromMesh()
    {
        // 선택된 객체가 Mesh인지 확인합니다.
        Mesh selectedMesh = Selection.activeObject as Mesh;
        if (selectedMesh == null)
        {
            EditorUtility.DisplayDialog("오류", "선택된 Mesh가 없습니다.", "확인");
            return;
        }

        // 새 GameObject 생성 (이름에 _Prefab 추가)
        GameObject tempGO = new GameObject(selectedMesh.name + "_Prefab");

        // MeshFilter 추가 및 추출한 Mesh 할당
        MeshFilter meshFilter = tempGO.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = selectedMesh;

        // MeshRenderer 추가 및 기본 Material 할당
        MeshRenderer meshRenderer = tempGO.AddComponent<MeshRenderer>();
        // 기본 Standard 셰이더를 사용하는 새 Material 생성
        Material defaultMat = new Material(Shader.Find("Standard"));
        defaultMat.name = selectedMesh.name + "_DefaultMat";
        meshRenderer.sharedMaterial = defaultMat;

        // Prefab으로 저장할 경로 설정 (Assets 폴더)
        string prefabPath = "Assets/" + tempGO.name + ".prefab";
        prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);
        PrefabUtility.SaveAsPrefabAsset(tempGO, prefabPath);

        // 임시 GameObject 삭제
        GameObject.DestroyImmediate(tempGO);

        EditorUtility.DisplayDialog("성공", "Prefab이 생성되었습니다:\n" + prefabPath, "확인");
    }
}
