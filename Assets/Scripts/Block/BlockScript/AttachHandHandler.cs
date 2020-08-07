using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class AttachHandHandler : MonoBehaviour
    {
        private BlockCommunication blockCommunication;
        private int frameUntilColliderReEvaluation = 2;
        private Hand holdingHand = null;
        public bool debug = false; 

        void Start()
        {
            blockCommunication = GetComponent<BlockCommunication>();
        }

        /// <summary>
        /// Called when the Block is attached to the Hand
        /// </summary>
        /// <param name="hand">The Hand the Block was attached to</param>
        public void OnAttachedToHand(Hand hand)
        {
            holdingHand = hand;
            //Send Message to all connected Blocks that the Structure was picked up
            blockCommunication.SendMessageToConnectedBlocks("OnIndirectAttachedtoHand");
        }

        /// <summary>
        /// Called when the Block is in a Structure with a Block that was attached to a Hand
        /// </summary>
        public void OnIndirectAttachedtoHand()
        {
            //GetComponent<Rigidbody>().isKinematic = false;
            //gameObject.transform.SetParent(null);
        }


        /// <summary>
        /// Called when the Block was detached from a Hand, check if detachement from Hand inidcates
        /// an attachment to an other Block 
        /// </summary>
        /// <param name="hand"></param>
        public void OnDetachedFromHand(Hand hand)
        {
            holdingHand = null;

            //Send message to Blocks in Structure that Stucture detached from Hand
            blockCommunication.SendMessageToConnectedBlocks("OnIndirectDetachedFromHand");

            //Check if any Block in the Structure is collding with an other Block
            GameObject block = blockCommunication.FindFirstCollidingBlock();

            //no Block is colliding, Structure is free
            if(block == null)
            {
                return;
            }

            //One of the Block is colliding, check if it collding on the Groove- or Tap
            block.GetComponent<AttachHandHandler>().GrooveOrTapHit(out List<CollisionObject> currentCollisionObjects, out OTHER_BLOCK_IS_CONNECTED_ON connectedOn);

            block.transform.SetParent(null);
            foreach (GameObject blockInStructure in block.GetComponent<BlockCommunication>().GetCurrentlyConnectedBlocks())
            {
                if(blockInStructure.GetComponent<Rigidbody>() != null)
                {
                    Destroy(blockInStructure.GetComponent<Rigidbody>());
                }
                if(blockInStructure.GetHashCode() != block.GetHashCode())
                {
                    blockInStructure.transform.SetParent(block.transform);
                } 
            }

            //The colliding Block IS indirectly attached to the Floor, call FloorHandler to connect the Structure to the Floor
            if (currentCollisionObjects[0].CollidedBlock.GetComponent<BlockCommunication>().IsIndirectlyAttachedToFloor())
            {
                GetComponent<AttachFloorHandler>().AttachToFloor();
            }

            //The collding Block is NOT indirectly attached to a Floor plate, call the AttachHandHandler of the collding Block
            else if (!currentCollisionObjects[0].CollidedBlock.GetComponent<BlockCommunication>().IsIndirectlyAttachedToFloor())
            {
                
                block.GetComponent<AttachHandHandler>().MatchRotationWithBlock(currentCollisionObjects, connectedOn);
            }

            
            
        }


        /// <summary>
        /// Match the Rotation with the collided Block according to the collision Blocks and start the Coroutine to check the
        /// Groove- and Tap Collider after matching.
        /// </summary>
        /// <param name="currentCollisionObjects">The collisionObject for rotation</param>
        /// <param name="connectedOn">There is the other Block connected</param>
        public void MatchRotationWithBlock(List<CollisionObject> currentCollisionObjects, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            
            //Set all Blocks in Structure to kinematic for rotation
            blockCommunication.SendMessageToConnectedBlocks("SetKinematic");

            //Rotate the Block
            GetComponent<BlockRotator>().RotateBlock(currentCollisionObjects, connectedOn);

            //gameObject.AddComponent<FixedJoint>().connectedBody = currentCollisionObjects[0].CollidedBlock.GetComponent<Rigidbody>();  
            transform.root.SetParent(currentCollisionObjects[0].CollidedBlock.transform.root);

            //Check which additional Groove or Taps were hit after Rotating
            StartCoroutine(EvaluateColliderAfterMatching(currentCollisionObjects));
            
        }

        /// <summary>
        /// Set the Blocks in Structure to non-kinematic again and send them the message to check their 
        /// Groove- and Tap Handler. This methods waits for two FixedUpdates before sending the message, since the
        /// Collider positions are not imidiatly updated
        /// </summary>
        /// <returns></returns>
        IEnumerator EvaluateColliderAfterMatching(List<CollisionObject> currentCollisionObjects)
        {
            
            for (int i = 0; i <= frameUntilColliderReEvaluation; i++)
            {
                if (i == frameUntilColliderReEvaluation)
                {
                    blockCommunication.SendMessageToConnectedBlocks("SearchNewConnectionsHand");
                    RemoveRedundantRigidBodies();

                    blockCommunication.SendMessageToConnectedBlocks("UnsetKinematic");
                    
                }
                yield return new WaitForFixedUpdate();
            }
        }


        /// <summary>
        /// Check how many new Blocks are now colliding, which Groove or Tap was hit and connects them together
        /// </summary>
        public void SearchNewConnectionsHand()
        {
            //Are the new Blocks collided via the Groove or Taps
            GrooveOrTapHit(out List<CollisionObject> currentCollisionObjects, out OTHER_BLOCK_IS_CONNECTED_ON connectedOn);

            //Dictionary to hold which Block has collided with how many Groove or Taps
            Dictionary<GameObject, int> blockToTapDict = new Dictionary<GameObject, int>();

            //Wieviele Taps oder Grooves wurden nach dem Rotieren getroffen?
            foreach (CollisionObject collisionObject in currentCollisionObjects)
            {
                if (!blockToTapDict.ContainsKey(collisionObject.CollidedBlock))
                {
                    blockToTapDict.Add(collisionObject.CollidedBlock, 1);
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

        private void RemoveRedundantRigidBodies()
        {
            foreach (GameObject blockInStructure in GetComponent<BlockCommunication>().GetCurrentlyConnectedBlocks())
            {
                if(blockInStructure.transform.root != blockInStructure.transform)
                {
                    Destroy(blockInStructure.GetComponent<Rigidbody>());
                }
            }
        }

        /// <summary>
        /// Sets the Block kinametic if it's not a Floor plate
        /// </summary>
        public void SetKinematic()
        {
            Rigidbody rigidBody = GetComponent<Rigidbody>();
            if (!gameObject.tag.Equals("Floor") && rigidBody != null)
            {
                rigidBody.isKinematic = true;
                GetComponent<BlockGeometryScript>().SetWallColliderTrigger(true);
            }
        }

        /// <summary>
        /// Sets the Block non-kinametic if it's not a Floor plate
        /// </summary>
        public void UnsetKinematic()
        {
            Rigidbody rigidBody = GetComponent<Rigidbody>();
            if (!gameObject.tag.Equals("Floor") && rigidBody != null)
            {
                rigidBody.isKinematic = false;
                GetComponent<BlockGeometryScript>().SetWallColliderTrigger(false);
            }
        }

        /// <summary>
        /// Is the Block attached to a Hand
        /// </summary>
        /// <returns>True if it is Attached to a Hand</returns>
        public bool IsAttachedToHand()
        {
            return holdingHand != null;
        }

        /// <summary>
        /// Checks if the Groove- or Tap Handler contains CollisionObjects that are not connected
        /// </summary>
        /// <param name="collisionList">OUT List with non connected CollisionObjects</param>
        /// <param name="connectedOn">OUT Other Block connected on</param>
        public void GrooveOrTapHit(out List<CollisionObject> collisionList, out OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            
            
            if (transform.Find("Taps")?.GetComponent<TapHandler>().GetCollidingObjects().Count > 0)
            {
                collisionList = transform.Find("Taps")?.GetComponent<TapHandler>().GetCollidingObjects();
                connectedOn = OTHER_BLOCK_IS_CONNECTED_ON.TAP;
                return;
            }

            if (transform.Find("Grooves")?.GetComponent<GrooveHandler>()?.GetCollidingObjects().Count > 0)
            {
                collisionList = transform.Find("Grooves")?.GetComponent<GrooveHandler>()?.GetCollidingObjects();
                connectedOn = OTHER_BLOCK_IS_CONNECTED_ON.GROOVE;
                return;
            }

            collisionList = new List<CollisionObject>();
            connectedOn = OTHER_BLOCK_IS_CONNECTED_ON.NOT_CONNECTED;
        }
    }

}
