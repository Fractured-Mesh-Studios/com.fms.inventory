using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace InventoryEngine
{
    public enum UI_EDragMode
    {
        RemoveOnDrag,
        RemoveOnDrop
    }

    public class UI_Slot : UI_Graphic, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        //static members
        protected static GameObject m_dragObject, m_hoverObject;

        /// <summary>
        /// The drag operation which is being performed in real time and which is eliminated by releasing the drag key
        /// </summary>
        public static UI_DragOperation m_dragOperation { internal set; get; }

        /// <summary>
        /// the drag operation that was last executed in this slot. it is not deleted once finished and remains until another operation overwrites it
        /// </summary>
        public static UI_DragOperation m_dragOperationCache { internal set; get; }

        //public serializable members
        [Header("Slot")]
        [SerializeField] public bool block;
        [SerializeField] public bool canEmpty;
        [SerializeField] public Vector2Int position;
        [SerializeField] public PointerEventData.InputButton button = PointerEventData.InputButton.Left;

        [Header("color")]
        [SerializeField] protected Color normalColor = Color.white;
        [SerializeField] protected Color highLightedColor = Color.grey;

        [Header("Components")]
        [SerializeField] protected Image m_background;
        [SerializeField] protected Image m_icon;

        [HideInInspector][SerializeField]
        public new RectTransform rectTransform;

        /// <summary>
        /// We get the container to which this slot belongs. (parent one)
        /// </summary>
        [SerializeField]
        public UI_Container container { get { return m_container; } }

        /// <summary>
        /// Return true if this slot its empty.
        /// </summary>
        [SerializeField]
        public bool isEmpty { get { return item == UI_Item.Empty; } }

        /// <summary>
        /// Slot current containing item.
        /// </summary>
        [SerializeField]
        public virtual UI_Item item
        {
            set
            {
                mItem = value;
                m_icon.enabled = value.Icon;
                m_icon.sprite = value.Icon;
                mSize = value.Size;
                bool bItem = mItem != UI_Item.Empty;
                float x = bItem ? m_container.GetGrid().spacing.x : 0.0f;
                float y = bItem ? m_container.GetGrid().spacing.y : 0.0f;
                Vector2 spacing = (new Vector2(x, y) * new Vector2(value.Size.x - 1, value.Size.y - 1));
                Vector2 sizeDelta = (rectTransform.sizeDelta * value.Size) + spacing;
                //Transform.sizeDelta = Transform.sizeDelta * value.Size;
                rectTransform.sizeDelta = sizeDelta;
                CalculateIconLayout();
            }
            get
            {
                return mItem;
            }
        }
        
        /// <summary>
        /// Access to the event system of the current slot, these events are only called internally.
        /// </summary>
        [HideInInspector]
        public UI_SlotEvent Events { get { return m_slotEvent; } }

        /// <summary>
        /// [Enable/Disable] only the icon component (image) for the current slot.
        /// </summary>
        [SerializeField]
        public virtual bool hide
        {
            set { m_icon.enabled = !value; }
            get { return !m_icon.enabled; }
        }

        //protected members
        protected string Log = "UI_Slot: ";
        protected UI_Container m_container;
        protected UI_SlotEvent m_slotEvent;
        protected UI_Item mItem = UI_Item.Empty;
        protected bool bDrag, isActive;
        protected Vector2Int mSize = Vector2Int.zero;
        protected float angle = 0;
        
        protected new virtual void Awake()
        {
            rectTransform = transform as RectTransform;
            m_background.color = normalColor;

            m_container = GetComponentInParent<UI_Container>();
            if (!m_container)
            {
                Debug.LogError(Log + "container component is null.");
                Debug.LogError(Log + "Current slot must be child of a container.");
                //enabled = false;
            }

            m_slotEvent = GetComponent<UI_SlotEvent>();
            if (!m_slotEvent)
            {
                UI_Debug.Log(Log + "Slot Events Disabled");
            }
            else
            {
                UI_Debug.Log(Log + "Slot Events Enabled");
            }

            if (m_icon)
            {
                m_icon.preserveAspect = true;
            }

        }

        //---------------------------------------------------------------------------------------------------------------------------------------//
        //drag&Drop Operation
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (m_dragObject && m_hoverObject)
            {
                UI_Drag drag = m_dragObject.GetComponent<UI_Drag>();
                UI_DragHover dragHover = m_hoverObject.GetComponent<UI_DragHover>();
                if (drag && dragHover)
                {
                    drag.cellSize = m_container.CellSize;
                    drag.size = drag.totalSize;

                    m_hoverObject.SetActive(true);
                    Color color = m_container.IsValid(position, drag.item.Size) ? 
                        dragHover.valid : dragHover.inValid;
                    int count = m_container.Contains(position, drag.item.Size);
                    dragHover.rectTransform.position = transform.position;
                    bool bItem = item != UI_Item.Empty;
                    Vector2 spacing = m_container.GetGrid().spacing * (drag.item.Size - Vector2.one);
                    dragHover.rectTransform.sizeDelta = (bItem) ? rectTransform.sizeDelta : drag.totalSize + spacing;
                    color = (count < 2) ? color : dragHover.inValid;
                    dragHover.color = color;
                    dragHover.transform.SetAsLastSibling();
                    m_dragObject.transform.SetAsLastSibling();
                }
            }
            else
            {
                m_background.color = highLightedColor;
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (m_dragObject && m_hoverObject) {
                UI_DragHover dragHover = m_hoverObject.GetComponent<UI_DragHover>();
                UI_Drag drag = m_dragObject.GetComponent<UI_Drag>();

                if (drag && dragHover)
                {
                    dragHover.rectTransform.position = transform.position;
                    dragHover.rectTransform.sizeDelta = drag.totalSize;
                    dragHover.color = m_container.IsValid(position, drag.item.Size) ?
                    dragHover.valid : dragHover.inValid;
                    dragHover.rectTransform.SetAsLastSibling();
                    drag.transform.SetAsLastSibling();

                    dragHover.gameObject.SetActive(false);
                }
            }

            m_background.color = normalColor;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (item == UI_Item.Empty || block) return;
            if (eventData.button != button) return;

            UI_Debug.Log("drag Operation Started.");
            Canvas canvas = FindObjectOfType<Canvas>();
            int Index = canvas.transform.childCount;
            m_hoverObject = Instantiate(UI_Manager.Instance.hover, canvas.transform);
            if (m_hoverObject)
            {
                UI_DragHover hover = m_hoverObject.GetComponent<UI_DragHover>();

                hover.rectTransform.sizeDelta = rectTransform.sizeDelta;
                hover.rectTransform.position = transform.position;
                Index = m_container.transform.GetSiblingIndex() + 1;
                hover.rectTransform.SetSiblingIndex(Index);
                hover.color = UI_Manager.Instance.mode == UI_EDragMode.RemoveOnDrag ?
                    hover.valid : hover.inValid;
                hover.rectTransform.SetAsLastSibling();
            }

            m_dragObject = Instantiate(UI_Manager.Instance.drag, canvas.transform);
            if (m_dragObject)
            {
                UI_Drag drag = m_dragObject.GetComponent<UI_Drag>();
                drag.item = item;
                drag.source = this;
                drag.sourceObject = gameObject;
                drag.icon = item.Icon;
                drag.size = rectTransform.sizeDelta; //HoverSize = Transform.sizeDelta;
                drag.cellSize = m_container.CellSize;
                drag.allStack = true;
                drag.angle = m_icon.transform.rotation.eulerAngles.z;

                Index = m_container.transform.GetSiblingIndex() + 2;
                drag.transform.SetSiblingIndex(Index);
                drag.sourceContainer = m_container;
                drag.transform.SetAsLastSibling();
                m_dragOperation = new UI_DragOperation(drag);
                m_dragOperationCache = m_dragOperation;

                if (UI_Manager.Instance.mode == UI_EDragMode.RemoveOnDrag)
                    m_container.RemoveItem(position);
            }
        
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (block) return;
            if (m_dragObject)
            {
                m_dragObject.transform.position = eventData.position;
            }

        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (block) return;

            UI_Debug.Log("drag Operation Ended");
            List<RaycastResult> RaycastResultCache = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, RaycastResultCache);

            GameObject Hovered = null;
            UI_Slot HoveredSlot = null;
            bool ResultCacheValid = false;
            List<GameObject> RaycastResultCacheObject = new List<GameObject>();

            for (int i = 0; i < RaycastResultCache.Count; i++)
            {
                Hovered = RaycastResultCache[i].gameObject;
                if (Hovered.GetComponent<UI_Slot>())
                    RaycastResultCacheObject.Add(Hovered);
            }
            Hovered = null;
            for (int i = 0; i < RaycastResultCache.Count; i++)
            {
                Hovered = RaycastResultCache[i].gameObject;
                if (Hovered)
                {
                    HoveredSlot = Hovered.GetComponent<UI_Slot>();
                    if (!HoveredSlot) HoveredSlot = Hovered.GetComponentInParent<UI_Slot>();
                    if (!HoveredSlot) HoveredSlot = Hovered.GetComponentInChildren<UI_Slot>();
                    //ResultCacheValid = RaycastResultCacheObject.Contains(HoveredSlot.gameObject); //<-- Posiblemente la correcta
                    ResultCacheValid = RaycastResultCacheObject.Contains(Hovered);
                    if (HoveredSlot && HoveredSlot != this && ResultCacheValid)
                    {
                        //Exit from the method if a slot was hit
                        UI_Debug.Log("Slot Hit Leaving...");
                        return;
                    }
                }
            }


            if (canEmpty)
            {
                //if this slot can be emptied
                if (!RaycastResultCacheObject.Contains(gameObject))
                {
                    m_container.RemoveItem(position);
                    if (m_slotEvent)
                        m_slotEvent.OnDropVoid.Invoke(this);
                    UI_Debug.Log(Log + "item dropped on void.");
                }
                else
                {
                    UI_Debug.Log(Log + "item dropped on itself.");
                    if (m_slotEvent)
                        m_slotEvent.OnDropSelf.Invoke(this);
                }
            }
            else
            {
                //ifnot
                if (m_dragObject && UI_Manager.Instance.mode == UI_EDragMode.RemoveOnDrag)
                {
                    UI_Debug.Log(Log + "drag Restabled");
                    UI_Drag objectDrag = m_dragObject.GetComponent<UI_Drag>();
                    m_container.AddItem(objectDrag.item, position);
                    //objectDrag.sourceContainer.AddItem(objectDrag.item, objectDrag.source.position);
                }
            }

            //Destroy the dragged object and the hover object
            m_dragOperation = null;

            m_dragObject.SetActive(false);
            m_hoverObject.SetActive(false);
            Destroy(m_dragObject);
            Destroy(m_hoverObject);
            m_dragObject = null;
            m_hoverObject = null;
        }

        public virtual void OnDrop(PointerEventData eventData)
        {
            StartCoroutine(OnDropLate(eventData));
        }

        public virtual IEnumerator OnDropLate(PointerEventData eventData)
        {
            yield return new WaitForEndOfFrame();

            if (m_dragObject)
            {
                UI_Drag objectDrag = m_dragObject.GetComponent<UI_Drag>();
                if (objectDrag)
                {
                    //Calculate item Rotation
                    angle = m_icon.transform.rotation.eulerAngles.z;
                    CalculateIconLayout(objectDrag.angle);

                    if (item == UI_Item.Empty)
                    {
                        UI_Debug.Log(Log + "Dropped on empty slot");
                        if (m_container.AddItem(objectDrag.item, position))
                        {
                            if (UI_Manager.Instance.mode == UI_EDragMode.RemoveOnDrop)
                            {
                                if (objectDrag.sourceContainer && objectDrag.source)
                                {
                                    //Grid
                                    objectDrag.sourceContainer.RemoveItem(objectDrag.source.position);
                                }
                                else
                                {
                                    //Other
                                    objectDrag.source.item = UI_Item.Empty;
                                }

                                if (m_slotEvent)
                                    m_slotEvent.OnDropValid.Invoke(this);
                            }
                        }
                        else
                        {
                            UI_Debug.Log(Log + "The object was dropped and the space is insufficient");
                            OnDropFalied(objectDrag);
                        }
                    }
                    else
                    {
                        UI_Debug.Log(Log + "Dropped on full slot");
                        if (objectDrag.sourceContainer && objectDrag.source)
                        {
                            //Grid
                            UI_Debug.Log("Grid Slot");
                            int count0 = objectDrag.sourceContainer.Contains(objectDrag.source.position, item.Size);
                            int count1 = m_container.Contains(position, objectDrag.item.Size);

                            bool valid0 = false, valid1 = false;
                            switch (UI_Manager.Instance.mode)
                            {
                                case UI_EDragMode.RemoveOnDrag:
                                    valid0 = count0 < 1;//  <- drag
                                    valid1 = count1 < 2;//  <- This
                                    break;

                                case UI_EDragMode.RemoveOnDrop:
                                    valid0 = count0 < 2;//  <- drag
                                    valid1 = count1 < 2;//  <- This
                                    break;
                            }

                            if (valid0 && valid1) //1 less
                            {
                                UI_Item cache = item;
                                //source
                                objectDrag.sourceContainer.RemoveItems(objectDrag.source.position, item.Size);
                                //Destination (this/self)
                                m_container.RemoveItems(position, objectDrag.item.Size);

                                objectDrag.sourceContainer.AddItem(cache, objectDrag.source.position);
                                m_container.AddItem(objectDrag.item, position);
                                if (m_slotEvent)
                                {
                                    bool Self0 = objectDrag.source.position == position;
                                    bool Self1 = objectDrag.sourceContainer == m_container;

                                    if (Self0 && Self1)
                                        m_slotEvent.OnDropSelf.Invoke(this);
                                    else
                                        m_slotEvent.OnDropValid.Invoke(this);
                                }
                            }
                            else
                            {
                                OnDropFalied(objectDrag);
                                UI_Debug.Log(Log + "item [" + objectDrag.item.Id + "] position " + ((valid1) ? "valid" : "Invalid"));
                                UI_Debug.Log(Log + "item [" + item.Id + "] position " + ((valid0) ? "valid" : "Invalid"));
                            }
                        }
                        else
                        {
                            //Other
                            UI_Debug.Log("Other Slot");
                            if (m_container.Contains(position, objectDrag.item.Size) < 2)
                            {
                                UI_Item cache = item;
                                m_container.RemoveItem(position);
                                m_container.AddItem(objectDrag.item, position);
                                objectDrag.source.item = cache;
                                if (m_slotEvent)
                                    m_slotEvent.OnDropValid.Invoke(this);
                            }
                            else
                            {
                                OnDropFalied(objectDrag);
                            }
                        }
                    }
                }
                
                m_dragOperation = null;
                Destroy(m_dragObject);
            }

            if (m_hoverObject)
            {
                Destroy(m_hoverObject);
            }
        }

        protected virtual void OnDropFalied(UI_Drag drag)
        {
            UI_Debug.Log("Drop Falied");

            m_icon.transform.rotation = Quaternion.identity;
            switch (UI_Manager.Instance.mode)
            {
                case UI_EDragMode.RemoveOnDrag:
                    if (drag.source && drag.sourceContainer)
                    {
                        bool valid;
                        Vector2Int position = drag.source.position;
                        valid = drag.sourceContainer.IsValid(position, drag.OldSize);
                        UI_Item temp = new UI_Item(drag.item.Id, drag.item.Icon, drag.OldSize, drag.item.Stack);
                        temp = valid ? temp : drag.item;
                        valid = drag.sourceContainer.AddItem(temp, drag.source.position);
                        drag.source.mItem.Stack += (!drag.allStack) ? drag.item.Stack : 0;
                        UI_Debug.Log(Log + "item [" + temp.Id + "]: " + (valid? "Re-established" : "Falied to re-establish"));
                        
                    }
                    else
                    {
                        drag.source.item = drag.item;
                    }
                    break;
                default: break;
            }

            if (m_icon.transform.rotation.eulerAngles.z != angle)
            {
                m_icon.transform.rotation = Quaternion.Euler(Vector3.forward * angle);
            }

            if (m_slotEvent)
                m_slotEvent.OnDropFalied.Invoke(this);
        }
        //drag&Drop OperationEnd
        //---------------------------------------------------------------------------------------------------------------------------------------//

        //protected methods

        /// <summary>
        /// Resize the icon image component to the correct orientation.
        /// </summary>
        protected void CalculateIconLayout()
        {
            if (m_icon.rectTransform.anchorMin == m_icon.rectTransform.anchorMax)
            {
                Vector2 deltaSize = rectTransform.sizeDelta;
                deltaSize = (m_icon.transform.rotation != Quaternion.identity)
                    ? new Vector2(deltaSize.y, deltaSize.x) : deltaSize;
                m_icon.rectTransform.sizeDelta = deltaSize;
            }
        }

        /// <summary>
        /// Rotate and resize the icon image component to the correct orientation (rotated objects).
        /// </summary>
        /// <param name="self">self slot rotation angle in degree</param>
        protected void CalculateIconLayout(float self)
        {
            if (m_icon.rectTransform.anchorMin == m_icon.rectTransform.anchorMax)
            {
                //Rotate current slot to desired angle passed by parameter
                m_icon.transform.rotation = Quaternion.Euler(Vector3.forward * self);

                //Resize the this icon component to correct size
                Vector2 deltaSize = rectTransform.sizeDelta;
                deltaSize = (m_icon.transform.rotation != Quaternion.identity)
                    ? new Vector2(deltaSize.y, deltaSize.x) : deltaSize;
                m_icon.rectTransform.sizeDelta = deltaSize;
            }
        }

        /// <summary>
        /// Rotate and resize the icon image component to the correct orientation (rotated objects).
        /// </summary>
        /// <param name="source">source slot rotation angle in degree</param>
        /// <param name="self">self slot rotation angle in degree</param>
        protected void CalculateIconLayout(float source, float self)
        {
            if(m_icon.rectTransform.anchorMin == m_icon.rectTransform.anchorMax)
            {
                //Rotate the source and current slot to desired angle passed by parameter
                UI_Drag drag = m_dragObject.GetComponent<UI_Drag>();
                drag.source.m_icon.transform.rotation = Quaternion.Euler(Vector3.forward * source);
                m_icon.transform.rotation = Quaternion.Euler(Vector3.forward * self);
                
                //Resize the this icon component to correct size
                Vector2 deltaSize = rectTransform.sizeDelta;
                deltaSize = (m_icon.transform.rotation != Quaternion.identity)
                    ? new Vector2(deltaSize.y, deltaSize.x) : deltaSize;
                m_icon.rectTransform.sizeDelta = deltaSize;
            }
        }
        
        //public methods
        /// <summary>
        /// [Enable/Disable] slot all current active image components from himself and his children, this turns the entire slot on or off (visibly only).
        /// The slot functionality remains intact.
        /// </summary>
        /// <param name="Value">boolerean value</param>
        public virtual void SetActive(bool Value)
        {
            Image[] Components = GetComponentsInChildren<Image>();
            for (int i = 0; i < Components.Length; i++)
            {
                Components[i].enabled = (Components[i].sprite) ? Value : false;
            }
            isActive = Value;
        }

        /// <summary>
        /// Return if this slot is active or not.
        /// </summary>
        /// <returns>Booolerean</returns>
        public virtual bool GetActive()
        {
            return isActive;
        }

        //private methssods
        private void OnDrawGizmos()
        {
            if (UI_Manager.settings.Debug)
            {
                Color validColor = Color.white;
                validColor = mItem != UI_Item.Empty ? 
                    UI_Manager.settings.ValidColor : UI_Manager.settings.InValidColor;
                Vector3 screenPosition = rectTransform.sizeDelta / 2;
                Vector3 worldPosition = transform.position + new Vector3(screenPosition.x, -screenPosition.y);
                string icon = block ? "AssemblyLock" : "sv_icon_dot0_sml";

                if (GetActive())
                {
                    Gizmos.color = validColor;
                    Gizmos.DrawWireCube(worldPosition, Vector3.one * UI_Manager.settings.Size);
                    Gizmos.DrawIcon(worldPosition, icon, true);
                }
            }
        }
    }
}


/* Not Used Code
/// <summary>
/// Remove all items on from the start to desired size
/// </summary>
/// <param name="Start">Vector2Int start point</param>
/// <param name="Size">Vector2Int end point</param>
protected virtual void Remove(Vector2Int Start, Vector2Int Size)
{
    Vector2Int position;
    UI_Item[] Items;

    //Destination item Removing:
    Items = m_container.GetItems(Start, Size);
    for (int i = 0; i < Items.Length; i++)
    {
        position = m_container.FindPosition(Items[i]);
        m_container.RemoveItem(position);
    }
}

/// <summary>
/// Remove all items from drag source (original drag operation) from start to desired size
/// </summary>
/// <param name="drag">drag Operation</param>
/// <param name="Start">Vector2Int Start position</param>
/// <param name="Size">Vector2Int End position</param>
protected virtual void RemoveFrom(UI_Drag drag, Vector2Int Start, Vector2Int Size)
{
    Vector2Int position;
    UI_Item[] Items;

    //source Items Removing:
    Items = drag.sourceContainer.GetItems(Start, Size);
    for (int i = 0; i < Items.Length; i++)
    {
        position = drag.sourceContainer.FindPosition(Items[i]);
        drag.sourceContainer.RemoveItem(position);
    }
}
*/
