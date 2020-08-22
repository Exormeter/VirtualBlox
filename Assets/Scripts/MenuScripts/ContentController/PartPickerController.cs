using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public GameObject CategorySelector;
        public ToggleGroup toggleGroup;
        public Hand hand;

        private bool wasInitialized = false;
        private int partContainerSize = 4;
        private string currentlyChoosen = "";
        private Color currentBlockColor = Color.red;
        private Dictionary<string, List<GameObject>> stringToPartcontrainer = new Dictionary<string, List<GameObject>>();

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
            stringToPartcontrainer.Add("PartFences", LoadSprites("PartFences"));
            stringToPartcontrainer.Add("PartWedges", LoadSprites("PartWedges"));
            stringToPartcontrainer.Add("PartCones", LoadSprites("PartCones"));
            stringToPartcontrainer.Add("PartSlopes", LoadSprites("PartSlopes"));
            stringToPartcontrainer.Add("PartCurves", LoadSprites("PartCurves"));

            foreach(Toggle categoryToggle in CategorySelector.transform.GetComponentsInChildren<Toggle>().ToList<Toggle>())
            {
                categoryToggle.onValueChanged.AddListener(delegate
                {
                    ToggleCategorySelected(categoryToggle);
                });
            }

            foreach(GameObject partContainer in stringToPartcontrainer["PartWedges"])
            {
                partContainer.transform.SetParent(PartListContent.transform);
                partContainer.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                partContainer.GetComponent<RectTransform>().localRotation = Quaternion.identity;
                partContainer.GetComponent<RectTransform>().localScale = new Vector3(2.5f, 2.5f, 1);
            }
            wasInitialized = true;
        }

        private void TogglePartSelected(Toggle toggle)
        {
            if (toggle.isOn)
            {
                currentlyChoosen = toggle.name;
            }

        }

        private void ToggleCategorySelected(Toggle toggle)
        {
            for (int i = PartListContent.transform.childCount - 1; i >= 0; i--)
            {
                PartListContent.transform.GetChild(i).gameObject.SetActive(false);
                PartListContent.transform.GetChild(i).SetParent(null);
            }


            foreach (GameObject partContainer in stringToPartcontrainer[toggle.name])
            {
                partContainer.SetActive(true);
                partContainer.transform.SetParent(PartListContent.transform);
                partContainer.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                partContainer.GetComponent<RectTransform>().localRotation = Quaternion.identity;
                partContainer.GetComponent<RectTransform>().localScale = new Vector3(2.5f, 2.5f, 1);
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

        public List<GameObject> LoadSprites(string folderName)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(folderName);
            List<GameObject> partContainerList = new List<GameObject>();

            for (int i = 0; i < sprites.Length; i += partContainerSize)
            {
                GameObject partContainer = Instantiate(ListPartPrecurser) as GameObject;

                PartContainerController partContainerController = partContainer.GetComponentInChildren<PartContainerController>();
                partContainerController.AddTogglesToGroup(toggleGroup);
                partContainerController.Toggles.ForEach(toggle => toggle.onValueChanged.AddListener(delegate {
                    TogglePartSelected(toggle);
                }));

                for (int j = 0; j < partContainerSize; j++)
                {
                    if(i + j < sprites.Length)
                    {
                        partContainerController.Containers[j].transform.GetChild(0).GetComponent<Image>().sprite = sprites[i + j];
                        partContainerController.Containers[j].name = sprites[i + j].name;
                    }
                    
                }
                partContainerList.Add(partContainer);
            }

            return partContainerList;
           
        }
    }
}

