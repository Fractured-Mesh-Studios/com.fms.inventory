using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using InventoryEngine;
using System.Diagnostics;
using System.IO;


namespace InventoryEditor
{ 
    public class PropertiesWindow : EditorWindow
    {
        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/InventoryEngine/Properties")]
        static void Init()
        {
            PropertiesWindow window = (PropertiesWindow)GetWindow(typeof(PropertiesWindow));
            window.titleContent = new GUIContent("Inventory Properties");
            window.minSize = new Vector2(350,100);
            window.maxSize = new Vector2(500,200);
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            UI_Manager.Settings.UsePersistentPath = EditorGUILayout.Toggle("Use Persistent Path", UI_Manager.Settings.UsePersistentPath);
            UI_Manager.Settings.Path = EditorGUILayout.TextField("Path", UI_Manager.Settings.Path);

            EditorGUILayout.BeginVertical("TextField");
            UI_Manager.Settings.Debug = EditorGUILayout.Toggle("Debug", UI_Manager.Settings.Debug);
            GUI.enabled = UI_Manager.Settings.Debug;
            UI_Manager.Settings.Size = EditorGUILayout.Slider("Size", UI_Manager.Settings.Size, 0, 10);
            UI_Manager.Settings.ValidColor = EditorGUILayout.ColorField("Valid Color", UI_Manager.Settings.ValidColor);
            UI_Manager.Settings.InValidColor = EditorGUILayout.ColorField("InValid Color", UI_Manager.Settings.InValidColor);
            GUI.enabled = true;
            EditorGUILayout.EndVertical();

            UI_Manager i_Manager = FindObjectOfType<UI_Manager>();
            if (!i_Manager) {
                string SceneName = SceneManager.GetActiveScene().name;
                EditorGUILayout.HelpBox("Manager Not Found In ["+SceneName+"]." , MessageType.Error);
            }
            

            GUILayout.Space(10);
            string ApplicationPath = UI_Manager.Settings.UsePersistentPath ? Application.persistentDataPath : UI_Manager.Settings.Path;
            char character = ' ', newCharacter = ' ';
            for (int i = 0; i < ApplicationPath.Length; i++)
            {
                character = ApplicationPath[i];
                newCharacter = (character == '/') ? (char)92 : ApplicationPath[i];
                ApplicationPath = ApplicationPath.Replace(character, newCharacter);
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Working Directory"))
            {
                if (Directory.Exists(ApplicationPath))
                {
                    ProcessStartInfo Info = new ProcessStartInfo
                    {
                        Arguments = ApplicationPath,
                        FileName = "explorer.exe"
                    };
                    Process.Start(Info);
                }
                else
                {
                    string DirectoryPath = string.IsNullOrWhiteSpace(ApplicationPath) ? "null" : ApplicationPath;
                    UnityEngine.Debug.Log("<" + DirectoryPath + "> Directory does not exist.");
                }
            }
            if (GUILayout.Button("Reset Directory"))
            {
                UI_Manager.Settings.Path = string.Empty;
                ApplicationPath = string.Empty;
            }
                
            EditorGUILayout.EndHorizontal();

            string DirectoryText = "[" + (UI_Manager.Settings.UsePersistentPath ? "Persistent" : "Dynamic") + "] Directory";
            GUIContent Content = new GUIContent(DirectoryText, "Actual working path (Read Only)");
            EditorGUILayout.TextField(Content, ApplicationPath);

            if (GUILayout.Button("Apply Settings"))
            {
                string Class = typeof(UI_Manager).ToString();
                string Data = JsonUtility.ToJson(UI_Manager.Settings, true);
                File.WriteAllText(Application.persistentDataPath + "/" + Class + ".cfg", Data);
            }
        }
    }
}