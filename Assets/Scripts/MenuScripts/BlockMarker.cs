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
        public float distanceBeforeColliderActivation;

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
            ResetMarker();
            
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
            if (!currentBoxCollider.enabled)
            {
                currentBoxCollider.enabled = true;
            }
            
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
            markedBlocks.Clear();
            currentBoxCollider.enabled = false;
        }

        public GameObject RebuildMarkedStructure(GameObject grabbedBlock)
        {
            List<GameObject> distinctBlocks = markedBlocks.Distinct().ToList();
            List<GameObject> copiedBlocks = new List<GameObject>();
            Dictionary<Guid, Guid> dictionaryCopyByOriginal = new Dictionary<Guid, Guid>();
            Dictionary<Guid, Guid> dictionaryOriginalByCopy = new Dictionary<Guid, Guid>();

            foreach (GameObject block in distinctBlocks)
            {
                if (grabbedBlock.GetComponent<BlockCommunication>().IsIndirectlyAttachedToBlockMarked(block))
                {
                    GameObject copiedBlock = BlockGenerator.GenerateBlock(block.GetComponent<BlockGeometryScript>().blockStructure);
                    copiedBlock.transform.rotation = block.transform.rotation;
                    copiedBlock.transform.position = block.transform.position;
                    copiedBlock.GetComponent<BlockGeometryScript>().SetWallColliderTrigger(true);
                    copiedBlock.GetComponent<BlockGeometryScript>().TapContainer.SetActive(false);
                    copiedBlock.GetComponent<BlockGeometryScript>().GroovesContainer.SetActive(false);
                    dictionaryCopyByOriginal.Add(block.GetComponent<BlockCommunication>().Guid, copiedBlock.GetComponent<BlockCommunication>().Guid);
                    dictionaryOriginalByCopy.Add(copiedBlock.GetComponent<BlockCommunication>().Guid, block.GetComponent<BlockCommunication>().Guid);
                    copiedBlocks.Add(copiedBlock);
                }
                
            }

            foreach (GameObject copiedBlock in copiedBlocks)
            {
                GameObject originalBlock = BlockManager.GetBlockByGuid(dictionaryOriginalByCopy[copiedBlock.GetComponent<BlockCommunication>().Guid]);

                foreach (BlockContainer blockContainer in originalBlock.GetComponent<BlockCommunication>().ConnectedBlocks)
                {
                    if (distinctBlocks.Exists(tempBlock => blockContainer.BlockRootObject.GetHashCode() == tempBlock.GetHashCode())){
                        Guid copiedBlockGuid = dictionaryCopyByOriginal[blockContainer.Guid];
                        copiedBlock.GetComponent<BlockCommunication>().ConnectBlocks(copiedBlock, BlockManager.GetBlockByGuid(copiedBlockGuid), blockContainer.ConnectedPinCount, blockContainer.ConnectedOn);
                    }
                }
            }

            Guid grabbedBlockGuidCopy = dictionaryCopyByOriginal[grabbedBlock.GetComponent<BlockCommunication>().Guid];
            StartCoroutine(CheckDistance(copiedBlocks[0], grabbedBlock, copiedBlocks));
            return BlockManager.GetBlockByGuid(grabbedBlockGuidCopy);
        }


        private IEnumerator CheckDistance(GameObject copiedBlock, GameObject grabbedBlock, List<GameObject> copiedBlocks)
        {
            while(Vector3.Distance(copiedBlock.transform.position, grabbedBlock.transform.position) < distanceBeforeColliderActivation)
            {
                yield return new WaitForEndOfFrame();
            }
            yield return EnableTapAndGrooveHandler(copiedBlocks);
        }
              

        private IEnumerator EnableTapAndGrooveHandler(List<GameObject> copiedBlocks)
        {
            foreach(GameObject copiedBlock in copiedBlocks)
            {
                copiedBlock.GetComponent<BlockGeometryScript>().TapContainer.SetActive(true);
                copiedBlock.GetComponent<BlockGeometryScript>().GroovesContainer.SetActive(true);
                copiedBlock.GetComponentInChildren<TapHandler>().AcceptCollisionsAsConnected(true);
                copiedBlock.GetComponentInChildren<GrooveHandler>().AcceptCollisionsAsConnected(true);
                yield return new WaitForSecondsRealtime(0.2f);
            }

            foreach (GameObject copiedBlock in copiedBlocks)
            {
                copiedBlock.GetComponentInChildren<TapHandler>().AcceptCollisionsAsConnected(false);
                copiedBlock.GetComponentInChildren<GrooveHandler>().AcceptCollisionsAsConnected(false);
                copiedBlock.GetComponent<BlockGeometryScript>().SetWallColliderTrigger(false);
            }


        }
    }
}
