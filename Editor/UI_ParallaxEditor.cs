using UnityEngine;
using UnityEditor;
using InventoryEngine;

namespace InventoryEditor
{
    [CustomEditor(typeof(UI_Parallax))]
    public class UI_ParallaxEditor : Editor
    {
        UI_Parallax Target;
        bool Foldout;

        void OnEnable()
        {
            Target = target as UI_Parallax;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Space(10);
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUILayout.Label("Parallax");
            Target.Smooth = EditorGUILayout.FloatField("Smooth", Target.Smooth);
            Target.Sensibility = EditorGUILayout.Slider("Sensibility", Target.Sensibility, 1.0f, 5.0f);
            Target.AutoCenter = EditorGUILayout.Toggle("AutoCenter", Target.AutoCenter);
            //Limits
            Foldout = EditorGUILayout.Foldout(Foldout, "Limits", true);
            if (Foldout)
            {
                Target.Limits.Enable = EditorGUILayout.Toggle("Enable", Target.Limits.Enable);
                Target.Limits.Vertical = EditorGUILayout.Vector2Field("Vertical", Target.Limits.Vertical);
                Target.Limits.Horizontal = EditorGUILayout.Vector2Field("Horizontal", Target.Limits.Horizontal);
            }
            Target.Mode = (UI_Parallax.UI_EParallaxMode)EditorGUILayout.EnumPopup("Mode", Target.Mode);

            GUILayout.Space(10);
            GUILayout.Label("Axis");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MouseX"));
            Target.InvertHorizontal = EditorGUILayout.ToggleLeft("Invert", Target.InvertHorizontal);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MouseY"));
            Target.InvertVertical = EditorGUILayout.ToggleLeft("Invert", Target.InvertVertical);
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}