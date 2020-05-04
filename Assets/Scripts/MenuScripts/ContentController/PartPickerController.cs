using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem
{
    public class PartPickerController : ContentController
    {
        public BlockGenerator BlockGenerator;
        public BlockManager BlockManager;
        public GameObject ListPartPrecurser;
        public GameObject PartListContent;
        public ToggleGroup toggleGroup;
        public Hand hand;

        private bool wasInitialized = false;
        private int partContainerSize = 7;
        private string currentlyChoosen = "";
        private Color currentBlockColor = Color.red;

        void Start()
        {
            if (!wasInitialized)
            {
                Initialize();
            }
        }

        private void OnEnable()
        {
            if (!wasInitialized)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>("LegoParts");
            
            for(int i = 0; i < sprites.Length; i += partContainerSize)
            {
                GameObject partContainer = Instantiate(ListPartPrecurser) as GameObject;
                
                PartContainerController partContainerController = partContainer.GetComponentInChildren<PartContainerController>();
                partContainerController.AddTogglesToGroup(toggleGroup);
                partContainerController.Toggles.ForEach(toggle => toggle.onValueChanged.AddListener(delegate {
                    ToggleValueChanged(toggle);
                }));

                for(int j = 0; j < partContainerSize; j++)
                {
                    partContainerController.Containers[j].transform.GetChild(0).GetComponent<Image>().sprite = sprites[i + j];
                    partContainerController.Containers[j].name = sprites[i + j].name;
                }
                partContainer.transform.SetParent(PartListContent.transform);
                partContainer.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                partContainer.GetComponent<RectTransform>().localRotation = Quaternion.identity;
                partContainer.GetComponent<RectTransform>().localScale = new Vector3(2.5f, 2.5f, 1);
            }
            wasInitialized = true;
        }

        private void ToggleValueChanged(Toggle toggle)
        {
            if (toggle.isOn)
            {
                currentlyChoosen = toggle.name;
            }
            
        }

        public void SpawnBlock()
        {   
            if(gameObject.activeSelf || WasLastActive)
            {
                GameObject spawnedBlock = BlockGenerator.GenerateBlock(new LDrawBlockStructure(currentlyChoosen, currentBlockColor));
                BlockGenerator.AttachNewBlockToHand(spawnedBlock, hand);
            }
            
        }

        public void ColorOnChange(Color blockColor)
        {
            currentBlockColor = blockColor;
        }
    }
}

