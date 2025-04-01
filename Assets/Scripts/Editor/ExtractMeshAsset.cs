using UnityEngine;
using UnityEditor;

namespace MCRGame.Editor
{
    public class ExtractMeshAsset
    {
        [MenuItem("Assets/Extract Selected Mesh as Asset", false, 200)]
        public static void ExtractSelectedMesh()
        {
            // Project 창에서 선택된 객체가 Mesh인지 확인합니다.
            Mesh selectedMesh = Selection.activeObject as Mesh;
            if (selectedMesh != null)
            {
                // 선택된 Mesh를 복제(clone)합니다.
                Mesh newMesh = Object.Instantiate(selectedMesh);
                newMesh.name = selectedMesh.name + "_Extracted";

                // 저장할 경로를 지정합니다. (예: Assets 폴더 루트)
                string assetPath = "Assets/" + newMesh.name + ".asset";

                // 동일 경로에 파일이 존재하는지 확인 후 생성합니다.
                AssetDatabase.CreateAsset(newMesh, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("Mesh Extraction", "Mesh asset created at:\n" + assetPath, "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Mesh Extraction", "선택된 Mesh가 없습니다.", "OK");
            }
        }
    }
}