using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using InventoryEngine;

namespace InventoryEditor
{
    [CustomEditor(typeof(UI_Slot))]
    public class UI_SlotEditor : Editor
    {
        protected UI_Slot Target;
        protected GUIContent Content;

        private void OnEnable()
        {
            Target = target as UI_Slot;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Space(10);
            OnInspectorSlot();

            //Color
            EditorGUILayout.PropertyField(serializedObject.FindProperty("NormalColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("HighLightedColor"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("BackgroundComponent"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IconComponent"));

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnInspectorSlot()
        {
            GUILayout.Label("Slot", EditorStyles.boldLabel);

            //Buttons Menu
            EditorGUILayout.BeginHorizontal("Box");
            GUI.color = new Color(0.65f, 0.65f, 0.65f, 1.0f);
            GUILayout.FlexibleSpace();
            string RaycastIcon = (!Target.raycastTarget) ? "GraphicRaycaster Icon" : "d_GraphicRaycaster Icon";
            Content = EditorGUIUtility.IconContent(RaycastIcon);
            Content.tooltip = "[Enable/Disable] the slot raycast target";
            Content.text = "Raycast";
            if (GUILayout.Button(Content, GUILayout.MaxWidth(75), GUILayout.MinHeight(30))){
                Target.raycastTarget = !Target.raycastTarget;
            }
            string LockIcon = (!Target.Lock) ? "d_AssemblyLock" : "AssemblyLock";
            Content = EditorGUIUtility.IconContent(LockIcon);
            Content.tooltip = "[Enable/Disable] Slot lock from drag and drop events.";
            Content.text = "Lock";
            if (GUILayout.Button(Content, GUILayout.MaxWidth(65), GUILayout.MinHeight(30)))
            {
                Target.Lock = !Target.Lock;
            }
            string EmptyIcon = (Target.CanEmpty) ? "LookDevSplit" : "d_LookDevSplit";
            Content = EditorGUIUtility.IconContent(EmptyIcon);
            Content.tooltip = "[Enable/Disable] if this value is true the item can be dropped on empty (void space) and be destroyed/removed from the current containing slot.";
            Content.text = "Empty";
            if (GUILayout.Button(Content, GUILayout.MaxWidth(65), GUILayout.MinHeight(30)))
            {
                Target.CanEmpty = !Target.CanEmpty;
            }
            GUILayout.FlexibleSpace();
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            //Position
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = (Target.Position.x < 0 || Target.Position.y < 0) ? Color.red : Color.white;
            Content = new GUIContent("Position", "This value determines the position of the slot in the inventory grid. (read only)");
            Target.Position = EditorGUILayout.Vector2IntField(Content,Target.Position);
            GUI.backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.76f, 0.76f, 0.76f, 1.0f) : Color.white;
            EditorGUILayout.EndHorizontal();

            //Drag Button
            Content = new GUIContent("Button", "Mouse button valid for the drag operation to work.");
            Target.Button = (PointerEventData.InputButton)EditorGUILayout.EnumPopup(Content, Target.Button);

            RectTransform Icon = Target.transform.Find("Icon") as RectTransform;
            if(Icon.anchorMin == Icon.anchorMax)
            {
                EditorGUILayout.BeginVertical("Box");
                GUILayout.Label("Item Rotation Supported", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndVertical();
            }

            if(Target.GetComponent<UI_SlotEvent>())
            {
                EditorGUILayout.BeginVertical("Box");
                GUILayout.Label("Slot Event Supported", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndVertical();
            }
        }
    }
}
