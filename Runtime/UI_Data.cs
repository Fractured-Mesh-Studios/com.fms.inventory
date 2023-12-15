using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryEngine
{
    public class UI_Data : MonoBehaviour
    {
        [Header("Database")]
        [SerializeField] UI_Item[] Items;

        protected string Log = "UI_Data: ";

        void Awake()
        {
            if(Items == null || Items.Length <= 0)
            {
                UI_Debug.LogError(Log + "Data items array is null.");
            }
        }


        /// <summary>
        /// You get a sprite of the array contained in the database by the filename
        /// </summary>
        /// <param name="fileName">the file name of the sprite</param>
        /// <returns></returns>
        public Sprite GetSprite(string fileName)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i].Icon && Items[i].Icon.name == fileName)
                    return Items[i].Icon;
            }

            return null;
        }

        /// <summary>
        /// you get a sprite of the array contained in the database by the array index
        /// </summary>
        /// <param name="index">index value of the sprite (0,Length)</param>
        /// <returns></returns>
        public Sprite GetSprite(int index)
        {
            if (index < 0 && index > Items.Length)
                return null;
            return Items[index].Icon;
        }
    }

}

