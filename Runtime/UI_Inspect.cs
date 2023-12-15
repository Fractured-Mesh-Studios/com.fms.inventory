using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// The class bust be rendered to a full viewport canvas otherwise the scale and the position of the 
/// items inside of the inspected element are incorrectly transformed to screen space.
/// </summary>
public class UI_Inspect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Inspect")]
    [Tooltip("targetObject to inspect (prefab)")] public GameObject targetObject;
    [Tooltip("camera to render (prefab)")] public new GameObject camera;
    [Tooltip("targetObject to lookat (root)")] public Transform target;
    [Range(0.01f, 5.0f)] public float sensibility = 1.0f;
    [Range(0.01f, 5.0f)] public float scrollSensibility = 1.0f;

    [Header("Zoom")]
    public float zoomMin = -8;
    public float ZoomMax = -2;

    [Header("Zoom raycast")]
    [Tooltip("[Activates / Deactivates] The automatic adjustment of the camera by impacts to the targetObject")]
    public bool raycast = false;
    [Tooltip("radius of the raycast for targetObject detection")]
    public float radius = 0.2f;
    public LayerMask Mask;

    [Header("Axis & Actions")]
    [SerializeField] protected string mouseX = "Mouse X";
    [SerializeField] protected string mouseY = "Mouse Y";
    [SerializeField] protected KeyCode rotateKey = KeyCode.Mouse0;
    [SerializeField] protected KeyCode resetKey = KeyCode.Mouse2;

    protected GameObject m_object;
    protected Camera m_camera;
    protected Transform m_pivot;
    protected string m_log = "UI_Inspect: ";
    protected RaycastHit m_raycast;
    private Vector3 m_mouseDelta;
    private Vector3 m_rotation;
    private float m_zoomDelta,m_zoomDistance,m_zoomTarget,m_zoomSpeed;
    private bool m_mouseOver = false;

    void Awake()
    {
        if (!target) {
            Debug.LogError(m_log + "target transform not found.");
            Debug.LogError(m_log + "target transform must be the root targetObject.");
        }

        if (!targetObject) Debug.LogError(m_log + "targetObject (prefab) to inspect cant be null.");
        if (!camera) Debug.LogError(m_log + "camera to render inspected targetObject cant be null."); 
    }

    void Update()
    {
        m_zoomDelta += Input.mouseScrollDelta.y * scrollSensibility;
        m_zoomDelta = Mathf.Clamp(m_zoomDelta, zoomMin, ZoomMax);

        if (Input.GetKey(rotateKey) && m_mouseOver)
        {
            float x = Input.GetAxis(mouseX) * sensibility;
            float y = -Input.GetAxis(mouseY) * sensibility;
            m_mouseDelta = new Vector3(y, x, 0.0f);
            m_rotation += m_mouseDelta;

            m_pivot.rotation = Quaternion.Euler(m_rotation);
        }

        if (Input.GetKeyDown(resetKey))
        {
            m_pivot.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
            m_rotation = new Vector3(0, 90, 0);
            m_zoomDelta = -(Mathf.Abs(zoomMin) + Mathf.Abs(ZoomMax) / 2);
        }
        
        if (raycast)
        {
            float Length = 5.0f;
            float TraceLength = Length + 0.3f;
            Vector3 TraceStart = -m_camera.transform.forward * Length;
            Vector3 TraceDirection = m_camera.transform.forward * 2;
            if (Physics.SphereCast(TraceStart, radius, TraceDirection, out m_raycast, TraceLength, Mask))
            {
                m_zoomTarget = (m_raycast.point.z > 0) ? -m_raycast.point.z : m_raycast.point.z;
                m_zoomSpeed = 2.0f;
            }
            else {
                m_zoomTarget = Mathf.Lerp(m_zoomTarget, TraceLength - Length, Time.deltaTime * 0.85f);
                m_zoomSpeed = 0.5f;
            }
        } else { m_zoomTarget = 0.0f; m_zoomSpeed = 1.5f; }
       
        m_zoomDistance = Mathf.Lerp(m_zoomDistance, m_zoomTarget, Time.deltaTime * m_zoomSpeed);

        m_camera.transform.localPosition = new Vector3(0.0f, 0.0f, m_zoomDelta + m_zoomDistance);
        m_camera.transform.LookAt(target);
    }

    private void OnEnable()
    {
        m_object = Instantiate(targetObject, target);
        m_camera = Instantiate(camera, target).GetComponent<camera>();
        m_pivot = new GameObject("m_pivot").transform;
        m_pivot.SetParent(target);
        m_pivot.localPosition = Vector3.zero;
        m_camera.transform.SetParent(m_pivot);
        m_camera.transform.position = new Vector3(0, 0, -3.5f);
    }

    private void OnDisable()
    {
        Destroy(m_object);
        Destroy(m_camera);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_mouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_mouseOver = false;
    }
}
