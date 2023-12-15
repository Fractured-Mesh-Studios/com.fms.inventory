using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace InventoryEngine
{
    [RequireComponent(typeof(UI_Container))]
    public class UI_ContainerSearch : MonoBehaviour
    {
        [System.Serializable] public class OnContainerSerachEvent : UnityEvent<UI_Slot> { }

        [Header("Search")]
        public GameObject Template;
        public bool StartEnabled = false;
        [Tooltip("Base sarch time per item")]
        public float Time = 1.0f;
        [Tooltip("Averange time rate multiplier")]
        public float Multiplier = 1.0f;
        public float Delay = 1.0f;
        public float Smooth = 1.0f;

        [Header("Events")]
        public OnContainerSerachEvent OnSearchSlot = new OnContainerSerachEvent();

        /// <summary>
        /// Detects if the current container is searched or not.
        /// </summary>
        public bool Searched { internal set; get; }

        protected RectTransform Transform;
        protected UI_Container ContainerComponent;
        protected string Log = "UI_ContainerSearch: ";
        protected int Index, Stack;
        protected GameObject TemplateObject;

        List<UI_Slot> Container = new List<UI_Slot>();

        //Internal
        protected virtual void Awake()
        {
            ContainerComponent = GetComponent<UI_Container>();
            if (!ContainerComponent)
            {
                Debug.LogError(Log + "Container component is null.");
                Debug.LogError(Log + "Container is requiered to this script to work.");
            }

            Searched = false;
        }

        protected virtual void Start()
        {
            if (StartEnabled)
                StartCoroutine(Search(Refresh));
        }
        
        /// <summary>
        /// called every time an item is detected in the container
        /// </summary>
        protected virtual void OnSearch()
        {
            if (Index >= 0 && Index < Container.Count)
            {
                ContainerComponent.SetVisibility(Container[Index].Position, true);
                OnSearchSlot.Invoke(Container[Index]);
            }
                
            if (Index >= Container.Count)
            {
                OnSearchEnd();
            }
            else
            {
                Index++;
            }
        }

        /// <summary>
        /// Called only one time at the start of the search.
        /// </summary>
        protected virtual void OnSearchStart()
        {
            if (Template)
            {
                TemplateObject = Instantiate(Template, transform);
                Image TemplateImage = TemplateObject.GetComponent<Image>();
                TemplateImage.raycastTarget = false;
            }
            Stack++;
        }

        /// <summary>
        /// Called only one time at the end of the search
        /// </summary>
        protected virtual void OnSearchEnd()
        {
            Container.Clear();
            Searched = true;
            CancelInvoke();
            Index = 0;
            Stack = 0;
            if (TemplateObject)
                Destroy(TemplateObject);
        }

        public void Search()
        {
            if (Container.Count > 0 && !IsInvoking() && Stack <= 0)
            {
                float ScaledTime = (Time * Container.Count);
                float Rate = (ScaledTime / Container.Count) * Multiplier;
                InvokeRepeating("OnSearch", Delay, Rate);
                OnSearchStart();
            }
        }

        /// <summary>
        /// Called to start the search system in the current container
        /// </summary>
        public IEnumerator Search(System.Func<bool> predicate)
        {
            yield return new WaitUntil(predicate);
            //Start Searching
            if (Container.Count > 0 && !IsInvoking() && Stack <= 0)
            {
                float ScaledTime = (Time * Container.Count);
                float Rate = (ScaledTime / Container.Count) * Multiplier;
                InvokeRepeating("OnSearch", Delay, Rate);
                OnSearchStart();
            }
        }

        /// <summary>
        /// Restart the search system and hide all the items it detects in the current container.
        /// </summary>
        public bool Refresh()
        {
            //Hide All Items
            for (int x = 0; x < ContainerComponent.GetLength(0); x++)
            {
                for (int y = 0; y < ContainerComponent.GetLength(1); y++)
                {
                    if (ContainerComponent[x, y].Item != UI_Item.Empty)
                    {
                        Vector2Int Position = new Vector2Int(x, y);
                        ContainerComponent.SetVisibility(Position, false);
                        Container.Add(ContainerComponent[x, y]);
                    }
                }
            }
            Searched = false;
            Stack = 0;
            Index = 0;
            
            return Container.Count > 0;
        }

    }
}