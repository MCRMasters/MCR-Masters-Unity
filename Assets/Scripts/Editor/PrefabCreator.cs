using System.IO;
using UnityEditor;
using UnityEngine;

public class PrefabCreator
{
    [MenuItem("Tools/Create Prefabs From Folder")]
    public static void CreatePrefabsFromFolder()
    {
        // ① 원본 FBX(모델) 에셋이 들어있는 폴더
        string sourceFolder = "Assets/Models";
        // ② 만들어질 Prefab을 저장할 폴더
        string targetFolder = "Assets/Prefabs";

        // sourceFolder 내부의 GameObject 타입 에셋(FBX 임포트 후 모델)을 모두 검색
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { sourceFolder });
        foreach (string guid in guids)
        {
            // 에셋 경로, 로드
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (model == null) continue;

            // 원본 폴더 이후 경로만 떼서, targetFolder 하위에 동일한 구조로 Prefab 경로 생성
            string relativePath = assetPath.Substring(sourceFolder.Length).TrimStart('/');
            string prefabPath = Path.Combine(targetFolder, Path.ChangeExtension(relativePath, "prefab"));

            // 폴더가 없으면 생성
            string dir = Path.GetDirectoryName(prefabPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // Prefab 생성(덮어쓰기)
            PrefabUtility.SaveAsPrefabAsset(model, prefabPath);
            Debug.Log($"[Prefab Created] {assetPath} → {prefabPath}");
        }

        // 저장하고 에디터 리프레시
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
