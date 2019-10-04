using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{


    public class BlockScript : MonoBehaviour
    {
        public HashSet<BlockContainer> connectedBlocksOnTap = new HashSet<BlockContainer>();
        public HashSet<BlockContainer> connectedBlocksOnGroove= new HashSet<BlockContainer>();

        private GrooveHandler grooveHandler;
        private TapHandler tapHandler;
        private Hand attachedHand = null;
        private BlockGeometryScript blockGeometry;
        public int breakForcePerPin = 25;
        public int frameUntilColliderReEvaluation = 2;
        private FixedJoint tempAttach;
        // Start is called before the first frame update
        void Start()
        {
            grooveHandler = GetComponentInChildren<GrooveHandler>();
            tapHandler = GetComponentInChildren<TapHandler>();
            blockGeometry = GetComponent<BlockGeometryScript>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnAttachedToHand(Hand hand)
        {
            attachedHand = hand;
        } 


        //TODO: Tap Collider Case
        public void OnDetachedFromHand(Hand hand)
        {
            attachedHand = null;
            List<CollisionObject> currentCollisionObjects;
            if (grooveHandler.GetCollidingObjects().Count != 0)
            {
                currentCollisionObjects = grooveHandler.GetCollidingObjects();
            }
            else
            {
                currentCollisionObjects = tapHandler.GetCollidingObjects();
            }
            

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
                gameObject.GetComponent<FixedJoint>().breakForce = breakForcePerPin * currentCollisionObjects.Count;
                rigidBody.isKinematic = false;
                StartCoroutine("EvaluateColliderAfterMatching");
            }
        }

        IEnumerable EvaluateColliderAfterMatching()
        {
            for(int i = 0; i <= frameUntilColliderReEvaluation; i++)
            {
                if(i == frameUntilColliderReEvaluation)
                {
                    EvaluateCollider();
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
                FixedJoint fixedJoint= gameObject.AddComponent<FixedJoint>();
                GetComponent<FixedJoint>().connectedBody = collidedBlock.GetComponent<Rigidbody>();
                fixedJoint.breakForce = breakForcePerPin * blockToTapDict[collidedBlock].Count;
                AddConnectedBlockGroove(collidedBlock, fixedJoint);
                collidedBlock.GetComponent<BlockScript>().AddConectedBlockTap(gameObject, fixedJoint);
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
            Debug.Log("Match Rotation complete");
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

            transform.RotateAround(matchedPin.TapPosition.transform.position, transform.up, angleRotation);

            if (transform.InverseTransformPoint(secoundPin.TapPosition.transform.position).x - secoundPin.GroovePosition.transform.localPosition.x != 0)
            {
                transform.RotateAround(matchedPin.TapPosition.transform.position, transform.up, angleRotation * -2);
            }
        }

        private void OnJointBreak(float breakForce)
        {
            BlockContainer container = SearchDestroyedJoint(connectedBlocksOnTap);
            if(container != null)
            {
                connectedBlocksOnTap.Remove(container);
            }
            else
            {
                container = SearchDestroyedJoint(connectedBlocksOnGroove);
            }
            if(container != null)
            {
                connectedBlocksOnGroove.Remove(container);
            }
        }

        private BlockContainer SearchDestroyedJoint(HashSet<BlockContainer> connectedBlocks)
        {
            foreach (BlockContainer blockContainer in connectedBlocks)
            {
                if (blockContainer.ConnectedJoint == null)
                {
                    return blockContainer;
                }
            }

            return null;
        }

        public bool IsAttachedToHand()
        {
            return attachedHand != null;
        }

        public void AddConectedBlockTap(GameObject block, Joint connectedJoint)
        {
            connectedBlocksOnTap.Add(new BlockContainer(block, connectedJoint));
        }

        public void AddConnectedBlockGroove(GameObject block, Joint connectedJoint)
        {
            connectedBlocksOnGroove.Add(new BlockContainer(block, connectedJoint));
        }

        public void RemoveConnectedBlockGroove(GameObject block, Joint connectedJoint)
        {
            connectedBlocksOnGroove.Remove(new BlockContainer(block, connectedJoint));
        }

        public void RemoveConnectedBlockTap(GameObject block, Joint connectedJoint)
        {
            connectedBlocksOnTap.Remove(new BlockContainer(block, connectedJoint));
        }
    }

    public class BlockContainer
    {
        public GameObject BlockRootObject { get; }
        public GrooveHandler GrooveHandler { get; }
        public TapHandler TapHandler { get; }
        public BlockGeometryScript BlockGeometry { get; }
        public Joint ConnectedJoint { get; }

        public BlockContainer(GameObject block, Joint connectedJoint)
        {
            BlockRootObject = block;
            GrooveHandler = block.GetComponentInChildren<GrooveHandler>();
            TapHandler = block.GetComponentInChildren<TapHandler>();
            BlockGeometry = block.GetComponent<BlockGeometryScript>();
            ConnectedJoint = connectedJoint;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                GameObject gameObject = (GameObject) obj;
                return gameObject.GetInstanceID() == BlockRootObject.GetInstanceID();
            }
        }

        public override int GetHashCode()
        {
            return BlockRootObject.GetHashCode();
        }
    }
}
