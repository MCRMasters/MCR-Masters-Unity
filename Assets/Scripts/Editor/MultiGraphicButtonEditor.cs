using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(MultiGraphicButton), true)]
[CanEditMultipleObjects]
public class MultiGraphicButtonEditor : ButtonEditor
{
    SerializedProperty additionalGraphics;

    protected override void OnEnable()
    {
        base.OnEnable();
        // MultiGraphicButton 스크립트에서 선언한 필드 이름과 일치해야 함.
        additionalGraphics = serializedObject.FindProperty("additionalGraphics");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // 기존 ButtonEditor GUI

        serializedObject.Update();
        EditorGUILayout.PropertyField(additionalGraphics, true);
        serializedObject.ApplyModifiedProperties();
    }
}
