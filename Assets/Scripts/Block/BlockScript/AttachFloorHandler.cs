using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{
    public class AttachFloorHandler : MonoBehaviour
    {

        //public PhysicSceneManager physicSceneManager;
        //private GrooveHandler grooveHandler;
        //private TapHandler tapHandler;
        private Rigidbody rigidBody;
        private BlockCommunication blockCommunication;
        public bool WasReMatchedWithBlock;

        public bool IsFroozen;
        public int frameUntilColliderReEvaluation;

        void Start()
        {
            rigidBody = GetComponent<Rigidbody>();
            blockCommunication = GetComponent<BlockCommunication>();
        }

        /// <summary>
        /// Attach the Block to the Floor plate or Block that is indirectly connected to the Floor plate
        /// </summary>
        public void AttachToFloor()
        {
            //Find a Block that is collding
            GameObject block = blockCommunication.FindFirstCollidingBlock();
            if (block != null)
            {
                //Check if the Block is collding with a Groove or a Tap
                block.GetComponent<AttachFloorHandler>().GrooveOrTapHit(out List<CollisionObject> currentCollisionObjects, out OTHER_BLOCK_IS_CONNECTED_ON connectedOn);

                //Match the Rotation of the Block with the collding one
                block.GetComponent<AttachFloorHandler>().MatchRotationWithCollidingBlock(currentCollisionObjects, connectedOn);
            }
        }

        /// <summary>
        /// Matches the rotation of the Block to the rotation of the colliding Block. Sets the WasReMatchedWithBlock Flag to true to indicate that
        /// it is now in line with the colliding Block, so that the other Blocks in the Structure can use this Block to rotate themself.
        /// </summary>
        /// <param name="currentCollisionObjects"></param>
        /// <param name="connectedOn"></param>
        public void MatchRotationWithCollidingBlock(List<CollisionObject> currentCollisionObjects, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            if (currentCollisionObjects.Count > 1)
            {
                //Send message to Blocks in Structure to set them to kinematic for rotation
                blockCommunication.SendMessageToConnectedBlocks("SetKinematic");

                //Rotate the Block
                GetComponent<BlockRotator>().RotateBlock(currentCollisionObjects, connectedOn);

                //Set flag that Block was rotated
                WasReMatchedWithBlock = true;
                
                //Send Message to Blocks in Structure to ReMatch their rotation in line with a Block that has
                //already rotated
                blockCommunication.SendMessageToConnectedBlocksBFS("ReMatchConnectedBlock");

                //Tell Blocks in Structure to add themself to the History
                blockCommunication.SendMessageToConnectedBlocks("OnAttachToFloor");

                //Check which additional Groove or Taps were hit after Rotating
                StartCoroutine(EvaluateColliderAfterMatching());
            }
        }

        /// <summary>
        /// Set the Blocks in Structure to non-kinematic again and send them the message to check their 
        /// Groove- and Tap Handler. This methods waits for two FixedUpdates before sending the message, since the
        /// Collider positions are not imidiatly updated
        /// </summary>
        /// <returns></returns>
        IEnumerator EvaluateColliderAfterMatching()
        {
            for (int i = 0; i <= frameUntilColliderReEvaluation; i++)
            {
                if (i == frameUntilColliderReEvaluation)
                {
                    blockCommunication.SendMessageToConnectedBlocks("EvaluateCollider");
                    //Check if all Blocks are connected to Floor and freeze them
                    blockCommunication.SendMessageToConnectedBlocks("CheckFreeze");
                    blockCommunication.SendMessageToConnectedBlocks("UnsetKinematic");
                }
                yield return new WaitForFixedUpdate();
            }

        }

        /// <summary>
        /// Called when Block was directly or indirectly attached to the Floor
        /// </summary>
        public void OnAttachToFloor()
        {
            AddGuidToHistory();
        }

        /// <summary>
        /// Adds the Block to the History of placed Blocks
        /// </summary>
        private void AddGuidToHistory()
        {
            int timeStamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            HistoryObject historyObject = new HistoryObject(blockCommunication.Guid, timeStamp);

            BlockManager blockManager = GameObject.FindGameObjectWithTag("BlockManager").GetComponent<BlockManager>();
            blockManager.AddHistoryEntry(historyObject);
            blockManager.ResetHistoryStack();
        }

        /// <summary>
        /// Check how many new Blocks are now colliding, which Groove or Tap was hit and connects them together
        /// </summary>
        private void EvaluateCollider()
        {
            GrooveOrTapHit(out List<CollisionObject> currentCollisionObjects, out OTHER_BLOCK_IS_CONNECTED_ON connectedOn);
            Dictionary<GameObject, int> blockToTapDict = new Dictionary<GameObject, int>();

            //Wieviele Taps oder Grooves wurden nach dem Rotieren getroffen?
            foreach (CollisionObject collisionObject in currentCollisionObjects)
            {
                if (!blockToTapDict.ContainsKey(collisionObject.CollidedBlock))
                {
                    blockToTapDict.Add(collisionObject.CollidedBlock, 0);
                }
                else
                {
                    blockToTapDict[collisionObject.CollidedBlock]++;
                }
            }

            //Zu welchem Block muss eine Verbindnung aufgebaut werden mit welcher Stärke?
            foreach (GameObject collidedBlock in blockToTapDict.Keys)
            {
                blockCommunication.ConnectBlocks(transform.gameObject, collidedBlock, blockToTapDict[collidedBlock], connectedOn);
            }
        }

        /// <summary>
        /// ReMatches the Blocks rotation based on an other connected Block that was already correctly rotated
        /// </summary>
        private void ReMatchConnectedBlock()
        {
            //This Block is already correcty rotated
            if (WasReMatchedWithBlock)
            {
                return;
            }
            
            WasReMatchedWithBlock = true;

            BlockContainer connectedBlockContainer = null;
            //Durchsuche Verbundene Blöcke nach Block welcher bereits ausgerichtet ist
            foreach (BlockContainer connectedBlock in blockCommunication.ConnectedBlocks)
            {
                if (connectedBlock.AttachFloorHandler.WasReMatchedWithBlock)
                {
                    connectedBlockContainer = connectedBlock;
                }
            }

            // Finde herraus ob die Verbdingung über Groove oder Tap
            List<CollisionObject> connectionsToBlock = new List<CollisionObject>();
            

            //Durchsuche je nach dem den Tap oder GrooveHandler nach den richtigen CollisionObjects
            switch (connectedBlockContainer.ConnectedOn){

                case OTHER_BLOCK_IS_CONNECTED_ON.GROOVE:
                    connectionsToBlock = GetComponentInChildren<GrooveHandler>().GetCollisionObjectsForGameObject(connectedBlockContainer.BlockRootObject);
                    break;

                case OTHER_BLOCK_IS_CONNECTED_ON.TAP:
                    connectionsToBlock = GetComponentInChildren<TapHandler>().GetCollisionObjectsForGameObject(connectedBlockContainer.BlockRootObject);
                    break;

            }

            //Rotiere den Block richtig
            GetComponent<BlockRotator>().RotateBlock(connectionsToBlock, connectedBlockContainer.ConnectedOn);

            
        }

        /// <summary>
        /// Contrains the RidigdBody of the Block in all directions
        /// </summary>
        public void FreezeBlock()
        {
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            IsFroozen = true;
        }

        /// <summary>
        /// Removes all Contrains if the RidigBody 
        /// </summary>
        public void UnfreezeBlock()
        {
            rigidBody.constraints = RigidbodyConstraints.None;
            //blockCommunication.blockScriptSim.DisableTwin();
            IsFroozen = false;
        }


        public void CheckFreeze()
        {
            if (blockCommunication.IsIndirectlyAttachedToFloor())
            {
                FreezeBlock();
                GetComponent<BlockGeometryScript>().SetWallColliderTrigger(false);
            }
            else
            {
                UnfreezeBlock();
            }
        }

        /// <summary>
        /// Sets the RidigBody to kinematic if the Block is not a Floor Plate
        /// </summary>
        public void SetKinematic()
        {
            if (!gameObject.tag.Equals("Floor"))
            {
                rigidBody.isKinematic = true;
            }
        }

        /// <summary>
        /// Sets the RidigBody to non-kinematic if the Block is not a Floor Plate and resets
        /// the WasReMatchedWithBlock Flag
        /// </summary>
        public void UnsetKinematic()
        {
            if (!gameObject.tag.Equals("Floor"))
            {
                rigidBody.isKinematic = false;
                WasReMatchedWithBlock = false;
            }
        }

        /// <summary>
        /// Checks if the Groove- or Tap Handler contains CollisionObjects that are not connected
        /// </summary>
        /// <param name="collisionList">OUT List with non connected CollisionObjects</param>
        /// <param name="connectedOn">OUT Other Block connected on</param>
        private void GrooveOrTapHit(out List<CollisionObject> collisionList, out OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            if (GetComponentInChildren<TapHandler>().GetCollidingObjects().Count > 0)
            {
                collisionList = GetComponentInChildren<TapHandler>().GetCollidingObjects();
                connectedOn = OTHER_BLOCK_IS_CONNECTED_ON.TAP;
                return;
            }

            if (GetComponentInChildren<GrooveHandler>().GetCollidingObjects().Count > 0)
            {
                collisionList = GetComponentInChildren<GrooveHandler>().GetCollidingObjects();
                connectedOn = OTHER_BLOCK_IS_CONNECTED_ON.GROOVE;
                return;
            }

            collisionList = new List<CollisionObject>();
            connectedOn = OTHER_BLOCK_IS_CONNECTED_ON.NOT_CONNECTED;
        }

    }

}
