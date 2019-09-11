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
        public bool hasRotated = false;
        public bool hasSnapped = false;
        public bool wasPlacedOnTap = false;
        private Hand attachedHand = null;
        private GameObject block;

        // Start is called before the first frame update
        void Start()
        {
            block = transform.root.gameObject;
            foreach (SnappingCollider snaps in GetComponentsInChildren<SnappingCollider>())
            {
                colliderDictionary.Add(snaps, new CollisionObject(snaps.GetComponent<BoxCollider>()));
            }
        }


        void FixedUpdate()
        {
            //if (hasSnapped)
            //{
            //    Rigidbody body = block.GetComponent<Rigidbody>();
            //    body.isKinematic = false;
            //}
            if (wasPlacedOnTap)
            {
                Debug.Log("Rotating Block");
                RotateBlock();
            }

            if (hasRotated && !hasSnapped)
            {
                SnappingCollider snappingCollider = null;
                foreach (SnappingCollider snap in colliderDictionary.Keys)
                {
                    if (colliderDictionary[snap].hasOffset)
                    {
                        snappingCollider = snap;
                        break;
                    }


                }
                if (snappingCollider == null)
                {
                    return;
                }


                CollisionObject collisionObject = colliderDictionary[snappingCollider];
                //Vector3 centerDistance = getCenterOffset(snappingCollider.GetComponent<BoxCollider>().bounds.center, tapCollider.bounds.center);
                Vector3 centerDistance = collisionObject.GetOffsetInWorldSpace(transform);
                Vector3 currentBlockPosition = block.transform.position;
                centerDistance.y = 0;
                switch (Mathf.Floor(block.transform.rotation.eulerAngles.y))
                {
                    case ZERO:

                        currentBlockPosition = currentBlockPosition - centerDistance;

                        //centerDistance = getCenterOffset(snappingCollider.GetComponent<BoxCollider>().bounds.center, tapCollider.bounds.center);
                        centerDistance = collisionObject.GetOffsetInWorldSpace(transform);
                        //Debug.Log("Distance after snap 0" + centerDistance.ToString("F4"));
                        break;

                    case NINETY:

                        currentBlockPosition.x = currentBlockPosition.x + centerDistance.z;
                        currentBlockPosition.z = currentBlockPosition.z - centerDistance.x;

                        //centerDistance = getCenterOffset(snappingCollider.GetComponent<BoxCollider>().bounds.center, tapCollider.bounds.center);
                        centerDistance = collisionObject.GetOffsetInWorldSpace(transform);
                        //Debug.Log("Distance after snap 90" + centerDistance.ToString("F4"));
                        break;

                    case ONE_EIGHTY:

                        currentBlockPosition = currentBlockPosition + centerDistance;


                        //centerDistance = getCenterOffset(snappingCollider.GetComponent<BoxCollider>().bounds.center, tapCollider.bounds.center);
                        centerDistance = collisionObject.GetOffsetInWorldSpace(transform);
                        //Debug.Log("Distance after snap 180" + centerDistance.ToString("F4"));
                        break;

                    case TWO_SEVENTY:
                        currentBlockPosition.x = currentBlockPosition.x - centerDistance.z;
                        currentBlockPosition.z = currentBlockPosition.z + centerDistance.x;
                        break;

                }
                block.transform.localPosition = currentBlockPosition;
                if (IsAlmostEqual(centerDistance.x, 0, PRECISION) && IsAlmostEqual(centerDistance.z, 0, PRECISION))
                {
                    hasSnapped = true;
                    block.AddComponent<FixedJoint>();
                    block.GetComponent<FixedJoint>().connectedBody = collisionObject.TapCollider.GetComponentInParent<Rigidbody>();
                    block.GetComponent<FixedJoint>().breakForce = 2000;
                }

            }
        }

        private void RotateBlock()
        {
            Vector3 correctedRotation = CorrectRotation(block.transform.rotation.eulerAngles);
            Rigidbody body = block.GetComponent<Rigidbody>();
            body.isKinematic = true;
            block.transform.rotation = Quaternion.Euler(correctedRotation);
            hasRotated = true;
            body.isKinematic = false;
        }

        private bool IsAlmostEqual(float float1, float float2, float precision)
        {
            return (Math.Abs(float1 - float2) <= precision);
        }

        public void RegisterCollision(SnappingCollider snappingCollider, Collider tapCollider)
        {
            colliderCount++;
            Debug.Log("Collision Registered");
            CollisionObject collisionObject = colliderDictionary[snappingCollider];
            collisionObject.TapCollider = tapCollider;
            colliderDictionary[snappingCollider] = collisionObject;
        }

        public void UnregisterCollision(SnappingCollider snappingCollider, Collider tapCollider)
        {
            colliderCount--;
            Debug.Log("Collision Unregistered");
            CollisionObject collisionObject = colliderDictionary[snappingCollider];
            collisionObject.TapCollider = null;
            colliderDictionary[snappingCollider] = collisionObject;
        }

        private Vector3 CorrectRotation(Vector3 rotation)
        {
            Vector3 correctedRotation = new Vector3(0f, 0f, 0f);
            if (rotation.y <= 45 || rotation.y > 315)
            {
                correctedRotation.y = ZERO;
            }

            else if (rotation.y > 45 && rotation.y <= 135)
            {
                correctedRotation.y = NINETY;
            }
            else if (rotation.y > 135 && rotation.y < 225)
            {
                correctedRotation.y = ONE_EIGHTY;
            }
            else
            {
                correctedRotation.y = TWO_SEVENTY;
            }
            return correctedRotation;
        }

        private Vector3 GetCenterOffset(Vector3 center, Vector3 otherCenter)
        {
            Vector3 centerWorld = transform.TransformDirection(center);
            Vector3 otherCenterWorld = transform.TransformDirection(otherCenter);
            return centerWorld - otherCenterWorld;
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
            wasPlacedOnTap = false;
            Destroy(block.GetComponent<FixedJoint>());
            Debug.Log("Block was pulled");
        }

        public void OnAttachedToHand(Hand hand)
        {
            attachedHand = hand;
        }

        public void OnDetachedFromHand(Hand hand)
        {
            Debug.Log("Detached");
            attachedHand = null;
            foreach (SnappingCollider snap in colliderDictionary.Keys)
            {
                isObjectNull(colliderDictionary[snap].TapCollider);
                if(colliderDictionary[snap].TapCollider != null)
                {
                    wasPlacedOnTap = true;
                }
            }
        }

        private void isObjectNull(System.Object obje)
        {
            if(obje == null)
            {
                Debug.Log("Ist Null");
            }
            else
            {
                Debug.Log("Nicht null");
            }
        }

    }
}



public class CollisionObject
{
    
    private Collider tapCollider = null;
    public Collider TapCollider
    {
        get
        {
            return tapCollider;
        }
        set
        {
            if(value == null)
            {
                hasOffset = false;
            }
            else
            {
                tapCollider = value;
                hasOffset = true;
            }
            
        }
            
    }

    private BoxCollider grooveCollider;
    public BoxCollider GrooveCollider
    {
        get
        {
            return grooveCollider;
        }
    }
    public bool hasOffset = false;

    public CollisionObject(BoxCollider grooveCollider)
    {
        this.grooveCollider = grooveCollider;
    }

    public Vector3 GetOffsetInWorldSpace(Transform transform)
    {
        if(tapCollider == null)
        {
            return new Vector3();
        }
        Vector3 centerWorld = transform.TransformDirection(grooveCollider.bounds.center);
        Vector3 otherCenterWorld = transform.TransformDirection(tapCollider.bounds.center);
        return centerWorld - otherCenterWorld;
    }
}
