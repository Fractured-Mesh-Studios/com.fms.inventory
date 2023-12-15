using UnityEngine;


namespace InventoryEngine
{
    [CreateAssetMenu(fileName = "UI_Database", menuName = "InventoryEngine/Database", order = 10)]
    public class UI_DataBase : ScriptableObject
    {
        [Header("Item")]
        [SerializeField] Sprite[] Sprites = null;

        [SerializeField] UI_Item[] Items;

        private void OnEnable()
        {
            
        }

       
    }

}

