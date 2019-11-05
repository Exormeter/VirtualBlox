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

        public int breakForcePerPin = 3;
        public bool IsFroozen;
        public int frameUntilColliderReEvaluation;

        void Start()
        {
            grooveHandler = GetComponentInChildren<GrooveHandler>();
            tapHandler = GetComponentInChildren<TapHandler>();
            rigidBody = GetComponent<Rigidbody>();
            blockCommunication = GetComponent<BlockCommunication>();
            physicSceneManager = GameObject.FindGameObjectWithTag("PhysicManager").GetComponent<PhysicSceneManager>();
        }


        void Update()
        {

        }


        public void OnDetachedFromHand(Hand hand)
        {
            GameObject block = blockCommunication.FindFirstCollidingBlock();
            block.GetComponent<AttachFloorHandler>().GrooveOrTapHit(out List<CollisionObject> currentCollisionObjects, out OTHER_BLOCK_IS_CONNECTED_ON connectedOn);
            if (block != null && currentCollisionObjects[0].CollidedBlock.GetComponent<BlockCommunication>().IsDirectlyAttachedToFloor())
            {
                block.GetComponent<AttachFloorHandler>().MatchRotationWithCollidingBlock(currentCollisionObjects, connectedOn);
            }

            //Duplicate in AttachHandHandler
            blockCommunication.SendMessageToConnectedBlocks("OnIndirectDetachedFromHand");
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
                    SyncronizePhysicBlocks(connectedBlocksBeforeEvaluation);
                    blockCommunication.SendMessageToConnectedBlocks("CheckFreeze");
                    blockCommunication.SendMessageToConnectedBlocks("UnsetKinematic");
                    physicSceneManager.StartSimulation();
                }
                yield return new WaitForFixedUpdate();
            }

        }

        private void SyncronizePhysicBlocks(List<GameObject> connectedBlocksBeforeEvaluation)
        {
            foreach (GameObject connectedBlockInHand in connectedBlocksBeforeEvaluation)
            {
                connectedBlockInHand.GetComponent<BlockCommunication>().blockScriptSim.MatchTwinBlock(connectedBlockInHand);
            }
            foreach (GameObject connectedBlockInHand in connectedBlocksBeforeEvaluation)
            {
                connectedBlockInHand.GetComponent<BlockCommunication>().blockScriptSim.ConnectBlocksAfterMatching(connectedBlockInHand);
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

        private void ReMatchConnectedBlock()
        {
            if (WasReMatchedWithBlock)
            {
                return;
            }

            WasReMatchedWithBlock = true;


            


            //Durchsuche Verbundene Blöcke nach Block welcher bereits ausgerichtet ist
            List<BlockContainer> connectedBlocks = blockCommunication.connectedBlocks;
            connectedBlocks.RemoveAll(block => !block.AttachFloorHandler.WasReMatchedWithBlock);

            // Finde herraus ob die Verbdingung über Groove oder Tap
            List<CollisionObject> connectionsToBlock = new List<CollisionObject>();
            BlockContainer connectedBlockContainer = connectedBlocks[0];

            //Durchsuche je nach dem den Tap oder GrooveHandler nach den richtigen CollisionObjects
            switch (connectedBlockContainer.ConnectedOn){

                case OTHER_BLOCK_IS_CONNECTED_ON.GROOVE:
                    connectionsToBlock = GetComponent<GrooveHandler>().GetCollisionObjectsForGameObject(connectedBlockContainer.BlockRootObject);
                    break;

                case OTHER_BLOCK_IS_CONNECTED_ON.TAP:
                    connectionsToBlock = GetComponent<TapHandler>().GetCollisionObjectsForGameObject(connectedBlockContainer.BlockRootObject);
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
                blockCommunication.blockScriptSim.DisableTwin();
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

        private ConfigurableJoint SetConfigurableJoint(Rigidbody connectedBody, int connectedPinCount)
        {
            ConfigurableJoint configurableJoint = gameObject.AddComponent<ConfigurableJoint>();
            configurableJoint.xMotion = ConfigurableJointMotion.Locked;
            configurableJoint.yMotion = ConfigurableJointMotion.Locked;
            configurableJoint.zMotion = ConfigurableJointMotion.Locked;
            configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
            configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
            configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
            //configurableJoint.autoConfigureConnectedAnchor = true;
            configurableJoint.projectionMode = JointProjectionMode.PositionAndRotation;
            configurableJoint.projectionAngle = 0.001f;
            configurableJoint.projectionDistance = 0.001f;
            //configurableJoint.connectedAnchor = connectedBody.position;
            //configurableJoint.anchor = gameObject.transform.position;
            //configurableJoint.enableCollision = true;
            configurableJoint.breakForce = connectedPinCount * breakForcePerPin * 10;
            configurableJoint.breakTorque = connectedPinCount * breakForcePerPin;
            configurableJoint.connectedBody = connectedBody;
            return configurableJoint;
        }
    }

}
