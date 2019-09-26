using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Valve.VR.InteractionSystem
{
    public class GrooveHandler : MonoBehaviour
    {

        private const float ZERO = 0f;
        private const float NINETY = 90f;
        private const float ONE_EIGHTY = 180f;
        private const float TWO_SEVENTY = 270f;
        private const float PRECISION = 0.000001f;
        public int colliderCount = 0;

        private Dictionary<SnappingCollider, CollisionObject> colliderDictionary = new Dictionary<SnappingCollider, CollisionObject>();
        private float lastResetTime;
        public float timeUntilSnap = 2.0f;
        public bool shouldSnap = false;
        public bool hasSnapped = false;
        public bool wasPlacedOnTap = false;
        public bool hasRotated = false;
        private Hand attachedHand = null;
        private GameObject block;
        private Vector3 detachPoint;
        private BlockScript blockScript;


// Start is called before the first frame update
void Start()
        {
            block = transform.root.gameObject;
            blockScript = block.GetComponent<BlockScript>();
            foreach (SnappingCollider snaps in GetComponentsInChildren<SnappingCollider>())
            {
                colliderDictionary.Add(snaps, new CollisionObject(snaps.gameObject));
            }
        }

        private void FixedUpdate()
        {

        }

        private void LateUpdate()
        {
            

        }

        private bool IsAlmostEqual(float float1, float float2, float precision)
        {
            return (Math.Abs(float1 - float2) <= precision);
        }

        public void RegisterCollision(SnappingCollider snappingCollider, GameObject tapCollider)
        {
            colliderDictionary[snappingCollider].TapPosition = tapCollider;
            colliderDictionary[snappingCollider].CollidedBlock = tapCollider.transform.root.gameObject;
        }

        public void UnregisterCollision(SnappingCollider snappingCollider)
        {          
            colliderDictionary[snappingCollider].TapPosition = null;
            colliderDictionary[snappingCollider].CollidedBlock = null;
        }

        public bool IsSnapped()
        {
            return hasSnapped;
        }

        public void OnBlockPulled()
        {
            foreach (SnappingCollider snaps in GetComponentsInChildren<SnappingCollider>())
            {
                colliderDictionary[snaps].TapPosition = null;
                colliderDictionary[snaps].CollidedBlock = null;
            }
            hasSnapped = false;
            
            Destroy(block.GetComponent<FixedJoint>());
            Debug.Log("GrooveHandler: Block was pulled");
        }

        public void OnAttachedToHand(Hand hand)
        {
            attachedHand = hand;
        }

        public void OnDetachedFromHand(Hand hand)
        {
            Debug.Log("GrooveHandler OnDetached: Detached from hand");
            attachedHand = null;
            
            foreach (SnappingCollider snap1 in colliderDictionary.Keys)
            {
                if (colliderDictionary[snap1].TapPosition != null)
                {
                    Debug.Log("GrooveHandler OnDeatched: Block should snap");
                    List<CollisionObject> currentCollisionObjects = new List<CollisionObject>();
                    foreach (SnappingCollider snap in colliderDictionary.Keys)
                    {
                        if (colliderDictionary[snap].TapPosition != null)
                        {
                            currentCollisionObjects.Add(colliderDictionary[snap]);
                        }
                    }

                    if (currentCollisionObjects.Count > 1)
                    {
                        Rigidbody rigidBody = block.GetComponent<Rigidbody>();
                        rigidBody.isKinematic = true;
                        MatchTargetBlockRotation(currentCollisionObjects[0]);
                        MatchTargetBlockDistance(currentCollisionObjects[0]);
                        MatchTargetBlockOffset(currentCollisionObjects[0]);
                        MatchPinRotation(currentCollisionObjects[0], currentCollisionObjects[1]);


                        block.AddComponent<FixedJoint>();
                        block.GetComponent<FixedJoint>().connectedBody = currentCollisionObjects[0].TapPosition.GetComponentInParent<Rigidbody>();
                        block.GetComponent<FixedJoint>().breakForce = 200;
                        rigidBody.isKinematic = false;
                        

                        hasSnapped = true;
                        break;

                    }
                }
            }
        }

        private void MatchTargetBlockRotation(CollisionObject collision)
        {
            block.transform.rotation = Quaternion.LookRotation(collision.CollidedBlock.transform.up, -transform.forward);
            block.transform.Rotate(Vector3.right, 90f);
        }

        private void MatchTargetBlockDistance(CollisionObject collision)
        {
            Plane groovePlane = new Plane(transform.TransformPoint(blockScript.CornerBottomA.transform.position),
                                                      transform.TransformPoint(blockScript.CornerBottomB.transform.position),
                                                      transform.TransformPoint(blockScript.CornerBottomC.transform.position));

            float distance = groovePlane.GetDistanceToPoint(transform.TransformPoint(collision.CollidedBlock.GetComponent<BlockScript>().CornerTopA.transform.position));
            block.transform.Translate(Vector3.up * distance, Space.Self);

            Debug.Log(Vector3.Dot(block.GetComponent<BlockScript>().GetBlockNormale(), collision.CollidedBlock.GetComponent<BlockScript>().GetBlockNormale()));
            Debug.Log("Match Rotation complete");
        }

        private void MatchTargetBlockOffset(CollisionObject collision)
        {
            Vector3 tapColliderCenterLocal = block.transform.InverseTransformPoint(collision.TapPosition.transform.position);
            Vector3 grooveColliderCenterLocal = collision.GroovePosition.transform.localPosition;
            Vector3 centerOffset = tapColliderCenterLocal - grooveColliderCenterLocal;
            block.transform.Translate(Vector3.right * centerOffset.x, Space.Self);
            block.transform.Translate(Vector3.forward * centerOffset.z, Space.Self);
        }

        private void MatchPinRotation(CollisionObject matchedPin, CollisionObject secoundPin)
        {
            Vector3 intersectionPointTap = block.transform.InverseTransformPoint(matchedPin.TapPosition.transform.position);
            Vector3 tapColliderCenter = block.transform.InverseTransformPoint(secoundPin.TapPosition.transform.position);
            Vector3 grooveColliderCenter = secoundPin.GroovePosition.transform.localPosition;

            Vector3 vectorIntersectToTap = intersectionPointTap - tapColliderCenter;
            Vector3 vectorIntersectionToGroove = intersectionPointTap - grooveColliderCenter;

            vectorIntersectToTap = Vector3.ProjectOnPlane(vectorIntersectToTap, Vector3.up);
            vectorIntersectionToGroove = Vector3.ProjectOnPlane(vectorIntersectionToGroove, Vector3.up);

            Debug.DrawLine(vectorIntersectToTap, vectorIntersectToTap * 3, Color.red, 90);
            Debug.DrawLine(vectorIntersectionToGroove, vectorIntersectionToGroove * 3, Color.blue, 90);


            float angleRotation = Vector3.Angle(vectorIntersectionToGroove, vectorIntersectToTap);

            Debug.Log("Angle: " + angleRotation);

            block.transform.RotateAround(matchedPin.TapPosition.transform.position, block.transform.up, angleRotation);
        }



    }




    public class CollisionObject
    {

        private GameObject tapPosition = null;
        public GameObject CollidedBlock { get; set; }

        public GameObject TapPosition
        {
            get
            {
                return tapPosition;
            }
            set
            {
                if (value == null)
                {
                    hasOffset = false;
                }
                else
                {
                    hasOffset = true;
                }
                tapPosition = value;
            }

        }
        public GameObject GroovePosition { get; }
        public bool hasOffset = false;

        public CollisionObject(GameObject groovePosition)
        {
            this.GroovePosition = groovePosition;
        }

        public Vector3 GetOffsetInWorldSpace(Transform transform)
        {
            if (tapPosition == null)
            {
                return new Vector3();
            }
            Vector3 centerWorld = transform.TransformDirection(GroovePosition.transform.position);
            Vector3 otherCenterWorld = transform.TransformDirection(tapPosition.transform.position);
            return centerWorld - otherCenterWorld;
        }

        public Vector3 GetOffset()
        {
            return tapPosition.transform.position - GroovePosition.transform.position;
        }
    }
}
