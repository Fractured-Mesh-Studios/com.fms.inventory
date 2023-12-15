using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace InventoryEngine
{
    [RequireComponent(typeof(Image))]
    public class UI_Drag : MonoBehaviour
    {
        /// <summary>
        /// item/Object currently dragged.
        /// </summary>
        [HideInInspector] public UI_Item item = UI_Item.Empty;
        /// <summary>
        /// The source slot from which the object is being dragged (if it exist)
        /// </summary>
        [HideInInspector] public UI_Slot source;
        /// <summary>
        /// the source game object (of the slot) from which the object is being dragged (if it exist)
        /// </summary>
        [HideInInspector] public GameObject sourceObject;
        /// <summary>
        /// the source container from which the object is being dragged
        /// </summary>
        [HideInInspector] public UI_Container sourceContainer;
        /// <summary>
        /// The color of the slot.
        /// </summary>
        [HideInInspector] public Color color;
        /// <summary>
        /// This variable represents if the entire stack is being dragged
        /// </summary>
        [HideInInspector] public bool allStack = false;

        /// <summary>
        /// This variable changes the icon of the currently dragged object
        /// (this does not affect the object which is being dragged)
        /// </summary>
        [HideInInspector] public Sprite icon
        {
            set { m_icon.sprite = value; }
            get { return m_icon.sprite; }
        }
        /// <summary>
        /// This variable resizes the size of the dragged object to its full value
        /// (this does not affect the object which is being dragged)
        /// </summary>
        [HideInInspector] public Vector2 size
        {
            set { rectTransform.sizeDelta = value; }
            get { return rectTransform.sizeDelta; }
        }
        /// <summary>
        /// Cell size represents the cell individual size of the grid (if it exist)
        /// (this does not affect the object which is being dragged)
        /// </summary>
        [HideInInspector] public Vector2 cellSize;
        /// <summary>
        /// This value represents the absolute size of the bounds for the current dragged object.
        /// The value of the total size depends on the size of the cells plus the size of the currently dragged object.
        /// </summary>
        [HideInInspector] public Vector2 totalSize
        {
            get { return cellSize * item.Size; }
        }
        /// <summary>
        /// The rotation angle of the z axis for the dragged object.
        /// </summary>
        [HideInInspector] public float angle;
        /// <summary>
        /// The actual rotation event count. add one for each time you press the rotate key 
        /// </summary>
        [HideInInspector] public int rotateCount;
        /// <summary>
        /// This variable is the initial size of the object
        /// (changing the angle by the event if it affects the object you drag)
        /// </summary>
        [HideInInspector] public Vector2Int OldSize { get { return m_oldSize; } }
        /// <summary>
        /// This is the key with which we trigger the rotate object event.
        /// </summary>

        [Header("Drag")]
        [SerializeField] KeyCode RotationKey = KeyCode.R;

        [Header("Events")]
        public UnityEvent onRotate = new UnityEvent();

        protected RectTransform rectTransform;
        protected string m_log = "UI_Drag: ";
        private Vector2Int m_oldSize;

        //Components
        protected Image m_icon;
        protected GraphicRaycaster m_raycaster;

        protected void Awake()
        {
            rectTransform = transform as RectTransform;

            m_icon = GetComponent<Image>();
            if (!m_icon)
                UI_Debug.LogError(m_log + "Icon image component is null.");
            else
            {
                m_icon.preserveAspect = true;
                m_icon.raycastTarget = false;
            }

            m_raycaster = FindObjectOfType<GraphicRaycaster>();
            if (!m_raycaster)
                UI_Debug.LogError(m_log + "Graphic raycaster component is null.");
            
        }

        protected void Start()
        {
            if (m_icon)
            {
                Vector3 rotation = Vector3.forward * angle;
                m_icon.transform.rotation = Quaternion.Euler(rotation);
                m_oldSize = item.Size;
                rotateCount = 0;
            }
        }

        protected void Update()
        {
            if (Input.GetKeyDown(RotationKey))
            {
                Rotate();
            }
        }

        /// <summary>
        /// Rotate the current dragged item (event)
        /// </summary>
        protected void Rotate()
        {
            if (item.Size.x == item.Size.y)
                return;

            angle = (angle > 0) ? 0 : 90;
            UI_Item temp = item;
            int x = item.Size.y;
            int y = item.Size.x;

            Vector2Int newSize = new Vector2Int(x, y);
            item = new UI_Item(temp.Id, temp.Icon, newSize, temp.Stack);
            Vector3 rotation = Vector3.forward * angle;
            m_icon.transform.localRotation = Quaternion.Euler(rotation);

            newSize = (angle > 0) ? new Vector2Int(y, x) : newSize;
            size = new Vector2(cellSize.x * newSize.x, cellSize.y * newSize.y);
            
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;

            GameObject hitObject = null;
            UI_Slot hitSlot = null;
            UI_DragHover hover = null;
            List<RaycastResult> raycastResultList = new List<RaycastResult>();

            m_raycaster.Raycast(eventData, raycastResultList);
            for(int i = 0; i < raycastResultList.Count; i++)
            {
                hitObject = raycastResultList[i].gameObject;
                hitSlot = hitObject.GetComponent<UI_Slot>();
                if(!hitSlot) hitSlot = hitObject.GetComponentInParent<UI_Slot>();
                if(!hitSlot) hitSlot = hitObject.GetComponentInChildren<UI_Slot>();
                if (hitObject && hitSlot) {
                    hover = FindObjectOfType<UI_DragHover>();
                    hover.rectTransform.position = hitSlot.transform.position;
                    hover.rectTransform.sizeDelta = totalSize;
                    hover.color = hitSlot.container.IsValid(hitSlot.position, item.Size) ?
                        hover.valid : hover.inValid;
                }
            }
            rotateCount++;
            UI_Debug.Log("item Rotated To " + angle + "° "+rotateCount+" Times.");
            onRotate.Invoke();
        }
       
    }
}