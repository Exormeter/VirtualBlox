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
                colliderDictionary.Add(snaps, new CollisionObject(snaps.GetComponent<BoxCollider>()));
            }
        }

        private void FixedUpdate()
        {
            if (shouldSnap)
            {
                List<CollisionObject> collisionObjects = new List<CollisionObject>();
                foreach (SnappingCollider snap in colliderDictionary.Keys)
                {
                    if (colliderDictionary[snap].TapCollider != null)
                    {
                        collisionObjects.Add(colliderDictionary[snap]);
                    }
                }

                if (collisionObjects.Count > 1)
                {


                    Rigidbody rigidBody = block.GetComponent<Rigidbody>();
                    rigidBody.isKinematic = true;
                    rigidBody.useGravity = false;

                    block.transform.rotation = Quaternion.LookRotation(collisionObjects[0].CollidedBlock.transform.up, transform.forward);
                    block.transform.Rotate(Vector3.right, 90f);

                    Plane groovePlane = new Plane(transform.TransformPoint(blockScript.CornerBottomA.transform.position),
                                                  transform.TransformPoint(blockScript.CornerBottomB.transform.position),
                                                  transform.TransformPoint(blockScript.CornerBottomC.transform.position));

                    float distance = groovePlane.GetDistanceToPoint(transform.TransformPoint(collisionObjects[0].CollidedBlock.GetComponent<BlockScript>().CornerTopA.transform.position));
                    block.transform.Translate(Vector3.up * distance, Space.Self);

                    Debug.Log(Vector3.Dot(block.GetComponent<BlockScript>().GetBlockNormale(), collisionObjects[0].CollidedBlock.GetComponent<BlockScript>().GetBlockNormale()));
                    hasRotated = true;
                    
                }
            }

        }

        private void LateUpdate()
        {
            if (hasRotated)
            {
                Rigidbody rigidBody = block.GetComponent<Rigidbody>();
                rigidBody.isKinematic = true;
                List<CollisionObject> collisionObjects = new List<CollisionObject>();
                foreach (SnappingCollider snap in colliderDictionary.Keys)
                {
                    if (colliderDictionary[snap].TapCollider != null)
                    {
                        collisionObjects.Add(colliderDictionary[snap]);
                    }
                }
                if (collisionObjects.Count > 1)
                {
                    Vector3 tapColliderCenterLocal = block.transform.InverseTransformPoint(collisionObjects[0].TapCollider.bounds.center);
                    Vector3 grooveColliderCenterLocal = block.transform.InverseTransformPoint(collisionObjects[0].GrooveCollider.bounds.center);

                    Vector3 centerOffset = tapColliderCenterLocal - grooveColliderCenterLocal;

                    block.transform.Translate(Vector3.right * centerOffset.x, Space.Self);
                    block.transform.Translate(Vector3.forward * centerOffset.z, Space.Self);


                    Plane groovePlane = new Plane(transform.TransformPoint(blockScript.CornerBottomA.transform.position),
                                                  transform.TransformPoint(blockScript.CornerBottomB.transform.position),
                                                  transform.TransformPoint(blockScript.CornerBottomC.transform.position));

                    float distance = groovePlane.GetDistanceToPoint(transform.TransformPoint(collisionObjects[0].CollidedBlock.GetComponent<BlockScript>().CornerTopA.transform.position));

                    Debug.Log("Distance Plane: " + distance.ToString("F6"));
                    Debug.Log("Distance Center: " + collisionObjects[0].GetOffset().ToString("F5"));
                    

                    Vector3 intersectionPointTap = collisionObjects[0].TapCollider.bounds.center;
                    Vector3 intersectionPointGroove = collisionObjects[0].GrooveCollider.bounds.center;
                    Vector3 tapColliderCenter = collisionObjects[1].TapCollider.bounds.center;
                    Vector3 grooveColliderCenter = collisionObjects[1].GrooveCollider.bounds.center;

    
                    Vector3 vectorIntersectToTap = tapColliderCenter - intersectionPointTap;
                    Vector3 vectorIntersectionToGroove = grooveColliderCenter - intersectionPointGroove;

                    Debug.DrawLine(intersectionPointTap, tapColliderCenter, Color.red, 90);
                    Vector3 debugIntersectionGroove = collisionObjects[0].GrooveCollider.bounds.center;
                    Debug.DrawLine(debugIntersectionGroove - centerOffset, grooveColliderCenter, Color.blue, 90);


                    float angleRotation = Vector3.Angle(vectorIntersectToTap, vectorIntersectionToGroove);

                    Debug.Log("Angle: " + angleRotation);

                    block.transform.RotateAround(intersectionPointTap, Vector3.up, angleRotation);
                    block.AddComponent<FixedJoint>();
                    block.GetComponent<FixedJoint>().connectedBody = collisionObjects[0].TapCollider.GetComponentInParent<Rigidbody>();
                    block.GetComponent<FixedJoint>().breakForce = 2000;

                    rigidBody.isKinematic = false;
                    hasSnapped = true;
                    shouldSnap = false;
                    hasRotated = false;
                }
            }

        }

        private bool IsAlmostEqual(float float1, float float2, float precision)
        {
            return (Math.Abs(float1 - float2) <= precision);
        }

        public void RegisterCollision(SnappingCollider snappingCollider, Collider tapCollider)
        {
            colliderDictionary[snappingCollider].TapCollider = tapCollider;
            colliderDictionary[snappingCollider].CollidedBlock = tapCollider.transform.root.gameObject;
        }

        public void UnregisterCollision(SnappingCollider snappingCollider, Collider tapCollider)
        {
            colliderDictionary[snappingCollider].TapCollider = null;
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
                colliderDictionary[snaps].TapCollider = null;
            }
            hasSnapped = false;
            hasRotated = false;
            shouldSnap = false;
            Destroy(block.GetComponent<FixedJoint>());
            Debug.Log("GrooveHandler: Block was pulled");
        }

        public void OnAttachedToHand(Hand hand)
        {
            attachedHand = hand;
        }

        public void OnDetachedFromHand(Hand hand)
        {
            Debug.Log("GrooveHandler: Detached from hand");
            attachedHand = null;
            foreach (SnappingCollider snap in colliderDictionary.Keys)
            {
                if (colliderDictionary[snap].TapCollider != null)
                {
                   // SnapBlock();
                    shouldSnap = true;
                    break;
                }
            }
        }

        //private void SnapBlock()
        //{
        //    List<CollisionObject> collisionObjects = new List<CollisionObject>();
        //    foreach (SnappingCollider snap in colliderDictionary.Keys)
        //    {
        //        if (colliderDictionary[snap].TapCollider != null)
        //        {
        //            collisionObjects.Add(colliderDictionary[snap]);
        //        }
        //    }

        //    if (collisionObjects.Count > 1)
        //    {


        //        Rigidbody rigidBody = block.GetComponent<Rigidbody>();
        //        rigidBody.isKinematic = true;

        //        block.transform.rotation = Quaternion.LookRotation(collisionObjects[0].CollidedBlock.transform.up, transform.forward);
        //        block.transform.Rotate(Vector3.right, 90f);

        //        Debug.Log(Vector3.Dot(block.GetComponent<BlockScript>().GetBlockNormale(), collisionObjects[0].CollidedBlock.GetComponent<BlockScript>().GetBlockNormale()));
        //        hasRotated = true;
        //        Debug.Break();
        //    }
    }




    public class CollisionObject
    {

        private Collider tapCollider = null;
        public GameObject CollidedBlock { get; set; }

        public Collider TapCollider
        {
            get
            {
                return tapCollider;
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
                tapCollider = value;
            }

        }
        public BoxCollider GrooveCollider { get; }
        public bool hasOffset = false;

        public CollisionObject(BoxCollider grooveCollider)
        {
            this.GrooveCollider = grooveCollider;
        }

        public Vector3 GetOffsetInWorldSpace(Transform transform)
        {
            if (tapCollider == null)
            {
                return new Vector3();
            }
            Vector3 centerWorld = transform.TransformDirection(GrooveCollider.bounds.center);
            Vector3 otherCenterWorld = transform.TransformDirection(tapCollider.bounds.center);
            return centerWorld - otherCenterWorld;
        }

        public Vector3 GetOffset()
        {
            return tapCollider.bounds.center - GrooveCollider.bounds.center;
        }
    }
}
