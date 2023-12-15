using UnityEngine;
using UnityEditor;

namespace InventoryEngine
{

    public class UI_Debugger : MonoBehaviour
    {
        [SerializeField] UI_Container Container;

        public Vector2Int Pos = Vector2Int.zero;
        public Vector2Int[] Size;
        public Sprite[] Icon;
        public int[] Id;
        [SerializeField] System.Collections.Generic.List<string[]> TagList = new System.Collections.Generic.List<string[]>();

        private void Start()
        {
            TagList.Add(new string[2] { "Weapon", "Rifle" });
            TagList.Add(new string[2] { "Consumable", "Food" });
            TagList.Add(new string[2] { "Head", "Mask" });
            TagList.Add(new string[2] { "Weapon", "Rifle" });
        }

        public void Create()
        {
            int Length = Id.Length;
            int Index = Random.Range(0, Length);

            Container.AddItem(new UI_Item(
                Id[Index],
                Icon[Index], 
                Size[Index],
                Random.Range(1, 50),
                TagList[Index]
                ), Pos);
        }

        public void Delete()
        {
            Container.RemoveItem(Pos);
        }

        public void Save()
        {
            UI_Manager.Save("inventory", Container.GetSerializableData());
        }

        public void Load()
        {
            UI_ItemData[,] Data = UI_Manager.Load("inventory") as UI_ItemData[,];
            UI_Item[,] IData = new UI_Item[Data.GetLength(0), Data.GetLength(1)];

            //IData = System.Array.ConvertAll<UI_Item, UI_ItemData>(IData, e => (UI_ItemData)e);
            for (int x = 0; x < Data.GetLength(0); x++)
            {
                for (int y = 0; y < Data.GetLength(1); y++)
                {
                    IData[x, y] = Data[x, y];
                }
            }
            Container.SetData(IData);
        }
    }

    [CustomEditor(typeof(UI_Debugger))]
    public class UI_DebugerEditor : Editor
    {
        UI_Debugger Target;
        private void OnEnable()
        {
            Target = (UI_Debugger)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("AddItem"))
            {
                Target.Create();
            }
            if (GUILayout.Button("RemoveItem"))
            {
                Target.Delete();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
                Target.Save();
            if (GUILayout.Button("Load"))
                Target.Load();
            EditorGUILayout.EndHorizontal();
        }
    }
}