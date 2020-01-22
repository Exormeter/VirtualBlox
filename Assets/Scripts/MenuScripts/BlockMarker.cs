using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{
    public class BlockMarker : MonoBehaviour
    {

        public BoxDrawer BoxDrawer;
        public GameObject HandLeft;
        public GameObject HandRight;
        public BlockGenerator BlockGenerator;
        public BlockManager BlockManager;

        public List<GameObject> markedBlocks = new List<GameObject>();

        private BoxCollider currentBoxCollider;
        void Start()
        {
            currentBoxCollider = gameObject.AddComponent<BoxCollider>();
            currentBoxCollider.enabled = false;
            currentBoxCollider.isTrigger = false;
        }

        
        void Update()
        {
            if (Input.anyKeyDown)
            {
                RebuildMarkedStructure(this.gameObject);
            }

        }

        public void MarkerPullingStarted(HANDSIDE handSide)
        {
            currentBoxCollider.enabled = false;
            ResetMarker();
            markedBlocks.Clear();
            switch (handSide)
            {
                case HANDSIDE.HAND_LEFT:
                    BoxDrawer.SetStartPosition(HandLeft.transform.position);
                    break;

                case HANDSIDE.HAND_RIGHT:
                    BoxDrawer.SetStartPosition(HandRight.transform.position);
                    break;
            }
        }

        public void MarkerPullingEnded(HANDSIDE handSide)
        {
            switch (handSide)
            {
                case HANDSIDE.HAND_LEFT:
                    BoxDrawer.GetBoxCollider(currentBoxCollider);
                    break;

                case HANDSIDE.HAND_RIGHT:
                    BoxDrawer.GetBoxCollider(currentBoxCollider);
                    break;
            }
        }

        public void MarkerIsPulled(HANDSIDE handSide)
        {
            currentBoxCollider.enabled = true;
            switch (handSide)
            {
                case HANDSIDE.HAND_LEFT:
                    BoxDrawer.GetBoxCollider(currentBoxCollider);
                    BoxDrawer.DrawCube(HandLeft.transform.position);
                    break;

                case HANDSIDE.HAND_RIGHT:
                    BoxDrawer.GetBoxCollider(currentBoxCollider);
                    BoxDrawer.DrawCube(HandRight.transform.position);
                    break;
            }
            
        }

        private void OnTriggerEnter(Collider other)
        {
            other.gameObject.SendMessageUpwards("OnMarkedBegin", SendMessageOptions.DontRequireReceiver);
            GameObject block = other.transform.root.gameObject;
            markedBlocks.Add(block);
        }

        private void OnTriggerExit(Collider other)
        {
            
            GameObject block = other.transform.root.gameObject;
            markedBlocks.Remove(block);
            if (!markedBlocks.Exists(tempBlock => tempBlock.GetHashCode() == block.GetHashCode()))
            {
                block.SendMessageUpwards("OnMarkedEnd", SendMessageOptions.DontRequireReceiver);
            }
        }

        private void ResetMarker()
        {
            foreach(GameObject block in markedBlocks)
            {
                block.SendMessageUpwards("OnMarkedEnd", SendMessageOptions.DontRequireReceiver);
            }
        }

        public GameObject RebuildMarkedStructure(GameObject grabbedBlock)
        {
            List<GameObject> distinctBlocks = markedBlocks.Distinct().ToList();
            Dictionary<Guid, Guid> copyToOriginalAssign = new Dictionary<Guid, Guid>();
            foreach(GameObject block in distinctBlocks)
            {
                GameObject copiedBlock = BlockGenerator.GenerateBlock(block.GetComponent<BlockGeometryScript>().blockStructure);
                copiedBlock.transform.rotation = block.transform.rotation;
                copiedBlock.transform.position = block.transform.position;
                //copiedBlock.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                copiedBlock.GetComponent<Rigidbody>().useGravity = false;
                copiedBlock.GetComponent<BlockGeometryScript>().SetWallColliderTrigger(true);
                //copiedBlock.GetComponent<Interactable>().enabled = false;
                //copiedBlock.GetComponent<BlockInteractable>().enabled = false;
                copyToOriginalAssign.Add(block.GetComponent<BlockCommunication>().Guid, copiedBlock.GetComponent<BlockCommunication>().Guid);
            }

            foreach (GameObject block in distinctBlocks)
            {
                GameObject copiedBlock = BlockManager.GetBlockByGuid(copyToOriginalAssign[block.GetComponent<BlockCommunication>().Guid]);

                foreach (BlockContainer blockContainer in block.GetComponent<BlockCommunication>().ConnectedBlocks)
                {
                    if (distinctBlocks.Exists(tempBlock => blockContainer.BlockRootObject.GetHashCode() == tempBlock.GetHashCode())){
                        Guid copiedBlockGuid = copyToOriginalAssign[blockContainer.Guid];
                        copiedBlock.GetComponent<BlockCommunication>().ConnectBlocks(copiedBlock, BlockManager.GetBlockByGuid(copiedBlockGuid), blockContainer.ConnectedPinCount, blockContainer.ConnectedOn);
                    }
                }
            }
            Guid grabbedBlockGuid = copyToOriginalAssign[grabbedBlock.GetComponent<BlockCommunication>().Guid];
            return BlockManager.GetBlockByGuid(grabbedBlockGuid);
        }
    }
}
