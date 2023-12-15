using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using InventoryEngine;

namespace InventoryEditor
{
    [CustomEditor(typeof(UI_SlotEvent))]
    public class UI_SlotEventEditor : Editor
    {
        UI_SlotEvent Target;
        bool MouseFoldout, DragFoldout, DropFoldout;
        int MouseCount, DragCount, DropCount;
        GUIStyle FoldOutStyle;

        private void OnEnable()
        {
            Target = target as UI_SlotEvent;
            MouseCount = DragCount = DropCount = 0;
        }

        public override void OnInspectorGUI()
        {
            
            GUILayout.Space(10);
            FoldOutStyle = EditorStyles.foldoutHeader;
            FoldOutStyle.fixedWidth = EditorGUIUtility.currentViewWidth - 20;

            GUILayout.Label(new GUIContent("Keys", "Mouse event key filter"), EditorStyles.boldLabel);
            Target.LeftClickKey = (KeyCode)EditorGUILayout.EnumPopup("LeftClickKey", Target.LeftClickKey);
            Target.RightClickKey = (KeyCode)EditorGUILayout.EnumPopup("RightClickKey", Target.RightClickKey);


            GUILayout.Label(new GUIContent("Events", "All available events by categories."), EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            GUIContent Content = new GUIContent("Click Event ["+MouseCount+"]", "Mouse click event system (depends on selected mouse keys)");
            MouseFoldout = EditorGUILayout.Foldout(MouseFoldout, Content, true, FoldOutStyle);
            if (MouseFoldout)
            {
                GUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnLeftClick"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnRightClick"));
               
            }

            GUILayout.Space(5);
            Content = new GUIContent("Drag Event ["+DragCount+"]", "Drag-Drop event system");
            DragFoldout = EditorGUILayout.Foldout(DragFoldout, Content, true, FoldOutStyle);
            if (DragFoldout)
            {
                GUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnBeginDragEvent"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDragEvent"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnEndDragEvent"));
            }

            GUILayout.Space(5);
            
            Content = new GUIContent("Drop Event ["+DropCount+"]", "Mouse Drag-Drop event system");
            DropFoldout = EditorGUILayout.Foldout(DropFoldout, Content, true, FoldOutStyle);
            if (DropFoldout)
            {
                GUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDropValid"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDropFalied"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDropVoid"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDropSelf"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnAnyDrop"));
                
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            MouseCount =
                   Target.OnLeftClick.GetPersistentEventCount() +
                   Target.OnRightClick.GetPersistentEventCount();

            DragCount =
                    Target.OnBeginDragEvent.GetPersistentEventCount() +
                    Target.OnDragEvent.GetPersistentEventCount() +
                    Target.OnEndDragEvent.GetPersistentEventCount();

            DropCount =
                    Target.OnDropValid.GetPersistentEventCount() +
                    Target.OnDropFalied.GetPersistentEventCount() +
                    Target.OnDropVoid.GetPersistentEventCount() +
                    Target.OnDropSelf.GetPersistentEventCount() +
                    Target.OnAnyDrop.GetPersistentEventCount();

            serializedObject.ApplyModifiedProperties();
            
        }
    }

}

