using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using InventoryEngine;

namespace InventoryEditor
{
    [CustomEditor(typeof(UI_ContainerSearch))]
    public class UI_ContainerSearchEditor : Editor
    {
        UI_ContainerSearch Target;

        private void OnEnable()
        {
            Target = target as UI_ContainerSearch;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Template"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("StartEnabled"));
            if (Target.StartEnabled) EditorGUILayout.HelpBox("This class depends on the order of execution of unity [Script Execution Order]", MessageType.Info);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Time"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Delay"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Multiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnSearchSlot"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}