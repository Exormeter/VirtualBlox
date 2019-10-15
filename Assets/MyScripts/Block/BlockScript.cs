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
            if (grooveHandler.GetCollidingObjects().Count > 0)
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
            List<CollisionObject> currentCollisionObjects = grooveHandler.GetCollidingObjects();
            if (currentCollisionObjects.Count > 1)
            {
                Rigidbody rigidBody = GetComponent<Rigidbody>();
                rigidBody.isKinematic = true;
                GetComponent<BlockRotator>().RotateBlock(currentCollisionObjects);

                tempAttach = gameObject.AddComponent<FixedJoint>();
                tempAttach.connectedBody = currentCollisionObjects[0].TapPosition.GetComponentInParent<Rigidbody>();
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
            Debug.Log("Evaluate Collier for Block: " + gameObject.name);
            Destroy(tempAttach);
            Dictionary<GameObject, List<GameObject>> blockToTapDict = new Dictionary<GameObject, List<GameObject>>();
            foreach (CollisionObject collisionObject in grooveHandler.GetCollidingObjects())
            {
                if (!blockToTapDict.ContainsKey(collisionObject.CollidedBlock))
                {
                    blockToTapDict.Add(collisionObject.CollidedBlock, new List<GameObject>(new GameObject[] { collisionObject.TapPosition }));
                }
                else
                {
                    blockToTapDict[collisionObject.CollidedBlock].Add(collisionObject.TapPosition);
                }
            }
            foreach(GameObject collidedBlock in blockToTapDict.Keys)
            {
                if(!connectedBlocks.Exists(alreadyConnected => collidedBlock.Equals(alreadyConnected.BlockRootObject)))
                {
                    FixedJoint oldJoint = gameObject.GetComponent<FixedJoint>();
                    FixedJoint fixedJoint = gameObject.AddComponent<FixedJoint>();
                    fixedJoint.connectedBody = collidedBlock.GetComponent<Rigidbody>();
                    fixedJoint.breakForce = breakForcePerPin * blockToTapDict[collidedBlock].Count;

                    
                    AddConnectedBlock(collidedBlock, fixedJoint, OtherBlockConnectedOn.GROOVE);
                    collidedBlock.GetComponent<BlockScript>().AddConnectedBlock(gameObject, fixedJoint, OtherBlockConnectedOn.GROOVE);
                    BroadcastMessage("OnBlockAttach", collidedBlock);
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

        public void AddConnectedBlock(GameObject block, Joint connectedJoint, OtherBlockConnectedOn connectedOn)
        {
            connectedBlocks.Add(new BlockContainer(block, connectedJoint, connectedOn));
        }

        public void RemoveConnectedBlock(BlockContainer container)
        {
            connectedBlocks.Remove(container);
        }

        
    }

    public class BlockContainer
    {
        public GameObject BlockRootObject { get; }
        public GrooveHandler GrooveHandler { get; }
        public TapHandler TapHandler { get; }
        public BlockGeometryScript BlockGeometry { get; }
        public Joint ConnectedJoint { get; }
        public OtherBlockConnectedOn ConnectedOn { get; }
        public BlockScript BlockScript { get; }

        public BlockContainer(GameObject block, Joint connectedJoint, OtherBlockConnectedOn connectedOn)
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

    public enum OtherBlockConnectedOn
    {
        TAP,
        GROOVE
    }
}
