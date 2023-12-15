using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using InventoryEngine;

namespace InventoryEditor
{
    [CustomEditor(typeof(UI_SlotStack))]
    public class UI_SlotStackEditor : UI_SlotEditor
    {
        private void OnEnable()
        {
            Target = target as UI_SlotStack;
        }

        public override void OnInspectorGUI()
        {
            UI_SlotStack Target = (UI_SlotStack)this.Target;

            GUILayout.Space(10);
            OnInspectorSlot();

            //Color
            EditorGUILayout.PropertyField(serializedObject.FindProperty("NormalColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("HighLightedColor"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("BackgroundComponent"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IconComponent"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("StackComponent"));

            GUILayout.Space(5);
            GUILayout.Label("Stack", EditorStyles.boldLabel);
            Content = new GUIContent("Stack", "[Enable/Disable] if the stack is updated every frame");
            Target.StackUpdate = EditorGUILayout.Toggle(Content, Target.StackUpdate);
            Target.MinStack = EditorGUILayout.IntField("Min Stack", Target.MinStack);
            Target.MaxStack = EditorGUILayout.IntField("Max Stack", Target.MaxStack);
            Target.StackKey = (KeyCode)EditorGUILayout.EnumPopup("StackKey", Target.StackKey);
            Target.StackKeyDrop = (KeyCode)EditorGUILayout.EnumPopup("StackKeyDrop", Target.StackKeyDrop);

            serializedObject.ApplyModifiedProperties();
        }

    }
}
