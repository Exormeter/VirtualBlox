using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{
    public class BlockMarker : MonoBehaviour
    {
        /// <summary>
        /// Draws the Box when pulled
        /// </summary>
        public BoxDrawer BoxDrawer;

        /// <summary>
        /// Position of the left Hand
        /// </summary>
        public GameObject HandLeft;

        /// <summary>
        /// Position of the right Hand
        /// </summary>
        public GameObject HandRight;

        public BlockGenerator BlockGenerator;
        public BlockManager BlockManager;

        /// <summary>
        /// Distance before the Collider in the cloned Block actvaite to avoid false CollidedObject hits
        /// </summary>
        public float distanceBeforeColliderActivation;

        /// <summary>
        /// The currently marked Blocks
        /// </summary>
        public List<GameObject> markedBlocks = new List<GameObject>();

        /// <summary>
        /// The Collider that matched the drawn Box
        /// </summary>
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

        /// <summary>
        /// Starts the pulling of the Marker, resets the old Marker
        /// </summary>
        /// <param name="handSide"></param>
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


        /// <summary>
        /// Ends the pulling of the Marker
        /// </summary>
        /// <param name="handSide"></param>
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

        /// <summary>
        /// Called while the Marker is pulled, updates the dimensions of the Box while the player pulls
        /// </summary>
        /// <param name="handSide"></param>
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

        /// <summary>
        /// Trigger Enter function of the Box Collider, is triggered when a Collider of aBlock enters the
        /// Box. Block is added to List of markedBlocks
        /// </summary>
        /// <param name="other">Collider of the Block</param>
        private void OnTriggerEnter(Collider other)
        {
            GameObject block = other.transform.root.gameObject;

            //Add to List if it is a Block
            if (block.tag.Equals("Block"))
            {
                other.gameObject.SendMessageUpwards("OnMarkedBegin", SendMessageOptions.DontRequireReceiver);
                markedBlocks.Add(block);
            }
            
        }

        /// <summary>
        /// Trigger Exit function of the Box Collider, is triggered when a Collider of a Block exits
        /// Box. Block is removed from List if last refernce of Block is removed from List
        /// </summary>
        /// <param name="other">Collider of the Block</param>
        private void OnTriggerExit(Collider other)
        {
            
            GameObject block = other.transform.root.gameObject;

            //Remove the first found Block from the List
            markedBlocks.Remove(block);

            //Marking ends if no Block is in the List anymore
            if (!markedBlocks.Exists(tempBlock => tempBlock.GetHashCode() == block.GetHashCode()))
            {
                block.SendMessageUpwards("OnMarkedEnd", SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <summary>
        /// Add a Block to the List that is not inside the Marker
        /// </summary>
        /// <param name="interactable"></param>
        public void MarkSeparateBlock(Interactable interactable)
        {
            interactable.OnMarkedBegin();
            markedBlocks.Add(interactable.transform.gameObject);
        }

        /// <summary>
        /// Send a OnMarkedEnd Message to all Blocks, clear the List of markedBlocks and disable the Box Collider
        /// </summary>
        private void ResetMarker()
        {
            foreach(GameObject block in markedBlocks)
            {
                if(block != null)
                {
                    block.SendMessageUpwards("OnMarkedEnd", SendMessageOptions.DontRequireReceiver);
                }
                
            }
            markedBlocks.Clear();
            currentBoxCollider.enabled = false;
        }

        /// <summary>
        /// Reconstructs the marked Blocks when a marked Block is grabbed by a Hand
        /// </summary>
        /// <param name="grabbedBlock">Which of the marked Block was grabbed</param>
        /// <returns>Return a copy of the grabbed Block</returns>
        public GameObject RebuildMarkedStructure(GameObject grabbedBlock)
        {
            //Remove duplicate GameObjects
            List<GameObject> distinctBlocks = markedBlocks.Distinct().ToList();

            //List to cache copied Blocks
            List<GameObject> copiedBlocks = new List<GameObject>();

            //Dicts to get from original Block Guid to the copied Block Guid and other direction
            Dictionary<Guid, Guid> dictionaryCopyByOriginal = new Dictionary<Guid, Guid>();
            Dictionary<Guid, Guid> dictionaryOriginalByCopy = new Dictionary<Guid, Guid>();

            //Generate a copy of the marked Blocks, but only if a connection from the grabbed Block to the
            //other markedBlock is available. This makes sure that only a coherent Structure is grabbed
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

            //Connect the copied Blocks, connections are made based on which connections the original Blocks had
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

            //Wait before activate the Groove- and Tap handler until a certain distance is reached from the original Blocks
            StartCoroutine(CheckDistance(copiedBlocks[0], grabbedBlock, copiedBlocks));

            //Return the copy of the original grabbed Block
            return BlockManager.GetBlockByGuid(grabbedBlockGuidCopy);
        }

        /// <summary>
        /// Check the Distance between an original a copied Block. Enable Groove and Tap Handler when a safe distance is reached
        /// </summary>
        /// <param name="copiedBlock">The copied Block</param>
        /// <param name="grabbedBlock">The original Block</param>
        /// <param name="copiedBlocks">The Blocks in the copiedStructure</param>
        /// <returns></returns>
        private IEnumerator CheckDistance(GameObject copiedBlock, GameObject grabbedBlock, List<GameObject> copiedBlocks)
        {
            while(Vector3.Distance(copiedBlock.transform.position, grabbedBlock.transform.position) < distanceBeforeColliderActivation)
            {
                yield return new WaitForEndOfFrame();
            }
            yield return EnableTapAndGrooveHandler(copiedBlocks);
        }
              
        /// <summary>
        /// Sets the Groove- and Tap Handler to accept Collisions as connections and activates the Wall Collider
        /// </summary>
        /// <param name="copiedBlocks"></param>
        /// <returns></returns>
        private IEnumerator EnableTapAndGrooveHandler(List<GameObject> copiedBlocks)
        {
            foreach(GameObject copiedBlock in copiedBlocks)
            {
                copiedBlock.GetComponent<BlockGeometryScript>().TapContainer.SetActive(true);
                copiedBlock.GetComponent<BlockGeometryScript>().GroovesContainer.SetActive(true);
                copiedBlock.GetComponentInChildren<TapHandler>().AcceptCollisionsAsConnected(true);
                copiedBlock.GetComponentInChildren<GrooveHandler>().AcceptCollisionsAsConnected(true);
                yield return new WaitForSecondsRealtime(0.02f);
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
