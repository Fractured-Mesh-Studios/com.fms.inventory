using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace InventoryEngine
{
    public class UI_SlotStack : UI_Slot
    {
        [SerializeField] protected Text StackComponent;

        [Header("Stack")]
        [SerializeField]
        [Tooltip("Always update stack value")]
        public bool StackUpdate = false;

        [SerializeField]
        [Tooltip("Min stack display limit")]
        public int MinStack = 0;

        [SerializeField]
        [Tooltip("Max stack display limit")]
        public int MaxStack = 9999;

        [SerializeField]
        [Tooltip("Stack drag key modifier")]
        public KeyCode StackKey = KeyCode.LeftControl;

        [SerializeField]
        [Tooltip("Stack drop key event")]
        public KeyCode StackKeyDrop = KeyCode.Mouse1;

        /// <summary>
        /// Stack display value of current slot. Limited between (ZoomMin,ZoomMax)
        /// </summary>
        public int Stack
        {
            set
            {
                int Clamped = Mathf.Clamp(value, MinStack, MaxStack);
                StackComponent.text = Clamped.ToString();
            }
            get
            {
                return Convert.ToInt32(StackComponent.text);
            }
        }

        public override bool hide {
            set { m_icon.enabled = !value; StackComponent.enabled = !value; }
            get { return !m_icon.enabled && !StackComponent.enabled; }
        }

        protected override void Awake()
        {
            base.Awake();

            if (!StackComponent)
            {
                UI_Debug.LogError(Log + "Stack text component is null.");
            }
        }

        protected virtual void Update()
        {
            if (StackComponent)
            {
                StackComponent.enabled = item != UI_Item.Empty && !hide;
                Stack = StackUpdate ? item.Stack : 0000;
            }
        }

        //Drag&Drop Begin------------------------------------------------------------------------------------
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (m_hoverObject && m_dragObject)
            {
                m_hoverObject.SetActive(true);
                UI_Drag Drag = m_dragObject.GetComponent<UI_Drag>();
                UI_DragHover DragHover = m_hoverObject.GetComponent<UI_DragHover>();
                if (Drag && DragHover)
                {
                    Drag.cellSize = m_container.CellSize;
                    //Drag.Size = m_container.cellSize * Drag.item.Size;

                    Color Color = m_container.IsValid(position, Drag.item.Size) ?
                        DragHover.valid : DragHover.inValid;
                    int Count = m_container.Contains(position, Drag.item.Size);
                    DragHover.rectTransform.position = transform.position;
                    bool bItem = item != UI_Item.Empty;
                    Vector2 spacing = m_container.GetGrid().spacing * (Drag.item.Size - Vector2.one);
                    DragHover.rectTransform.sizeDelta = (bItem) ? rectTransform.sizeDelta : Drag.totalSize + spacing;
                    Color = (Count < 2) ? Color : DragHover.inValid;
                    Color = item == Drag.item ? DragHover.valid : Color;
                    
                    DragHover.color = Color;
                    DragHover.transform.SetAsLastSibling();
                    m_dragObject.transform.SetAsLastSibling();
                }
            }
            else
            {
                m_background.color = highLightedColor;
            }

        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            bool ValidStack = item.Stack > 0;
            base.OnBeginDrag(eventData);
            if (m_dragObject)
            {
                UI_Drag Drag = m_dragObject.GetComponent<UI_Drag>();
                UI_DragHover Hover = m_hoverObject.GetComponent<UI_DragHover>();
                float Value = (Drag) ? Drag.item.Stack / 2.0f : 0.0f;
                float FractionalValue = Value - (float)Math.Truncate(Value);
                int FloorValue = Mathf.FloorToInt(Value);
                int NewStack = FloorValue + ((FractionalValue > 0.0f) ? 1 : 0);

                UI_Debug.Log(Log + "Drag Operation Started");

                if (Input.GetKey(StackKey))
                {
                    if (NewStack > 0 && ValidStack)
                    {
                        switch (UI_Manager.Instance.mode)
                        {
                            case UI_EDragMode.RemoveOnDrag:
                                bool AddedItem = false;
                                UI_Item NewItem = Drag.item;
                                NewItem.Stack -= NewStack;
                                Drag.item.Stack = NewStack;
                                if (NewItem.Stack > 0)
                                {
                                    m_container.AddItem(NewItem, position);
                                    AddedItem = true;
                                }

                                //Drag.allStack = item.Stack == (NewStack + Drag.item.Stack);
                                Drag.allStack = item.Stack <= 0;
                                Drag.allStack = AddedItem ? false : Drag.allStack; 
                                
                                Hover.color = Hover.valid;
                                break;
                            case UI_EDragMode.RemoveOnDrop:
                                int OldStack = item.Stack;
                                bool RemovedItem = false;
                                
                                mItem.Stack -= NewStack;
                                if (mItem.Stack <= 0)
                                {
                                    m_container.RemoveItem(position);
                                    RemovedItem = true;
                                    Hover.color = Hover.valid;
                                }
                                //Debug.LogWarning(NewStack);
                                Drag.allStack = item.Stack == (NewStack + OldStack);
                                Drag.allStack = (RemovedItem) ? true : Drag.allStack;
                                Drag.item.Stack = NewStack;
                                break;
                        }
                    }
                    else
                    {
                        Drag.allStack = item.Stack <= 0;
                        UI_Debug.Log(Log + "Invalid Drag&Drop Operation");
                        UI_Debug.Log(Log + "Drag Stack: [" + Drag.item.Stack + "]");
                        UI_Debug.Log(Log + "item Stack: [" + item.Stack + "]");
                    }

                    UI_Debug.Log(Log + "[" + (Drag.allStack ? "Normal" : "Multiple") + "] Drag Operation");
                }
                else
                {
                    UI_Debug.Log(Log + "Normal Operation");
                    Drag.allStack = true;
                }
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (block) return;

            UI_Debug.Log(Log + "Drag Operation Ended");

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
                    ResultCacheValid = RaycastResultCacheObject.Contains(Hovered);
                    if (HoveredSlot && HoveredSlot != this && ResultCacheValid)
                    {
                        //Exit from the method if a slot was hit
                        UI_Debug.Log(Log + "Slot Detected, Living");
                        return;
                    }
                }
            }

            if (canEmpty)
            {
                //if this slot can be emptied
                //if(RaycastResultCacheObject.Contains(gameObject)) <-- LineaOriginal
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
                if (m_dragObject)
                {
                    bool Success = false;
                    UI_Drag Drag = m_dragObject.GetComponent<UI_Drag>();
                    switch (UI_Manager.Instance.mode)
                    {
                        //Drag
                        case UI_EDragMode.RemoveOnDrag:
                            if (RaycastResultCacheObject.Contains(gameObject))
                            {
                                //Drop On Itself
                                UI_Item NewItem = Drag.item;
                                NewItem.Stack += (Drag.allStack) ? 0 : item.Stack;
                                if (item != UI_Item.Empty || item.Stack > 0)
                                    m_container.RemoveItem(position);

                                if(m_container.IsValid(position, NewItem.Size))
                                {
                                    Success = m_container.AddItem(NewItem, position);
                                }
                                else
                                {
                                    NewItem.Size = Drag.OldSize;
                                    Success = m_container.AddItem(NewItem, position);
                                }

                                UI_Debug.Log(Log + "item [" + NewItem.Id + "]");
                                UI_Debug.Log(Log + (!Success ? "Not " : string.Empty) + "Successfully Reset.");
                            }
                            else
                            {
                                UI_Item NewItem = Drag.item;
                                //CalculateIconLayout(0.0f, Drag.Angle);
                                if (mItem == UI_Item.Empty) {
                                    if (!m_container.IsValid(position, Drag.item.Size))
                                    {
                                        NewItem.Size = Drag.OldSize;
                                        Success = m_container.AddItem(NewItem, position);
                                    }
                                    else {
                                        Success = m_container.AddItem(NewItem, position);
                                    }
                                        
                                }
                                else
                                {
                                    mItem.Stack += (!Drag.allStack) ? Drag.item.Stack : 0;
                                    Success = true;
                                }

                                UI_Debug.Log(Log + "item [" + item.Id + "]");
                                UI_Debug.Log(Log + (!Success ? "Not " : string.Empty) + "Successfully Reset.");
                            }
                            break;
                        //Drop
                        case UI_EDragMode.RemoveOnDrop:
                            if (RaycastResultCacheObject.Contains(gameObject))
                            {
                                //Drop On Itself
                                UI_Item item = this.item;
                                if (Drag.allStack)
                                {
                                    if (item == UI_Item.Empty)
                                        Drag.sourceContainer.AddItem(Drag.item, Drag.source.position);

                                    mItem.Stack = (
                                        Drag.item.Id == mItem.Id && 
                                        Drag.item.Icon == mItem.Icon) ? 
                                        mItem.Stack : mItem.Stack + Drag.item.Stack;
                                    Success = m_container.IsValid(position, Drag.item.Size);
                                    //if(Success) CalculateIconLayout(Angle);
                                    Success = mItem != UI_Item.Empty && (mItem.Stack == Drag.item.Stack);
                                }
                                else
                                {
                                    if (Drag.item.Stack > 0)
                                        Drag.sourceContainer.AddItem(Drag.item, Drag.source.position);

                                    mItem.Stack -= Drag.item.Stack;
                                    int Count = m_container.Contains(position, Drag.item.Size);
                                    if (Count > 0)
                                    {
                                        item.Stack += Drag.item.Stack;
                                        mItem = item;
                                    }
                                    else
                                    {
                                        Debug.LogWarning("Value: " + Count);
                                        mItem.Stack += Drag.item.Stack;
                                    }
                                    Success = mItem != UI_Item.Empty;
                                }
                            }
                            else
                            {
                                //Drop On Void
                                if (!Drag.allStack)
                                {
                                    if (Drag.source.item == UI_Item.Empty && Drag.item.Stack > 0)
                                        Drag.sourceContainer.AddItem(Drag.item, Drag.source.position);

                                    int Stack = mItem.Stack;
                                    mItem.Stack += Drag.item.Stack;
                                    Success = mItem != UI_Item.Empty &&
                                        mItem.Stack == Stack + Drag.item.Stack;
                                }
                                else
                                {
                                    if (Drag.source.item == UI_Item.Empty && Drag.item.Stack > 0)
                                        Drag.sourceContainer.AddItem(Drag.item, Drag.source.position);

                                    Success = mItem != UI_Item.Empty;
                                    Success &= mItem.Stack == Drag.item.Stack;
                                }
                            }
                            
                            UI_Debug.Log(Log + "item [" + item.Id + "]");
                            UI_Debug.Log(Log + (!Success ? "Not " : string.Empty) + "Successfully Reset.");
                            break;
                    }
                }
                
            }
            m_dragOperation = null;

            //Remove drag and hover objects 
            Destroy(m_dragObject);
            Destroy(m_hoverObject);
            m_dragObject = null;
            m_hoverObject = null;
        }

        public override void OnDrop(PointerEventData eventData)
        {
            StartCoroutine(OnDropLate(eventData));
        }

        public override IEnumerator OnDropLate(PointerEventData eventData)
        {
            yield return new WaitForEndOfFrame();

            if (m_dragObject)
            {
                UI_Drag ObjectDrag = m_dragObject.GetComponent<UI_Drag>();
                if (ObjectDrag)
                {
                    //Calculate item Rotation Here
                    angle = m_icon.transform.rotation.eulerAngles.z;
                    CalculateIconLayout(ObjectDrag.angle);

                    if (item == UI_Item.Empty)
                    {
                        //REVISAR CODIGO POSIBLE PROBLEMA
                        UI_Debug.Log(Log + "Dropped on empty slot");
                        if (m_container.AddItem(ObjectDrag.item, position))
                        {
                            m_slotEvent.OnDropValid.Invoke(this);

                            if (ObjectDrag.sourceContainer && ObjectDrag.source)
                            {
                                //Grid
                                if (!ObjectDrag.source.isEmpty && (ObjectDrag.allStack || ObjectDrag.source.item.Stack <= 0))
                                    ObjectDrag.sourceContainer.RemoveItem(ObjectDrag.source.position);
                                //Fix if the object shares a position with the old one.

                                //posible problema dependiendo del modo
                                if (
                                    ObjectDrag.source.position != position &&
                                    ObjectDrag.source.isEmpty &&
                                    ObjectDrag.source.GetActive() &&
                                    ObjectDrag.sourceContainer.Contains(ObjectDrag.source.position, this))
                                {
                                    ObjectDrag.source.SetActive(false);
                                    ObjectDrag.source.enabled = false;
                                }
                            }
                            else
                            {
                                //Other
                                if (ObjectDrag.allStack || ObjectDrag.source.item.Stack <= 0)
                                    ObjectDrag.source.item = UI_Item.Empty;

                                //Falta REVISAR
                            }
                        }
                        else
                        {
                            UI_Debug.Log(Log + "The object was dropped and the space is insufficient");
                            OnDropFalied(ObjectDrag);
                        }
                    }
                    else
                    {
                        UI_Debug.Log(Log + "Dropped on full slot");

                        //Drop Check
                        if (item == ObjectDrag.item)
                        {
                            //Drop on equal item
                            UI_Debug.Log(Log + "Drop On Equal item");
                            if (ObjectDrag.source && ObjectDrag.sourceContainer)
                            {
                                //Grid
                                switch (UI_Manager.Instance.mode)
                                {
                                    //Remove
                                    case UI_EDragMode.RemoveOnDrag:
                                        UI_Debug.Log(Log + "Remove On Drag Detected");
                                        /*if (ObjectDrag.source.Events)
                                            ObjectDrag.source.Events.OnDropValid.Invoke(ObjectDrag.source);*/
                                        if (m_slotEvent)
                                            m_slotEvent.OnDropValid.Invoke(this);

                                        if (ObjectDrag.source != this)
                                        {
                                            mItem.Stack += ObjectDrag.item.Stack;
                                            if (ObjectDrag.source.item.Stack <= 0 || ObjectDrag.allStack)
                                                ObjectDrag.sourceContainer.RemoveItem(ObjectDrag.source.position);
                                        }
                                        break;
                                    //Remove
                                    case UI_EDragMode.RemoveOnDrop:
                                        UI_Debug.Log(Log + "Remove On Drop Detected");
                                        //SlotEvetComponet.OnDropValid.Invoke(this) <-- antigua linea a revisar
                                        if (m_slotEvent)
                                            m_slotEvent.OnDropValid.Invoke(this);

                                        if (ObjectDrag.source != this)
                                        {
                                            mItem.Stack += ObjectDrag.item.Stack;
                                            if (ObjectDrag.source.item.Stack <= 0 || ObjectDrag.allStack)
                                                ObjectDrag.sourceContainer.RemoveItem(ObjectDrag.source.position);
                                        }
                                        break;
                                }
                                
                            }
                            else
                            {
                                //Other
                                UI_SlotEx Slot = ObjectDrag.sourceObject.GetComponent<UI_SlotEx>();
                                switch (UI_Manager.Instance.mode)
                                {
                                    //Remove
                                    case UI_EDragMode.RemoveOnDrag:
                                        mItem.Stack += (Slot.item == UI_Item.Empty) ? 0 : ObjectDrag.item.Stack;
                                        UI_Debug.Log(Log + "Remove On Drag Detected");
                                        break;
                                    //Remove
                                    case UI_EDragMode.RemoveOnDrop:
                                        mItem.Stack += ObjectDrag.item.Stack;
                                        Slot.item = UI_Item.Empty;
                                        UI_Debug.Log(Log + "Remove On Drop Detected");
                                        break;
                                }
                                
                                //REVISAR EVENTO 
                                /*if (ObjectDrag.source.Events)
                                    ObjectDrag.source.Events.OnDropValid.Invoke(ObjectDrag.source);*/
                            }

                        }
                        else
                        {
                            //Drop on not equal item
                            if (ObjectDrag.sourceContainer && ObjectDrag.source)
                            {
                                //Grid
                                bool Success0 = false, Success1 = false;

                                UI_Debug.Log(Log + "Grid Slot");
                                int Count0 = ObjectDrag.sourceContainer.Contains(ObjectDrag.source.position, item.Size);
                                int Count1 = m_container.Contains(position, ObjectDrag.item.Size);

                                bool Valid0 = false, Valid1 = false;
                                switch (UI_Manager.Instance.mode)
                                {
                                    case UI_EDragMode.RemoveOnDrag:
                                        Valid0 = Count0 < 1;//  <- Drag
                                        Valid1 = Count1 < 2;//  <- This
                                        break;

                                    case UI_EDragMode.RemoveOnDrop:
                                        Valid0 = Count0 < 2;//  <- Drag
                                        Valid1 = Count1 < 2;//  <- This
                                        break;
                                }

                                if (Valid0 && Valid1) //1 less
                                {
                                    UI_Item Cache = item;

                                    switch (UI_Manager.Instance.mode)
                                    {
                                        case UI_EDragMode.RemoveOnDrag:
                                            //source
                                            ObjectDrag.sourceContainer.RemoveItems(ObjectDrag.source.position, item.Size);

                                            //Destination (this/self)
                                            m_container.RemoveItems(position, ObjectDrag.item.Size);

                                            Success0 = ObjectDrag.sourceContainer.AddItem(Cache, ObjectDrag.source.position);
                                            Success1 = m_container.AddItem(ObjectDrag.item, position);

                                            if (!Success0 || !Success1)
                                            {
                                                ObjectDrag.sourceContainer.RemoveItem(ObjectDrag.source.position);
                                                m_container.RemoveItem(position);
                                                ObjectDrag.sourceContainer.AddItem(ObjectDrag.item, ObjectDrag.source.position);
                                                m_container.AddItem(Cache, position);
                                                OnDropFalied(ObjectDrag);
                                            }
                                            else
                                            {
                                                if (m_slotEvent)
                                                    m_slotEvent.OnDropValid.Invoke(this);
                                                
                                                //EFECTUAR EL CAMBIO DE ROTACION DE ICONOS DE SER NECESARIO ACA (TO FIX)
                                            }
                                            break;
                                        case UI_EDragMode.RemoveOnDrop:
                                            ObjectDrag.item.Stack += (!ObjectDrag.allStack) ?
                                                ObjectDrag.source.item.Stack : 0;
                                            //source
                                            ObjectDrag.sourceContainer.RemoveItem(ObjectDrag.source.position);
                                            //Destination (self/this)
                                            m_container.RemoveItem(position);

                                            Success0 = ObjectDrag.sourceContainer.AddItem(Cache, ObjectDrag.source.position);
                                            Success1 = m_container.AddItem(ObjectDrag.item, position);
                                            if (!Success0 || !Success1)
                                            {
                                                ObjectDrag.sourceContainer.RemoveItem(ObjectDrag.source.position);
                                                m_container.RemoveItem(position);
                                                ObjectDrag.sourceContainer.AddItem(ObjectDrag.item, ObjectDrag.source.position);
                                                m_container.AddItem(Cache, position);
                                                OnDropFalied(ObjectDrag);
                                            }
                                            else
                                            {
                                                if (m_slotEvent)
                                                    m_slotEvent.OnDropValid.Invoke(this);
                                            }
                                            break;
                                    }

                                }
                                else
                                {
                                    OnDropFalied(ObjectDrag);

                                    UI_Debug.Log(Log + "item [" + ObjectDrag.item.Id + "] position " + ((Valid1) ? "Valid" : "Invalid"));
                                    UI_Debug.Log(Log + "item [" + item.Id + "] position " + ((Valid0) ? "Valid" : "Invalid"));
                                }
                            }
                            else
                            {
                                //Other
                                UI_Debug.Log(Log + "Other Slot");
                                if (m_container.Contains(position, ObjectDrag.item.Size) < 2)
                                {
                                    UI_Item Cache = item;
                                    m_container.RemoveItem(position);
                                    m_container.AddItem(ObjectDrag.item, position);
                                    ObjectDrag.source.item = Cache;

                                    if (m_slotEvent)
                                        m_slotEvent.OnDropValid.Invoke(this);
                                    
                                }
                                else
                                {
                                    OnDropFalied(ObjectDrag);
                                }
                            }
                        }
                    }
                    
                }

                Destroy(m_dragObject);
            }

            if (m_hoverObject)
            {
                Destroy(m_hoverObject);
            }
            //DropCode End

        }

        protected override void OnDropFalied(UI_Drag Drag)
        {
            base.OnDropFalied(Drag);
            UI_Debug.Log(Log + "On Drop Falied");
            
            switch (UI_Manager.Instance.mode)
            {
                case UI_EDragMode.RemoveOnDrop:
                    if (!Drag.allStack)
                    {
                        UI_Item Cache = Drag.item;
                        Cache.Size = (Drag.rotateCount > 0) ? Drag.OldSize : Cache.Size;
                        //Stacks
                        int SourceStack = Drag.sourceContainer[Drag.source.position].item.Stack;
                        int DragStack = Drag.item.Stack;

                        Cache.Stack = SourceStack + DragStack;
                        Drag.sourceContainer.RemoveItem(Drag.source.position);
                        Drag.sourceContainer.AddItem(Cache, Drag.source.position);
                    }
                    else
                    {
                        if (Drag.source.item == UI_Item.Empty)
                            Drag.sourceContainer.AddItem(Drag.item, Drag.source.position);
                        else
                        {
                            UI_SlotStack Slot = Drag.source as UI_SlotStack;
                            if (Slot)
                            {
                                Slot.mItem = (Slot.item == UI_Item.Empty && Drag.item.Stack > 0) ?
                                    Drag.item : Slot.item;
                            }
                        }
                    }
                    break;

                case UI_EDragMode.RemoveOnDrag:
                    break;
            }
            
            if (m_icon.transform.rotation.eulerAngles.z != angle || Drag.rotateCount > 0)
            {
                Debug.LogWarning("ROTATE FIX");
                m_icon.transform.rotation = Quaternion.Euler(Vector3.forward * angle);
            }
        }

        //Drag&Drop End--------------------------------------------------------------------------------------
        
        /// <summary>
        /// [Enable/Disable] the current slot. (also hide all image and text elements in child objects)
        /// </summary>
        /// <param name="Value"></param>
        public override void SetActive(bool Value)
        {
            Image[] Components = GetComponentsInChildren<Image>();
            for (int i = 0; i < Components.Length; i++)
            {
                Components[i].enabled = (Components[i].sprite) ? Value : false;
            }
            Text[] TextComponents = GetComponentsInChildren<Text>();
            for (int i = 0; i < TextComponents.Length; i++)
            {
                TextComponents[i].enabled = Value;
            }
            isActive = Value;
        }

        /// <summary>
        /// Get if the current slot is active or not.
        /// </summary>
        /// <returns>true if the slot is currently active.</returns>
        public override bool GetActive()
        {
            return base.GetActive();
        }

        /// <summary>
        /// The split method is a functionality for the current slot that contains an item which we want to divide by 2. (the item cannot be split being less than 2)
        /// </summary>
        public void Split()
        {
            bool Success = false;
            if(UI_Item.Empty != item)
            {
                if(item.Stack >= 2)
                {
                    UI_Item NewItem = item;
                    int OldStack = item.Stack;
                    float Value = Convert.ToSingle(item.Stack / 2);
                    float FractionalValue = Value - (float)Math.Truncate(Value);
                    int FloorValue = Mathf.FloorToInt(Value);
                    int NewStack = FloorValue + ((FractionalValue > 0.0f) ? 1 : 0);
                    mItem.Stack -= NewStack;
                    if(item.Stack <= 0) m_container.RemoveItem(position);

                    Success = true;
                    NewItem.Stack = NewStack;
                    if (!m_container.AddItem(NewItem))
                    {
                        if (item != UI_Item.Empty)
                        {
                            mItem.Stack += NewStack;
                            Success = false;
                        }
                        else
                        {
                            NewItem.Stack = OldStack;
                            m_container.AddItem(NewItem, position);
                            Success = false;
                        }
                            
                    }
                }
                else
                {
                    UI_Debug.Log(Log + "The item cannot be split being less than 2.");
                }
            }
            
            UI_Debug.Log(Log + "Split " + ((Success) ? "Success" : "Falied"));
        }

        /// <summary>
        /// The split method is a functionality for the current slot that contains an item which we want to extract by an given amount.
        /// </summary>
        /// <param name="Value">The new amount of the new item stack</param>
        public void Split(int Value)
        {
            bool Success = false;
            if(item != UI_Item.Empty)
            {
                UI_Item NewItem = item;
                int OldValue = item.Stack;
                int NewValue = item.Stack - Value;
                mItem.Stack -= NewValue;
                NewItem.Stack = Value;

                if (item.Stack <= 0) { m_container.RemoveItem(position); }

                Success = true;
                if (!m_container.AddItem(NewItem))
                {
                    if(item == UI_Item.Empty)
                    {
                        NewItem.Stack = OldValue;
                        m_container.AddItem(NewItem, position);
                        Success = false;
                    }
                    else
                    {
                        mItem.Stack += Value;
                        Success = false;
                    }
                }
            }

            UI_Debug.Log(Log + "Split " + ((Success) ? "Success" : "Falied") + " by <"+Value+">");
        }
    }
}