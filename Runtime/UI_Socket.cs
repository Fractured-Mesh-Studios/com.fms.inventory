using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace InventoryEngine.Extra
{
    public class UI_Socket : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [System.Serializable] public class OnClickEvent : UnityEvent<Socket> { }

        [Header("Socket")]
        public Socket Socket;
        public bool Interactable
        {
            set { bInteractable = value; OnInteractable(value); }
            get { return bInteractable; }
        }
        [SerializeField] Selectable.Transition Transition = Selectable.Transition.ColorTint;
        [SerializeField] Graphic Target;
        [SerializeField] ColorBlock Block = ColorBlock.defaultColorBlock;

        public RectTransform[] DetectionFilter;

        [Header("Events")]
        [SerializeField] OnClickEvent OnClick = new OnClickEvent();

        public RectTransform Transform;
        private bool bInteractable = true;
        private Color TargetColor;

        void Awake()
        {
            Transform = transform as RectTransform;
            Target = (Target) ? Target : GetComponentInChildren<Image>();
            TargetColor = Block.normalColor;
        }

        void Update()
        {
            if (Target)
            {
                float Duration = (1 / Block.fadeDuration) * Time.deltaTime;
                Target.color = Color.Lerp(Target.color, TargetColor, Duration);
            }
        }

        //Events
        public void OnPointerEnter(PointerEventData eventData)
        {
            switch (Transition)
            {
                case Selectable.Transition.ColorTint:
                    TargetColor = Detection(eventData) ? Block.highlightedColor : TargetColor;
                    break;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            switch (Transition)
            {
                case Selectable.Transition.ColorTint:
                    TargetColor = Detection(eventData) ? Block.normalColor : TargetColor;
                    break;
            }
        }

        public virtual void OnInteractable(bool Value)
        {
            if (!Target) return;
            switch (Transition)
            {
                case Selectable.Transition.ColorTint:
                    TargetColor = (!Value) ? Block.disabledColor : Block.normalColor;
                    break;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (DetectionFilter.Length > 0)
            {
                List<RaycastResult> Hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, Hits);
                foreach (RaycastResult r in Hits)
                {
                    foreach (Transform t in DetectionFilter)
                    {
                        if (r.gameObject == t.gameObject)
                        {
                            Debug.Log("Clicked Socket " + name);
                            TargetColor = Block.selectedColor;
                            OnClick.Invoke(Socket);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("Clicked Socket " + name);
                TargetColor = Block.selectedColor;
                OnClick.Invoke(Socket);
            }
        }

        protected virtual bool Detection(PointerEventData eventData)
        {
            if (DetectionFilter.Length > 0)
            {
                List<RaycastResult> Hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, Hits);
                foreach (RaycastResult r in Hits)
                {
                    foreach (Transform t in DetectionFilter)
                    {
                        if (r.gameObject == t.gameObject)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            else
            {
                return false;
            }
        }
    }
}