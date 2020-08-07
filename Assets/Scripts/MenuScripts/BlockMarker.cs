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
        /// Distance before a box is drawn, Box is resettet otherwise
        /// </summary>
        public float PullThreshold;

        /// <summary>
        /// The Collider that matched the drawn Box
        /// </summary>
        private BoxCollider currentBoxCollider;

        private Vector3 startPosition = new Vector3();
        void Start()
        {
            currentBoxCollider = gameObject.AddComponent<BoxCollider>();
            currentBoxCollider.enabled = false;
            currentBoxCollider.isTrigger = true;
        }

        /// <summary>
        /// Starts the pulling of the Marker, resets the old Marker
        /// </summary>
        /// <param name="handSide"></param>
        public void MarkerPullingStarted(HANDSIDE handSide)
        { 
            ResetMarker();
            BoxDrawer.ActivateBox();
            switch (handSide)
            {
                case HANDSIDE.HAND_LEFT:
                    startPosition = HandLeft.transform.position;
                    break;

                case HANDSIDE.HAND_RIGHT:
                    startPosition = HandRight.transform.position;
                    break;
            }

            BoxDrawer.StartPosition = startPosition;
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
                    if (Vector3.Distance(startPosition, HandLeft.transform.position) < PullThreshold)
                    {
                        BoxDrawer.RemoveBox();
                        ResetMarker();
                    }
                    BoxDrawer.ConfigureBoxCollider(currentBoxCollider);
                    break;

                case HANDSIDE.HAND_RIGHT:
                    if (Vector3.Distance(startPosition, HandRight.transform.position) < PullThreshold)
                    {
                        BoxDrawer.RemoveBox();
                        ResetMarker();
                    }
                    BoxDrawer.ConfigureBoxCollider(currentBoxCollider);
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
                    BoxDrawer.DrawCube(HandLeft.transform.position);
                    break;

                case HANDSIDE.HAND_RIGHT:
                    BoxDrawer.DrawCube(HandRight.transform.position);
                    
                    break;
            }
            BoxDrawer.ConfigureBoxCollider(currentBoxCollider);
        }

        /// <summary>
        /// The Marker is being edited and the Collidersize need to be updated
        /// </summary>
        public void OnUpdateCollider()
        {
            BoxDrawer.ConfigureBoxCollider(currentBoxCollider);
        }

        /// <summary>
        /// Trigger Enter function of the Box Collider, is triggered when a Collider of aBlock enters the
        /// Box. Block is added to List of markedBlocks
        /// </summary>
        /// <param name="other">Collider of the Block</param>
        private void OnTriggerEnter(Collider other)
        {
            GameObject block = other.transform.gameObject;

            //Add to List if it is a Block
            if (block.tag.Equals("Block"))
            {
                other.gameObject.SendMessage("OnMarkedBegin", SendMessageOptions.DontRequireReceiver);
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
            
            GameObject block = other.transform.gameObject;

            //Remove the first found Block from the List
            markedBlocks.Remove(block);

            //Marking ends if no Block is in the List anymore
            if (!markedBlocks.Exists(tempBlock => tempBlock.GetHashCode() == block.GetHashCode()))
            {
                block.SendMessage("OnMarkedEnd", SendMessageOptions.DontRequireReceiver);
            }
        }

        /*/// <summary>
        /// Adds or Removes a Block to/from the List
        /// </summary>
        /// <param name="interactable"></param>
        public void MarkSeparateBlock(Interactable interactable)
        {
            if(interactable.isMarked)
            {
                markedBlocks.RemoveAll(tempBlock => tempBlock.GetHashCode() == interactable.transform.gameObject.GetHashCode());
                interactable.OnMarkedEnd();
            }

            else
            {
                interactable.OnMarkedBegin();
                markedBlocks.Add(interactable.transform.gameObject);
            }
            
        }*/

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
            List<GameObject> copiedBlockList = new List<GameObject>();

            //Dicts to get from original Block Guid to the copied Block Guid and other direction
            Dictionary<Guid, Guid> dictionaryGetCopyByOriginal = new Dictionary<Guid, Guid>();
            Dictionary<Guid, Guid> dictionaryGetOriginalByCopy = new Dictionary<Guid, Guid>();

            //Generate a copy of the marked Blocks, but only if a connection from the grabbed Block to the
            //other markedBlock is available. This makes sure that only a coherent structure is grabbed
            foreach (GameObject block in distinctBlocks)
            {
                if (grabbedBlock.GetComponent<BlockCommunication>().IsIndirectlyAttachedToBlockMarked(block))
                {
                    GameObject copiedBlock = BlockGenerator.GenerateBlock(block.GetComponent<BlockGeometryScript>().BlockStructure);
                    copiedBlock.transform.rotation = block.transform.rotation;
                    copiedBlock.transform.position = block.transform.position;
                    copiedBlock.GetComponent<BlockGeometryScript>().SetWallColliderTrigger(true);
                    copiedBlock.GetComponent<BlockGeometryScript>().TapContainer.SetActive(false);
                    copiedBlock.GetComponent<BlockGeometryScript>().GroovesContainer.SetActive(false);
                    dictionaryGetCopyByOriginal.Add(block.GetComponent<BlockCommunication>().Guid, copiedBlock.GetComponent<BlockCommunication>().Guid);
                    dictionaryGetOriginalByCopy.Add(copiedBlock.GetComponent<BlockCommunication>().Guid, block.GetComponent<BlockCommunication>().Guid);
                    copiedBlockList.Add(copiedBlock);
                }
                
            }

            Guid grabbedBlockGuidCopy = dictionaryGetCopyByOriginal[grabbedBlock.GetComponent<BlockCommunication>().Guid];
            GameObject grabbedBlockCopy = BlockManager.GetBlockByGuid(grabbedBlockGuidCopy);
            //Connect the copied Blocks, connections are made based on which connections the original Blocks had
            foreach (GameObject copiedBlock in copiedBlockList)
            {
                GameObject originalBlock = BlockManager.GetBlockByGuid(dictionaryGetOriginalByCopy[copiedBlock.GetComponent<BlockCommunication>().Guid]);

                foreach (BlockContainer blockContainer in originalBlock.GetComponent<BlockCommunication>().ConnectedBlocks)
                {
                    if (distinctBlocks.Exists(tempBlock => blockContainer.BlockRootObject.GetHashCode() == tempBlock.GetHashCode())){
                        Guid copiedBlockGuid = dictionaryGetCopyByOriginal[blockContainer.Guid];
                        copiedBlock.GetComponent<BlockCommunication>().ConnectBlocks(copiedBlock, BlockManager.GetBlockByGuid(copiedBlockGuid), blockContainer.ConnectedPinCount, blockContainer.ConnectedOn);
                    }
                }

                if(!grabbedBlockGuidCopy.Equals(copiedBlock.GetComponent<BlockCommunication>().Guid))
                {
                    copiedBlock.transform.SetParent(grabbedBlockCopy.transform);
                    Destroy(copiedBlock.GetComponent<Rigidbody>());
                }
                
            }

            

            //Wait before activate the Groove- and Tap handler until a certain distance is reached from the original Blocks
            StartCoroutine(CheckDistance(grabbedBlockCopy, grabbedBlock, copiedBlockList));

            //Return the copy of the original grabbed Block
            return grabbedBlockCopy;
        }

        /// <summary>
        /// Check the Distance between an original a copied Block. Enable Groove and Tap Handler when a safe distance is reached
        /// </summary>
        /// <param name="copiedBlock">The copied Block</param>
        /// <param name="grabbedBlock">The original Block</param>
        /// <param name="copiedBlockList">The Blocks in the copiedStructure</param>
        /// <returns></returns>
        private IEnumerator CheckDistance(GameObject grabbedBlockCopy, GameObject grabbedBlock, List<GameObject> copiedBlockList)
        {
            while(Vector3.Distance(grabbedBlockCopy.transform.position, grabbedBlock.transform.position) < distanceBeforeColliderActivation)
            {
                yield return new WaitForEndOfFrame();
            }
            yield return EnableTapAndGrooveHandler(copiedBlockList, grabbedBlockCopy);
        }
              
        /// <summary>
        /// Sets the Groove- and Tap Handler to accept Collisions as connections and activates the Wall Collider
        /// </summary>
        /// <param name="copiedBlockList"></param>
        /// <returns></returns>
        private IEnumerator EnableTapAndGrooveHandler(List<GameObject> copiedBlockList, GameObject grabbedCopiedBlock)
        {
            
            Dictionary<GameObject, Quaternion> rotation = new Dictionary<GameObject, Quaternion>();
            Dictionary<GameObject, Vector3> position = new Dictionary<GameObject, Vector3>();

            foreach (GameObject copiedBlock in copiedBlockList)
            {
                
                if(copiedBlock.GetHashCode() != grabbedCopiedBlock.GetHashCode())
                {
                    rotation.Add(copiedBlock, copiedBlock.transform.localRotation);
                    position.Add(copiedBlock, copiedBlock.transform.localPosition);
                    copiedBlock.transform.SetParent(null);
                    copiedBlock.AddComponent<Rigidbody>();
                    copiedBlock.AddComponent<FixedJoint>().connectedBody = grabbedCopiedBlock.GetComponent<Rigidbody>();
                }
            }
            
            foreach (GameObject copiedBlock in copiedBlockList)
            {
                copiedBlock.GetComponent<BlockGeometryScript>().TapContainer.SetActive(true);
                copiedBlock.GetComponent<BlockGeometryScript>().GroovesContainer.SetActive(true);
                copiedBlock.transform.Find("Taps").GetComponent<TapHandler>().AcceptCollisionsAsConnected(true);
                copiedBlock.transform.Find("Grooves").GetComponent<GrooveHandler>().AcceptCollisionsAsConnected(true);
                yield return new WaitForSecondsRealtime(0.02f);
            }

            foreach (GameObject copiedBlock in copiedBlockList)
            {
                copiedBlock.transform.Find("Taps").GetComponent<TapHandler>().AcceptCollisionsAsConnected(false);
                copiedBlock.transform.Find("Grooves").GetComponent<GrooveHandler>().AcceptCollisionsAsConnected(false);
                copiedBlock.GetComponent<BlockGeometryScript>().SetWallColliderTrigger(false);

                if (copiedBlock.GetHashCode() != grabbedCopiedBlock.GetHashCode())
                {
                    Destroy(copiedBlock.GetComponent<FixedJoint>());
                    Destroy(copiedBlock.GetComponent<Rigidbody>());
                    copiedBlock.transform.SetParent(grabbedCopiedBlock.transform);
                    copiedBlock.transform.localPosition = position[copiedBlock];
                    copiedBlock.transform.localRotation = rotation[copiedBlock];
                }
            }
        }
    }
}
