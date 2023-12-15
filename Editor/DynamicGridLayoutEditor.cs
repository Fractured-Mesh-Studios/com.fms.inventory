using UnityEngine;
using UnityEditor;
using InventoryEngine;

[CustomEditor(typeof(DynamicGridLayout))]
public class DynamicGridLayoutEditor : Editor
{
    DynamicGridLayout Target;
    bool PaddingFoldOut;

    private void OnEnable()
    {
        Target = target as DynamicGridLayout;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        Target.dynamic = EditorGUILayout.Toggle("dynamic", Target.dynamic);
        if (!Target.dynamic)
        {
            EditorGUILayout.HelpBox("The dynamic mode is the only one supported. the system reverts to default grid mode", MessageType.Warning);
            DrawDefaultInspector();
        }
        else
        {
            PaddingFoldOut = EditorGUILayout.Foldout(PaddingFoldOut, "Padding", true);
            if (PaddingFoldOut)
            {
                Target.padding.left = EditorGUILayout.IntField("Left", Target.padding.left);
                Target.padding.right = EditorGUILayout.IntField("Right", Target.padding.right);
                Target.padding.top = EditorGUILayout.IntField("Top", Target.padding.top);
                Target.padding.bottom = EditorGUILayout.IntField("Botton", Target.padding.bottom);
            }

            Target.childAlignment = (TextAnchor)EditorGUILayout.EnumPopup("Child Alignment", Target.childAlignment);
            Target.cellSize = EditorGUILayout.Vector2Field("Cell Size", Target.cellSize);
            Target.spacing = EditorGUILayout.Vector2Field("Spacing", Target.spacing);
            Target.constraint = (UnityEngine.UI.GridLayoutGroup.Constraint)EditorGUILayout.EnumPopup("Constraint", Target.constraint);
            Target.constraintCount = EditorGUILayout.IntField("Constraint Count", Target.constraintCount);

        }

        
        
    }
}
