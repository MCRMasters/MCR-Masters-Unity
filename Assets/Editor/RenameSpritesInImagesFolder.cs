using UnityEngine;
using UnityEditor;
using System.IO;

public class RenameSingleSpriteInImagesFolder
{
    [MenuItem("Tools/Rename Single Sprites In Resources/Images Folder")]
    public static void RenameSingleSprites()
    {
        // 변경할 폴더 경로 지정 (Assets/Resources/Images)
        string folderPath = "Assets/Resources/Images";
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
        Debug.Log($"Found {guids.Length} Texture2D assets in {folderPath}");

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                continue;

            // 단일 스프라이트만 처리 (멀티 스프라이트는 건너뜁니다)
            if (importer.spriteImportMode != SpriteImportMode.Single)
                continue;

            // 파일 이름(확장자 제외) 추출
            string fileName = Path.GetFileNameWithoutExtension(assetPath);

            // Texture2D 에셋 로드 후 이름 비교
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture != null && texture.name != fileName)
            {
                AssetDatabase.RenameAsset(assetPath, fileName);
                Debug.Log($"[{assetPath}] 단일 스프라이트 이름을 '{fileName}'으로 변경했습니다.");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Resources/Images 폴더 내 단일 스프라이트 이름 변경 완료.");
    }
}
