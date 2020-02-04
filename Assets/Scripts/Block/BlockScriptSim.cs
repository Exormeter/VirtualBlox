//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;


//namespace Valve.VR.InteractionSystem
//{


//    public class BlockScriptSim : MonoBehaviour
//    {
//        public List<BlockContainerSim> connectedBlocks = new List<BlockContainerSim>();
//        public List<TempConnection> tempConnectionList = new List<TempConnection>();
//        public int connectedBlocksCount;
//        //public PhysicSceneManager physicSceneManager;
//        private Rigidbody rigidBody;
//        public int frameUntilColliderReEvaluation = 2; 
//        public Guid guid;

//        public int breakForcePerPin;

//        void Start()
//        {
//            rigidBody = GetComponent<Rigidbody>();
//            //physicSceneManager = GameObject.FindGameObjectWithTag("PhysicManager").GetComponent<PhysicSceneManager>();
//            transform.gameObject.SetActive(false);
//        }


//        void Update()
//        {
//            connectedBlocksCount = connectedBlocks.Count;
//        }

//        public void ConnectBlocks(GameObject block, GameObject collidedBlock, OTHER_BLOCK_IS_CONNECTED_ON connectedOn, int connectedPinsCount)
//        {
//            FixedJoint joint = block.AddComponent<FixedJoint>();
//            joint.connectedBody = collidedBlock.GetComponent<Rigidbody>();
//            joint.breakForce = Mathf.Infinity;
//            joint.breakTorque = breakForcePerPin * connectedPinsCount;
//            //ConfigurableJoint joint = SetConfigurableJoint(collidedBlock.GetComponent<Rigidbody>(), blockToTapDict[collidedBlock]);

            

//            OTHER_BLOCK_IS_CONNECTED_ON otherConnection;
//            if (connectedOn == OTHER_BLOCK_IS_CONNECTED_ON.GROOVE)
//            {
//                otherConnection = OTHER_BLOCK_IS_CONNECTED_ON.TAP;
//            }
//            else
//            {
//                otherConnection = OTHER_BLOCK_IS_CONNECTED_ON.GROOVE;
//            }

//            AddConnectedBlock(collidedBlock, joint, connectedOn, connectedPinsCount);
//            collidedBlock.GetComponent<BlockScriptSim>().AddConnectedBlock(transform.gameObject, joint, otherConnection, connectedPinsCount);
//            BroadcastMessage("OnBlockAttach", collidedBlock, SendMessageOptions.DontRequireReceiver);
//            collidedBlock.BroadcastMessage("OnBlockAttach", transform.gameObject, SendMessageOptions.DontRequireReceiver);
//        }



//        private void OnJointBreak(float breakForce)
//        {
//            Debug.Log("Joint Break Simulation");
//            StartCoroutine(EvaluateJoints());
//        }

//        IEnumerator EvaluateJoints()
//        {
//            for (int i = 0; i <= frameUntilColliderReEvaluation; i++)
//            {
//                if (i == frameUntilColliderReEvaluation)
//                {
//                    RemoveBlockConnections();
//                }
//                yield return new WaitForFixedUpdate();
//            }

//        }

//        public void RemoveBlockConnections()
//        {
//            List<BlockContainerSim> containerList = SearchDestroyedJoint();
//            foreach (BlockContainerSim container in containerList)
//            {
//                connectedBlocks.Remove(container);
//                container.BlockScriptSim.RemoveBlockConnections();
//                BroadcastMessage("OnBlockDetach", container.BlockRootObject, SendMessageOptions.DontRequireReceiver);
//                SendMessageToConnectedBlocks("RemovedConnection");
//                //physicSceneManager.JointBreak(guid, container.BlockScriptSim.guid);
//            }
//        }

//        public void MatchTwinBlock(GameObject realBlock)
//        {
//            transform.gameObject.SetActive(true);
//            transform.SetPositionAndRotation(realBlock.transform.position, realBlock.transform.rotation);
//        }

//        public void ConnectBlocksAfterMatching(GameObject realBlock)
//        {
//            foreach (BlockContainer blockContainerReal in realBlock.GetComponent<BlockCommunication>().ConnectedBlocks)
//            {
//                //GameObject containerSim = physicSceneManager.GetSimBlockByGuid(blockContainerReal.BlockCommunication.Guid);
//                if (!connectedBlocks.Exists(alreadyConnected => containerSim.Equals(alreadyConnected.BlockRootObject)))
//                {
//                    containerSim.SetActive(true);
//                    ConnectBlocks(transform.gameObject, containerSim, blockContainerReal.ConnectedOn, blockContainerReal.ConnectedPinCount);
//                }
//            }
//        }

//        public List<BlockContainerSim> SearchDestroyedJoint()
//        {
//            return connectedBlocks.FindAll(container => container.ConnectedJoint == null);
//        }


//        public void SendMessageToConnectedBlocks(string message, bool selfNotification = true, List<int> visitedNodes = null)
//        {
//            if (visitedNodes == null)
//            {
//                visitedNodes = new List<int>();
//            }

//            visitedNodes.Add(gameObject.GetHashCode());
//            foreach (BlockContainerSim blockContainer in connectedBlocks)
//            {
//                if (!visitedNodes.Contains(blockContainer.BlockRootObject.GetHashCode()))
//                {
//                    blockContainer.BlockScriptSim.SendMessageToConnectedBlocks(message, true, visitedNodes);
//                }
//            }
//            if (selfNotification)
//            {
//                BroadcastMessage(message, SendMessageOptions.DontRequireReceiver);
//            }
//        }

//        public void AddConnectedBlock(GameObject block, Joint connectedJoint, OTHER_BLOCK_IS_CONNECTED_ON connectedOn, int connectedPinsCount)
//        {
//            connectedBlocks.Add(new BlockContainerSim(block, connectedJoint, connectedOn, connectedPinsCount));
//        }

//        public void RemoveConnectedBlock(BlockContainerSim container)
//        {
//            connectedBlocks.Remove(container);
//        }

//        public void RemoveConnectedBlock(GameObject block)
//        {
//            connectedBlocks.RemoveAll(connectedBlock => connectedBlock.BlockRootObject.GetHashCode() == block.GetHashCode());
//        }

//        public void SetKinematic()
//        {
//            if (!gameObject.tag.Equals("Floor"))
//            {
//                rigidBody.isKinematic = true;
//            }
//        }

//        public void UnsetKinematic()
//        {
//            if (!gameObject.tag.Equals("Floor"))
//            {
//                rigidBody.isKinematic = false;
//            }
//        }

//        private ConfigurableJoint SetConfigurableJoint(Rigidbody connectedBody, int connectedPinCount)
//        {
//            ConfigurableJoint configurableJoint = gameObject.AddComponent<ConfigurableJoint>();
//            configurableJoint.xMotion = ConfigurableJointMotion.Locked;
//            configurableJoint.yMotion = ConfigurableJointMotion.Locked;
//            configurableJoint.zMotion = ConfigurableJointMotion.Locked;
//            configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
//            configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
//            configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
//            //configurableJoint.autoConfigureConnectedAnchor = true;
//            configurableJoint.projectionMode = JointProjectionMode.PositionAndRotation;
//            configurableJoint.projectionAngle = 0.001f;
//            configurableJoint.projectionDistance = 0.001f;
//            //configurableJoint.connectedAnchor = connectedBody.position;
//            //configurableJoint.anchor = gameObject.transform.position;
//            //configurableJoint.enableCollision = true;
//            configurableJoint.breakForce = connectedPinCount * breakForcePerPin * 10;
//            configurableJoint.breakTorque = connectedPinCount * breakForcePerPin;
//            configurableJoint.connectedBody = connectedBody;
//            return configurableJoint;
//        }

//        public void DisableTwin()
//        {
//            foreach(BlockContainerSim blockContainerSim in connectedBlocks)
//            {
//                blockContainerSim.BlockScriptSim.RemoveConnectedBlock(transform.gameObject);
//                Destroy(blockContainerSim.ConnectedJoint);
//            }
//            connectedBlocks.Clear();
//            transform.gameObject.SetActive(false);
//        }
//    }



//    public class BlockContainerSim
//    {
//        public GameObject BlockRootObject { get; }

//        public Joint ConnectedJoint { get; }
//        public OTHER_BLOCK_IS_CONNECTED_ON ConnectedOn { get; }
//        public BlockScriptSim BlockScriptSim { get; }

//        public int ConnectedPinsCount {get;}

//        public BlockContainerSim(GameObject block, Joint connectedJoint, OTHER_BLOCK_IS_CONNECTED_ON connectedOn, int connectedPinsCount )
//        {
//            BlockRootObject = block;
//            BlockScriptSim = block.GetComponent<BlockScriptSim>();
//            ConnectedJoint = connectedJoint;
//            ConnectedOn = connectedOn;
//            ConnectedPinsCount = connectedPinsCount;
//        }

//        public override bool Equals(object obj)
//        {
//            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
//            {
//                return false;
//            }
//            else
//            {
//                BlockContainerSim blockContainer = (BlockContainerSim)obj;
//                return blockContainer.BlockRootObject.GetInstanceID() == BlockRootObject.GetInstanceID();
//            }
//        }


//        public override int GetHashCode()
//        {
//            return BlockRootObject.GetHashCode();
//        }
//    }

//    public struct TempConnection
//    {
//        public GameObject otherBlock;
//        public int jointStrength;
//        public OTHER_BLOCK_IS_CONNECTED_ON connected_on;

//        public TempConnection(GameObject otherBlock, int jointStrength, OTHER_BLOCK_IS_CONNECTED_ON connected_on)
//        {
//            this.otherBlock = otherBlock;
//            this.jointStrength = jointStrength;
//            this.connected_on = connected_on;
//        }
//    }
//}
