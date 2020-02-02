using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{
    public class BlockCommunication : MonoBehaviour
    {

        public int connectedBlockCount;

        /// <summary>
        /// The Blocks that are directly connected to this Block
        /// </summary>
        private List<BlockContainer> connectedBlocks = new List<BlockContainer>();
        public List<BlockContainer> ConnectedBlocks => connectedBlocks;

        /// <summary>
        /// Guid do identify the Block, unique for every Block
        /// </summary>
        private Guid _guid = Guid.NewGuid();
        public Guid Guid
        {
            get => _guid;
            set
            {
                //A new Guid can be set, in this case the new Guid is relayed to the BlockManager, so that
                //the block can be referenced by the new Guid
                blockManager = GameObject.FindGameObjectWithTag("BlockManager").GetComponent<BlockManager>();
                blockManager.ChangeGuid(_guid, value, this.gameObject);
                _guid = value;
                Debug.Log(value.ToString());
            }
        }

        /// <summary>
        /// BlockManager to access all Blocks by Guid
        /// </summary>
        public BlockManager blockManager;

        
        public int frameUntilColliderReEvaluation;
        public int breakForcePerPin;

        private void Awake()
        {
            
            blockManager = GameObject.FindGameObjectWithTag("BlockManager").GetComponent<BlockManager>();
            blockManager.AddBlock(Guid, this.gameObject);
        }
        void Start()
        {
            //physicSceneManager = GameObject.FindGameObjectWithTag("PhysicManager").GetComponent<PhysicSceneManager>();
            //physicSceneManager.AddGameObjectRefInGame(transform.gameObject);
            //if (!physicSceneManager.AlreadyExisits(Guid))
            //{
                
            //    GameObject twinBlock = Instantiate(transform.gameObject);
                
            //    twinBlock.AddComponent<BlockScriptSim>();
            //    twinBlock.GetComponent<BlockScriptSim>().breakForcePerPin = breakForcePerPin;
            //    twinBlock.GetComponent<BlockScriptSim>().guid = Guid;
            //    blockScriptSim = twinBlock.GetComponent<BlockScriptSim>();
            //    physicSceneManager.AddGameObjectToPhysicScene(twinBlock);
            //}
        }

        private void Update()
        {
            connectedBlockCount = ConnectedBlocks.Count;
        }

        /// <summary>
        /// Gets all Blocks that are currently directly and indirectly attached to the Block.
        /// Useful when a Structure of mutiple Blocks is attached to the hand. This function
        /// is recursive.
        /// </summary>
        /// <param name="visitedNodes">Keeps track of visted nodes throu recursion</param>
        /// <returns>List of all connected GameObjects</returns>
        public List<GameObject> GetCurrentlyConnectedBlocks(List<GameObject> visitedNodes = null)
        {
            if (visitedNodes == null)
            {
                visitedNodes = new List<GameObject>();
            }

            visitedNodes.Add(transform.gameObject);

            foreach (BlockContainer blockContainer in ConnectedBlocks)
            {

                if (!visitedNodes.Exists(block => block.GetHashCode() == blockContainer.BlockRootObject.GetHashCode()))
                {
                    blockContainer.BlockCommunication.GetCurrentlyConnectedBlocks(visitedNodes);
                }
            }

            return visitedNodes;
        }


        /// <summary>
        /// Clears all ConnectedBlocks
        /// </summary>
        public void ClearConnectedBlocks()
        {
            connectedBlocks.Clear();
        }

        /// <summary>
        /// Searches the connected and indirectly connected Blocks for the first Block that is colliding with an
        /// other Block. Colliding in this case means that the GrooveHandler or TapHandler has CollisionObjects which
        /// are overlapping with an other Grove- or TapCollider, but are not connected.
        /// </summary>
        /// <param name="visitedNodes">Keeps track of visited Blocks</param>
        /// <returns>The first GameObject that has a unconnected CollisionObject</returns>
        public GameObject FindFirstCollidingBlock(List<int> visitedNodes = null)
        {
            if (visitedNodes == null)
            {
                visitedNodes = new List<int>();
            }

            visitedNodes.Add(gameObject.GetHashCode());

            if (GetComponentInChildren<GrooveHandler>().GetCollidingObjects().Count > 0 || GetComponentInChildren<TapHandler>().GetCollidingObjects().Count > 0)
            {
                return transform.gameObject;
            }

            foreach (BlockContainer blockContainer in ConnectedBlocks)
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

        /// <summary>
        /// Removes all connections from the other Blocks that have this Block
        /// in their connectedBlock List
        /// </summary>
        public void RemoveAllBlockConnections()
        {
            foreach (BlockContainer container in connectedBlocks)
            {
                container.BlockCommunication.RemoveBlockByGuid(Guid);
            }   
        }

        /// <summary>
        /// Removes a Block with a specific Guid from the connected Blocks
        /// </summary>
        /// <param name="guid">The Guid to Remove</param>
        private void RemoveBlockByGuid(Guid guid)
        {
            ConnectedBlocks.RemoveAll(container => container.Guid == guid);
        }


        /// <summary>
        /// Removes all Connections where the Joint has broken. This methods also
        /// removes the Connected Block from the other Block. A CheckFreeze message is
        /// send to all remaining connected Blocks to check if the Rigidbody Contraints should
        /// be lifted as a direct or indirect connection to Fllor might be served.
        /// </summary>
        public void RemoveBlockConnectionsWithoutJoint()
        {
            List<BlockContainer> containerList = SearchDestroyedJoint();
            Debug.Log("Found Destroyed Joints: " + containerList.Count);
            foreach (BlockContainer container in containerList)
            {
                ConnectedBlocks.Remove(container);
                container.BlockCommunication.RemoveBlockConnectionsWithoutJoint();
                BroadcastMessage("OnBlockDetach", container.BlockRootObject, SendMessageOptions.DontRequireReceiver);
                SendMessageToConnectedBlocks("RemovedConnection");
            }
            SendMessageToConnectedBlocks("CheckFreeze");
        }


        /// <summary>
        /// Searches the Connected Blocks for a Joint that is null. This would indicate that the two Blocks
        /// are no longer connected.
        /// </summary>
        /// <returns></returns>
        public List<BlockContainer> SearchDestroyedJoint()
        {
            return ConnectedBlocks.FindAll(container => container.ConnectedJoint == null);
        }

        /// <summary>
        /// Sends a message to all directly and indirectly connected Blocks via a recusive depth search algorithmus.
        /// Message is received by all Components on the Block and it's childs.
        /// </summary>
        /// <param name="message">The Method name to call</param>
        /// <param name="selfNotification">Should the Block that sends the message message itself</param>
        /// <param name="visitedNodes">Keeps track of alreadz notified Blocks</param>
        public void SendMessageToConnectedBlocks(string message, bool selfNotification = true, List<int> visitedNodes = null)
        {
            if (visitedNodes == null)
            {
                visitedNodes = new List<int>();
            }

            visitedNodes.Add(gameObject.GetHashCode());
            foreach (BlockContainer blockContainer in ConnectedBlocks)
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


        /// <summary>
        /// Send a message to all Blocks via a breadth-first search, needed in some caeses like when to algin a Block
        /// on an Block that has already aligned.
        /// Message is only reviced by Components in the Block, not the children.
        /// </summary>
        /// <param name="message">The Method name to call</param>
        public void SendMessageToConnectedBlocksBFS(string message)
        {
            Queue<GameObject> blocksToVisit = new Queue<GameObject>();
            List<int> visitedBlocks = new List<int>();
            blocksToVisit.Enqueue(transform.gameObject);
            visitedBlocks.Add(transform.gameObject.GetHashCode());

            while (blocksToVisit.Count > 0)
            {
                GameObject currentBlock = blocksToVisit.Dequeue();
                currentBlock.SendMessage(message);
                foreach (BlockContainer currentBlockNeighbours in currentBlock.GetComponent<BlockCommunication>().ConnectedBlocks)
                {

                    if (!visitedBlocks.Contains(currentBlockNeighbours.BlockRootObject.GetHashCode()))
                    {
                        blocksToVisit.Enqueue(currentBlockNeighbours.BlockRootObject);
                        visitedBlocks.Add(currentBlockNeighbours.BlockRootObject.GetHashCode());
                    }
                }
            }

        }

        /// <summary>
        /// Adds the Block to the List of connected Block, if it isn't alredy in the List. It creates a Joint between both Blocks and keeps track
        /// if the other Block is connected on a Tap or a Groove and how many Pins connected them. This method is then called for the colliding Block
        /// where this Block is added to the List. 
        /// </summary>
        /// <param name="block">This Block</param>
        /// <param name="collidedBlock">The other Block</param>
        /// <param name="connectedPinCount">How many Pins connected them</param>
        /// <param name="connectedOn">Is the other Block connedted via Grooves or Taps</param>
        public void ConnectBlocks(GameObject block, GameObject collidedBlock, int connectedPinCount, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            if (ConnectedBlocks.Exists(alreadyConnected => collidedBlock.Equals(alreadyConnected.BlockRootObject))) {
                return;
            }

            FixedJoint joint = block.AddComponent<FixedJoint>();
            joint.connectedBody = collidedBlock.GetComponent<Rigidbody>();
            joint.breakForce = Mathf.Infinity;
            joint.breakTorque = Mathf.Infinity;
            //ConfigurableJoint joint = SetConfigurableJoint(collidedBlock.GetComponent<Rigidbody>(), blockToTapDict[collidedBlock]);

            OTHER_BLOCK_IS_CONNECTED_ON otherConnection;
            if (connectedOn == OTHER_BLOCK_IS_CONNECTED_ON.GROOVE)
            {
                otherConnection = OTHER_BLOCK_IS_CONNECTED_ON.TAP;
            }
            else
            {
                otherConnection = OTHER_BLOCK_IS_CONNECTED_ON.GROOVE;
            }

            AddConnectedBlock(collidedBlock, joint, connectedOn, connectedPinCount);
            collidedBlock.GetComponent<BlockCommunication>().AddConnectedBlock(transform.gameObject, joint, otherConnection, connectedPinCount);
            BroadcastMessage("OnBlockAttach", collidedBlock, SendMessageOptions.DontRequireReceiver);
            collidedBlock.BroadcastMessage("OnBlockAttach", transform.gameObject, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Adds a BlockContainer to the Connected Block List
        /// </summary>
        /// <param name="collidedBlock">The other Block</param>
        /// <param name="connectedPinCount">How many Pins connected them</param>
        /// <param name="connectedOn">Is the other Block connedted via Grooves or Taps</param>
        public void AddConnectedBlock(GameObject collidedBlock, Joint connectedJoint, OTHER_BLOCK_IS_CONNECTED_ON connectedOn, int connectedPinCount)
        {
            ConnectedBlocks.Add(new BlockContainer(collidedBlock, connectedJoint, connectedOn, connectedPinCount));
        }


        /// <summary>
        /// Returns true if this Block is a Floor plate
        /// </summary>
        /// <returns>True if Block is Floor</returns>
        public bool IsFloor()
        {
            return transform.gameObject.tag.Equals("Floor");
        }

        /// <summary>
        /// Returns true if the Block has a Connection in it's List that is a Floor plate,
        /// making it directly connected ti the Floor
        /// </summary>
        /// <returns>True if directlyAttached to the Floor</returns>
        public bool IsDirectlyAttachedToFloor()
        {
            foreach (BlockContainer blockContainer in ConnectedBlocks)
            {
                if (blockContainer.BlockRootObject.tag.Equals("Floor"))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the Block is indirecty connected to the Floor, meaning that there
        /// is a way from the connectedBlocks to a Floor plate
        /// </summary>
        /// <param name="visitedNodes">Keep track of visited Block</param>
        /// <returns>True if indirectly connected to Floor</returns>
        public bool IsIndirectlyAttachedToFloor(List<int> visitedNodes = null)
        {
            if (visitedNodes == null)
            {
                visitedNodes = new List<int>();
            }

            visitedNodes.Add(gameObject.GetHashCode());
            if (IsFloor() || IsDirectlyAttachedToFloor())
            {
                return true;
            }

            foreach (BlockContainer blockContainer in ConnectedBlocks)
            {
                if (!visitedNodes.Contains(blockContainer.BlockRootObject.GetHashCode()))
                {
                    if (blockContainer.BlockCommunication.IsIndirectlyAttachedToFloor(visitedNodes))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if a Hand is currently holding the Block directly
        /// </summary>
        /// <returns>True if directly held</returns>
        public bool IsDirectlyAttachedToHand()
        {
            return GetComponent<AttachHandHandler>().IsAttachedToHand();
        }


        /// <summary>
        /// Returns true if the Block is indirectly connected to a Block that is held by a Hand
        /// </summary>
        /// <param name="visitedNodes">Kepp track of visited Block</param>
        /// <returns>True if indirectly held by Hand</returns>
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

            foreach (BlockContainer blockContainer in ConnectedBlocks)
            {
                if (!visitedNodes.Contains(blockContainer.BlockRootObject.GetHashCode()))
                {
                    if (blockContainer.BlockCommunication.IsIndirectlyAttachedToHand(visitedNodes))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if a Block is directly connected to an other Block
        /// </summary>
        /// <param name="block">The Block to check wherever they are connected</param>
        /// <returns>True if they are directly connected</returns>
        public bool IsDirectlyAttachedToBlock(GameObject block)
        {
            foreach(BlockContainer blockContainer in connectedBlocks)
            {
                if(block.GetHashCode() == blockContainer.GetHashCode())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if a Block is indireclty connected to an other Block and if the connection is made only via marked Block,
        /// meaning a connection that uses the Floor or an unmarked Block would return false
        /// </summary>
        /// <param name="block">The Block to check wherever they are connected</param>
        /// <param name="visitedNodes">Kepp track of visited Block</param>
        /// <returns>True if a indirect connection only via marked Blocks can be found</returns>
        public bool IsIndirectlyAttachedToBlockMarked(GameObject block, List<int> visitedNodes = null)
        {
            if (visitedNodes == null)
            {
                visitedNodes = new List<int>();
            }

            visitedNodes.Add(gameObject.GetHashCode());
            if (IsDirectlyAttachedToBlock(block))
            {
                return true;
            }

            foreach (BlockContainer blockContainer in ConnectedBlocks)
            {
                if (!visitedNodes.Contains(blockContainer.BlockRootObject.GetHashCode()) && !blockContainer.BlockRootObject.tag.Equals("Floor") && blockContainer.Interactable.isMarked)
                {
                    if (blockContainer.BlockCommunication.IsIndirectlyAttachedToBlockMarked(block, visitedNodes))
                    {
                        return true;
                    }
                }
            }

            return false;
        }



        //public void RemoveJointViaSimulation(Guid connectedBlockGuid)
        //{
        //    foreach (BlockContainer blockContainer in ConnectedBlocks)
        //    {
        //        if (blockContainer.BlockCommunication.Guid == connectedBlockGuid)
        //        {
        //            Destroy(blockContainer.ConnectedJoint);
        //            StartCoroutine(EvaluateJoints());
        //        }
        //    }
        //}

        /// <summary>
        /// Starts a Coroutine to check which connection has broken
        /// </summary>
        /// <param name="breakForce"></param>
        private void OnJointBreak(float breakForce)
        {
            Debug.Log("Joint Break");
            StartCoroutine(EvaluateJoints());
        }


        /// <summary>
        /// Starts the search for the destoryed joints after two Frames
        /// </summary>
        /// <returns></returns>
        IEnumerator EvaluateJoints()
        {
            for (int i = 0; i <= frameUntilColliderReEvaluation; i++)
            {
                if (i == frameUntilColliderReEvaluation)
                {
                    RemoveBlockConnectionsWithoutJoint();
                }
                yield return new WaitForFixedUpdate();
            }

        }

        /// <summary>
        /// Checks if the Block is only connected on a Tap or a Groove. If it is connected on
        /// both sides it can be removes to keep the structure in tacked. Otherwise, the holding
        /// joint is destroyed and the Block unfrozen to allow an attachment to hand. Call removal
        /// methods here.
        ///
        /// Need more work
        ///
        /// 
        /// </summary>
        /// <returns>True if the Block can be removed</returns>
        public bool AttemptToFreeBlock()
        {
            OTHER_BLOCK_IS_CONNECTED_ON otherConnection = ConnectedBlocks[0].ConnectedOn;
            foreach(BlockContainer connectedBlock in ConnectedBlocks)
            {
                if(connectedBlock.ConnectedOn != otherConnection)
                {
                    return false;
                }
            }

            foreach(BlockContainer connectedBlock in ConnectedBlocks)
            {
                Destroy(connectedBlock.ConnectedJoint);
                GetComponent<AttachFloorHandler>().UnfreezeBlock();
                StartCoroutine(EvaluateJoints());
            }

            blockManager.RemoveEntryFromHistory(Guid);
            blockManager.ResetHistoryStack();
            return true;
        }
    }

    /// <summary>
    /// A container that allows quick access to Components and Variables of the connected Block, otherrides the Equals and GetHashCode
    /// Methods so they represent the Block
    /// </summary>
    public class BlockContainer
    {
        public GameObject BlockRootObject { get; }
        public GrooveHandler GrooveHandler { get; }
        public TapHandler TapHandler { get; }
        public BlockGeometryScript BlockGeometry { get; }
        public Joint ConnectedJoint { get; }
        public OTHER_BLOCK_IS_CONNECTED_ON ConnectedOn { get; } 
        public AttachFloorHandler AttachFloorHandler { get; }
        public AttachHandHandler AttachHandHandler { get; }
        public BlockCommunication BlockCommunication{ get;}
        public Interactable Interactable { get; }
        public int ConnectedPinCount { get; }
        public Guid Guid { get; }

        public BlockContainer(GameObject block, Joint connectedJoint, OTHER_BLOCK_IS_CONNECTED_ON connectedOn, int connectedPin)
        {
            BlockRootObject = block;
            GrooveHandler = block.GetComponentInChildren<GrooveHandler>();
            TapHandler = block.GetComponentInChildren<TapHandler>();
            BlockGeometry = block.GetComponent<BlockGeometryScript>();
            BlockCommunication = block.GetComponent<BlockCommunication>();
            AttachHandHandler = block.GetComponent<AttachHandHandler>();
            AttachFloorHandler = block.GetComponent<AttachFloorHandler>();
            Interactable = block.GetComponent<Interactable>();
            ConnectedJoint = connectedJoint;
            ConnectedOn = connectedOn;
            ConnectedPinCount = connectedPin;
            Guid = block.GetComponent<BlockCommunication>().Guid;
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
