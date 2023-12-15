using UnityEngine;
using UnityEditor;
using InventoryEngine.Extra;

namespace InventoryEditor
{

    [CustomEditor(typeof(UI_Socket))]
    public class UI_SocketEditor : Editor
    {
        UI_Socket Target;
        bool Interactable = true, Array0;

        private void OnEnable()
        {
            Target = target as UI_Socket;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Socket"));
            Interactable = EditorGUILayout.Toggle("Interactable", Interactable);
            Target.Interactable = Interactable;
            GUI.enabled = Interactable;
            SerializedProperty Transition = serializedObject.FindProperty("Transition");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Target"));
            EditorGUILayout.PropertyField(Transition);
            switch ((UnityEngine.UI.Selectable.Transition)Transition.intValue)
            {
                //Color
                case UnityEngine.UI.Selectable.Transition.ColorTint:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Block"));
                    break;
                //Sprite
                case UnityEngine.UI.Selectable.Transition.SpriteSwap:
                    GUI.skin.label.fontStyle = FontStyle.Bold;
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label("Work in Progres");
                    break;
                //Animation
                case UnityEngine.UI.Selectable.Transition.Animation:
                    GUI.skin.label.fontStyle = FontStyle.Bold;
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label("Work in Progres");
                    break;
            }
            GUI.enabled = true;

            Array0 = EditorGUILayout.Foldout(Array0, "DetectionFilter", true);
            if (Array0)
            {
                SerializedProperty Array = serializedObject.FindProperty("DetectionFilter");
                Array.arraySize = EditorGUILayout.IntField("Size", Array.arraySize);
                EditorGUILayout.BeginVertical();
                for (int i = 0; i < Array.arraySize; i++)
                {
                    var Element = Array.GetArrayElementAtIndex(i);
                    EditorGUILayout.ObjectField(Element, typeof(Transform));
                }
                EditorGUILayout.EndVertical();
            }


            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnClick"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}