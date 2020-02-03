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
            foreach (TapCollider snaps in GetComponentsInChildren<TapCollider>())
            {
                colliderDictionary.Add(snaps, new CollisionObject());
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

            colliderDictionary[snappingCollider].TapPosition = snappingCollider.gameObject;
            colliderDictionary[snappingCollider].GroovePosition = grooveCollider;
            colliderDictionary[snappingCollider].CollidedBlock = grooveCollider.transform.root.gameObject;
        }

        public void UnregisterCollision(IConnectorCollider snappingCollider, GameObject grooveCollider)
        {
            if (colliderDictionary[snappingCollider].TapPosition == null || colliderDictionary[snappingCollider].GroovePosition.GetHashCode() != grooveCollider.GetHashCode())
            {
                return;
            }

            colliderDictionary[snappingCollider].ResetObject();
        }

        

        

        
    }
}

