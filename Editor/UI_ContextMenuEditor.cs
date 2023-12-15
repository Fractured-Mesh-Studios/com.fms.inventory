using System.Reflection;
using UnityEngine;
using UnityEditor;
using InventoryEngine;

namespace InventoryEditor
{
    [CustomEditor(typeof(UI_ContextMenu))]
    public class UI_ContextMenuEditor : Editor
    {
        UI_ContextMenu Target;
        bool MethodFoldOut = false, InspectClass = false;
        Behaviour Class;


        private void OnEnable()
        {
            Target = target as UI_ContextMenu;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Space(10);
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUILayout.Label("Context Menu");
            GUI.skin.label.fontStyle = FontStyle.Normal;

            GUIContent Content = new GUIContent("TimeToLive", "Time to destroy the menu if mouse leave it or if one button is clicked");

            //Target.Fit = (UI_ContextMenu.UI_ContextMenuFit)EditorGUILayout.EnumPopup("Fit", Target.Fit);
            Target.Button = (GameObject)EditorGUILayout.ObjectField("Button", Target.Button, typeof(GameObject), true);
            Target.TimeToLive = EditorGUILayout.FloatField(Content, Target.TimeToLive);
            Target.UseFlags = EditorGUILayout.Toggle("UseFlags", Target.UseFlags);
            GUI.enabled = Target.UseFlags;
            Target.Flags = (BindingFlags)EditorGUILayout.MaskField(new GUIContent("Flags"), (int)Target.Flags, GetBindingFlags());
            GUI.enabled = true;

            GUILayout.Space(15);
            Content = new GUIContent("Inspect Class", "[Enable/Disable] the class viewer (preview only)");
            if(GUILayout.Button(Content)) InspectClass = !InspectClass;
            if (InspectClass)
            {
                EditorGUILayout.BeginVertical("Box");
                Content = new GUIContent("Class", "Drop here a script or gameobject to preview the available methods.");
                Class = (Behaviour)EditorGUILayout.ObjectField(Content, Class, typeof(Behaviour), true);
                if (Class)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    MethodFoldOut = EditorGUILayout.Foldout(MethodFoldOut, "Available Methods", true);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    if (MethodFoldOut)
                    {
                        MethodInfo[] Methods = Class.GetType().GetMethods();
                        EditorGUILayout.BeginVertical();
                        for (int i = 0; i < Methods.Length; i++)
                        {
                            if (Methods[i] != null)
                            {
                                ParameterInfo[] MethodParameters = Methods[i].GetParameters();
                                int ParameterLength = MethodParameters.Length;
                                if (ParameterLength <= 0 && Methods[i].ReturnType == typeof(void))
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.Label(Methods[i].Name + "()");
                                    GUILayout.FlexibleSpace();
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.HelpBox("List is only a preview.", MessageType.Info);
                }
                EditorGUILayout.EndVertical();
            }
            
        }

        private string[] GetBindingFlags()
        {
            string[] Flags = new string[19];
            Flags[0] = BindingFlags.CreateInstance.ToString();
            Flags[1] = BindingFlags.DeclaredOnly.ToString();
            Flags[2] = BindingFlags.Default.ToString();
            Flags[3] = BindingFlags.ExactBinding.ToString();
            Flags[4] = BindingFlags.FlattenHierarchy.ToString();
            Flags[5] = BindingFlags.GetField.ToString();
            Flags[6] = BindingFlags.GetProperty.ToString();
            Flags[7] = BindingFlags.IgnoreCase.ToString();
            Flags[8] = BindingFlags.Instance.ToString();
            Flags[9] = BindingFlags.InvokeMethod.ToString();
            Flags[10] = BindingFlags.NonPublic.ToString();
            Flags[11] = BindingFlags.OptionalParamBinding.ToString();
            Flags[12] = BindingFlags.Public.ToString();
            Flags[13] = BindingFlags.PutDispProperty.ToString();
            Flags[14] = BindingFlags.PutRefDispProperty.ToString();
            Flags[15] = BindingFlags.SetField.ToString();
            Flags[16] = BindingFlags.SetProperty.ToString();
            Flags[17] = BindingFlags.Static.ToString();
            Flags[18] = BindingFlags.SuppressChangeType.ToString();
            return Flags;
        }
    }
}