using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class AttachHandHandler : MonoBehaviour
    {
        private BlockCommunication blockCommunication;
        private GrooveHandler grooveHandler;
        private TapHandler tapHandler;
        private Rigidbody rigidBody;
        private int frameUntilColliderReEvaluation = 2;
        private Hand holdingHand = null;

        void Start()
        {
            grooveHandler = GetComponentInChildren<GrooveHandler>();
            tapHandler = GetComponentInChildren<TapHandler>();
            rigidBody = GetComponent<Rigidbody>();
            blockCommunication = GetComponent<BlockCommunication>();
        }


        void Update()
        {

        }


        public void OnAttachedToHand(Hand hand)
        {
            holdingHand = hand;
            blockCommunication.SendMessageToConnectedBlocks("OnIndirectAttachedtoHand");
        }



        public void OnDetachedFromHand(Hand hand)
        {
            holdingHand = null;
            blockCommunication.SendMessageToConnectedBlocks("OnIndirectDetachedFromHand");

            GameObject block = blockCommunication.FindFirstCollidingBlock();

            if(block == null)
            {
                return;
            }

            block.GetComponent<AttachHandHandler>().GrooveOrTapHit(out List<CollisionObject> currentCollisionObjects, out OTHER_BLOCK_IS_CONNECTED_ON connectedOn);

            if (!currentCollisionObjects[0].CollidedBlock.GetComponent<BlockCommunication>().IsIndirectlyAttachedToFloor())
            {
                block.GetComponent<AttachHandHandler>().MatchRotationWithBlock(currentCollisionObjects, connectedOn);
            }
            else if(currentCollisionObjects[0].CollidedBlock.GetComponent<BlockCommunication>().IsIndirectlyAttachedToFloor())
            {
                GetComponent<AttachFloorHandler>().AttachToFloor();
            }
            
        }



        

        public void MatchRotationWithBlock(List<CollisionObject> currentCollisionObjects, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            if (currentCollisionObjects.Count > 1)
            {
                blockCommunication.SendMessageToConnectedBlocks("SetKinematic");
                GetComponent<BlockRotator>().RotateBlock(currentCollisionObjects, connectedOn);
                StartCoroutine(EvaluateColliderAfterMatching());
            }
        }

        IEnumerator EvaluateColliderAfterMatching()
        {
            for (int i = 0; i <= frameUntilColliderReEvaluation; i++)
            {
                if (i == frameUntilColliderReEvaluation)
                {
                    blockCommunication.SendMessageToConnectedBlocks("EvaluateCollider");
                    blockCommunication.SendMessageToConnectedBlocks("UnsetKinematic");
                }
                yield return new WaitForFixedUpdate();
            }
        }


        private void EvaluateCollider()
        {
            Debug.Log("Evaluate Collider for Block: " + gameObject.name);
            Debug.Log("Is Kinematic: " + rigidBody.isKinematic);
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

        public void SetKinematic()
        {
            if (!gameObject.tag.Equals("Floor"))
            {
                rigidBody.isKinematic = true;
            }
        }

        public void UnsetKinematic()
        {
            if (!gameObject.tag.Equals("Floor"))
            {
                rigidBody.isKinematic = false;
            }
        }

        public bool IsAttachedToHand()
        {
            return holdingHand != null;
        }

        private void GrooveOrTapHit(out List<CollisionObject> collisionList, out OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            if (tapHandler.GetCollidingObjects().Count > 0)
            {
                collisionList = tapHandler.GetCollidingObjects();
                connectedOn = OTHER_BLOCK_IS_CONNECTED_ON.TAP;
                return;
            }

            if (grooveHandler.GetCollidingObjects().Count > 0)
            {
                collisionList = grooveHandler.GetCollidingObjects();
                connectedOn = OTHER_BLOCK_IS_CONNECTED_ON.GROOVE;
                return;
            }

            collisionList = new List<CollisionObject>();
            connectedOn = OTHER_BLOCK_IS_CONNECTED_ON.NOT_CONNECTED;
        }
    }

}
