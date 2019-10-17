using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{


    public class BlockScript : MonoBehaviour
    {
        public List<BlockContainer> connectedBlocks = new List<BlockContainer>();

        public int connectedBlocksCount;
        private GrooveHandler grooveHandler;
        private TapHandler tapHandler;
        private Hand attachedHand = null;
        private BlockGeometryScript blockGeometry;
        
        public int breakForcePerPin = 25;
        public int frameUntilColliderReEvaluation = 2;
        private FixedJoint tempAttach;

        void Start()
        {
            grooveHandler = GetComponentInChildren<GrooveHandler>();
            tapHandler = GetComponentInChildren<TapHandler>();
            blockGeometry = GetComponent<BlockGeometryScript>();
        }

        
        void Update()
        {
            connectedBlocksCount = connectedBlocks.Count;
        }

        public void OnAttachedToHand(Hand hand)
        {
            attachedHand = hand;
            SendMessageToConnectedBlocks("OnIndirectAttachedtoHand");
        } 


        //TODO: Tap Collider Case
        public void OnDetachedFromHand(Hand hand)
        {
            attachedHand = null;
            BlockScript block = FindFirstCollidingBlock();
            if(block != null)
            {
                block.MatchRotationWithBlock();
            }
            SendMessageToConnectedBlocks("OnIndirectDetachedFromHand");
        }

        private BlockScript FindFirstCollidingBlock(List<int> visitedNodes = null)
        {
            if(visitedNodes == null)
            {
                visitedNodes = new List<int>();
            }

            visitedNodes.Add(gameObject.GetHashCode());

            if (grooveHandler.GetCollidingObjects().Count > 0 || tapHandler.GetCollidingObjects().Count > 0)
            {
                return this;
            }

            foreach (BlockContainer blockContainer in connectedBlocks)
            {
                if (!visitedNodes.Contains(blockContainer.BlockRootObject.GetHashCode()))
                {
                    BlockScript tempScript = blockContainer.BlockScript.FindFirstCollidingBlock(visitedNodes);
                    if (tempScript != null)
                    {
                        return tempScript;
                    }

                }
            }

            return null;
        }

        public void MatchRotationWithBlock()
        {
            GrooveOrTapHit(out List<CollisionObject> currentCollisionObjects, out OTHER_BLOCK_IS_CONNECTED_ON connectedOn);


            if (currentCollisionObjects.Count > 1)
            {
                Rigidbody rigidBody = GetComponent<Rigidbody>();
                rigidBody.isKinematic = true;
                GetComponent<BlockRotator>().RotateBlock(currentCollisionObjects, connectedOn);

                tempAttach = gameObject.AddComponent<FixedJoint>();
                if(connectedOn == OTHER_BLOCK_IS_CONNECTED_ON.GROOVE)
                {
                    tempAttach.connectedBody = currentCollisionObjects[0].TapPosition.GetComponentInParent<Rigidbody>();
                }
                else
                {
                    tempAttach.connectedBody = currentCollisionObjects[0].GroovePosition.GetComponentInParent<Rigidbody>();
                }
                
                rigidBody.isKinematic = false;
                StartCoroutine(EvaluateColliderAfterMatching());
            }
        }

        IEnumerator EvaluateColliderAfterMatching()
        {
            for (int i = 0; i <= frameUntilColliderReEvaluation; i++)
            {
                if(i == frameUntilColliderReEvaluation)
                {
                    Debug.Log("Evaluating Colliders");
                    
                    SendMessageToConnectedBlocks("EvaluateCollider");
                }
                yield return new WaitForFixedUpdate();
            }
            
        }

        //TODO: Tap Collider Case
        private void EvaluateCollider()
        {
            Debug.Log("Evaluate Collider for Block: " + gameObject.name);
            Destroy(tempAttach);
            GrooveOrTapHit(out List<CollisionObject> currentCollisionObjects, out OTHER_BLOCK_IS_CONNECTED_ON connectedOn);
            Dictionary<GameObject, int> blockToTapDict = new Dictionary<GameObject, int>();
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

            foreach(GameObject collidedBlock in blockToTapDict.Keys)
            {
                if(!connectedBlocks.Exists(alreadyConnected => collidedBlock.Equals(alreadyConnected.BlockRootObject)))
                {
                    FixedJoint oldJoint = gameObject.GetComponent<FixedJoint>();
                    FixedJoint fixedJoint = gameObject.AddComponent<FixedJoint>();
                    fixedJoint.connectedBody = collidedBlock.GetComponent<Rigidbody>();
                    fixedJoint.breakForce = breakForcePerPin * blockToTapDict[collidedBlock];
                    fixedJoint.breakForce = breakForcePerPin * blockToTapDict[collidedBlock];

                    
                    AddConnectedBlock(collidedBlock, fixedJoint, connectedOn);
                    OTHER_BLOCK_IS_CONNECTED_ON otherConnection;
                    if(connectedOn == OTHER_BLOCK_IS_CONNECTED_ON.GROOVE)
                    {
                        otherConnection = OTHER_BLOCK_IS_CONNECTED_ON.TAP;
                    }
                    else
                    {
                        otherConnection = OTHER_BLOCK_IS_CONNECTED_ON.GROOVE;
                    }
                    collidedBlock.GetComponent<BlockScript>().AddConnectedBlock(gameObject, fixedJoint, otherConnection);
                    BroadcastMessage("OnBlockAttach", collidedBlock);
                    collidedBlock.BroadcastMessage("OnBlockAttach", gameObject);
                }
            }
        }

        

        private void OnJointBreak(float breakForce)
        {
            Debug.Log("Joint Break");
            StartCoroutine(EvaluateJoints());
        }

        IEnumerator EvaluateJoints()
        {
            for (int i = 0; i <= frameUntilColliderReEvaluation; i++)
            {
                if (i == frameUntilColliderReEvaluation)
                {
                    RemoveBlockConnections();
                }
                yield return new WaitForFixedUpdate();
            }
            
        }

        public void OnHandTryingToPull()
        {
            RemoveBlockConnections();
        }

        public void RemoveBlockConnections()
        {
            List<BlockContainer> containerList = SearchDestroyedJoint(); 
            foreach(BlockContainer container in containerList)
            {
                connectedBlocks.Remove(container);
                container.BlockScript.RemoveBlockConnections();
                BroadcastMessage("OnBlockDetach", container.BlockRootObject, SendMessageOptions.DontRequireReceiver);
                SendMessageToConnectedBlocks("RemovedConnection");
            } 
        }

        public List<BlockContainer> SearchDestroyedJoint()
        {
            return connectedBlocks.FindAll(container => container.ConnectedJoint == null);
        }

        public bool IsDirectlyAttachedToHand()
        {
            return attachedHand != null;
        }

        public bool IsIndirectlyAttachedToHand(List<int> visitedNodes = null)
        {
            if (visitedNodes == null)
            {
                visitedNodes = new List<int>();
            }

            visitedNodes.Add(gameObject.GetHashCode());
            if (IsDirectlyAttachedToHand())
            {
                return true;
            }
            
            foreach (BlockContainer blockContainer in connectedBlocks)
            {
                if (!visitedNodes.Contains(blockContainer.BlockRootObject.GetHashCode()))
                {
                    if (blockContainer.BlockScript.IsIndirectlyAttachedToHand(visitedNodes))
                    {
                        return true;
                    }
                     
                }
            }

            return false;
        }

        public bool IsDirectlyAttachedToFloor()
        {
            foreach(BlockContainer blockContainer in connectedBlocks)
            {
                if (blockContainer.BlockRootObject.tag.Equals("Floor"))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsIndirectlyAttachedToFloor(List<int> visitedNodes = null)
        {
            if (visitedNodes == null)
            {
                visitedNodes = new List<int>();
            }

            visitedNodes.Add(gameObject.GetHashCode());
            if (IsDirectlyAttachedToFloor())
            {
                return true;
            }

            foreach (BlockContainer blockContainer in connectedBlocks)
            {
                if (!visitedNodes.Contains(blockContainer.BlockRootObject.GetHashCode()))
                {
                    if (blockContainer.BlockScript.IsIndirectlyAttachedToFloor(visitedNodes))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public void SendMessageToConnectedBlocks(string message, bool selfNotification = true, List<int> visitedNodes = null)
        {
            if(visitedNodes == null)
            {
                visitedNodes = new List<int>();
            }
            
            visitedNodes.Add(gameObject.GetHashCode());
            foreach (BlockContainer blockContainer in connectedBlocks)
            {
                if (!visitedNodes.Contains(blockContainer.BlockRootObject.GetHashCode()))
                {
                    blockContainer.BlockScript.SendMessageToConnectedBlocks(message, true, visitedNodes);
                }
            }
            if (selfNotification)
            {
                BroadcastMessage(message, SendMessageOptions.DontRequireReceiver);
            }
        }

        public void AddConnectedBlock(GameObject block, Joint connectedJoint, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            connectedBlocks.Add(new BlockContainer(block, connectedJoint, connectedOn));
        }

        public void RemoveConnectedBlock(BlockContainer container)
        {
            connectedBlocks.Remove(container);
        }

        private void GrooveOrTapHit(out List<CollisionObject> collisionList, out OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            if(tapHandler.GetCollidingObjects().Count > 0)
            {
                collisionList = tapHandler.GetCollidingObjects();
                connectedOn = OTHER_BLOCK_IS_CONNECTED_ON.TAP;
                return;
            }

            if(grooveHandler.GetCollidingObjects().Count > 0)
            {
                collisionList = grooveHandler.GetCollidingObjects();
                connectedOn = OTHER_BLOCK_IS_CONNECTED_ON.GROOVE;
                return;
            }

            collisionList = new List<CollisionObject>();
            connectedOn = OTHER_BLOCK_IS_CONNECTED_ON.NOT_CONNECTED;
        }

        
    }

    public class BlockContainer
    {
        public GameObject BlockRootObject { get; }
        public GrooveHandler GrooveHandler { get; }
        public TapHandler TapHandler { get; }
        public BlockGeometryScript BlockGeometry { get; }
        public Joint ConnectedJoint { get; }
        public OTHER_BLOCK_IS_CONNECTED_ON ConnectedOn { get; }
        public BlockScript BlockScript { get; }

        public BlockContainer(GameObject block, Joint connectedJoint, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            BlockRootObject = block;
            GrooveHandler = block.GetComponentInChildren<GrooveHandler>();
            TapHandler = block.GetComponentInChildren<TapHandler>();
            BlockGeometry = block.GetComponent<BlockGeometryScript>();
            BlockScript = block.GetComponent<BlockScript>();
            ConnectedJoint = connectedJoint;
            ConnectedOn = connectedOn;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                BlockContainer blockContainer = (BlockContainer) obj;
                return blockContainer.BlockRootObject.GetInstanceID() == BlockRootObject.GetInstanceID();
            }
        }


        public override int GetHashCode()
        {
            return BlockRootObject.GetHashCode();
        }
    }

    public enum OTHER_BLOCK_IS_CONNECTED_ON
    {
        TAP,
        GROOVE,
        NOT_CONNECTED
    }
}
