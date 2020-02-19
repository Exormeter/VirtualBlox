using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace Valve.VR.InteractionSystem
{
    public class BlockCreateController : MonoBehaviour
    {
        public ColorToggleGroup colorToggleGroup;
        public BlockToggleGroup blockToggleGroup;
        public MatrixController matrixController;
        public Hand hand;
        
        private SteamVR_Input_Sources handInput;
        private BLOCKSIZE currentBlocksize = BLOCKSIZE.NORMAL;
        private Color currentBlockColor  = Color.red;
        private BlockGenerator blockGenerator;
        private readonly int frameUntilColliderReEvaluation = 2;
        private bool wasInitialized = false;

        // Start is called before the first frame update
        void Start()
        {
            if (!wasInitialized)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            handInput = hand.handType;
            //colorToggleGroup.OnChange += ColorOnChange;
            blockToggleGroup.OnChange += BlockOnChange;
            blockGenerator = GameObject.FindGameObjectWithTag("BlockGenerator").GetComponent<BlockGenerator>();
            wasInitialized = true;
        }

        private void OnEnable()
        {
            if (!wasInitialized)
            {
                Initialize();
            }
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void SpawnBlock()
        {
            List<BlockStructure> blockStructures = matrixController.GetStructures();
            if (blockStructures.Count == 0)
            {
                return;
            }
            BlockStructure blockStructure = blockStructures[0];
            blockStructure.BlockColor = currentBlockColor;
            blockStructure.BlockSize = currentBlocksize;
            GameObject spawnedBlock = blockGenerator.GenerateBlock(blockStructure);
            blockGenerator.AttachNewBlockToHand(spawnedBlock, hand);
        }

        public void ColorOnChange(Color blockColor)
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

