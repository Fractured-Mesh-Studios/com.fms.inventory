using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryEngine
{
    [System.Serializable]
    public struct UI_Item
    {
        public static UI_Item Empty = new UI_Item(int.MinValue, null, Vector2Int.one, 0, null);

        /// <summary>
        /// Default item contructor
        /// </summary>
        /// <param name="Id">Identification value of the item</param>
        /// <param name="Icon">Sprite image icon of the item</param>
        /// <param name="Size">The size that the item occupies in the width and height in the container.</param>
        /// <param name="Stack">The value that represents how many copies of the item you have in the same slot</param>
        /// <param name="Tags">Filter tags for the item in an array</param>
        public UI_Item(int Id, Sprite Icon, Vector2Int Size, int Stack = 1, string[] Tags = null)
        {
            this.Id = Id;
            this.Icon = Icon;
            this.Size = Size;
            this.Stack = Stack;
            this.Tags = Tags;
            this.Data = null;
        }

        /// <summary>
        /// The constructor used to initialize the item from a class of item data (serializable)
        /// </summary>
        /// <param name="item"></param>
        public UI_Item(UI_ItemData item)
        {
            Id = item.Id;
            Size = new Vector2Int(item.Size[0], item.Size[1]);
            UI_Data Data = null;
            if (UI_Manager.Instance)
                Data = UI_Manager.Instance.Data;
            else
            {
                UI_Debug.LogError("UI_Item: The item could not be created from the serializable data.");
                UI_Debug.LogError("UI_Item: please make sure you have the database functional without any errors.");
            }

            Icon = Data.GetSprite(item.Icon);
            Stack = item.Stack;
            Tags = item.Tags;
            this.Data = item.Data;
        }

        public int Id;
        public Vector2Int Size;
        public Sprite Icon;
        public int Stack;
        public string[] Tags;
        public object Data;

        /// <summary>
        /// The first value in the tag array. (if the array is created)
        /// </summary>
        public string Tag {
            set {
                if (Tags != null && Tags.Length > 0)
                    Tags[0] = value;
                else
                    Tags = new string[1] { value };
            }
            get {
                return (Tags != null && Tags.Length > 0)
                    ? Tags[0] : string.Empty;
            }
        }

        public static bool operator ==(UI_Item a, UI_Item b)
        {
            bool Success = true;
            Success &= a.Id == b.Id;
            Success &= a.Icon == b.Icon;
            Success &= a.Size == b.Size;
            return Success;
        }

        public static bool operator !=(UI_Item a, UI_Item b)
        {
            bool Success = true;
            Success &= a.Id != b.Id;
            Success &= a.Icon != b.Icon;
            Success &= a.Size != b.Size;
            return Success;
        }

        public static UI_Item operator +(UI_Item a, UI_Item b)
        {
            if (a == b) {
                string[] NewTags = null;
                if(a.Tags != null && b.Tags != null)
                {
                    NewTags = new string[a.Tags.Length + b.Tags.Length];
                    a.Tags.CopyTo(NewTags, 0);
                    b.Tags.CopyTo(NewTags, a.Tags.Length);
                    NewTags = NewTags.Distinct().ToArray();
                }
                return new UI_Item(a.Id, a.Icon, a.Size, a.Stack + b.Stack, NewTags);
            }
            else {
                return Empty;
            }
        }

        public static UI_Item operator -(UI_Item a, UI_Item b)
        {
            if (a == b)
            {
                List<string> NewTags = new List<string>();
                if (a.Tags != null && b.Tags != null)
                {
                    for (int i = 0; i < a.Tags.Length; i++)
                    {
                        if (!Array.Exists(b.Tags, (string s) => { return a.Tags[i] == s; }) &&
                            NewTags.Contains(a.Tags[i]))
                        {
                            NewTags.Add(a.Tags[i]);
                        }
                    }
                }
                return new UI_Item(a.Id, a.Icon, a.Size, a.Stack - b.Stack, NewTags.ToArray());
            }
            else
            {
                return Empty;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            string IconName = Icon ? Icon.name : "<null>";
            return "Id =" + Id + " Size =" + Size + " Icon=" + IconName + " Stack=" + Stack;
        }

        public static implicit operator UI_Item(UI_ItemData item)
        {
            return new UI_Item(item);
        }
    }

    [System.Serializable]
    public struct UI_ItemData
    {
        public UI_ItemData(UI_Item item)
        {
            Id = item.Id;
            Size = new int[2];
            Size[0] = item.Size.x;
            Size[1] = item.Size.y;
            Icon = (item.Icon) ? item.Icon.name : string.Empty;
            Stack = item.Stack;
            Tags = item.Tags;
            Data = item.Data;
        }

        public UI_ItemData(int Id, Vector2Int Size, Sprite Icon, int Stack = 1, string[] Tags = null)
        {
            this.Id = Id;
            this.Size = new int[2];
            this.Size[0] = Size.x;
            this.Size[1] = Size.y;
            this.Icon = Icon.texture.name;
            this.Stack = Stack;
            this.Tags = Tags;
            this.Data = null;
        }

        public int Id;
        public int[] Size;
        public string Icon;
        public int Stack;
        public string[] Tags;
        public object Data;

        public static implicit operator UI_ItemData(UI_Item other)
        {
            return new UI_ItemData(other);
        }
    }
}