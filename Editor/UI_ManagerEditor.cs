
using UnityEngine;
using UnityEditor;
using InventoryEngine;
using UnityEngine.UI;

[CustomEditor(typeof(UI_Manager))]
public class UI_ManagerEditor : Editor
{
    UI_Manager Target;

    private void OnEnable()
    {
        Target = target as UI_Manager;
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("Manager", EditorStyles.boldLabel);
        GUIContent Content;
        GUIStyle LabelStyle = new GUIStyle();
        LabelStyle.normal.textColor = Color.red;
        LabelStyle.fontStyle = FontStyle.Bold;
        LabelStyle.alignment = TextAnchor.MiddleCenter;

        Content = new GUIContent("Persistence", "[Enable/Disable] the object persist between scenes. (singleton)");
        Target.Persistence = EditorGUILayout.Toggle(Content, Target.Persistence);

        Content = new GUIContent("Drag", "Prefabricated drag object to be created by the system");
        Target.Drag = (GameObject)EditorGUILayout.ObjectField(Content, Target.Drag, typeof(GameObject), true);
        if (Target.Drag)
        {
            if (!Target.Drag.GetComponent<UI_Drag>())
                GUILayout.Label("Drag Object Incompatible", LabelStyle);
        }
        
        Content = new GUIContent("Hover", "Prefabricated hover object to be created by the system");
        Target.Hover = (GameObject)EditorGUILayout.ObjectField(Content, Target.Hover, typeof(GameObject), true);
        if (Target.Hover)
        {
            if (!Target.Hover.GetComponent<UI_DragHover>())
                GUILayout.Label("Hover Object Incompatible", LabelStyle);
        }
        
        Target.Mode = (UI_EDragMode)EditorGUILayout.EnumPopup("Mode", Target.Mode);
        Target.Data = (UI_Data)EditorGUILayout.ObjectField("Data", Target.Data, typeof(UI_Data), true);
        
        if(!Target.Drag || !Target.Hover || !Target.Data)
        {
            int Count = 0;
            Count += !Target.Drag ? 1 : 0;
            Count += !Target.Hover ? 1 : 0;
            Count += !Target.Data ? 1 : 0;

            string[] LogText = new string[3];
            LogText[0] = Target.Drag ? string.Empty : "Drag";
            LogText[1] = Target.Hover ? string.Empty : "Hover";
            LogText[2] = Target.Data ? string.Empty : "Database";

            string[] Separator = new string[2];
            Separator[0] = (!Target.Drag && !Target.Hover) ? "/" : string.Empty;
            Separator[1] = (!Target.Hover &&!Target.Data) ? "/" : string.Empty;

            string FinalText = "[" + LogText[0] + Separator[0] + LogText[1] + Separator[1] + LogText[2] + "] "+(Count>1?"are":"is")+" null.";
            EditorGUILayout.HelpBox(FinalText, MessageType.Warning);

            EditorUtility.SetDirty(Target);
        }
    }
}
