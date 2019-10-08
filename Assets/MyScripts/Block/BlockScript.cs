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
        public bool visited = false;
        

        void Start()
        {
            grooveHandler = GetComponentInChildren<GrooveHandler>();
            tapHandler = GetComponentInChildren<TapHandler>();
            blockGeometry = GetComponent<BlockGeometryScript>();
        }

        
        void Update()
        {
            visited = false;
            connectedBlocksCount = connectedBlocks.Count;
        }

        public void OnAttachedToHand(Hand hand)
        {
            attachedHand = hand;
        } 


        //TODO: Tap Collider Case
        public void OnDetachedFromHand(Hand hand)
        {
            attachedHand = null;
            SendMessageToConnectedBlocks("OnIndirectDetachedFromHand", false);
            List<CollisionObject> currentCollisionObjects = grooveHandler.GetCollidingObjects();

            //if (grooveHandler.GetCollidingObjects().Count != 0)
            //{
            //    currentCollisionObjects = grooveHandler.GetCollidingObjects();
            //}

            //else
            //{
            //    currentCollisionObjects = tapHandler.GetCollidingObjects();
            //}
            

            if (currentCollisionObjects.Count > 1)
            {
                Rigidbody rigidBody = GetComponent<Rigidbody>();
                rigidBody.isKinematic = true;
                MatchTargetBlockRotation(currentCollisionObjects[0]);
                MatchTargetBlockDistance(currentCollisionObjects[0]);
                MatchTargetBlockOffset(currentCollisionObjects[0]);
                MatchPinRotation(currentCollisionObjects[0], currentCollisionObjects[1]);

                tempAttach = gameObject.AddComponent<FixedJoint>();
                GetComponent<FixedJoint>().connectedBody = currentCollisionObjects[0].TapPosition.GetComponentInParent<Rigidbody>();
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
                FixedJoint fixedJoint = gameObject.AddComponent<FixedJoint>();
                fixedJoint.connectedBody = collidedBlock.GetComponent<Rigidbody>();
                fixedJoint.breakForce = breakForcePerPin * blockToTapDict[collidedBlock].Count;
                AddConnectedBlock(collidedBlock, fixedJoint, OtherBlockConnectedOn.GROOVE);
                collidedBlock.GetComponent<BlockScript>().AddConnectedBlock(gameObject, fixedJoint, OtherBlockConnectedOn.GROOVE);
                SendMessage("OnBlockAttach");
                
            }
        }

        private void MatchTargetBlockRotation(CollisionObject collision)
        {
            gameObject.transform.rotation = Quaternion.LookRotation(collision.CollidedBlock.transform.up, -transform.forward);
            gameObject.transform.Rotate(Vector3.right, 90f);
        }

        private void MatchTargetBlockDistance(CollisionObject collision)
        {
            Plane groovePlane = new Plane(transform.TransformPoint(blockGeometry.CornerBottomA.transform.position),
                                                      transform.TransformPoint(blockGeometry.CornerBottomB.transform.position),
                                                      transform.TransformPoint(blockGeometry.CornerBottomC.transform.position));

            float distance = groovePlane.GetDistanceToPoint(transform.TransformPoint(collision.CollidedBlock.GetComponent<BlockGeometryScript>().CornerTopA.transform.position));
            transform.Translate(Vector3.up * distance, Space.Self);

            Debug.Log(Vector3.Dot(GetComponent<BlockGeometryScript>().GetBlockNormale(), collision.CollidedBlock.GetComponent<BlockGeometryScript>().GetBlockNormale()));
        }

        private void MatchTargetBlockOffset(CollisionObject collision)
        {
            Vector3 tapColliderCenterLocal = transform.InverseTransformPoint(collision.TapPosition.transform.position);
            Vector3 grooveColliderCenterLocal = collision.GroovePosition.transform.localPosition;
            Vector3 centerOffset = tapColliderCenterLocal - grooveColliderCenterLocal;
            transform.Translate(Vector3.right * centerOffset.x, Space.Self);
            transform.Translate(Vector3.forward * centerOffset.z, Space.Self);
        }

        private void MatchPinRotation(CollisionObject matchedPin, CollisionObject secoundPin)
        {
            Vector3 intersectionPointTap = transform.InverseTransformPoint(matchedPin.TapPosition.transform.position);
            Vector3 tapColliderCenter = transform.InverseTransformPoint(secoundPin.TapPosition.transform.position);
            Vector3 grooveColliderCenter = secoundPin.GroovePosition.transform.localPosition;

            Vector3 vectorIntersectToTap = intersectionPointTap - tapColliderCenter;
            Vector3 vectorIntersectionToGroove = intersectionPointTap - grooveColliderCenter;

            vectorIntersectToTap = Vector3.ProjectOnPlane(vectorIntersectToTap, Vector3.up);
            vectorIntersectionToGroove = Vector3.ProjectOnPlane(vectorIntersectionToGroove, Vector3.up);

            float angleRotation = Vector3.Angle(vectorIntersectionToGroove, vectorIntersectToTap);
            Debug.Log("Angle Rotation: " + angleRotation);

            transform.RotateAround(matchedPin.TapPosition.transform.position, transform.up, angleRotation);
        }

        private void OnJointBreak(float breakForce)
        {
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

        public void OnBlockPulled()
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

        public bool IsIndirectlyAttachedToHand()
        {
            visited = true;
            if (IsDirectlyAttachedToHand())
            {
                return true;
            }
            
            foreach (BlockContainer blockContainer in connectedBlocks)
            {
                if (!blockContainer.BlockScript.visited)
                {
                    return blockContainer.BlockScript.IsIndirectlyAttachedToHand();
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

        public bool IsIndirectlyAttachedToFloor()
        {
            visited = true;
            if (IsDirectlyAttachedToFloor())
            {
                return true;
            }

            foreach (BlockContainer blockContainer in connectedBlocks)
            {
                if (!blockContainer.BlockScript.visited)
                {
                    return blockContainer.BlockScript.IsIndirectlyAttachedToHand();
                }
            }

            return false;
        }

        public void SendMessageToConnectedBlocks(string message, bool selfNotification = true)
        {
            visited = true;
            if (selfNotification)
            {
                BroadcastMessage(message);
            }
            
            foreach (BlockContainer blockContainer in connectedBlocks)
            {
                if (!blockContainer.BlockScript.visited)
                {
                    blockContainer.BlockScript.SendMessageToConnectedBlocks(message);
                }
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

        //public override bool Equals(object obj)
        //{
        //    if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        GameObject gameObject = (GameObject) obj;
        //        return gameObject.GetInstanceID() == BlockRootObject.GetInstanceID();
        //    }
        //}

        //public override int GetHashCode()
        //{
        //    return BlockRootObject.GetHashCode();
        //}
    }

    public enum OtherBlockConnectedOn
    {
        TAP,
        GROOVE
    }
}
