using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryEngine
{
    public class UI_DragOperation 
    {
        public UI_DragOperation(UI_Drag Drag)
        {
            Item = Drag.Item;
            Source = Drag.Source;
            SourceObject = Drag.SourceObject;
            SourceContainer = Drag.SourceContainer;
            Color = Drag.Color;
            AllStack = Drag.AllStack;
            
            Size = Drag.Size;
            CellSize = Drag.CellSize;
            TotalSize = Drag.TotalSize;
            Angle = Drag.Angle;
            
        }

        /// <summary>
        /// The item that belongs to the drag operation (main)
        /// </summary>
        [HideInInspector] public UI_Item Item = UI_Item.Empty;
        /// <summary>
        /// The source slot from which the object is being dragged (if it exist)
        /// </summary>
        [HideInInspector] public UI_Slot Source;
        /// <summary>
        /// the source game object (of the slot) from which the object is being dragged (if it exist)
        /// </summary>
        [HideInInspector] public GameObject SourceObject;
        /// <summary>
        /// the source container from which the object is being dragged
        /// </summary>
        [HideInInspector] public UI_Container SourceContainer;
        /// <summary>
        /// The color of the slot.
        /// </summary>
        [HideInInspector] public Color Color;
        /// <summary>
        /// This variable represents if the entire stack is being dragged
        /// </summary>
        [HideInInspector] public bool AllStack = false;
        
        /// <summary>
        /// This variable resizes the size of the dragged object to its full value
        /// (this does not affect the object which is being dragged)
        /// </summary>
        [HideInInspector] public Vector2 Size;
        
        /// <summary>
        /// Cell size represents the cell individual size of the grid (if it exist)
        /// (this does not affect the object which is being dragged)
        /// </summary>
        [HideInInspector] public Vector2 CellSize;
        /// <summary>
        /// This value represents the absolute size of the bounds for the current dragged object.
        /// The value of the total size depends on the size of the cells plus the size of the currently dragged object.
        /// </summary>
        [HideInInspector] public Vector2 TotalSize;
        
        /// <summary>
        /// The rotation angle of the z axis for the dragged object.
        /// </summary>
        [HideInInspector] public float Angle;
        /// <summary>
        /// This variable is the initial size of the object
        /// (changing the angle by the event if it affects the object you drag)
        /// </summary>
        [HideInInspector] public Vector2Int OldSize;

        public static implicit operator UI_DragOperation(UI_Drag Other)
        {
            return new UI_DragOperation(Other);
        }
    }
}


