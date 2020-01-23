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
            currentBoxCollider.isTrigger = true;
        }

        
        void Update()
        {

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
            GameObject block = other.transform.root.gameObject;
            if (block.tag.Equals("Block"))
            {
                other.gameObject.SendMessageUpwards("OnMarkedBegin", SendMessageOptions.DontRequireReceiver);
                markedBlocks.Add(block);
            }
            
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
            List<GameObject> copiedBlocks = new List<GameObject>();
            Dictionary<Guid, Guid> copyToOriginalAssign = new Dictionary<Guid, Guid>();

            foreach(GameObject block in distinctBlocks)
            {
                GameObject copiedBlock = BlockGenerator.GenerateBlock(block.GetComponent<BlockGeometryScript>().blockStructure);
                copiedBlock.transform.rotation = block.transform.rotation;
                copiedBlock.transform.position = block.transform.position;
                copiedBlock.GetComponent<BlockGeometryScript>().SetWallColliderTrigger(true);
                copiedBlock.GetComponent<BlockGeometryScript>().TapContainer.SetActive(false);
                copiedBlock.GetComponent<BlockGeometryScript>().GroovesContainer.SetActive(false);
                copyToOriginalAssign.Add(block.GetComponent<BlockCommunication>().Guid, copiedBlock.GetComponent<BlockCommunication>().Guid);
                copiedBlocks.Add(copiedBlock);
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
            StartCoroutine(EnableTapAndGrooveHandler(copiedBlocks));
            return BlockManager.GetBlockByGuid(grabbedBlockGuid);
        }

        private IEnumerator EnableTapAndGrooveHandler(List<GameObject> copiedBlocks)
        {
            foreach(GameObject copiedBlock in copiedBlocks)
            {
                copiedBlock.GetComponent<BlockGeometryScript>().TapContainer.SetActive(true);
                copiedBlock.GetComponent<BlockGeometryScript>().GroovesContainer.SetActive(true);
                copiedBlock.GetComponentInChildren<TapHandler>().AcceptCollisionsAsConnected(true);
                copiedBlock.GetComponentInChildren<GrooveHandler>().AcceptCollisionsAsConnected(true);
                yield return new WaitForSecondsRealtime(1f);
            }

            foreach (GameObject copiedBlock in copiedBlocks)
            {
                copiedBlock.GetComponentInChildren<TapHandler>().AcceptCollisionsAsConnected(false);
                copiedBlock.GetComponentInChildren<GrooveHandler>().AcceptCollisionsAsConnected(false);
            }


        }
    }
}
