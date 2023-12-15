using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InventoryEngine
{
    [System.Serializable]
    public class UI_Managersettings
    {
        [Header("Properties")]
        public bool UsePersistentpath = true;
        public string path = string.Empty;
        public bool Debug;
        public float Size = 2;
        public Color ValidColor = Color.green;
        public Color InValidColor = Color.red;
    }

    [RequireComponent(typeof(UI_Data))]
    public class UI_Manager : MonoBehaviour
    {
        //Static fields
        public static UI_Manager Instance { get { return instance; } }
        private static UI_Manager instance;

        public static UI_Managersettings settings = new UI_Managersettings();


        [Header("drag&Drop")]
        [Tooltip("drag object prefab")]
        [SerializeField] public GameObject drag;

        [Tooltip("hover/HighLight object prefab")]
        [SerializeField] public GameObject hover;
        [SerializeField] public UI_EDragMode mode = UI_EDragMode.RemoveOnDrag;
        [SerializeField] public bool persistence = true;

        [Header("Database")]
        [SerializeField] public UI_Data Data;

        protected static string Log = "UI_Manager: ";

        protected virtual void Awake()
        {
            if (persistence)
            {
                if (instance == null)
                {
                    instance = this;
                    DontDestroyOnLoad(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            
            if (!Data) Data = GetComponent<UI_Data>();
            if (!Data) { UI_Debug.LogError(Log + "Data is not set on the manager component."); }

            string type = typeof(UI_Manager).ToString();
            string path = Application.persistentDataPath + "/" + type + ".cfg";
            if (Directory.Exists(Application.persistentDataPath))
            {
                string Data = File.ReadAllText(path);
                settings = JsonUtility.FromJson<UI_Managersettings>(Data);
            }
            else
            {
                UI_Debug.Log(Log + "Persistent path not found, Default settings loaded.");
            }
        }

        /// <summary>
        /// Assigns the selected key to all objects found in the scene
        /// </summary>
        /// <param name="Key"> keys that is considered valid for the drag system </param>
        public void SetDragKey(PointerEventData.InputButton Key)
        {
            UI_Slot[] Slots = FindObjectsOfType<UI_Slot>();
            for (int i = 0; i < Slots.Length; i++)
            {
                Slots[i].button = Key;
            }
        }

        /// <summary>
        /// Assigns the selected key to all objects found in the scene filtered by tag
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Tag"></param>
        public void SetDragKey(PointerEventData.InputButton Key, string Tag)
        {
            UI_Slot[] Slots = FindObjectsOfType<UI_Slot>();
            List<UI_Slot> FilterSlots = new List<UI_Slot>();

            for (int i = 0; i < Slots.Length; i++)
            {
                if (Slots[i] && Slots[i].tag == Tag)
                {
                    FilterSlots.Add(Slots[i]);
                }
            }

            for (int i = 0; i < FilterSlots.Count; i++)
            {
                if (FilterSlots[i])
                    FilterSlots[i].button = Key;
            }
        }

        /// <summary>
        /// Save data binarized on selected directory.
        /// </summary>
        /// <param name="fileName">The name of the file (without extension)</param>
        /// <param name="data">data to save</param>
        public static void Save(string fileName, object data)
        {
            if (data == null)
            {
                UI_Debug.LogError(Log + "Data to save is null.");
                return;
            }

            fileName = string.IsNullOrEmpty(fileName) ? "GenericSave" : fileName;
            BinaryFormatter Formatter = new BinaryFormatter();

            string path = GeneratePath(fileName);
            FileStream Stream = new FileStream(path, FileMode.Create);
            Formatter.Serialize(Stream, data);
            Stream.Close();
            UI_Debug.Log(Log + "path: [" + Application.persistentDataPath + "]");
        }

        /// <summary>
        /// Load previusly saved data from a file 
        /// </summary>
        /// <param name="fileName">The name of the file to load (without extension)</param>
        /// <returns>The data that has been loaded</returns>
        public static object Load(string fileName)
        {
            string path = GeneratePath(fileName);
            if (File.Exists(path))
            {
                BinaryFormatter Formatter = new BinaryFormatter();
                FileStream Stream = new FileStream(path, FileMode.Open);
                object Data = Formatter.Deserialize(Stream);
                Stream.Close();
                return Data;
            }
            else
            {
                UI_Debug.LogError(Log + "File to load has not been found.");
                UI_Debug.LogError(Log + path);
                return null;
            }
        }

        /// <summary>
        /// Internal use only. generate a path from a filename and its corresponding system and aplication path
        /// </summary>
        /// <param name="name">name of the file</param>
        /// <returns>complete path with the name,extension and path</returns>
        private static string GeneratePath(string name)
        {
            string path = string.Empty;
            string[] paths = new string[2];
            paths[0] = Application.persistentDataPath;
            paths[1] = string.IsNullOrWhiteSpace(settings.path) ?
                Application.dataPath : settings.path;
            int Index = settings.UsePersistentpath ? 0 : 1;
            
            path = paths[Index] + "/" + name + ".dat";
            return path;
        }

    }
}