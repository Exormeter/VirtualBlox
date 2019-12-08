using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace Valve.VR.InteractionSystem
{
    public class ControllerMenu : MonoBehaviour
    {
        public GameObject CanvasContainer;
        public ColorToggleGroup toggleGroup;
        public BlockToggleGroup blockToggleGroup;
        public MatrixController matrixController;
        public GameObject pointer;

        
        public Hand hand;
        
        public SteamVR_Action_Boolean spawnBlockAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SpawnBlock");

        private SteamVR_Input_Sources handInput;
        private BLOCKSIZE currentBlocksize = BLOCKSIZE.NORMAL;
        private Color currentBlockColor = Color.gray;
        private BlockGenerator blockGenerator;
        private readonly int frameUntilColliderReEvaluation = 2;

        // Start is called before the first frame update
        void Start()
        {
            

            handInput = hand.handType;
           
            blockGenerator = GameObject.FindGameObjectWithTag("BlockGenerator").GetComponent<BlockGenerator>();

            toggleGroup.OnChange += ColorOnChange;
            blockToggleGroup.OnChange += BlockOnChange;

            CanvasContainer.SetActive(false);

            
        }

        // Update is called once per frame
        void Update()
        {
            if (spawnBlockAction.GetStateDown(handInput))
            {
                if(hand.currentAttachedObject == null && hand.hoveringInteractable == null)
                {
                    List<BlockStructure> blockStructures = matrixController.GetStructures();
                    BlockStructure blockStructure = blockStructures[0];
                    blockStructure.BlockColor = currentBlockColor;
                    blockStructure.BlockSize = currentBlocksize;
                    StartCoroutine(AttachNewBlockToHand(blockGenerator.GenerateBlock(blockStructure)));
                }
            }

            if (Input.GetKeyUp("space"))
            {
                List<BlockStructure> blockStructures = matrixController.GetStructures();
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

        private void BlockOnChange(BlockStructure structure)
        {
            matrixController.SetStructure(structure);
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

