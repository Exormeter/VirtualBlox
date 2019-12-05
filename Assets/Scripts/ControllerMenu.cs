using System;
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
        public ColorToggleGroup toggleGroup;
        public GameObject pointer;

        
        public Hand hand;
        
        public SteamVR_Action_Boolean spawnBlockAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SpawnBlock");

        private SteamVR_Input_Sources handInput;
        private Canvas canvas;
        private List<List<GameObject>> matrix = new List<List<GameObject>>();
        private BLOCKSIZE currentBlocksize = BLOCKSIZE.NORMAL;
        private Color currentBlockColor = Color.gray;
        private BlockGenerator blockGenerator;
        private readonly int frameUntilColliderReEvaluation = 2;

        // Start is called before the first frame update
        void Start()
        {
            

            handInput = hand.handType;
            canvas = CanvasContainer.GetComponent<Canvas>();
            PopulateMatrix();
            blockGenerator = GameObject.FindGameObjectWithTag("BlockGenerator").GetComponent<BlockGenerator>();

            toggleGroup.OnChange += ColorOnChange;

            CanvasContainer.SetActive(false);

            
        }

        // Update is called once per frame
        void Update()
        {
            if (spawnBlockAction.GetStateDown(handInput))
            {
                if(hand.currentAttachedObject == null && hand.hoveringInteractable == null)
                {
                    List<BlockStructure> blockStructures = FindStructures();
                    StartCoroutine(AttachNewBlockToHand(blockGenerator.GenerateBlock(blockStructures[0])));
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

        private IEnumerator AttachNewBlockToHand(GameObject generatedBlock)
        {
            {
                for (int i = 0; i <= frameUntilColliderReEvaluation; i++)
                {
                    if (i == frameUntilColliderReEvaluation)
                    {

                        generatedBlock.transform.position = hand.objectAttachmentPoint.transform.position;
                        generatedBlock.GetComponent<BlockInteractable>().PhysicsAttach(hand, GrabTypes.Grip);
                    }
                    yield return new WaitForFixedUpdate();
                }

            }
        }

        private void ColorOnChange(Color blockColor)
        {
            currentBlockColor = blockColor;
        }

        public void OpenMenu(HANDSIDE hand)
        {
            CanvasContainer.SetActive(true);
        }

        public void CloseMenu(HANDSIDE hand)
        {
            CanvasContainer.SetActive(false);
            DeactivatePointer();
        }

        public void ActivatePointer(HANDSIDE hand)
        { 
            pointer.SetActive(true);
            pointer.transform.SetParent(gameObject.transform);

            if(hand == HANDSIDE.HAND_LEFT)
            {
                pointer.transform.localPosition = new Vector3(0.5f, 0, 0);
            }
            else
            {
                pointer.transform.localPosition = Vector3.zero;
            }
            pointer.transform.localRotation = Quaternion.identity;
        }

        public void DeactivatePointer()
        {
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

    public enum HANDSIDE
    {
        HAND_LEFT,
        HAND_RIGHT
    }
}

