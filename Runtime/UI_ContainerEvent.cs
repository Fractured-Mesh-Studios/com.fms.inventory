using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace InventoryEngine
{
    [RequireComponent(typeof(UI_Container))]
    public class UI_ContainerEvent : MonoBehaviour
    {
        [System.Serializable] public class ContainerEvent : UnityEvent<UI_Item> { };

        [Header("Events")]
        [SerializeField] public ContainerEvent OnAddItem = new ContainerEvent();
        [SerializeField] public ContainerEvent OnRemoveItem = new ContainerEvent();

        protected UI_Container ContainerComponent;
        protected string Log = "UI_ContainerEvent: ";
        
        void Awake()
        {
            ContainerComponent = GetComponent<UI_Container>();
            if (!ContainerComponent)
            {
                Debug.LogError(Log + "Container component is null on gameobject");
            }
        }
        
    }
}