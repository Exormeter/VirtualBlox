using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem
{
    public class TabController : MonoBehaviour
    {
        private ToggleGroup tabGroup;
        private Canvas tabCanvas;
        private Canvas currentActiveCanvas;
        public Canvas TabSaveScene;
        public Canvas TabBlockCreate;
        public Canvas TabSavePrefab;
        public GameObject pointer;
        public GameObject pointerOrigin;
        

        void Start()
        {
            if(TabBlockCreate != null)
            {
                TabBlockCreate.enabled = false;
                TabSavePrefab.enabled = false;
                TabSaveScene.enabled = false;
                currentActiveCanvas = TabBlockCreate;
                tabCanvas = GetComponent<Canvas>();
                tabGroup = GetComponent<ToggleGroup>();
                tabCanvas.enabled = false;
            }
            
        }

        
        void Update()
        {

        }

        public void OpenTabBlockCreate()
        {
            TabBlockCreate.enabled = true;
            TabSavePrefab.enabled = false;
            TabSaveScene.enabled = false;
            currentActiveCanvas = TabBlockCreate;
        }

        public void OpenTabSaveScene()
        {
            TabBlockCreate.enabled = false;
            TabSavePrefab.enabled = false;
            TabSaveScene.enabled = true;
            currentActiveCanvas = TabSaveScene;
        }

        public void OpenTabSavePrefab()
        {
            TabBlockCreate.enabled = false;
            TabSavePrefab.enabled = true;
            TabSaveScene.enabled = false;
            currentActiveCanvas = TabSavePrefab;
        }

        public void OpenMenu(HANDSIDE hand)
        {
            tabCanvas.enabled = true;
            currentActiveCanvas.enabled = true;
        }

        public void CloseMenu(HANDSIDE hand)
        {
            tabCanvas.enabled = false;
            currentActiveCanvas.enabled = false;
            DeactivatePointer();
        }

        public void ActivatePointer(HANDSIDE handSide)
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

