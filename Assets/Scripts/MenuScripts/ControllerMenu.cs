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
        public ColorToggleGroup colorToggleGroup;
        public BlockToggleGroup blockToggleGroup;
        public MatrixController matrixController;
        public Hand hand;
        
        public SteamVR_Action_Boolean spawnBlockAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SpawnBlock");

        private SteamVR_Input_Sources handInput;
        private BLOCKSIZE currentBlocksize = BLOCKSIZE.NORMAL;
        private Color currentBlockColor  = Color.red;
        private BlockGenerator blockGenerator;
        private readonly int frameUntilColliderReEvaluation = 2;

        // Start is called before the first frame update
        void Start()
        {
            handInput = hand.handType;
            colorToggleGroup.OnChange += ColorOnChange;
            blockToggleGroup.OnChange += BlockOnChange;
            blockGenerator = GameObject.FindGameObjectWithTag("BlockGenerator").GetComponent<BlockGenerator>();
        }

        // Update is called once per frame
        void Update()
        {
            if (spawnBlockAction.GetStateDown(handInput))
            {
                if(hand.currentAttachedObject == null && hand.hoveringInteractable == null)
                {
                    List<BlockStructure> blockStructures = matrixController.GetStructures();
                    if(blockStructures.Count == 0)
                    {
                        return;
                    }
                    BlockStructure blockStructure = blockStructures[0];
                    blockStructure.BlockColor = currentBlockColor;
                    blockStructure.BlockSize = currentBlocksize;
                    StartCoroutine(AttachNewBlockToHand(blockGenerator.GenerateBlock(blockStructure)));
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
        HAND_RIGHT,
        HAND_NONE
    }
}

