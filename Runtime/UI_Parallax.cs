using System;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryEngine
{

    [System.Serializable]
    public struct UI_ParallaxLimit
    {
        public bool Enable;
        public Vector2 Horizontal;
        public Vector2 Vertical;
    }

    [RequireComponent(typeof(Image))]
    public class UI_Parallax : MonoBehaviour
    {
        public enum UI_EParallaxMode
        {
            None,
            Linear,
            Spherical,
        }

        [Header("Parallax")]
        public float Smooth = 1.0f;
        [Range(1.0f, 5.0f)]
        public float Sensibility = 1.0f;
        public bool InvertHorizontal = true;
        public bool InvertVertical = true;
        [SerializeField] public UI_ParallaxLimit Limits;
        public UI_EParallaxMode Mode = UI_EParallaxMode.Spherical;
        public bool AutoCenter = false;

        [SerializeField] string MouseX = "Mouse X";
        [SerializeField] string MouseY = "Mouse Y";

        protected RectTransform Transform;
        protected Image BackgoundComponent;
        protected string Log = "UI_Parallax: ";
        protected Vector2 MouseDelta;
        protected Vector3 Offset, OffsetLimitLess;
        protected Vector3 InitialPosition, TargetPosition;


        void Awake()
        {
            Transform = transform as RectTransform;
            BackgoundComponent = GetComponent<Image>();
            if (!BackgoundComponent)
                Debug.LogError(Log + "Background image component is null.");

            InitialPosition = transform.position;
            Offset = Vector3.zero;
            OffsetLimitLess = Vector3.zero;
        }

        void Update()
        {
            //Mouse
            Sensibility = Mathf.Clamp(Sensibility, 1.0f, float.MaxValue);
            float x = (Input.GetAxis(MouseX) * Sensibility) * (InvertHorizontal ? -1 : 1);
            float y = (Input.GetAxis(MouseY) * Sensibility) * (InvertVertical ? -1 : 1);
            MouseDelta = new Vector2(x, y);
            //Transform & Limits
            Offset.x = Mathf.Clamp(Offset.x + MouseDelta.x, Limits.Horizontal.x, Limits.Horizontal.y);
            Offset.y = Mathf.Clamp(Offset.y + MouseDelta.y, Limits.Vertical.x, Limits.Vertical.y);
            Offset.z = 0.0f;
            OffsetLimitLess += (Vector3)MouseDelta;
            TargetPosition = InitialPosition + (Limits.Enable ? Offset : OffsetLimitLess);
            //Lerp

            bool Validation = AutoCenter && MouseDelta == Vector2.zero;

            TargetPosition = (Validation) ? InitialPosition : TargetPosition;
            float Smooth = (Validation) ? this.Smooth / 2 : this.Smooth;

            switch (Mode)
            {
                case UI_EParallaxMode.Linear:
                    transform.position = Vector3.Lerp(transform.position, TargetPosition, Time.deltaTime * Smooth);
                    break;
                case UI_EParallaxMode.Spherical:
                    transform.position = Vector3.Slerp(transform.position, TargetPosition, Time.deltaTime * Smooth);
                    break;
                default:
                    if (!Validation)
                        transform.position = TargetPosition;
                    else
                        transform.position = Vector3.Slerp(
                            transform.position, InitialPosition, Time.deltaTime * Smooth);
                    break;
            }
        }

        private void OnEnable()
        {
            Offset = Vector3.zero;
            OffsetLimitLess = Vector3.zero;
        }

        private void OnDisable()
        {
            transform.position = InitialPosition;
        }
    }
}