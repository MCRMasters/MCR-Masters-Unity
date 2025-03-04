using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BuildScript
{
    [MenuItem("Build/Build Linux Server")]
    public static void PerformBuild()
    {
        string[] scenes = { "Assets/Scenes/RoomScene.unity", "Assets/Scenes/GameScene.unity" };

        // ✅ Scripting Backend를 IL2CPP로 명시적으로 설정
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.IL2CPP);

        // ✅ API 호환성 레벨을 Unity에서 지원하는 값으로 설정
        PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Unity_4_8);
        // 또는 필요에 따라
        // PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard_2_0);

        // ✅ 서버 전용 빌드 설정
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Build/LinuxServer/mcr_masters.x86_64",
            target = BuildTarget.StandaloneLinux64,
            options = BuildOptions.None
        };
        buildPlayerOptions.options |= BuildOptions.Development;


        // ✅ 빌드 실행
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            UnityEngine.Debug.Log("✅ Linux Server Build succeeded: " + summary.totalSize + " bytes");
        }
        else if (summary.result == BuildResult.Failed)
        {
            UnityEngine.Debug.LogError("❌ Linux Server Build failed");
        }
    }
}
