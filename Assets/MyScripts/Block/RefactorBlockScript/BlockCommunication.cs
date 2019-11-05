using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{
    public class BlockCommunication : MonoBehaviour
    {
        public List<BlockContainer> connectedBlocks = new List<BlockContainer>();
        public Guid guid = Guid.NewGuid();
        public PhysicSceneManager physicSceneManager;
        public BlockScriptSim blockScriptSim;
        public int frameUntilColliderReEvaluation;


        void Start()
        {
            physicSceneManager = GameObject.FindGameObjectWithTag("PhysicManager").GetComponent<PhysicSceneManager>();
            physicSceneManager.AddGameObjectRefInGame(transform.gameObject);
            if (!physicSceneManager.AlreadyExisits(guid))
            {
                GameObject twinBlock = Instantiate(transform.gameObject);
                twinBlock.AddComponent<BlockScriptSim>();
                twinBlock.GetComponent<BlockScriptSim>().breakForcePerPin = breakForcePerPin;
                twinBlock.GetComponent<BlockScriptSim>().guid = guid;
                blockScriptSim = twinBlock.GetComponent<BlockScriptSim>();
                physicSceneManager.AddGameObjectToPhysicScene(twinBlock);
            }
        }

        public List<GameObject> GetCurrentlyConnectedBlocks(List<GameObject> visitedNodes = null)
        {
            if (visitedNodes == null)
            {
                visitedNodes = new List<GameObject>();
            }

            visitedNodes.Add(transform.gameObject);

            foreach (BlockContainer blockContainer in connectedBlocks)
            {

                if (!visitedNodes.Exists(block => block.GetHashCode() == blockContainer.BlockRootObject.GetHashCode()))
                {
                    blockContainer.BlockCommunication.GetCurrentlyConnectedBlocks(visitedNodes);
                }
            }

            return visitedNodes;
        }

        public GameObject FindFirstCollidingBlock(List<int> visitedNodes = null)
        {
            if (visitedNodes == null)
            {
                visitedNodes = new List<int>();
            }

            visitedNodes.Add(gameObject.GetHashCode());

            if (GetComponent<GrooveHandler>().GetCollidingObjects().Count > 0 || GetComponent<TapHandler>().GetCollidingObjects().Count > 0)
            {
                return transform.gameObject;
            }

            foreach (BlockContainer blockContainer in connectedBlocks)
            {
                if (!visitedNodes.Contains(blockContainer.BlockRootObject.GetHashCode()))
                {
                    GameObject tempScript = blockContainer.BlockCommunication.FindFirstCollidingBlock(visitedNodes);
                    if (tempScript != null)
                    {
                        return tempScript;
                    }

                }
            }

            return null;
        }

        public void RemoveBlockConnections()
        {
            List<BlockContainer> containerList = SearchDestroyedJoint();
            Debug.Log("Found Destroyed Joints: " + containerList.Count);
            foreach (BlockContainer container in containerList)
            {
                connectedBlocks.Remove(container);
                container.BlockCommunication.RemoveBlockConnections();
                BroadcastMessage("OnBlockDetach", container.BlockRootObject, SendMessageOptions.DontRequireReceiver);
                SendMessageToConnectedBlocks("RemovedConnection");
            }
            SendMessageToConnectedBlocks("CheckFreeze");
        }

        

        public List<BlockContainer> SearchDestroyedJoint()
        {
            return connectedBlocks.FindAll(container => container.ConnectedJoint == null);
        }

        public void SendMessageToConnectedBlocks(string message, bool selfNotification = true, List<int> visitedNodes = null)
        {
            if (visitedNodes == null)
            {
                visitedNodes = new List<int>();
            }

            visitedNodes.Add(gameObject.GetHashCode());
            foreach (BlockContainer blockContainer in connectedBlocks)
            {
                if (!visitedNodes.Contains(blockContainer.BlockRootObject.GetHashCode()))
                {
                    blockContainer.BlockCommunication.SendMessageToConnectedBlocks(message, true, visitedNodes);
                }
            }
            if (selfNotification)
            {
                BroadcastMessage(message, SendMessageOptions.DontRequireReceiver);
            }
        }

        public void SendMessageToConnectedBlocksBFS(String message)
        {
            Queue<GameObject> blocksToVisit = new Queue<GameObject>();
            List<int> visitedBlocks = new List<int>();
            blocksToVisit.Enqueue(transform.gameObject);
            visitedBlocks.Add(transform.gameObject.GetHashCode());

            while (blocksToVisit.Count > 0)
            {
                GameObject currentBlock = blocksToVisit.Dequeue();
                currentBlock.SendMessage(message);
                foreach (BlockContainer currentBlockNeighbours in currentBlock.GetComponent<BlockCommunication>().connectedBlocks)
                {

                    if (!visitedBlocks.Contains(currentBlockNeighbours.BlockRootObject.GetHashCode()))
                    {
                        blocksToVisit.Enqueue(currentBlockNeighbours.BlockRootObject);
                        visitedBlocks.Add(currentBlockNeighbours.BlockRootObject.GetHashCode());
                    }
                }
            }

        }

        public void ConnectBlocks(GameObject block, GameObject collidedBlock, int jointStrength, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            if(connectedBlocks.Exists(alreadyConnected => collidedBlock.Equals(alreadyConnected.BlockRootObject))){
                return;
            }

            FixedJoint joint = block.AddComponent<FixedJoint>();
            joint.connectedBody = collidedBlock.GetComponent<Rigidbody>();
            joint.breakForce = Mathf.Infinity;
            joint.breakTorque = breakForcePerPin * jointStrength;
            //ConfigurableJoint joint = SetConfigurableJoint(collidedBlock.GetComponent<Rigidbody>(), blockToTapDict[collidedBlock]);

            AddConnectedBlock(collidedBlock, joint, connectedOn);

            OTHER_BLOCK_IS_CONNECTED_ON otherConnection;
            if (connectedOn == OTHER_BLOCK_IS_CONNECTED_ON.GROOVE)
            {
                otherConnection = OTHER_BLOCK_IS_CONNECTED_ON.TAP;
            }
            else
            {
                otherConnection = OTHER_BLOCK_IS_CONNECTED_ON.GROOVE;
            }

            collidedBlock.GetComponent<BlockCommunication>().AddConnectedBlock(transform.gameObject, joint, otherConnection);
            BroadcastMessage("OnBlockAttach", collidedBlock, SendMessageOptions.DontRequireReceiver);
            collidedBlock.BroadcastMessage("OnBlockAttach", transform.gameObject, SendMessageOptions.DontRequireReceiver);
        }

        public void AddConnectedBlock(GameObject block, Joint connectedJoint, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            connectedBlocks.Add(new BlockContainer(block, connectedJoint, connectedOn));
        }

        public void RemoveConnectedBlock(BlockContainer container)
        {
            connectedBlocks.Remove(container);
        }

        public bool IsDirectlyAttachedToFloor()
        {
            foreach (BlockContainer blockContainer in connectedBlocks)
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

        public void RemoveJointViaSimulation(Guid connectedBlockGuid)
        {
            foreach (BlockContainer blockContainer in connectedBlocks)
            {
                if (blockContainer.BlockScript.guid == connectedBlockGuid)
                {
                    Destroy(blockContainer.ConnectedJoint);
                    StartCoroutine(EvaluateJoints());
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
        public AttachFloorHandler AttachFloorHandler { get; }
        public AttachHandHandler AttachHandHandler { get; }
        public BlockCommunication BlockCommunication{ get;}

        public BlockContainer(GameObject block, Joint connectedJoint, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            BlockRootObject = block;
            GrooveHandler = block.GetComponentInChildren<GrooveHandler>();
            TapHandler = block.GetComponentInChildren<TapHandler>();
            BlockGeometry = block.GetComponent<BlockGeometryScript>();
            BlockScript = block.GetComponent<BlockScript>();
            BlockCommunication = block.GetComponent<BlockCommunication>();
            AttachHandHandler = block.GetComponent<AttachHandHandler>();
            AttachFloorHandler = block.GetComponent<AttachFloorHandler>();
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
                BlockContainer blockContainer = (BlockContainer)obj;
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
