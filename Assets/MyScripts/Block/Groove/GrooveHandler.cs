using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Valve.VR.InteractionSystem
{
    public class GrooveHandler : MonoBehaviour, IConnectorHandler
    {
        private Dictionary<IConnectorCollider, CollisionObject> colliderDictionary = new Dictionary<IConnectorCollider, CollisionObject>();
        private List<GameObject> listHighLighter = new List<GameObject>();
        public GameObject pinHighLight;
        public int occupiedGrooves = 0;

        // Start is called before the first frame update
        void Start()
        {
            foreach (GrooveCollider snaps in GetComponentsInChildren<GrooveCollider>())
            {
                colliderDictionary.Add(snaps, new CollisionObject());
            }
        }

        void Update()
        {
            occupiedGrooves = GetOccupiedGrooves().Count;
        }

        private bool IsAlmostEqual(float float1, float float2, float precision)
        {
            return Math.Abs(float1 - float2) <= precision;
        }

        public void RegisterCollision(GrooveCollider snappingCollider, GameObject tapCollider)
        {
            if (GetComponentInParent<BlockCommunication>().IsIndirectlyAttachedToHand() && tapCollider.transform.childCount == 0)
            {
                AddPinHighLight(tapCollider);
            }

            colliderDictionary[snappingCollider].TapPosition = tapCollider;
            colliderDictionary[snappingCollider].GroovePosition = snappingCollider.gameObject;
            colliderDictionary[snappingCollider].CollidedBlock = tapCollider.transform.root.gameObject;
        }

        public void UnregisterCollision(GrooveCollider snappingCollider, GameObject tapCollider)
        {
            RemovePinHighLight(tapCollider);
            colliderDictionary[snappingCollider].ResetObject();
        }

        public void AddPinHighLight(GameObject tapCollider)
        {
            if(pinHighLight != null)
            {
                GameObject highLight = Instantiate(pinHighLight, tapCollider.transform.position, tapCollider.transform.rotation);
                highLight.tag = "HighLight";
                highLight.transform.SetParent(tapCollider.transform);
                highLight.transform.localPosition = new Vector3(0, 0.0082f, 0);
                listHighLighter.Add(highLight);
            }
            
        }

        public void RemovePinHighLight(GameObject tapCollider)
        {
            for (int childIndex = 0; childIndex < tapCollider.transform.childCount; childIndex++)
            {
                if (tapCollider.transform.GetChild(childIndex).tag == "HighLight")
                {
                    listHighLighter.Remove(tapCollider.transform.GetChild(childIndex).gameObject);
                    Destroy(tapCollider.transform.GetChild(childIndex).gameObject);
                }
            }
        }


        //Works only for the case that Block had one Connection
        public void OnBlockPulled()
        {
            foreach (CollisionObject collisionObject in colliderDictionary.Values)
            {
                collisionObject.ResetObject();
            }
        }



        public List<CollisionObject> GetCollidingObjects()
        {
            List<CollisionObject> collisionList = new List<CollisionObject>(colliderDictionary.Values);
            collisionList.RemoveAll(collision => collision.CollidedBlock == null || collision.IsConnected);
            return collisionList;
        }

        public List<CollisionObject> GetOccupiedGrooves()
        {
            List<CollisionObject> collisionList = new List<CollisionObject>(colliderDictionary.Values);
            collisionList.RemoveAll(collision => !collision.IsConnected);
            return collisionList;
        }

        public void OnBlockAttach(GameObject block)
        {
            listHighLighter.ForEach(highLigher => Destroy(highLigher));
            listHighLighter.Clear();
            foreach (CollisionObject collisionObject in colliderDictionary.Values)
            {
                if (collisionObject.CollidedBlock != null && collisionObject.CollidedBlock.GetHashCode() == block.GetHashCode())
                {
                    collisionObject.IsConnected = true;
                }
            }
        }

        public void OnBlockDetach(GameObject block)
        {
            foreach (CollisionObject collisionObject in colliderDictionary.Values)
            {
                if (collisionObject.CollidedBlock != null && collisionObject.CollidedBlock.GetHashCode() == block.GetHashCode())
                {
                    collisionObject.IsConnected = false;
                }
            }
        }

        public List<CollisionObject> GetCollisionObjectsForGameObject(GameObject gameObject)
        {
            List<CollisionObject> collisionList = new List<CollisionObject>(colliderDictionary.Values);
            collisionList.RemoveAll(block => block.CollidedBlock == null);
            collisionList.RemoveAll(block => block.CollidedBlock.GetHashCode() != gameObject.GetHashCode());
            return collisionList;
        }
    }




    public class CollisionObject
    {

        public GameObject TapPosition { get; set; } = null;
        public GameObject GroovePosition { get; set; } = null;


        public GameObject CollidedBlock { get; set; }

        public bool IsConnected { get; set; }


        public CollisionObject()
        {

        }

        public Vector3 GetOffsetInWorldSpace(Transform transform)
        {
            if (TapPosition == null)
            {
                return new Vector3();
            }
            Vector3 centerWorld = transform.TransformDirection(GroovePosition.transform.position);
            Vector3 otherCenterWorld = transform.TransformDirection(TapPosition.transform.position);
            return centerWorld - otherCenterWorld;
        }

        public Vector3 GetOffset()
        {
            return TapPosition.transform.position - GroovePosition.transform.position;
        }

        public void ResetObject()
        {
            IsConnected = false;
            TapPosition = null;
            GroovePosition = null;
            CollidedBlock = null;
        }
    }
}
