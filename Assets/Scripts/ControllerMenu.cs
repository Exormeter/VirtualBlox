using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Valve.VR.InteractionSystem
{
    public class ControllerMenu : MonoBehaviour
    {

        
        public int Rows;
        public int Columns;
        public GameObject Toggle;
        public GameObject CanvasContainer;

        
        public Hand hand;
        
        public SteamVR_Action_Boolean spawnBlockAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SpawnBlock");

        [HideInInspector]
        public bool MenuOpen = false;

        private SteamVR_Input_Sources handInput;
        private Canvas canvas;
        private List<List<GameObject>> matrix = new List<List<GameObject>>();
        private ColorToggleGroup toggleGroup;
        private BLOCKSIZE currentBlocksize = BLOCKSIZE.NORMAL;
        private Color currentBlockColor = Color.gray;
        private BlockGenerator blockGenerator;

        // Start is called before the first frame update
        void Start()
        {
            PopulateMatrix();

            handInput = hand.handType;
            canvas = CanvasContainer.GetComponent<Canvas>();

            blockGenerator = GameObject.FindGameObjectWithTag("BlockGenerator").GetComponent<BlockGenerator>();
            
            //Change to own ColorChooser
            toggleGroup = GameObject.FindGameObjectWithTag("ColorChooser").GetComponent<ColorToggleGroup>();
            toggleGroup.OnChange += ColorOnChange;

            CanvasContainer.SetActive(false);

            
        }

        // Update is called once per frame
        void Update()
        {
            if (spawnBlockAction.GetStateDown(handInput))
            {

                List<BlockStructure> blockStructures = FindStructures();
                foreach (BlockStructure blockStructure in blockStructures)
                {
                    GameObject generatedBlock = blockGenerator.GenerateBlock(blockStructure);
                    generatedBlock.GetComponent<Rigidbody>().isKinematic = true;
                    hand.AttachObject(generatedBlock, GrabTypes.Grip, generatedBlock.GetComponent<BlockInteractable>().attachmentFlags);
                }
            }

            if (Input.GetKeyUp("space"))
            {
                List<BlockStructure> blockStructures = FindStructures();
                foreach (BlockStructure blockStructure in blockStructures)
                {
                    blockGenerator.GenerateBlock(blockStructure);
                }

            }
        }

        private void ColorOnChange(Color blockColor)
        {
            currentBlockColor = blockColor;
        }

        public void OpenMenu(SteamVR_Input_Sources hand)
        {
            CanvasContainer.SetActive(true);
        }

        public void CloseMenu(SteamVR_Input_Sources hand)
        {
            CanvasContainer.SetActive(false);
            DeactivatePointer();
        }

        public void ActivatePointer(SteamVR_Input_Sources hand)
        {
            GameObject pointer = GameObject.FindGameObjectWithTag("LaserPointer");
            pointer.SetActive(true);
            pointer.transform.SetParent(gameObject.transform);
        }

        public void DeactivatePointer()
        {
            GameObject pointer = GameObject.FindGameObjectWithTag("LaserPointer");
            pointer.SetActive(false);
        }


        private void PopulateMatrix()
        {
            for (int r = 0; r < Rows; r++)
            {
                matrix.Add(new List<GameObject>());
                for (int c = 0; c < Columns; c++)
                {

                    GameObject toggle = Instantiate(Toggle, Vector3.zero, Quaternion.identity, canvas.transform);
                    RectTransform rectTransfrom = toggle.GetComponent<RectTransform>();
                    rectTransfrom.localScale = new Vector3(2, 2, 1);
                    rectTransfrom.localPosition = Vector3.zero;
                    rectTransfrom.localRotation = Quaternion.identity;

                    Vector3 anchorPosition = new Vector3(c * rectTransfrom.sizeDelta.x, -r * rectTransfrom.sizeDelta.y, 0);

                    toggle.GetComponent<RectTransform>().anchoredPosition = anchorPosition;
                    matrix[r].Add(toggle);
                }
            }
        }

        private List<BlockStructure> FindStructures()
        {
            List<BlockStructure> blockStructures = new List<BlockStructure>();
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    Toggle toggle = matrix[row][col].GetComponent<Toggle>();
                    if (toggle.isOn && !matrix[row][col].tag.Equals("Visited"))
                    {
                        BlockStructure structure = new BlockStructure(Rows, Columns, currentBlocksize, currentBlockColor);
                        FindAdjacentNodes(structure, row, col);
                        blockStructures.Add(structure);
                    }
                }
            }
            ResetMatrix();
            return blockStructures;
        }

        private void FindAdjacentNodes(BlockStructure structure, int row, int column)
        {
            if (row >= matrix.Count || column >= matrix[0].Count || row < 0 || column < 0 || matrix[row][column].tag.Equals("Visited"))
                return;

            matrix[row][column].tag = "Visited";

            if (matrix[row][column].GetComponent<Toggle>().isOn)
            {
                structure.AddNode(new BlockPart(row, column), row, column);
                FindAdjacentNodes(structure, row, column - 1);
                FindAdjacentNodes(structure, row, column + 1);
                FindAdjacentNodes(structure, row - 1, column);
                FindAdjacentNodes(structure, row + 1, column);
            }
        }

        private void ResetMatrix()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    Toggle toggle = matrix[row][col].GetComponent<Toggle>();
                    matrix[row][col].tag = "Untagged";
                }
            }
        }

        public void ChangeBlockSize()
        {
            if(currentBlocksize == BLOCKSIZE.NORMAL)
            {
                currentBlocksize = BLOCKSIZE.FLAT;
            }
            else
            {
                currentBlocksize = BLOCKSIZE.NORMAL;
            }
        }

        
    }
}

