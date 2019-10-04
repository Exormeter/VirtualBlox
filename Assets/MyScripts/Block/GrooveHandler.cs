using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Valve.VR.InteractionSystem
{
    public class GrooveHandler : MonoBehaviour, IConnector
    {
        private Dictionary<GrooveCollider, CollisionObject> colliderDictionary = new Dictionary<GrooveCollider, CollisionObject>();
        private BlockGeometryScript blockScript;
        public GameObject pinHighLight;

        private int colliderCount = 0;


        // Start is called before the first frame update
        void Start()
        {
            foreach (GrooveCollider snaps in GetComponentsInChildren<GrooveCollider>())
            {
                colliderDictionary.Add(snaps, new CollisionObject(snaps.gameObject));
            }
        }

        private void FixedUpdate()
        {

        }

        private void Update()
        {
        }

        private void LateUpdate()
        {

        }

        private bool IsAlmostEqual(float float1, float float2, float precision)
        {
            return Math.Abs(float1 - float2) <= precision;
        }

        public void RegisterCollision(GrooveCollider snappingCollider, GameObject tapCollider)
        {
            colliderCount++;
            if (GetComponentInParent<BlockScript>().IsAttachedToHand() && tapCollider.transform.childCount == 0)
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
            if (tapCollider.transform.childCount == 1)
            {
                Destroy(tapCollider.transform.GetChild(0).gameObject);
            }

        }


        public void OnBlockPulled()
        {
            foreach (GrooveCollider snaps in GetComponentsInChildren<GrooveCollider>())
            {
                colliderDictionary[snaps].TapPosition = null;
                colliderDictionary[snaps].CollidedBlock = null;
            }
        }

        

        public List<CollisionObject> GetCollidingObjects()
        {
            List<CollisionObject> collisionList = new List<CollisionObject>(colliderDictionary.Values);
            collisionList.RemoveAll(collision => collision.CollidedBlock == null);
            return collisionList;
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
