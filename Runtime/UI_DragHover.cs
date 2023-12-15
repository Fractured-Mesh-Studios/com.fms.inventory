using UnityEngine;
using UnityEngine.UI;

namespace InventoryEngine
{
    public class UI_DragHover : Graphic
    {
        [Header("Hover")]
        [SerializeField] public Color valid = new Color(0, 1, 0, 0.35f);
        [SerializeField] public Color inValid = new Color(1, 0, 0, 0.35f);

        public Sprite icon
        {
            set { m_icon.sprite = value; }
            get { return m_icon.sprite; }
        }

        public new Color color
        {
            set { m_icon.color = value; }
            get { return m_icon.color; }
        }

        /*public RectTransform RectTransform
        {
            get { return transform as RectTransform; }
        }*/

        protected Image m_icon;
        protected string m_log = "UI_DragHover: ";

        protected override void Awake()
        {
            base.Awake();

            m_icon = GetComponent<Image>();
            if (!m_icon) m_icon = GetComponentInChildren<Image>();
            if (!m_icon) Debug.LogError(m_log + "Icon image component null.");
            if (m_icon)
            {
                m_icon.raycastTarget = false;
            }
        }

    }
}