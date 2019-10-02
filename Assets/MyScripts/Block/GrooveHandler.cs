using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Valve.VR.InteractionSystem
{
    public class GrooveHandler : MonoBehaviour
    {
        private Dictionary<GrooveCollider, CollisionObject> colliderDictionary = new Dictionary<GrooveCollider, CollisionObject>();
        private Hand attachedHand = null;
        private GameObject block;
        private BlockGeometryScript blockScript;

        public bool hasSnapped = false;
        public bool checkCollider = false;
        public int breakForcePerPin = 25;
        public GameObject pinHighLight;

        private int colliderCount = 0;


        // Start is called before the first frame update
        void Start()
        {
            block = transform.root.gameObject;
            
            blockScript = block.GetComponent<BlockGeometryScript>();
            foreach (GrooveCollider snaps in GetComponentsInChildren<GrooveCollider>())
            {
                colliderDictionary.Add(snaps, new CollisionObject(snaps.gameObject));
            }
        }

        private void FixedUpdate()
        {
            if (checkCollider)
            {
                List<CollisionObject> collisionList = new List<CollisionObject>(colliderDictionary.Values);
                collisionList.RemoveAll(collision => collision.CollidedBlock == null);
                Debug.Log("Collider Count after Fixed Update" + colliderCount);
                StartCoroutine(Example());
                foreach (CollisionObject collision in collisionList)
                {

                }
            }
        }

        private void Update()
        {
            if (checkCollider)
            {
                Debug.Log("Collider Count after Update" + colliderCount);
            }
        }

        private void LateUpdate()
        {
            if (checkCollider)
            {
                Debug.Log("Collider Count after Late Update" + colliderCount);
                
            }
        }

        private bool IsAlmostEqual(float float1, float float2, float precision)
        {
            return Math.Abs(float1 - float2) <= precision;
        }

        public void RegisterCollision(GrooveCollider snappingCollider, GameObject tapCollider)
        {
            colliderCount++;
            if(attachedHand != null && tapCollider.transform.childCount == 0)
            {
                AddPinHighLight(tapCollider);
            }
            
            colliderDictionary[snappingCollider].TapPosition = tapCollider;
            colliderDictionary[snappingCollider].CollidedBlock = tapCollider.transform.root.gameObject;
        }

        public void UnregisterCollision(GrooveCollider snappingCollider, GameObject tapCollider)
        {
            colliderCount--;
            removePinHighLight(tapCollider);
            colliderDictionary[snappingCollider].TapPosition = null;
            colliderDictionary[snappingCollider].CollidedBlock = null;
        }

        public void AddPinHighLight(GameObject tapCollider)
        {
            GameObject highLight = Instantiate(pinHighLight, tapCollider.transform.position, tapCollider.transform.rotation);
            highLight.tag = "Light";
            highLight.transform.SetParent(tapCollider.transform);
            highLight.transform.localPosition = new Vector3(0, 0.0082f, 0);
           
        }

        public void removePinHighLight(GameObject tapCollider)
        {
            if(tapCollider.transform.childCount == 1)
            {
                Destroy(tapCollider.transform.GetChild(0).gameObject);
            }
            
        }


        public bool IsSnapped()
        {
            return hasSnapped;
        }

        public void OnBlockPulled()
        {
            foreach (GrooveCollider snaps in GetComponentsInChildren<GrooveCollider>())
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
            
            foreach (GrooveCollider snap1 in colliderDictionary.Keys)
            {
                if (colliderDictionary[snap1].TapPosition != null)
                {
                    Debug.Log("GrooveHandler OnDeatched: Block should snap");
                    List<CollisionObject> currentCollisionObjects = new List<CollisionObject>();
                    foreach (GrooveCollider snap in colliderDictionary.Keys)
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
                        block.GetComponent<FixedJoint>().breakForce = breakForcePerPin * currentCollisionObjects.Count;
                        rigidBody.isKinematic = false;
                        hasSnapped = true;
                        checkCollider = true;
                        Debug.Log("Collider Count on snap " + colliderCount);
                        
                        break;

                    }
                }
            }
        }

        IEnumerator Example()
        {
            YieldInstruction wait = new WaitForFixedUpdate();
            Debug.Log("Collider Count on snap Example" + colliderCount);
            yield return wait;
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

            float distance = groovePlane.GetDistanceToPoint(transform.TransformPoint(collision.CollidedBlock.GetComponent<BlockGeometryScript>().CornerTopA.transform.position));
            block.transform.Translate(Vector3.up * distance, Space.Self);

            Debug.Log(Vector3.Dot(block.GetComponent<BlockGeometryScript>().GetBlockNormale(), collision.CollidedBlock.GetComponent<BlockGeometryScript>().GetBlockNormale()));
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

            float angleRotation = Vector3.Angle(vectorIntersectionToGroove, vectorIntersectToTap);

            block.transform.RotateAround(matchedPin.TapPosition.transform.position, block.transform.up, angleRotation);

            if(block.transform.InverseTransformPoint(secoundPin.TapPosition.transform.position).x - secoundPin.GroovePosition.transform.localPosition.x != 0)
            {
                block.transform.RotateAround(matchedPin.TapPosition.transform.position, block.transform.up, angleRotation * -2);
            }
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
