using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryEngine
{
    public class UI_SlotProgressBar : MonoBehaviour
    {
        [Header("General")]
        public bool HideFull;
        public float MinValue;
        public float MaxValue;

        [Header("Color")]
        public Color MinColor = Color.gray;
        public Color MaxColor = Color.white;

        [Header("Components")]
        [SerializeField] protected Image BackgroundComponent = null;
        [SerializeField] protected Image FillComponent = null;
        [SerializeField] protected Text ValueComponent = null;

        protected UI_Slot SlotComponent;
        protected string Log = "UI_SlotProgressBar: ";
        protected float Value, Ratio;

        void Awake()
        {
            SlotComponent = GetComponent<UI_Slot>();
            if (!SlotComponent) SlotComponent = GetComponentInParent<UI_Slot>();
            if (!SlotComponent) SlotComponent = GetComponentInParent<UI_Slot>();
            if (!SlotComponent) UI_Debug.LogError(Log + "Slot component is null.");

            if (!BackgroundComponent) UI_Debug.LogError(Log + "Background image component is null.");
            else
            {
                BackgroundComponent.raycastTarget = false;
                BackgroundComponent.enabled = enabled;
            }
            if (!FillComponent) UI_Debug.LogError(Log + "Fill image component is null.");
            else {
                FillComponent.raycastTarget = false;
                FillComponent.enabled = enabled;
            }
            if (!ValueComponent) UI_Debug.Log(Log + "Value text component is null.");
            else
            {
                ValueComponent.raycastTarget = false;
                ValueComponent.enabled = enabled;
            }

            if (!enabled)
                UI_Debug.LogError(Log + "Progress bar is inactive and disabled.");

        }
        
        void Update()
        {
            Value = Mathf.Clamp(Value, MinValue, MaxValue);
            Ratio = Mathf.Clamp01(Value / MaxValue);

            if(HideFull)
            {
                BackgroundComponent.enabled = Value == MaxValue;
                FillComponent.enabled = Value == MaxValue;
                if (ValueComponent)
                    ValueComponent.enabled = Value == MaxValue;
            }

            if (ValueComponent)
            {
                ValueComponent.text = (Ratio * 100) + "%";
            }

            FillComponent.color = Color.Lerp(MinColor, MaxColor, Ratio);
        }
    }

}
