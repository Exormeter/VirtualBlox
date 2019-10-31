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
        public PhysicSceneManager physicSceneManager;
        private GrooveHandler grooveHandler;
        private TapHandler tapHandler;
        private Hand attachedHand = null;
        private BlockGeometryScript blockGeometry;
        private Rigidbody rigidBody;
        private BlockScriptSim blockScriptSim;
        public Guid guid = Guid.NewGuid();

        public int breakForcePerPin = 3;
        public bool IsFroozen;
        public bool UnfroozenForPhsicsCheck;
        public int frameUntilColliderReEvaluation;

        void Start()
        {
            grooveHandler = GetComponentInChildren<GrooveHandler>();
            tapHandler = GetComponentInChildren<TapHandler>();
            blockGeometry = GetComponent<BlockGeometryScript>();
            rigidBody = GetComponent<Rigidbody>();
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

        
        void Update()
        {
            connectedBlocksCount = connectedBlocks.Count;
        }


        public void OnAttachedToHand(Hand hand)
        {
            attachedHand = hand;
            SendMessageToConnectedBlocks("OnIndirectAttachedtoHand");
        } 


        
        public void OnDetachedFromHand(Hand hand)
        {
            attachedHand = null;
            BlockScript block = FindFirstCollidingBlock();
            if(block != null)
            {
                block.GetComponent<BlockScript>().GrooveOrTapHit(out List<CollisionObject> currentCollisionObjects, out OTHER_BLOCK_IS_CONNECTED_ON connectedOn);
                block.MatchRotationWithBlock(currentCollisionObjects, connectedOn);

                //Richte alle Blöcke nach Plazieren des ersten Blockes korrekt aus, alte Joints müssen vermutlich neu gesetzt werden 
                block.SendMessageToConnectedBlocksBFS("MatchRotationWitchBlock");
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

        public void MatchRotationWithBlock(List<CollisionObject> currentCollisionObjects, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            if (currentCollisionObjects.Count > 1)
            {
                SendMessageToConnectedBlocks("SetKinematic");
                GetComponent<BlockRotator>().RotateBlock(currentCollisionObjects, connectedOn);
                StartCoroutine(EvaluateColliderAfterMatching());
            }
        }

        IEnumerator EvaluateColliderAfterMatching()
        {
            for (int i = 0; i <= frameUntilColliderReEvaluation; i++)
            {
                if(i == frameUntilColliderReEvaluation)
                {
                    List<GameObject> connectedBlocksBeforeEvaluation = GetCurrentlyConnectedBlocks();
                    SendMessageToConnectedBlocks("EvaluateCollider");
                    if (IsIndirectlyAttachedToFloor())
                    {
                        SyncronizePhysicBlocks(connectedBlocksBeforeEvaluation);
                    }
                    SendMessageToConnectedBlocks("CheckFreeze"); 
                    SendMessageToConnectedBlocks("UnsetKinematic"); 
                    physicSceneManager.StartSimulation();
                }
                yield return new WaitForFixedUpdate();
            }
            
        }

        private void SyncronizePhysicBlocks(List<GameObject> connectedBlocksBeforeEvaluation)
        {
            foreach (GameObject connectedBlockInHand in connectedBlocksBeforeEvaluation)
            {
                connectedBlockInHand.GetComponent<BlockScript>().blockScriptSim.MatchTwinBlock(connectedBlockInHand);
            }
            foreach (GameObject connectedBlockInHand in connectedBlocksBeforeEvaluation)
            {
                connectedBlockInHand.GetComponent<BlockScript>().blockScriptSim.ConnectBlocksAfterMatching(connectedBlockInHand);
            }
        }

        private List<GameObject> GetCurrentlyConnectedBlocks(List<GameObject> visitedNodes = null)
        {
            if (visitedNodes == null)
            {
                visitedNodes = new List<GameObject>();
            }

            visitedNodes.Add(transform.gameObject);

            foreach (BlockContainer blockContainer in connectedBlocks)
            {

                if (!visitedNodes.Exists(block => block.GetHashCode() == blockContainer.BlockRootObject.GetHashCode()) )
                {
                    blockContainer.BlockScript.GetCurrentlyConnectedBlocks(visitedNodes);
                }
            }

            return visitedNodes;
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
            foreach(GameObject collidedBlock in blockToTapDict.Keys)
            {
                if(!connectedBlocks.Exists(alreadyConnected => collidedBlock.Equals(alreadyConnected.BlockRootObject)))
                {
                    ConnectBlocks(transform.gameObject, collidedBlock, blockToTapDict[collidedBlock], connectedOn);
                }
            }
        }

        public void ConnectBlocks(GameObject block, GameObject collidedBlock, int jointStrength, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
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

            collidedBlock.GetComponent<BlockScript>().AddConnectedBlock(transform.gameObject, joint, otherConnection);
            BroadcastMessage("OnBlockAttach", collidedBlock, SendMessageOptions.DontRequireReceiver);
            collidedBlock.BroadcastMessage("OnBlockAttach", transform.gameObject, SendMessageOptions.DontRequireReceiver);
        }

        

        private void OnJointBreak(float breakForce)
        {
            Debug.Log("Joint Break");
            StartCoroutine(EvaluateJoints());
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

        public void RemoveBlockConnections()
        {
            List<BlockContainer> containerList = SearchDestroyedJoint();
            Debug.Log("Found Destroyed Joints: " + containerList.Count);
            foreach (BlockContainer container in containerList)
            {
                connectedBlocks.Remove(container);
                container.BlockScript.RemoveBlockConnections();
                BroadcastMessage("OnBlockDetach", container.BlockRootObject, SendMessageOptions.DontRequireReceiver);
                SendMessageToConnectedBlocks("RemovedConnection"); 
            }
            SendMessageToConnectedBlocks("CheckFreeze");
        }


        public void OnHandTryingToPull()
        {
            RemoveBlockConnections();
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

        public void SendMessageToConnectedBlocksBFS(String message)
        {
            Queue<GameObject> blocksToVisit = new Queue<GameObject>();
            List<int> visitedBlocks = new List<int>();
            blocksToVisit.Enqueue(transform.gameObject);
            visitedBlocks.Add(transform.gameObject.GetHashCode());

            while(blocksToVisit.Count > 0)
            {
                GameObject currentBlock = blocksToVisit.Dequeue();
                currentBlock.SendMessage(message);
                foreach(BlockContainer currentBlockNeighbours in currentBlock.GetComponent<BlockScript>().connectedBlocks) {

                    if (!visitedBlocks.Contains(currentBlockNeighbours.BlockRootObject.GetHashCode())){
                        blocksToVisit.Enqueue(currentBlockNeighbours.BlockRootObject);
                        visitedBlocks.Add(currentBlockNeighbours.BlockRootObject.GetHashCode());
                    }
                }
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
            if (IsIndirectlyAttachedToFloor())
            {
                FreezeBlock();
            }
            else
            {
                UnfreezeBlock();
                blockScriptSim.DisableTwin();
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

    

    public class BlockContainer
    {
        public GameObject BlockRootObject { get; }
        public GrooveHandler GrooveHandler { get; }
        public TapHandler TapHandler { get; }
        public BlockGeometryScript BlockGeometry { get; }
        public Joint ConnectedJoint { set;  get; }
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
