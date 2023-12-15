using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using InventoryEngine;
using UnityEditorInternal;

namespace InventoryEditor
{
    [CustomEditor(typeof(UI_SlotEx))]
    [CanEditMultipleObjects]
    public class UI_SlotExEditor : Editor
    {
        UI_SlotEx Target;
        GUIContent Content;
        ReorderableList TagList;

        private void OnEnable()
        {
            Target = target as UI_SlotEx;
            TagList = new ReorderableList(serializedObject,serializedObject.FindProperty("FilterTags"), true, true, true, true);

            TagList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                var element = TagList.serializedProperty.GetArrayElementAtIndex(index);
                Content = new GUIContent(string.Empty, "Active tag");
                element.stringValue = EditorGUI.TextField(rect, Content, element.stringValue);
            };

            TagList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Filter Tags ["+TagList.count+"]");
            };
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("Slot", EditorStyles.boldLabel);
            Target.raycastTarget = EditorGUILayout.Toggle("Raycast Target", Target.raycastTarget);
            Target.Lock = EditorGUILayout.Toggle("Lock", Target.Lock);
            Target.Position = EditorGUILayout.Vector2IntField("Position", Target.Position);
            Content = new GUIContent("Can Empty", "if this value is true the item can be dropped on empty (void space) and be destroyed/removed from the current containing slot.");
            Target.CanEmpty = EditorGUILayout.Toggle(Content, Target.CanEmpty);
            Target.Button = (PointerEventData.InputButton)EditorGUILayout.EnumPopup("Button", Target.Button);

            //Color
            EditorGUILayout.PropertyField(serializedObject.FindProperty("NormalColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("HighLightedColor"));
            //Components
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BackgroundComponent"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IconComponent"));
            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(5);
            GUILayout.Label("Extended", EditorStyles.boldLabel);
            Target.Id = EditorGUILayout.IntField("Id", Target.Id);
            Target.MinStack = EditorGUILayout.IntField("MinStack", Target.MinStack);

            //Filter
            Content = new GUIContent("Filter", "[Enable/Disable] the item tags filter");
            Target.Filter = EditorGUILayout.BeginToggleGroup(Content, Target.Filter);
                Content = new GUIContent("FilterValidMin", "Minimum value in matches to be accepted by the filter system. (based on filter tag length)");
                Target.FilterValidMin = EditorGUILayout.IntSlider(Content,Target.FilterValidMin, 0, Target.FilterTags.Length);
                serializedObject.Update();
                TagList.DoLayoutList();
            EditorGUILayout.EndToggleGroup();
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            //EndFilter
        }

    }
}