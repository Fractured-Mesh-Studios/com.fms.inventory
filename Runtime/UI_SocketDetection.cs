using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryEngine.Extra
{
    public class UI_SocketDetection : MonoBehaviour
    {
        [Header("Detection")]
        [Tooltip("Target object to search for sockets (self if not set)")]
        public GameObject Target = null;
        [Tooltip("Filter sockets by their tag")]
        public string Filter = string.Empty;
        [Tooltip("Socket interface prefab")]
        public GameObject Socket = null;

        [Header("Interpolation")]
        public bool Lerp = false;
        public float Smooth = 40.0f;

        [HideInInspector] public RectTransform Transform;

        protected Camera CameraComponent;
        protected Socket[] Nodes;
        protected UI_Socket[] UiNodes;
        protected string Log = "UI_SocketDetection: ";

        protected virtual void Awake()
        {
            Transform = transform as RectTransform;
        }

        protected virtual void Update()
        {
            CameraComponent = Camera.current ? Camera.current : CameraComponent;

            if (UiNodes != null)
            {
                Vector3 WorldPosition, ScreenPosition;
                for (int i = 0; i < UiNodes.Length; i++)
                {
                    WorldPosition = UiNodes[i].Socket.transform.position;
                    ScreenPosition = CameraComponent.WorldToScreenPoint(WorldPosition);
                    if (Lerp)
                    {
                        Vector3 Position = UiNodes[i].Transform.anchoredPosition;
                        UiNodes[i].Transform.anchoredPosition = Vector3.Lerp(Position, ScreenPosition, Time.deltaTime * Smooth);
                    }
                    else
                        UiNodes[i].Transform.anchoredPosition = ScreenPosition;
                }
            }
        }

        /// <summary>
        /// Function called when the detection system starts
        /// this function is internaly called not accessible outside of the class
        /// </summary>
        protected virtual void OnDetect()
        {
            List<Socket> Cache = new List<Socket>();
            Target = (Target) ? Target : FindObjectOfType<Inspect>().gameObject;

            Inspect InspectComponent = Target.GetComponent<Inspect>();
            if (InspectComponent)
            {
                Debug.Log(Log + "Inspect System Detected");
                Nodes = InspectComponent.Inspected.GetComponentsInChildren<Socket>();
            }
            else
            {
                Debug.Log(Log + "Standalone System Detected");
                Nodes = Target.GetComponentsInChildren<Socket>();
            }

            if (Nodes == null)
                Debug.LogError(Log + "The inspected object lacks sockets to be detected by the system");
            else
            {
                Debug.Log(Log + "Nodes Detected: <" + Nodes.Length + ">");
                for (int i = 0; i < Nodes.Length; i++)
                {
                    if (Nodes[i].tag == Filter)
                    {
                        Cache.Add(Nodes[i]);
                    }
                }
                Nodes = (string.IsNullOrWhiteSpace(Filter)) ? Nodes : Cache.ToArray();

                //Canvas Canvas = FindObjectOfType<Canvas>();
                UiNodes = new UI_Socket[Nodes.Length];
                for (int i = 0; i < Nodes.Length; i++)
                {
                    UiNodes[i] = Instantiate(Socket, Transform).GetComponent<UI_Socket>();
                    UiNodes[i].Socket = Nodes[i];
                    UiNodes[i].name = "Socket[" + i + "]";
                }
            }

        }

        /// <summary>
        /// Initialize the detection system for the current available inspect object.
        /// if no one is detected this object is used to get the sockets.
        /// </summary>
        /// <param name="Delay">Time it takes to invoke the detection function (in seconds)</param>
        public virtual void Detect(float Delay = 0.1f)
        {
            Invoke("OnDetect", Delay);
        }

        /// <summary>
        /// Removes all spawned nodes on the object.
        /// </summary>
        public virtual void Clear()
        {
            if (UiNodes == null) return;

            for (int i = 0; i < UiNodes.Length; i++)
            {
                Destroy(UiNodes[i].gameObject);
            }

            Nodes = null;
            UiNodes = null;
        }

        /// <summary>
        /// Get the selected socket if the index is valid.
        /// </summary>
        /// <param name="Index">index of the socket to search for</param>
        /// <returns></returns>
        public virtual Socket this[int Index]
        {
            set { Nodes[Index] = value; }
            get { return Nodes[Index]; }
        }
    }
}