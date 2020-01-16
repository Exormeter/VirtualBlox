using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{
    public class AttachFloorHandler : MonoBehaviour
    {

        public PhysicSceneManager physicSceneManager;
        private GrooveHandler grooveHandler;
        private TapHandler tapHandler;
        private Rigidbody rigidBody;
        private BlockCommunication blockCommunication;
        public bool WasReMatchedWithBlock;

        public bool IsFroozen;
        public int frameUntilColliderReEvaluation;

        void Start()
        {
            grooveHandler = GetComponentInChildren<GrooveHandler>();
            tapHandler = GetComponentInChildren<TapHandler>();
            rigidBody = GetComponent<Rigidbody>();
            blockCommunication = GetComponent<BlockCommunication>();
            //physicSceneManager = GameObject.FindGameObjectWithTag("PhysicManager").GetComponent<PhysicSceneManager>();
        }


        public void AttachToFloor()
        {
            GameObject block = blockCommunication.FindFirstCollidingBlock();
            if (block != null)
            {
                block.GetComponent<AttachFloorHandler>().GrooveOrTapHit(out List<CollisionObject> currentCollisionObjects, out OTHER_BLOCK_IS_CONNECTED_ON connectedOn);
                block.GetComponent<AttachFloorHandler>().MatchRotationWithCollidingBlock(currentCollisionObjects, connectedOn);
            }
        }

        public void MatchRotationWithCollidingBlock(List<CollisionObject> currentCollisionObjects, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            if (currentCollisionObjects.Count > 1)
            {
                blockCommunication.SendMessageToConnectedBlocks("SetKinematic");
                GetComponent<BlockRotator>().RotateBlock(currentCollisionObjects, connectedOn);
                WasReMatchedWithBlock = true;
                //Richte alle Blöcke nach Plazieren des ersten Blockes korrekt aus, alte Joints müssen vermutlich neu gesetzt werden 
                blockCommunication.SendMessageToConnectedBlocksBFS("ReMatchConnectedBlock");
                blockCommunication.SendMessageToConnectedBlocks("AddGuidToHistory");


                StartCoroutine(EvaluateColliderAfterMatching());
            }
        }

        IEnumerator EvaluateColliderAfterMatching()
        {
            for (int i = 0; i <= frameUntilColliderReEvaluation; i++)
            {
                if (i == frameUntilColliderReEvaluation)
                {
                    List<GameObject> connectedBlocksBeforeEvaluation = blockCommunication.GetCurrentlyConnectedBlocks();
                    blockCommunication.SendMessageToConnectedBlocks("EvaluateCollider");
                    //SyncronizePhysicBlocks(connectedBlocksBeforeEvaluation);
                    blockCommunication.SendMessageToConnectedBlocks("CheckFreeze");
                    blockCommunication.SendMessageToConnectedBlocks("UnsetKinematic");
                    //physicSceneManager.StartSimulation();
                }
                yield return new WaitForFixedUpdate();
            }

        }

        //private void SyncronizePhysicBlocks(List<GameObject> connectedBlocksBeforeEvaluation)
        //{
        //    foreach (GameObject connectedBlockInHand in connectedBlocksBeforeEvaluation)
        //    {
        //        connectedBlockInHand.GetComponent<BlockCommunication>().blockScriptSim.MatchTwinBlock(connectedBlockInHand);
        //    }
        //    foreach (GameObject connectedBlockInHand in connectedBlocksBeforeEvaluation)
        //    {
        //        connectedBlockInHand.GetComponent<BlockCommunication>().blockScriptSim.ConnectBlocksAfterMatching(connectedBlockInHand);
        //    }
        //}

        public void AddGuidToHistory()
        {
            int timeStamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            HistoryObject historyObject = new HistoryObject(blockCommunication.Guid, timeStamp);

            BlockManager blockManager = GameObject.FindGameObjectWithTag("BlockManager").GetComponent<BlockManager>();
            blockManager.AddHistoryEntry(historyObject);
            blockManager.ResetHistoryStack();
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

        private void ReMatchConnectedBlock()
        {
            if (WasReMatchedWithBlock)
            {
                return;
            }
            Debug.Log("Rematched");
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

        public void FreezeBlock()
        {
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            IsFroozen = true;
        }

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
            }
            else
            {
                UnfreezeBlock();
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
                WasReMatchedWithBlock = false;
            }
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
