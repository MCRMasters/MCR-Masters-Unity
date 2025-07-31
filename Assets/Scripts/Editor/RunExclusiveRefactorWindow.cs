using UnityEditor;
using UnityEngine;

namespace MCRGame.Editor
{
    public class RunExclusiveRefactorWindow : EditorWindow
    {
        [MenuItem("Tools/RunExclusive Refactor Window")]
        public static void ShowWindow()
        {
            GetWindow<RunExclusiveRefactorWindow>("RunExclusive Refactor");
        }

        private void OnGUI()
        {
            GUILayout.Label("RunExclusive → DOTween Sequence", EditorStyles.boldLabel);
            GUILayout.Space(5);
            if (GUILayout.Button("1. GameHandManager")) ExecuteStep1();
            if (GUILayout.Button("2. FlowerReplacementController")) ExecuteStep2();
            if (GUILayout.Button("3. RightClickManager")) ExecuteStep3();
            if (GUILayout.Button("4. GameManager.Network")) ExecuteStep4();
            if (GUILayout.Button("5. GameManager.Animation")) ExecuteStep5();
        }

        private static void ExecuteStep1()
        {
            Debug.Log("[Refactor Step1] Convert GameHandManager logic to DOTween Sequence.");
        }

        private static void ExecuteStep2()
        {
            Debug.Log("[Refactor Step2] Refactor FlowerReplacementController coroutine to Sequence.");
        }

        private static void ExecuteStep3()
        {
            Debug.Log("[Refactor Step3] Replace RightClickManager coroutine call with Sequence.");
        }

        private static void ExecuteStep4()
        {
            Debug.Log("[Refactor Step4] Update GameManager.Network methods to use Sequence.");
        }

        private static void ExecuteStep5()
        {
            Debug.Log("[Refactor Step5] Migrate GameManager.Animation to Sequence-driven animations.");
        }
    }
}
