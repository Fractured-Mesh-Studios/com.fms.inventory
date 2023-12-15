using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InventoryEngine
{
    public class UI_ContextMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject Button;
        public float TimeToLive = 0.4f;
        public bool UseFlags = false;
        public BindingFlags Flags = BindingFlags.DeclaredOnly | BindingFlags.NonPublic;

        [HideInInspector] protected List<Button> Data = new List<Button>();

        protected LayoutGroup LayoutGroup;
        protected string Log = "UI_ContextMenu: ";
        protected bool MouseOver;
        protected RectTransform RectTransform;

        //EngineMethods
        protected void Awake()
        {
            RectTransform = transform as RectTransform;
            LayoutGroup = GetComponentInChildren<LayoutGroup>();
            if (!LayoutGroup)
                UI_Debug.LogError(Log + "Layout Group component has not found.");

            transform.position = Input.mousePosition;
        }

        protected void Destroy()
        {
            Destroy(gameObject);
        }
        //EngineMethods End

        //Method
        /// <summary>
        /// We add a method from a class by its name. (all derived scripts from behaviour)
        /// </summary>
        /// <param name="Class">class to search the method</param>
        /// <param name="Name">method name (exactly)</param>
        /// <param name="DisplayName">method display name (not requiered)</param>
        /// <returns></returns>
        public bool AddMethod(Behaviour Class, string Name, string DisplayName = "", int Category = -1)
        {
            MethodInfo Method = (!UseFlags) ?
                Class.GetType().GetMethod(Name) : Class.GetType().GetMethod(Name, Flags);
            if (Method == null)
            {
                UI_Debug.LogWarning(Log + "Method Named <" + Name + "> has not been found.");
                UI_Debug.LogWarning(Log + "On Selected Class [" + Class.GetType().Name + "]. ");
                return false;
            }
            Transform LayoutTransform = LayoutGroup.transform;
            Button Cache = Instantiate(Button, LayoutTransform).GetComponent<Button>();
            Action MethodAction = (Action)Delegate.CreateDelegate(typeof(Action), Class, Method);
            UnityAction UnityAction = new UnityAction(MethodAction);
            Cache.onClick.AddListener(UnityAction);
            Cache.onClick.AddListener(Destroy);
            Cache.name = Method.Name;
            Text CacheText = Cache.GetComponentInChildren<Text>();
            if (CacheText)
            {
                string DisplayCacheName = (string.IsNullOrWhiteSpace(DisplayName)) ? Cache.name : DisplayName;
                CacheText.text = DisplayCacheName;
            }

            Data.Add(Cache);
            UI_Debug.Log(Log + "Method Added.");

            return true;
        }

        /// <summary>
        /// We remove a method from the list by its name.
        /// </summary>
        /// <param name="Name">name of the method</param>
        public void RemoveMethod(string Name)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i] && Data[i].name == Name)
                    Data.Remove(Data[i]);
            }
        }

        /// <summary>
        /// Clear all methods list and reset the system.
        /// </summary>
        public void ClearMethods()
        {
            for (int i = 0; i < Data.Count; i++)
            {
                Destroy(Data[i].gameObject);
            }
            Data.Clear();
        }

        /// <summary>
        /// Get the selected method by index from the list.
        /// </summary>
        /// <param name="index">index of the method</param>
        /// <returns></returns>
        public Button GetMethod(int index)
        {
            if (index >= 0 && index < Data.Count)
            {
                return Data[index];
            }

            return null;
        }

        /// <summary>
        /// Get the selected method by name from the list.
        /// </summary>
        /// <param name="Name">name of the method</param>
        /// <returns></returns>
        public Button GetMethod(string Name)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i] && Data[i].name == Name)
                    return Data[i];
            }

            return null;
        }
        //MethodEnd

        //Separator
        /// <summary>
        /// Add a separator at the end of the index order in the layout group.
        /// </summary>
        /// <param name="Object">prefab object to instantiate</param>
        /// <param name="Value">index(id) of the separator</param>
        /// <returns></returns>
        public bool AddSeparator(GameObject Object, int Value)
        {
            GameObject Cache = Instantiate(Object, LayoutGroup.transform);
            if (!Cache) return false;

            Cache.transform.SetAsLastSibling();
            Cache.name = "Separator " + Value;

            return true;
        }

        /// <summary>
        /// Remove separator from the current layout group by passing its index(id).
        /// </summary>
        /// <param name="Value">index(id) of the separator</param>
        public void RemoveSeparator(int Value)
        {
            GameObject Obj = LayoutGroup.transform.Find("Separator " + Value).gameObject;
            if (Obj) Destroy(Obj); else UI_Debug.Log(Log + "Separator not found on current menu.");
        }
        //SeparatorEnd

        //PointerEvents
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            MouseOver = false;
            Invoke("Destroy", TimeToLive);
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            MouseOver = true;
            if (IsInvoking())
                CancelInvoke();
        }
        //PointerEvents End
    }
}