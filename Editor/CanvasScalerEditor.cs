using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(CanvasScalerBase))]
public class CanvasScalerBaseEditor : CanvasScalerEditor
{
    private CanvasScalerBase Target;

    protected override void OnEnable()
    {
        base.OnEnable();

        Target = target as CanvasScalerBase;
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.BeginVertical("Box");
        Target.Resolution = EditorGUILayout.Toggle("Resolution", Target.Resolution);
        Target.MinResolution = EditorGUILayout.FloatField("Min Resolution", Target.MinResolution);
        Target.MaxResolution = EditorGUILayout.FloatField("Max Resolution", Target.MaxResolution);
        EditorGUILayout.EndVertical();
        GUILayout.Space(10);
    }
}
