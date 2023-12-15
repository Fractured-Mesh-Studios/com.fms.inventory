using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InventoryEngine
{
    public class UI_SlotEx : UI_Slot
    {
        /// <summary>
        /// Add an item to this slot ignoring all the requieriments to be used.
        /// </summary>
        public override UI_Item item
        {
            set
            {
                mItem = value;
                m_icon.enabled = (value.Icon != null);
                m_icon.sprite = value.Icon;
                mSize = value.Size;
            }
            get { return mItem; }
        }

        [Header("Slot Extended")]
        public int Id = 0;
        public int MinStack;
        public int MaxStack;

        public bool Filter = false;
        public string[] FilterTags;
        public int FilterValidMin = 1;

        protected Vector2 TotalSize = Vector2.zero;
        protected Vector2Int OriginalSize = Vector2Int.zero;

        protected override void Awake()
        {
            Log = "UI_SlotEx: ";
            rectTransform = transform as RectTransform;

            if (!m_icon)
                UI_Debug.LogError(Log + "Icon image component is null");
            else
            {
                m_icon.enabled = mItem.Icon != null;
                m_icon.preserveAspect = true;
            }

            if (!m_background)
                UI_Debug.LogError(Log + "Background image component is null");
            else
            {
                m_background.color = normalColor;
            }

            m_slotEvent = GetComponent<UI_SlotEvent>();
            UI_Debug.Log(Log + "Slot Events " + (m_slotEvent ? "Enabled" : "Disabled"));
        }

        //Begin Drag&Drop
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (m_dragObject)
            {
                UI_Drag Drag = m_dragObject.GetComponent<UI_Drag>();
                if (Drag)
                {
                    Drag.size = rectTransform.sizeDelta;
                }
            }

            if (m_hoverObject)
            {
                m_hoverObject.SetActive(false);
            }
            m_background.color = highLightedColor;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            m_background.color = normalColor;
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (item == UI_Item.Empty || block) return;

            Canvas Canvas = FindObjectOfType<Canvas>();
            m_hoverObject = Instantiate(UI_Manager.Instance.hover, Canvas.transform);
            if (m_hoverObject)
            {
                //rectTransform Tr = m_hoverObject.transform as rectTransform;
                UI_DragHover Hover = m_hoverObject.GetComponent<UI_DragHover>();

                Hover.rectTransform.sizeDelta = TotalSize;
                Hover.rectTransform.position = transform.position;
                Hover.rectTransform.SetAsLastSibling();
                Hover.rectTransform.gameObject.SetActive(false);
            }

            m_dragObject = Instantiate(UI_Manager.Instance.drag, Canvas.transform);
            if (m_dragObject)
            {
                UI_Drag Drag = m_dragObject.GetComponent<UI_Drag>();
                Drag.item = item;
                Drag.source = this;
                Drag.sourceObject = gameObject;
                Drag.icon = item.Icon;
                Drag.cellSize = TotalSize / item.Size;
                Drag.allStack = true; // <-- Cambiarlo de ser necesario (not tested)

                Drag.transform.SetAsLastSibling();
                Drag.sourceObject = null;
                m_dragOperation = new UI_DragOperation(Drag);
                m_dragOperationCache = m_dragOperation;

                if (UI_Manager.Instance.mode == UI_EDragMode.RemoveOnDrag)
                {
                    item = UI_Item.Empty;
                }
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (block) return;
            if (m_dragObject)
            {
                m_dragObject.transform.position = Input.mousePosition;
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (block) return;

            UI_Debug.Log("EndDrag Operation Started.");
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
                //!eventData.hovered.Contains(gameObject)
                if (!RaycastResultCacheObject.Contains(gameObject))
                {
                    item = UI_Item.Empty;
                    TotalSize = Vector2.zero;
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
                switch (UI_Manager.Instance.mode)
                {
                    case UI_EDragMode.RemoveOnDrag:
                        if (m_dragObject)
                        {
                            UI_Drag Drag = m_dragObject.GetComponent<UI_Drag>();
                            item = Drag.item;
                        }
                        else
                        {
                            UI_Debug.Log(Log + "Drag Object is null or not found");
                        }
                        break;
                }
            }

            m_dragOperation = null;
            Destroy(m_dragObject);
            Destroy(m_hoverObject);
        }

        public override void OnDrop(PointerEventData eventData)
        {
            StartCoroutine(OnDropLate(eventData));
        }

        public override IEnumerator OnDropLate(PointerEventData eventData)
        {
            yield return new WaitForEndOfFrame();

            bool Success = false;
            if (m_dragObject)
            {
                UI_Drag Drag = m_dragObject.GetComponent<UI_Drag>();
                bool StackValid = true;
                StackValid &= Drag.item.Stack > 0;
                StackValid &= Drag.item.Stack >= MinStack;

                //Filter the dragged item.
                bool FilterValid = false;
                bool FilterSuccess = false;
                int FilterValidCount = 0;
                for (int i = 0; i < FilterTags.Length; i++)
                {
                    if (Drag.item.Tags != null)
                    {
                        for (int j = 0; j < Drag.item.Tags.Length; j++)
                        {
                            FilterSuccess = Drag.item.Tags[j] == FilterTags[i];
                            FilterValidCount += (FilterSuccess) ? 1 : 0;
                        }
                    }
                }
                FilterValid = FilterValidCount >= FilterValidMin;

                if (StackValid && (FilterValid || !Filter))
                {
                    if (Drag.source && Drag.sourceContainer)
                    {
                        //Grid
                        switch (UI_Manager.Instance.mode)
                        {
                            case UI_EDragMode.RemoveOnDrop:
                                Drag.sourceContainer.RemoveItem(Drag.source.position);
                                break;
                        }

                        if (item == UI_Item.Empty)
                        {
                            item = Drag.item;
                            TotalSize = Drag.totalSize;
                            Success = item == Drag.item;
                            OriginalSize = Drag.OldSize;
                        }
                        else if (Drag.allStack)
                        {
                            Vector2Int Size = Drag.sourceContainer.IsValid(
                                Drag.source.position, item.Size) ?
                                item.Size : OriginalSize;

                            UI_Item NewItem = new UI_Item(item.Id, item.Icon, Size, item.Stack);
                            Success = Drag.sourceContainer.AddItem(NewItem, Drag.source.position);
                            item = Drag.item;
                            OriginalSize = Drag.OldSize;
                        }
                    }
                    else
                    {
                        //Other
                        switch (UI_Manager.Instance.mode)
                        {
                            case UI_EDragMode.RemoveOnDrop:
                                //if(Drag.Source as UI_SlotEx) (Drag.Source as UI_SlotEx).mItem = UI_Item.Empty;
                                Drag.source.item = UI_Item.Empty;
                                break;
                        }

                        if (item != UI_Item.Empty)
                        {
                            //(Drag.Source as UI_SlotEx).mItem = item;
                            Drag.source.item = item;
                            TotalSize = Drag.totalSize;
                            Success = item == Drag.item;
                        }
                        else
                        {
                            if (gameObject == Drag.sourceObject)
                            {
                                if (m_slotEvent)
                                    m_slotEvent.OnDropSelf.Invoke(this);

                                switch (UI_Manager.Instance.mode)
                                {
                                    case UI_EDragMode.RemoveOnDrag:
                                        item = Drag.item;
                                        TotalSize = Drag.totalSize;
                                        break;
                                }
                            }

                            //Falta Codigo
                            UI_Item Cache = item;
                            item = Drag.item;
                            TotalSize = Drag.totalSize;
                            Drag.sourceObject.GetComponent<UI_SlotEx>().item = Cache;
                            Success = item == Drag.item;
                        }
                    }

                }
                else
                {
                    //Drag Falied
                    Success = false;
                    UI_Debug.Log(Log + "The dragged object did not complete the minimum requirements to be used.");
                }

                if (!Success)
                {
                    OnDropFalied(Drag);
                }
                else
                {
                    /*if (Drag.Source.Events)
                        Drag.Source.Events.OnDropValid.Invoke(Drag.Source);*/
                    if (m_slotEvent)
                        m_slotEvent.OnDropValid.Invoke(this);
                }

                Destroy(m_dragObject);
            }

            if (m_hoverObject)
            {
                Destroy(m_hoverObject);
            }
        }

        protected override void OnDropFalied(UI_Drag Drag)
        {
            base.OnDropFalied(Drag);
        }
        //End Drag&Drop

        //Filter
        protected virtual bool OnFilter(UI_Item item)
        {
            //Filter the dragged item.
            bool StackValid = true;
            StackValid &= item.Stack > 0;
            StackValid &= item.Stack >= MinStack;

            bool FilterValid = false;
            int FilterValidCount = 0;
            for (int i = 0; i < FilterTags.Length; i++)
            {
                if (item.Tags != null) {
                    FilterValidCount += Array.Exists(item.Tags, e => e == FilterTags[i]) ? 1 : 0;
                    /*for (int j = 0; j < item.Tags.Length; j++)
                    {
                        FilterValidCount += (item.Tags[j] == FilterTags[i]) ? 1 : 0;
                    }*/
                }
                else {
                    FilterValidCount = -1;
                    i = FilterTags.Length;
                }
            }
            FilterValid = FilterValidCount >= FilterValidMin;
            return StackValid && (FilterValid || !Filter);
        }
        //FilterEnd

        /// <summary>
        /// Add a new item to this socket checking that all the requirements are correct.
        /// </summary>
        /// <param name="NewItem">item to be added</param>
        /// <returns>true if the item was added successfully</returns>
        public virtual bool AddItem(UI_Item NewItem)
        {
            bool Success = OnFilter(NewItem);
            if (Success){
                item = NewItem;
            }
            else
            {
                string Text0 = item.ToString();
                string Text1 = "The " + Text0 + " does not meet the requirements to be added to the slot " + Id;
                UI_Debug.Log(Log + Text1);
            }
            return Success;
        }

        /// <summary>
        /// Remove the current item from this slot.
        /// </summary>
        public virtual void RemoveItem()
        {
            item = UI_Item.Empty;
        }

        /// <summary>
        /// Check if the new item can be added to the slot.
        /// </summary>
        /// <param name="NewItem">item to compare (check)</param>
        /// <param name="Empty">Allows an empty item to be considered valid.</param>
        /// <returns>True if the item can be added to the slot</returns>
        public virtual bool ValidItem(UI_Item NewItem, bool Empty = false)
        {
            if(UI_Item.Empty == NewItem && Empty) {
                return true;
            } 
            else
            {
                return OnFilter(NewItem);
            }
        }
    }
}