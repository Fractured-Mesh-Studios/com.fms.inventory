using System;
using UnityEngine;

namespace InventoryEngine.Extra
{
    public class Inspect : MonoBehaviour
    {
        /// <summary>
        /// Template prefab for the main inspected object.
        /// </summary>
        [Header("General")]
        public GameObject Object = null;
        /// <summary>
        /// Template prefab for the main inspect camera rendering.
        /// </summary>
        public GameObject Camera = null;

        /// <summary>
        /// Start position of the root transfrom in world coordinates.
        /// </summary>
        public Vector3 StartPosition = Vector3.zero;
        [Range(0.001f, 10.0f)]
        public float Sensibility = 2.0f, ScrollSensibility = 2.0f;
        public float ZoomMin = 1.5f, ZoomMax = 8.0f;
        public float Smooth = 0.0f;
        public bool CanInput = true;
        public bool Layer = false;
        public int LayerIndex = -1;

        [Header("Raycast")]
        public bool Raycast = true;
        public float Radius = 0.2f;
        public LayerMask Mask;

        [Header("Axis")]
        public bool InvertHorizontal = false;
        public bool InvertVertical = false;

        [SerializeField]
        protected string MouseX = "Mouse X", MouseY = "Mouse Y";
        [SerializeField]
        protected KeyCode
            RotateKey = KeyCode.Mouse0,
            MoveKey = KeyCode.Mouse1,
            ResetKey = KeyCode.Mouse2;

        /// <summary>
        /// Inspected instantiated object of the system. (instantiated version of object prefab variable)
        /// only works if the system is active. otherwise will be null
        /// </summary>
        [HideInInspector] public GameObject Inspected { get { return ObjectComponent; } }
        /// <summary>
        /// The main root transform of the system.
        /// </summary>
        [HideInInspector] public Transform Root;

        protected Transform Pivot;
        protected Camera CameraComponent, MainCamera;
        protected GameObject ObjectComponent = null;
        protected Vector3 MouseDelta, Rotation, Position;
        protected RaycastHit Hit;
        protected string Log = "UI_Inspect: ";
        protected bool IsOpen;
        protected float ZoomDelta, ZoomTarget, ZoomDistance, ZoomSpeed;


        void Update()
        {
            if (CameraComponent && IsOpen && CanInput)
            {
                ZoomDelta -= Input.mouseScrollDelta.y * ScrollSensibility;
                ZoomDelta = Mathf.Clamp(ZoomDelta, ZoomMin, ZoomMax);

                if (Input.GetKey(MoveKey))
                {
                    float x = Input.GetAxis(MouseX) * Sensibility;
                    float y = Input.GetAxis(MouseY) * Sensibility;
                    MouseDelta = new Vector3(0, y, x);
                    Position += MouseDelta * Time.deltaTime;
                }

                if (Input.GetKey(RotateKey))
                {
                    int h = InvertHorizontal ? -1 : 1;
                    int v = InvertVertical ? -1 : 1;
                    float x = Input.GetAxis(MouseX) * Sensibility * h;
                    float y = Input.GetAxis(MouseY) * Sensibility * v;
                    MouseDelta = new Vector3(y, x, 0.0f);
                    Rotation += MouseDelta;
                }

                if (Input.GetKeyDown(ResetKey))
                {
                    Reset();
                }

                if (Raycast)
                {
                    float Length = 5.0f;
                    float TraceLength = Length + 0.1f;
                    Vector3 TraceStart = -CameraComponent.transform.forward * Length;
                    Vector3 TraceDirection = CameraComponent.transform.forward * 2;
                    if (Physics.SphereCast(TraceStart, Radius, TraceDirection, out Hit, TraceLength, Mask))
                    {
                        ZoomTarget = (Hit.point.z > 0) ? Hit.point.z : -Hit.point.z;
                        ZoomSpeed = 2.0f;
                    }
                    else
                    {
                        ZoomTarget = Mathf.Lerp(ZoomTarget, TraceLength - Length, Time.deltaTime * 0.85f);
                        ZoomSpeed = 0.5f;
                    }
                }
                else { ZoomTarget = 0.0f; ZoomSpeed = 1.5f; }

                ZoomDistance = Mathf.Lerp(ZoomDistance, ZoomTarget, Time.deltaTime * ZoomSpeed);
                CameraComponent.transform.localPosition = new Vector3(0.0f, 0.0f, ZoomDelta + ZoomDistance);

                Smooth = Mathf.Abs(Smooth);
                if (Smooth > 0.0f)
                {
                    Pivot.rotation = Quaternion.Slerp(Pivot.rotation, Quaternion.Euler(Rotation), Time.deltaTime * Smooth);
                    Pivot.localPosition = Vector3.Lerp(Pivot.localPosition, Position, Time.deltaTime * Smooth);
                }
                else
                {
                    Pivot.position = Position;
                    Pivot.rotation = Quaternion.Euler(Rotation);
                }
                CameraComponent.transform.LookAt(Pivot);
            }
        }

        //public members
        /// <summary>
        /// Open the inspect system
        /// </summary>
        public void Open()
        {
            if (!Object || !Camera)
            {
                Debug.LogError(Log + "Object <" + Object + ">");
                Debug.LogError(Log + "Camera <" + Camera + ">");
                return;
            }

            Root = new GameObject("Root").transform;
            Pivot = new GameObject("Pivot").transform;
            Pivot.SetParent(Root);
            Root.position = StartPosition;
            CameraComponent = Instantiate(Camera, Pivot).GetComponent<Camera>();
            ObjectComponent = Instantiate(Object, Root);

            if (Layer)
            {
                GameObject Child = null;
                ObjectComponent.layer = LayerIndex;
                Root.gameObject.layer = LayerIndex;
                Pivot.gameObject.layer = LayerIndex;
                CameraComponent.gameObject.layer = LayerIndex;
                for (int i = 0; i < ObjectComponent.transform.childCount; i++)
                {
                    Child = ObjectComponent.transform.GetChild(i).gameObject;
                    Child.layer = LayerIndex;
                }
            }

            MainCamera = UnityEngine.Camera.main;
            UnityEngine.Camera.main.gameObject.SetActive(false);
            UnityEngine.Camera.SetupCurrent(CameraComponent);
            IsOpen = (Root && CameraComponent && ObjectComponent);

            ZoomMax = Mathf.Abs(ZoomMax);
            ZoomMin = Mathf.Abs(ZoomMin);
            if (ZoomMax < ZoomMin)
            {
                float Old = ZoomMax;
                ZoomMax = ZoomMin;
                ZoomMin = Old;
            }

            Reset();
        }

        /// <summary>
        /// Close the inspect system
        /// </summary>
        public void Close()
        {
            if (!Root || !CameraComponent || !CameraComponent)
            {
                Debug.LogWarning(Log + "Inspect must be open first.");
                return;
            }

            Destroy(CameraComponent.gameObject);
            Destroy(Pivot.gameObject);
            Destroy(ObjectComponent);
            Destroy(Root.gameObject);

            MainCamera.gameObject.SetActive(true);
            UnityEngine.Camera.main.gameObject.SetActive(true);
            UnityEngine.Camera.SetupCurrent(UnityEngine.Camera.main);
            IsOpen = (!Root && !CameraComponent && !ObjectComponent);
        }

        /// <summary>
        /// Reset the camera position and rotation of the inspect system
        /// (this only occurs if the system is active and enabled)
        /// </summary>
        public void Reset()
        {
            //Pivot.position = Vector3.zero;
            //Pivot.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
            Position = Vector3.zero;
            Rotation = new Vector3(0, 90, 0);
            ZoomDelta = Mathf.Lerp(ZoomMin, ZoomMax, 0.5f);
        }
    }
}