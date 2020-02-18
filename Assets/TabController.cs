using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem
{
    public class TabController : MonoBehaviour
    {
        private GameObject currentActiveMenu;
        public GameObject ParentCanvas;
        public GameObject TabSaveScene;
        public GameObject TabBlockCreate;
        public GameObject TabSavePrefab;
        public GameObject pointer;
        public GameObject pointerOrigin;

        void Start()
        {
            currentActiveMenu = TabBlockCreate;
            ParentCanvas.SetActive(false);
        }

        public void OpenTabBlockCreate()
        {
            TabBlockCreate.SetActive(true);
            TabSavePrefab.SetActive(false);
            TabSaveScene.SetActive(false);
            currentActiveMenu = TabBlockCreate;
        }

        public void OpenTabSaveScene()
        {
            TabBlockCreate.SetActive(false);
            TabSavePrefab.SetActive(true);
            TabSaveScene.SetActive(false);
            currentActiveMenu = TabSaveScene;
        }

        public void OpenTabSavePrefab()
        {
            TabBlockCreate.SetActive(false);
            TabSavePrefab.SetActive(false);
            TabSaveScene.SetActive(true);
            currentActiveMenu = TabSavePrefab;
        }


        public void ActivateControllerMenu()
        {
            ParentCanvas.SetActive(true);
            currentActiveMenu.SetActive(true);
        }

        public void DeactivateControllerMenu()
        {
            currentActiveMenu.SetActive(false);
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

