using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Valve.VR.InteractionSystem
{
    public class GrooveHandler : Connector
    {
        private List<GameObject> listHighLighter = new List<GameObject>();
        public GameObject pinHighLight;
        
        public override void Start()
        {
            pinHighLight = Resources.Load("CheckMark", typeof(GameObject)) as GameObject;

            foreach (GrooveCollider snaps in GetComponentsInChildren<GrooveCollider>())
            {
                colliderDictionary.Add(snaps, new CollisionObject());
            }
        }

        private bool IsAlmostEqual(float float1, float float2, float precision)
        {
            return Math.Abs(float1 - float2) <= precision;
        }

        public void RegisterCollision(GrooveCollider snappingCollider, GameObject tapCollider)
        {
            //Debug.Log("Registered Collision");
            if (colliderDictionary[snappingCollider].TapPosition != null)
            {
                return;
            }

            if (GetComponentInParent<BlockCommunication>().IsIndirectlyAttachedToHand() && tapCollider.transform.childCount == 0)
            {
                AddPinHighLight(tapCollider);
            }

            //Debug.Log("Collision Entried");
            
            colliderDictionary[snappingCollider].IsConnected = acceptNewCollisionsAsConnected;
            
            colliderDictionary[snappingCollider].TapPosition = tapCollider;
            colliderDictionary[snappingCollider].GroovePosition = snappingCollider.gameObject;
            colliderDictionary[snappingCollider].CollidedBlock = tapCollider.transform.root.gameObject;
        }

        public void UnregisterCollision(GrooveCollider snappingCollider, GameObject tapCollider)
        {
            if(colliderDictionary[snappingCollider].TapPosition == null || colliderDictionary[snappingCollider].TapPosition.GetHashCode() != tapCollider.GetHashCode() || colliderDictionary[snappingCollider].IsConnected)
            {
                return;
            }
            //Debug.Log("Groove Unregistered");
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
                highLight.transform.localPosition = new Vector3(0, 0.017f, 0);
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

        public override void OnBlockAttach(GameObject block)
        {
            listHighLighter.ForEach(highLighter => Destroy(highLighter));
            listHighLighter.Clear();
            foreach (CollisionObject collisionObject in colliderDictionary.Values)
            {
                if (collisionObject.CollidedBlock != null && collisionObject.CollidedBlock.GetHashCode() == block.GetHashCode())
                {
                    collisionObject.IsConnected = true;
                }
            }
        }
    }
}
