using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace InventoryEngine
{

    [RequireComponent(typeof(UI_Slot))]
    public class UI_SlotEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        [System.Serializable] public class OnClickEvent : UnityEvent<UI_Item> { }
        [System.Serializable] public class OnSlotEvent : UnityEvent<UI_Slot> { }

        [SerializeField] public KeyCode LeftClickKey = KeyCode.Mouse0;
        [SerializeField] public KeyCode RightClickKey = KeyCode.Mouse1;

        [SerializeField] public OnClickEvent OnLeftClick = new OnClickEvent();
        [SerializeField] public OnClickEvent OnRightClick = new OnClickEvent();

        //Drag
        [SerializeField] public OnSlotEvent OnBeginDragEvent = new OnSlotEvent();
        [SerializeField] public OnSlotEvent OnDragEvent = new OnSlotEvent();
        [SerializeField] public OnSlotEvent OnEndDragEvent = new OnSlotEvent();

        //Drop
        [SerializeField] public OnSlotEvent OnDropValid = new OnSlotEvent();
        [SerializeField] public OnSlotEvent OnDropFalied = new OnSlotEvent();
        [SerializeField] public OnSlotEvent OnDropVoid = new OnSlotEvent();
        [SerializeField] public OnSlotEvent OnDropSelf = new OnSlotEvent();
        [SerializeField] public OnSlotEvent OnAnyDrop = new OnSlotEvent();

        protected string Log = "UI_SlotEvent: ";
        protected UI_Slot SlotComponent;
        protected bool isHover = false;
        
        protected void Awake()
        {
            SlotComponent = GetComponent<UI_Slot>();
            if (!SlotComponent)
                Debug.LogError(Log + "Slot component is null or disabled.");
            
            OnDropValid.AddListener(OnAnyCall);
            OnDropFalied.AddListener(OnAnyCall);
            OnDropVoid.AddListener(OnAnyCall);
            OnDropSelf.AddListener(OnAnyCall);
        }

        protected void Update()
        {
            if (Input.GetKeyDown(LeftClickKey) && isHover)
            {
                OnLeftClick.Invoke(SlotComponent.Item);
            }

            if (Input.GetKeyDown(RightClickKey) && isHover)
            {
                OnRightClick.Invoke(SlotComponent.Item);
            }

        }

        //Mouse Operation
        public void OnPointerEnter(PointerEventData eventData)
        {
            isHover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHover = false;
        }
        //Mouse OperationEnd
        
        private void OnAnyCall(UI_Slot Slot)
        {
            OnAnyDrop.Invoke(Slot);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnBeginDragEvent.Invoke(SlotComponent);
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnDragEvent.Invoke(SlotComponent);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragEvent.Invoke(SlotComponent);
        }
    }
}