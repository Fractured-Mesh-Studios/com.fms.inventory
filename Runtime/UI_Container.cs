using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryEngine
{
    [System.Serializable]
    public class UI_Container : MonoBehaviour
    {
        [Header("Inventory")]
        [Tooltip("Allows the container to adjust to the size of the slots in the child objects created by the grid.")]
        public bool Fit;
        public Vector2Int Size;
        public GameObject Slot;
        public Vector2 CellSize { get { return GridComponent.cellSize; } }
        public int Length { get { return Grid.Length; } }
        public RectTransform RectTransform { internal set; get; }

        protected GridLayoutGroup GridComponent;
        protected UI_ContainerEvent EventComponent;
        protected string Log = "UI_Container: ";
        protected UI_Slot[,] Grid;
        //protected Vector2Int ConstraintSize;
        
        /// <summary>
        /// Initialize and build the container system
        /// </summary>
        protected virtual void Awake()
        {
            GridComponent = GetComponentInChildren<GridLayoutGroup>();
            if (!GridComponent)
            {
                UI_Debug.LogError(Log + "Grid layout group component is null.");
            }
            else
            {
                int Length = -1;
                switch (GridComponent.constraint)
                {
                    case GridLayoutGroup.Constraint.FixedColumnCount:
                        Length = Size.x * Size.y;
                        Size.x = GridComponent.constraintCount;
                        Size.y = Length / GridComponent.constraintCount;
                        break;
                    case GridLayoutGroup.Constraint.FixedRowCount:
                        Length = Size.x * Size.y;
                        Size.x = Length / GridComponent.constraintCount;
                        Size.y = GridComponent.constraintCount;
                        break;
                    default: break;
                }

                Grid = new UI_Slot[Size.x, Size.y];
                for (int x = 0; x < Size.x; x++)
                {
                    for (int y = 0; y < Size.y; y++)
                    {
                        Grid[x, y] = Instantiate(Slot, GridComponent.transform).GetComponent<UI_Slot>();
                        if (Grid[x, y])
                        {
                            Grid[x, y].name = "Slot [" + x + "-" + y + "]";
                            Grid[x, y].Position = new Vector2Int(x, y);
                            Grid[x, y].RectTransform.sizeDelta = GridComponent.cellSize;
                        }
                    }
                }
                GridComponent.startAxis = GridLayoutGroup.Axis.Vertical;
            }

            switch (GridComponent.constraint)
            {
                case GridLayoutGroup.Constraint.FixedColumnCount:
                    Size.x = GridComponent.constraintCount;
                    Size.y = Grid.GetLength(1);
                    break;
                case GridLayoutGroup.Constraint.FixedRowCount:
                    Size.x = Grid.GetLength(0);
                    Size.y = GridComponent.constraintCount;
                    break;
                default: break;
            }

            RectTransform = transform as RectTransform;
            if (Fit)
            {
                Vector2 PaddingOffset = new Vector2(
                GridComponent.padding.left + GridComponent.padding.right,
                GridComponent.padding.top + GridComponent.padding.bottom
                );
                
                Vector2 Spacing = new Vector2(
                    GridComponent.spacing.x * Size.x, 
                    GridComponent.spacing.y * Size.y
                    );

                RectTransform.sizeDelta = (GridComponent.cellSize * Size) + PaddingOffset + Spacing;
                RectTransform GridTransform = GridComponent.transform as RectTransform;
                GridTransform.anchorMin = Vector2.zero;
                GridTransform.anchorMax = Vector2.one;
                GridTransform.pivot = new Vector2(0.5f, 0.5f);

                GridTransform.anchoredPosition = Vector2.zero;
                GridTransform.sizeDelta = Vector2.zero;
            }
            else
            {
                RectTransform GridTransform = GridComponent.transform as RectTransform;
                Vector2 PaddingOffset = new Vector2(
                    GridComponent.padding.left + GridComponent.padding.right,
                    GridComponent.padding.top + GridComponent.padding.bottom
                    );

                Vector2 Spacing = new Vector2(
                    GridComponent.spacing.x * Size.x,
                    GridComponent.spacing.y * Size.y
                    );

                GridTransform.sizeDelta = GridComponent.cellSize * Size + PaddingOffset + Spacing;
                GridTransform.anchorMin = new Vector2(0.5f,0.5f);
                GridTransform.anchorMax = new Vector2(0.5f,0.5f);
                GridTransform.pivot = new Vector2(0.5f, 0.5f);
            }

            Image BackgorundComponent = GetComponent<Image>();
            if (BackgorundComponent)
            {
                BackgorundComponent.raycastTarget = false;
            }

            EventComponent = GetComponent<UI_ContainerEvent>();
            UI_Debug.Log(Log + "Events [" + (EventComponent ? "Enabled" : "Disabled") + "]");

        }

        //Add&Remove
        /// <summary>
        /// Add an item to the current container class,
        /// at the first valid position in the grid.
        /// </summary>
        /// <param name="Item">Item to be added</param>
        /// <returns>true if the item has been added</returns>
        public virtual bool AddItem(UI_Item Item)
        {
            for (int x = 0; x < Grid.GetLength(0); x++)
            {
                for (int y = 0; y < Grid.GetLength(1); y++)
                {
                    if (AddItem(Item, new Vector2Int(x, y)))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        ///  Add an item to the current container class at desired
        ///  position (only if the position is valid)
        /// </summary>
        /// <param name="Item">Item to be added</param>
        /// <param name="Position">Position to add the item</param>
        /// <returns>true if the item has a valid position and added successfully</returns>
        public virtual bool AddItem(UI_Item Item, Vector2Int Position)
        {
            if (IsValid(Position, Item.Size))
            {
                UI_Debug.Log(Log + "Valid Inventory Position");

                for (int x = Position.x; x < Position.x + Item.Size.x; x++)
                {
                    for (int y = Position.y; y < Position.y + Item.Size.y; y++)
                    {
                        Grid[x, y].SetActive(false);
                        Grid[x, y].enabled = false;
                    }
                }

                Grid[Position.x, Position.y].enabled = true;
                Grid[Position.x, Position.y].Item = Item;
                Grid[Position.x, Position.y].SetActive(true);

                if (EventComponent)
                    EventComponent.OnAddItem.Invoke(Item);

                return true;
            }
            else
            {
                UI_Debug.Log(Log + "Invalid Inventory Position");
                return false;
            }
        }

        /// <summary>
        /// Remove an item in the container by its id.
        /// </summary>
        /// <param name="Id">Id of the item to remove</param>
        public virtual void RemoveItem(int Id)
        {
            Vector2Int Position = new Vector2Int(-1, -1);
            for (int x = 0; x < Grid.GetLength(0); x++)
            {
                for (int y = 0; y < Grid.GetLength(1); y++)
                {
                    if (Grid[x, y].Item.Id == Id)
                    {
                        Position = new Vector2Int(x, y);
                    }
                }
            }

            RemoveItem(Position);
        }

        /// <summary>
        /// Remove an item in the container at desired position
        /// </summary>
        /// <param name="Position">Position of the item to remove</param>
        public virtual void RemoveItem(Vector2Int Position)
        {
            Vector2Int Size = Grid[Position.x, Position.y].Item.Size;
            RectTransform Transform = Grid[Position.x, Position.y].transform as RectTransform;

            for (int x = Position.x; x < Position.x + Size.x; x++)
            {
                for (int y = Position.y; y < Position.y + Size.y; y++)
                {
                    Grid[x, y].enabled = true;
                    Grid[x, y].SetActive(true);
                }
            }

            if (EventComponent)
                EventComponent.OnRemoveItem.Invoke(Grid[Position.x,Position.y].Item);

            Grid[Position.x, Position.y].Item = UI_Item.Empty;
            Transform.sizeDelta = GridComponent.cellSize;
        }
        
        /// <summary>
        /// Remove all items in the current container. (Beware of this function)
        /// </summary>
        /// <returns>All items removed on an array</returns>
        public virtual UI_Item[] RemoveItems()
        {
            List<UI_Item> List = new List<UI_Item>();
            for (int x = 0; x < Grid.GetLength(0); x++)
            {
                for (int y = 0; y < Grid.GetLength(1); y++)
                {
                    if(Grid[x,y].Item != UI_Item.Empty)
                    {
                        List.Add(Grid[x, y].Item);
                        RemoveItem(new Vector2Int(x, y));
                    }
                }
            }
            return List.ToArray();
        }

        /// <summary>
        /// Remove items from position to position with given size
        /// </summary>
        /// <param name="Position">Start position</param>
        /// <param name="Size">Start position + size</param>
        public virtual void RemoveItems(Vector2Int Position, Vector2Int Size)
        {
            bool xV, yV;
            for (int x = Position.x; x < Position.x + Size.x; x++)
            {
                for (int y = Position.y; y < Position.y + Size.y; y++)
                {
                    xV = x >= 0 && x < Grid.GetLength(0);
                    yV = y >= 0 && y < Grid.GetLength(1);
                    if (xV && yV && Grid[x, y].Item != UI_Item.Empty)
                    {
                        RemoveItem(new Vector2Int(x, y));
                    }
                }
            }
            //
        }

        /// <summary>
        /// Get desired slot by the position in the grid of him.
        /// </summary>
        /// <param name="x">x index of the grid</param>
        /// <param name="y">y index if the grid</param>
        /// <returns>Selected slot</returns>
        public UI_Slot GetSlot(int x, int y)
        {
            return Grid[x, y];
        }

        /// <summary>
        /// Get desired slot by the position in the grid of him.
        /// with a vector2int parameter
        /// </summary>
        /// <param name="Position">index of the grid</param>
        /// <returns>Selected slot</returns>
        public UI_Slot GetSlot(Vector2Int Position)
        {
            return Grid[Position.x, Position.y];
        }

        /// <summary>
        /// Get the slot through the position, of this in the grid
        /// </summary>
        /// <param name="Position">Position of slot</param>
        /// <returns>Selected slot</returns>
        public UI_Slot this[Vector2Int Position]
        {
            set { Grid[Position.x, Position.y] = value; }
            get { return Grid[Position.x, Position.y]; }
        }

        /// <summary>
        /// Get the slot through the position, of this in the grid
        /// </summary>
        /// <param name="x">x index in the grid.</param>
        /// <param name="y">y index in the grid.</param>
        /// <returns>Selected Slot</returns>
        public UI_Slot this[int x, int y]
        {
            set { Grid[x, y] = value; }
            get { return Grid[x, y]; }
        }

        //Data
        /// <summary>
        /// Set the data from an two-dimensional array of UI_Item
        /// </summary>
        /// <param name="data">data array</param>
        /// <returns>True if loaded</returns>
        public virtual bool SetData(UI_Item[,] data)
        {
            UI_Debug.Log(Log + "SetData Begin");
            if (Grid == null || data == null)
            {
                string ContainerError = (Grid == null ? "Null Container" : "Valid Container");
                string DataError = (data == null ? "Null Data" : "Valid Data");
                UI_Debug.LogError(Log + "Trying to put " + DataError + " in a " + ContainerError);
                UI_Debug.LogError(Log + "Grid Length: " + ((Grid != null) ? Grid.Length.ToString() : "<null>"));
                UI_Debug.LogError(Log + "Data Length: " + ((data != null) ? data.Length.ToString() : "<null>"));
                UI_Debug.Log(Log + "SetData End");
                return false;
            }
            int Count = 0;
            Vector2Int Position = Vector2Int.zero;
            for (int x = 0; x < data.GetLength(0); x++)
            {
                for (int y = 0; y < data.GetLength(1); y++)
                {
                    Position = new Vector2Int(x, y);
                    AddItem(data[x, y], Position);
                    Count += Convert.ToInt32(data[x, y] != UI_Item.Empty);
                }
            }

            UI_Debug.Log(Log + "Data Loaded: " + Count);
            UI_Debug.Log(Log + "SetData End");
            return Count > 0;
        }

        /// <summary>
        /// Get the data in a two-dimensional array of UI_Item
        /// </summary>
        /// <returns>Data array</returns>
        public virtual UI_Item[,] GetData()
        {
            UI_Item[,] ItemData = new UI_Item[Grid.GetLength(0), Grid.GetLength(1)];
            for (int x = 0; x < ItemData.GetLength(0); x++)
            {
                for (int y = 0; y < ItemData.GetLength(1); y++)
                {
                    ItemData[x, y] = Grid[x, y].Item;
                }
            }
            return ItemData;
        }

        /// <summary>
        /// Get the serializable data in a two-dimensional array of UI_ItemSerializable
        /// </summary>
        /// <returns>data array</returns>
        public virtual UI_ItemData[,] GetSerializableData()
        {
            UI_Item[,] Value = GetData();
            UI_ItemData[,] Data = new UI_ItemData[Value.GetLength(0), Value.GetLength(1)];
            for (int x = 0; x < Data.GetLength(0); x++)
            {
                for (int y = 0; y < Data.GetLength(1); y++)
                {
                    Data[x, y] = Value[x, y];
                }
            }
            return Data;
        }

        //Comprobations
        /// <summary>
        /// Check the container if the item can be added or not (only check, not add).
        /// </summary>
        /// <param name="Item">Item to be checked</param>
        /// <returns>true if the item can be added, false otherwise</returns>
        public bool IsValid(UI_Item Item)
        {
            if (Item == UI_Item.Empty) return true;

            bool IsValid = false;
            Vector2Int Position = Vector2Int.zero;
            for (int x = 0; x < Grid.GetLength(0); x++)
            {
                for (int y = 0; y < Grid.GetLength(1); y++)
                {
                    Position = new Vector2Int(x, y);
                    IsValid = this.IsValid(Position, Item.Size);
                    x = IsValid ? Grid.GetLength(0) : x;
                    y = IsValid ? Grid.GetLength(1) : y;
                }
            }
            return IsValid;
        }

        /// <summary>
        /// Check if the container can have space available from the position to the given size.
        /// </summary>
        /// <param name="Position">Start position</param>
        /// <param name="Size">End position</param>
        /// <returns>true if all are empty</returns>
        public bool IsValid(Vector2Int Position, Vector2Int Size)
        {
            bool Success = true;

            for (int x = Position.x; x < Position.x + Size.x; x++)
            {
                for (int y = Position.y; y < Position.y + Size.y; y++)
                {
                    //Grid
                    Success &= (x >= 0 && x < Grid.GetLength(0));
                    Success &= (y >= 0 && y < Grid.GetLength(1));
                    //Slots
                    if (x >= 0 && x < Grid.GetLength(0) && y >= 0 && y < Grid.GetLength(1))
                    {
                        Success &= (Grid[x, y].Item == UI_Item.Empty);
                        Success &= (Grid[x, y].enabled == true);
                    }
                    else
                    {
                        Success &= false;
                    }
                }
            }
            return Success;
        }

        /// <summary>
        /// Check if there is an item in the container equal to the item passed by the parameter
        /// </summary>
        /// <param name="Other">item to be comparer with</param>
        /// <returns>true if the item was found</returns>
        public bool Contains(UI_Item Other)
        {
            bool Success = true;
            for (int x = 0; x < Grid.GetLength(0); x++)
            {
                for (int y = 0; y < Grid.GetLength(1); y++)
                {
                    Success &= (Grid[x, y].Item == Other);
                }
            }
            return Success;
        }

        /// <summary>
        /// Check if there is an item in the container equal to the item passed by the parameter
        /// from the position up to the given size. 
        /// </summary>
        /// <param name="Position">Start position</param>
        /// <param name="Size">End position</param>
        /// <param name="other">Item to be comparer with</param>
        /// <returns>true if the item was found</returns>
        public bool Contains(Vector2Int Position, Vector2Int Size, UI_Item other)
        {
            bool Success = true;
            int WidthValid = 0, HeightValid = 0;
            for (int x = Position.x; x < Position.x + Size.x; x++)
            {
                for (int y = Position.y; y < Position.y + Size.y; y++)
                {
                    WidthValid = Convert.ToInt32(x >= 0 && x < Grid.GetLength(0));
                    HeightValid = Convert.ToInt32(y >= 0 && y < Grid.GetLength(1));
                    Success &= Convert.ToBoolean(WidthValid);
                    Success &= Convert.ToBoolean(HeightValid);

                    if (WidthValid > 0 && HeightValid > 0)
                    {
                        Success &= (Grid[x, y].Item == other);
                        Success &= (Grid[x, y].enabled == true);
                    }
                    else
                    {
                        Success &= false;
                    }
                }
            }

            return Success;
        }

        /// <summary>
        /// Count how many items are in the container from position to given size
        /// </summary>
        /// <param name="Position">Start position</param>
        /// <param name="Size">End position</param>
        /// <returns>How many items was found on the given size</returns>
        public int Contains(Vector2Int Position, Vector2Int Size)
        {
            UI_Item Current = UI_Item.Empty;
            List<UI_Item> Cache = new List<UI_Item>();
            for (int x = Position.x; x < Position.x + Size.x; x++)
            {
                for (int y = Position.y; y < Position.y + Size.y; y++)
                {
                    if (x >= 0 && x < Grid.GetLength(0) && y >= 0 && y < Grid.GetLength(1))
                    {
                        Current = Grid[x, y].Item;
                        if (Current != UI_Item.Empty && !Cache.Contains(Current))
                            Cache.Add(Grid[x, y].Item);
                    }
                    else
                    {
                        return int.MaxValue;
                    }
                }
            }
            return Cache.Count;
        }

        /// <summary>
        /// Check if the given position is inside the selected socket. (if an item is found in the target slot it is also checked)
        /// </summary>
        /// <param name="Position">Position to check</param>
        /// <param name="Slot">Target slot to be checked</param>
        /// <returns>true if the position is contained by the slot</returns>
        public bool Contains(Vector2Int Position, UI_Slot Slot)
        {
            if (Position.x >= 0 && Position.x < Grid.GetLength(0) &&
                Position.y >= 0 && Position.y < Grid.GetLength(1))
            {
                //Recorrer el objeto
                for(int x = Slot.Position.x; x < Slot.Position.x + Slot.Item.Size.x; x++)
                {
                    for(int y = Slot.Position.y; y < Slot.Position.y + Slot.Item.Size.y; y++)
                    {
                        if (x == Position.x && y == Position.y)
                            return true;
                    }
                }
            }

            return false;
        }

        //Get Item
        /// <summary>
        /// Returns the first valid item found in the entire container
        /// </summary>
        /// <returns>First item found</returns>
        public UI_Item GetItem()
        {
            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    if (Grid[x, y].Item != UI_Item.Empty)
                    {
                        return Grid[x, y].Item;
                    }
                }
            }

            return UI_Item.Empty;
        }

        /// <summary>
        /// Returns the first valid item from the passed position on the container
        /// </summary>
        /// <param name="Position">Index position of the item in the container</param>
        /// <returns>Selected item</returns>
        public UI_Item GetItem(Vector2Int Position)
        {
            bool xV, yV;
            xV = Position.x >= 0 && Position.y < Grid.GetLength(0);
            yV = Position.y >= 0 && Position.y < Grid.GetLength(1);
            return (xV && yV) ?
                Grid[Position.x, Position.y].Item : UI_Item.Empty;
        }

        /// <summary>
        /// Returns the first valid item from the passed from position to given size
        /// and return item 
        /// </summary>
        /// <param name="Position">Start position</param>
        /// <param name="Size">End position</param>
        /// <returns>Item selected</returns>
        public UI_Item GetItem(Vector2Int Position, Vector2Int Size)
        {
            bool xV, yV;
            for (int x = Position.x; x < Position.x + Size.x; x++)
            {
                for (int y = Position.y; y < Position.y + Size.y; y++)
                {
                    xV = x >= 0 && x < Grid.GetLength(0);
                    yV = y >= 0 && y < Grid.GetLength(1);
                    if (xV && yV && Grid[x, y].Item != UI_Item.Empty)
                    {
                        return Grid[x, y].Item;
                    }
                }
            }

            return UI_Item.Empty;
        }

        /// <summary>
        /// Returns all valid items from the container through an array
        /// </summary>
        /// <returns>Array of the valid items</returns>
        public UI_Item[] GetItems()
        {
            List<UI_Item> Cache = new List<UI_Item>();
            for (int x = 0; x < Grid.GetLength(0); x++)
            {
                for (int y = 0; y < Grid.GetLength(1); y++)
                {
                    if (Grid[x, y].Item != UI_Item.Empty)
                        Cache.Add(Grid[x, y].Item);
                }
            }
            return Cache.ToArray();
        }

        /// <summary>
        /// Returns all valid items from the container through an array
        /// at given position and desired size.
        /// </summary>
        /// <param name="Position">Start position</param>
        /// <param name="Size">End Position</param>
        /// <returns>Array of valid items</returns>
        public UI_Item[] GetItems(Vector2Int Position, Vector2Int Size)
        {
            List<UI_Item> Cache = new List<UI_Item>();
            bool xV, yV;
            for (int x = Position.x; x < Position.x + Size.x; x++)
            {
                for (int y = Position.y; y < Position.y + Size.y; y++)
                {
                    xV = x >= 0 && x < Grid.GetLength(0);
                    yV = y >= 0 && y < Grid.GetLength(1);
                    if (xV && yV && Grid[x, y].Item != UI_Item.Empty)
                    {
                        Cache.Add(Grid[x, y].Item);
                    }
                }
            }
            return Cache.ToArray();
        }

        /// <summary>
        /// Get the length of the grid. in the selected dimension
        /// 0 = First array column, 1 = Second array column
        /// </summary>
        /// <param name="Dimension">Selected column index</param>
        /// <returns>The length of the grid</returns>
        public int GetLength(int Dimension)
        {
            return Grid.GetLength(Dimension);
        }

        /// <summary>
        /// Hide a selected item from the container without disabling or removing it entirely.
        /// </summary>
        /// <param name="Position">Selected position of the item to hide</param>
        /// <param name="NewVisibility">boolerean value of the state of the item</param>
        public void SetVisibility(Vector2Int Position, bool NewVisibility)
        {
            UI_Item Item = GetSlot(Position).Item;
            if (NewVisibility)
            {
                //true
                for (int x = Position.x; x < Position.x + Item.Size.x; x++)
                {
                    for (int y = Position.y; y < Position.y + Item.Size.y; y++)
                    {
                        Grid[x, y].SetActive(false);
                        Grid[x, y].enabled = false;
                        Grid[x, y].Lock = false;
                    }
                }
                Grid[Position.x, Position.y].enabled = true;
                Grid[Position.x, Position.y].SetActive(true);
                Grid[Position.x, Position.y].Hide = false;
                Grid[Position.x, Position.y].Lock = false;
                
                Vector2 Spacing = new Vector2(GridComponent.spacing.x % Item.Size.x, GridComponent.spacing.y % Item.Size.y);
                Grid[Position.x, Position.y].RectTransform.sizeDelta = (CellSize * Item.Size) + Spacing;
            }
            else
            {
                //false
                for (int x = Position.x; x < Position.x + Item.Size.x; x++)
                {
                    for (int y = Position.y; y < Position.y + Item.Size.y; y++)
                    {
                        Grid[x, y].SetActive(true);
                        Grid[x, y].enabled = true;
                        Grid[x, y].Lock = true;
                    }
                }
                Grid[Position.x, Position.y].Hide = true;
                Grid[Position.x, Position.y].Lock = true;
                Grid[Position.x, Position.y].RectTransform.sizeDelta = CellSize;
            }

        }

        /// <summary>
        /// Get the selected item from the container the actual state of it.
        /// [True = Active and Enabled/False = Hide and disabled]
        /// </summary>
        /// <param name="Position">Current index of the item to check</param>
        /// <returns>State of the selected item</returns>
        public bool GetVisibility(Vector2Int Position)
        {
            bool IsVisible = true;
            UI_Item Item = GetSlot(Position).Item;
            for (int x = Position.x + 1; x < Position.x + Item.Size.x; x++)
            {
                for (int y = Position.y + 1; y < Position.y + Item.Size.y; y++)
                {
                    IsVisible &= !Grid[x, y].enabled && !Grid[x, y].GetActive();
                }
            }

            IsVisible &= Grid[Position.x, Position.y].GetActive();
            IsVisible &= Grid[Position.x, Position.y].enabled;
            return IsVisible;
        }

        /// <summary>
        /// Get the current container grid layout group. (dynamic grid is casteable)
        /// </summary>
        /// <returns>Grid layout group base class</returns>
        public GridLayoutGroup GetGrid()
        {
            return GridComponent;
        }

        //Find Item
        /// <summary>
        /// Returns the position of the first item that has the same properties-characteristics as the item given by parameter.
        /// </summary>
        /// <param name="Item">Item to be comparer with</param>
        /// <returns>The item position on the grid</returns>
        public Vector2Int FindPosition(UI_Item Item)
        {
            for (int x = 0; x < Grid.GetLength(0); x++)
            {
                for (int y = 0; y < Grid.GetLength(1); y++)
                {
                    if (Grid[x, y].Item == Item)
                        return new Vector2Int(x, y);
                }
            }
            return new Vector2Int(-1, -1);
        }
        
        /// <summary>
        /// Sets all available slot lock status values ​​in the current container
        /// </summary>
        /// <param name="NewLock">Enable/Disable lock status</param>
        public void Lock(bool NewLock)
        {
            if (Grid == null) return;
            int Count = 0;

            for (int x = 0; x < Grid.GetLength(0); x++)
            {
                for (int y = 0; y < Grid.GetLength(1); y++)
                {
                    Grid[x, y].Lock = NewLock;
                    Count = Grid[x, y].Lock == NewLock ? Count + 1 : Count; 
                }
            }

            UI_Debug.Log(Log + "Slots " + Count + (NewLock ? "Locked" : "Unlocked"));
        }

        /// <summary>
        /// Sort and stack all items in the grid by its size.
        /// </summary>
        public virtual void Sort()
        {
            List<UI_Item> Data = new List<UI_Item>(GetItems());
            if (Data.Count <= 0) {
                UI_Debug.Log(Log + "Unable to sort an empty container.");
                return;
            }
            RemoveItems();

            //Stack all duplicate items on the list
            UI_Item Temp;
            int Stack = 0,Count = 0;
            List<UI_Item> Stacked = new List<UI_Item>();
            for(int i = 0; i < Data.Count; i++)
            {
                Temp = Data[i];
                Stacked = Data.FindAll((UI_Item item) => { return item == Temp; });
                if (Stacked.Count >= 1)
                {
                    Stack = 0;
                    for (int s = 0; s < Stacked.Count; s++)
                    {
                        Stack += Stacked[s].Stack;
                    }
                }

                Count = Data.Count;
                do {
                    Count = Data.RemoveAll((UI_Item item) => { return item == Temp; });
                } while (Count <= 0);

                if (!Data.Contains(Temp)) {
                    Temp.Stack = Stack;
                    Data.Add(Temp);
                }

            }

            //Data = Data.Distinct().ToList();

            //Sort Items On List (+-).
            for (int i = 0; i < Data.Count; i++) {
                for(int k = 0; k < Data.Count; k++)
                {
                    if (Data[i].Size.magnitude > Data[k].Size.magnitude)
                    {
                        Temp = Data[k];
                        Data[k] = Data[i];
                        Data[i] = Temp;
                    }
                }
            }
            
            //Put Items On Grid:
            for(int y = 0; y < Grid.GetLength(1); y++)
            {
                for(int x = 0; x < Grid.GetLength(0); x++)
                {
                    if(GetSlot(x,y).GetActive() && GetSlot(x, y).IsEmpty)
                    {
                        for (int i = 0; i < Data.Count; i++)
                        {
                            if(AddItem(Data[i], new Vector2Int(x, y)))
                            {
                                Data.Remove(Data[i]);
                            }
                        }
                    }

                    if (Data.Count <= 0)
                    {
                        x = Grid.GetLength(0);
                        y = Grid.GetLength(1);
                    }
                }
            }
        }
    }
}
