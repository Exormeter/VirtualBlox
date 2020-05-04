using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem
{
    public class TabController : MonoBehaviour
    {
        private TabContentController currentActiveMenu;

        public GameObject ParentCanvas;
        public GameObject pointer;
        public GameObject pointerOrigin;

        private List<TabContentController> tabContentControllers;
        void Start()
        {
            
            ParentCanvas.SetActive(false);

            TabContentController[] contentControllers = GetComponentsInChildren<TabContentController>(true) as TabContentController[];
            foreach(TabContentController tabContentController in contentControllers)
            {
                tabContentController.DeactivateContent();
                EventTrigger eventTrigger = tabContentController.gameObject.GetComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerDown;
                entry.callback.AddListener((eventData) => { ToggleActiveState(tabContentController); });
                eventTrigger.triggers.Add(entry);
            }

            tabContentControllers = new List<TabContentController>(contentControllers);
            currentActiveMenu = tabContentControllers[0];
        }

        public void ToggleActiveState(TabContentController contentController)
        {
            foreach(TabContentController tabContentController in tabContentControllers)
            {
                tabContentController.DeactivateContent();
            }
            contentController.ActivateContent();
            currentActiveMenu = contentController;
        }

        public void ActivateControllerMenu()
        {
            ParentCanvas.SetActive(true);
            currentActiveMenu.ActivateContent();
        }

        public void DeactivateControllerMenu()
        {
            foreach(TabContentController tabContentController in tabContentControllers)
            {
                tabContentController.CloseMenu();
            }
            ParentCanvas.SetActive(false);
        }

        public void ActivatePointer()
        {
            pointer.SetActive(true);
            pointer.transform.SetParent(pointerOrigin.transform);
            pointer.transform.localPosition = Vector3.zero;
            pointer.transform.localRotation = Quaternion.identity;
        }

        public void DeactivatePointer()
        {
            pointer.SetActive(false);
        }
    }
}

