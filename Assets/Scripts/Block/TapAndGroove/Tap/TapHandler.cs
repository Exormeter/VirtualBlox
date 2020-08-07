using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Valve.VR.InteractionSystem
{
    public class TapHandler : Connector
    {

        public GameObject pinHighLight;
        

        public override void Start()
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name.Equals("Collider"))
                {
                    transform.GetChild(i).GetComponent<TapCollider>().tag = "Tap";
                    colliderDictionary.Add(transform.GetChild(i).GetComponent<TapCollider>(), new CollisionObject());
                }
            }
        }

        //TODO: TapPosition ändern in Groove Poition und anpassen
        public void RegisterCollision(TapCollider snappingCollider, GameObject grooveCollider)
        {
            
            if (colliderDictionary[snappingCollider].GroovePosition != null)
            {
                return;
            }
            colliderDictionary[snappingCollider].IsConnected = acceptNewCollisionsAsConnected;
            //Debug.Log("Tap Registered");
            colliderDictionary[snappingCollider].TapPosition = snappingCollider.gameObject;
            colliderDictionary[snappingCollider].GroovePosition = grooveCollider;
            colliderDictionary[snappingCollider].CollidedBlock = grooveCollider.transform.parent.parent.gameObject;
        }

        public void UnregisterCollision(IConnectorCollider snappingCollider, GameObject grooveCollider)
        {
            if (colliderDictionary[snappingCollider].TapPosition == null || colliderDictionary[snappingCollider].GroovePosition.GetHashCode() != grooveCollider.GetHashCode() || colliderDictionary[snappingCollider].IsConnected)
            {
                return;
            }
            //Debug.Log("Tap Unregistered");
            colliderDictionary[snappingCollider].ResetObject();
        }

        

        

        
    }
}

