using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using InventoryEngine;
using System.Reflection;

namespace InventoryEditor
{
    [CustomEditor(typeof(UI_Data))]
    public class UI_DataEditor : Editor
    {
        protected UI_Data Target;
        protected SerializedProperty Items;
        protected bool ItemsBool;
        protected Color BackgroundColor;
        Object Obj;

        private void OnEnable()
        {
            Target = target as UI_Data;
            Items = serializedObject.FindProperty("Items");
        }
        
        public override void OnInspectorGUI()
        {
            GUILayout.Space(15);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            
            GUIContent Content = new GUIContent("Items [" + Items.arraySize + "]", "This variable needs to contain all the items for loading or saving to disk. (basic IO)");
            ItemsBool = EditorGUILayout.Foldout(ItemsBool, Content, true);
            if (GUILayout.Button("Add")) {
                Items.InsertArrayElementAtIndex(Items.arraySize);
            }
            if (GUILayout.Button("Clear")) Items.ClearArray();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            if (ItemsBool)
            {
                for(int i = 0; i < Items.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical("TextArea");
                    var Element = Items.GetArrayElementAtIndex(i);
                    GUILayout.Space(3);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    Element.isExpanded = EditorGUILayout.Foldout(Element.isExpanded, "Element " + i, true);
                    Obj = EditorGUILayout.ObjectField(Obj, typeof(Object), true);
                    if (
                        GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), 
                        GUILayout.MaxWidth(30), 
                        GUILayout.MaxHeight(15))
                        ) { Items.DeleteArrayElementAtIndex(i); break; }
                    if (Obj)
                    {
                        UI_Item Item = SearchFields(Obj); //Set Item From Object Draged.
                        if(Item != UI_Item.Empty)
                        {
                            Element.isExpanded = true;
                            Element.FindPropertyRelative("Id").intValue = Item.Id;
                            Element.FindPropertyRelative("Size").vector2IntValue = Item.Size;
                            Element.FindPropertyRelative("Icon").objectReferenceValue = Item.Icon;
                            Element.FindPropertyRelative("Stack").intValue = Item.Stack;

                            SerializedProperty TagArray = Element.FindPropertyRelative("Tags");
                            TagArray.arraySize = Item.Tags.Length;
                            for (int z = 0; z < Item.Tags.Length; z++)
                            {
                                var Tag = TagArray.GetArrayElementAtIndex(z);
                                Tag.stringValue = Item.Tags[z];
                            }

                            UI_Debug.Log("Item Set At Index="+i+"");
                        }
                        Obj = null;
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(3);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    EditorGUILayout.BeginVertical();
                    if (Element.isExpanded)
                    {
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("Id"));
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("Size"));
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("Icon"));
                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("Stack"));
                        DrawDefaultArray(Element.FindPropertyRelative("Tags"));
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.HelpBox("This Class is For Saving And Loading Data", MessageType.Info);
        }

        protected void DrawDefaultArray(SerializedProperty Prop)
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            Prop.isExpanded = EditorGUILayout.Foldout(Prop.isExpanded, Prop.name, true);
            EditorGUILayout.EndHorizontal();
            //EditorGUILayout.PropertyField( Prop.FindPropertyRelative("size"));
            if (Prop.isExpanded)
            {
                Prop.arraySize = EditorGUILayout.IntField("Size", Prop.arraySize);
                for (int i = 0; i < Prop.arraySize; i++)
                {
                    var Element = Prop.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(Element);
                }
            }
            EditorGUILayout.EndVertical();
        }

        protected UI_Item SearchFields(Object Obj)
        {
            UI_Item NewItem = UI_Item.Empty;
            GameObject GameObject = Obj as GameObject;
            Behaviour[] behaviours = GameObject.GetComponentsInChildren<Behaviour>();

            //Search for the same naming fields
            for(int i = 0; i < behaviours.Length; i++)
            {
                System.Type T = behaviours[i].GetType();
                FieldInfo[] Fields = T.GetFields();
                for (int k = 0; k < Fields.Length; k++)
                {
                    switch (Fields[k].Name.ToLower())
                    {
                        case "id":
                            NewItem.Id = (int)Fields[k].GetValue(behaviours[i]);
                            break;
                        case "size":
                            NewItem.Size = (Vector2Int)Fields[k].GetValue(behaviours[i]);
                            break;

                        case "icon":
                            NewItem.Icon = (Sprite)Fields[k].GetValue(behaviours[i]);
                            break;

                        case "stack":
                            NewItem.Stack = (int)Fields[k].GetValue(behaviours[i]);
                            break;
                        case "tags":
                            NewItem.Tags = (string[])Fields[k].GetValue(behaviours[i]);
                            break;
                    }
                }
            }
            return NewItem;
        }
    }

}

