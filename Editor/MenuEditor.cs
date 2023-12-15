using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using InventoryEngine;

namespace InventoryEditor
{
    public class MenuEditor
    {
        [MenuItem("GameObject/Inventory/Slot", false, 1)]
        static void CreateSlot(MenuCommand menuCommand)
        {
            GameObject Canvas = GameObject.FindObjectOfType<Canvas>().gameObject;
            GameObject Context = (menuCommand.context) ? (GameObject)menuCommand.context : Canvas;

            GameObject Slot = new GameObject("Slot");
            GameObjectUtility.SetParentAndAlign(Slot, Context);
            Slot.AddComponent<UI_Slot>();
            Slot.AddComponent<UI_SlotEvent>();
            Slot.AddComponent<RectTransform>();

            (Slot.transform as RectTransform).anchorMin = new Vector2(0, 1);
            (Slot.transform as RectTransform).anchorMax = new Vector2(0, 1);
            (Slot.transform as RectTransform).pivot = new Vector2(0, 1);

            //Background
            GameObject Background = new GameObject("Background");
            GameObjectUtility.SetParentAndAlign(Background, Slot);
            Background.AddComponent<Image>();
            (Background.transform as RectTransform).anchorMin = Vector2.zero;
            (Background.transform as RectTransform).anchorMax = Vector2.one;

            //Icon
            GameObject Icon = new GameObject("Icon");
            GameObjectUtility.SetParentAndAlign(Icon, Slot);
            Icon.AddComponent<Image>();
            (Icon.transform as RectTransform).anchorMin = new Vector2(0.5f,0.5f);
            (Icon.transform as RectTransform).anchorMax = new Vector2(0.5f, 0.5f);
            (Icon.transform as RectTransform).pivot = new Vector2(0.5f, 0.5f);

            Debug.LogWarning(Slot.name + " created checks the status of the components and bind them!");
            Undo.RegisterCreatedObjectUndo(Slot, "Create " + Slot.name);
            Selection.activeObject = Slot;
        }

        [MenuItem("GameObject/Inventory/Container", false, 2)]
        static void CreateContainer(MenuCommand menuCommand)
        {
            GameObject Canvas = GameObject.FindObjectOfType<Canvas>().gameObject;
            GameObject Context = (menuCommand.context) ? (GameObject)menuCommand.context : Canvas;

            GameObject Container = new GameObject("Container");
            GameObjectUtility.SetParentAndAlign(Container, Context);
            Container.AddComponent<RectTransform>();
            Container.AddComponent<UI_Container>();
            Container.AddComponent<UI_ContainerEvent>();
            Container.AddComponent<Image>();
            RectTransform ContainerTransform = Container.transform as RectTransform;
            ContainerTransform.sizeDelta = new Vector2(200, 200);

            //Grid
            GameObject Grid = new GameObject("Grid");
            GameObjectUtility.SetParentAndAlign(Grid, Container);
            Grid.AddComponent<DynamicGridLayout>();
            RectTransform GridTransform = Grid.transform as RectTransform;

            GridTransform.anchorMin = Vector2.zero;
            GridTransform.anchorMax = Vector2.one;
            GridTransform.pivot = Vector2.zero;
            GridTransform.anchoredPosition = Vector2.zero;
            GridTransform.sizeDelta = Vector2.zero;

            Debug.LogWarning(Container.name + " created checks the status of the components and bind them!");
            Undo.RegisterCreatedObjectUndo(Container, "Create " + Container.name);
            Selection.activeObject = Container;
        }
        
        [MenuItem("GameObject/Inventory/Drag", false, 20)]
        static void CreateDrag(MenuCommand menuCommand)
        {
            GameObject Canvas = GameObject.FindObjectOfType<Canvas>().gameObject;
            GameObject Drag = new GameObject("Drag");
            GameObjectUtility.SetParentAndAlign(Drag, Canvas);
            RectTransform Rect = Drag.AddComponent<RectTransform>();
            Drag.AddComponent<UI_Drag>();
            Drag.AddComponent<Image>();
            Rect.anchorMin = Rect.anchorMax = Vector2.zero;
            Rect.pivot = new Vector2(0, 1);

            Debug.Log(Drag.name + " created successfuly");
            Undo.RegisterCreatedObjectUndo(Drag, "Create " + Drag.name);
            Selection.activeObject = Drag;
        }

        [MenuItem("GameObject/Inventory/Hover", false, 21)]
        static void CreateHover(MenuCommand menuCommand)
        {
            GameObject Canvas = GameObject.FindObjectOfType<Canvas>().gameObject;
            GameObject Hover = new GameObject("Hover");
            GameObjectUtility.SetParentAndAlign(Hover, Canvas);
            RectTransform Rect = Hover.AddComponent<RectTransform>();
            Hover.AddComponent<UI_DragHover>();
            Hover.AddComponent<Image>();
            Rect.anchorMin = Rect.anchorMax = Vector2.zero;
            Rect.pivot = new Vector2(0, 1);

            Debug.LogWarning(Hover.name + " created checks the status of the components and bind them!");
            Undo.RegisterCreatedObjectUndo(Hover, "Create " + Hover.name);
            Selection.activeObject = Hover;
        }

        [MenuItem("GameObject/Inventory/Manager", false, 50)]
        static void CreateManager(MenuCommand menuCommand)
        {
            GameObject Manager = new GameObject("UI_Manager");
            GameObjectUtility.SetParentAndAlign(Manager, menuCommand.context as GameObject);
            Manager.AddComponent<UI_Manager>();

            Debug.LogWarning(Manager.name + " created checks the status of the components and bind them!");
            Undo.RegisterCreatedObjectUndo(Manager, "Create " + Manager.name);
            Selection.activeObject = Manager;
        }
    }
}