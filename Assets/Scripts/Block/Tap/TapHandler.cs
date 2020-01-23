﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Valve.VR.InteractionSystem
{
    public class TapHandler : MonoBehaviour, IConnectorHandler
    {

        private Dictionary<IConnectorCollider, CollisionObject> colliderDictionary = new Dictionary<IConnectorCollider, CollisionObject>();
        public GameObject pinHighLight;
        public bool acceptNewCollisionsAsConnected;
        public int occupiedTaps = 0;

        // Start is called before the first frame update
        void Start()
        {
            foreach (TapCollider snaps in GetComponentsInChildren<TapCollider>())
            {
                colliderDictionary.Add(snaps, new CollisionObject());
            }
        }

        void Update()
        {
            occupiedTaps = GetOccupiedTaps().Count;
           
        }

        public List<CollisionObject> GetCollidingObjects()
        {
            List<CollisionObject> collisionList = new List<CollisionObject>(colliderDictionary.Values);
            collisionList.RemoveAll(collision => collision.CollidedBlock == null || collision.IsConnected);
            return collisionList;
        }

        public List<CollisionObject> GetOccupiedTaps()
        {
            List<CollisionObject> collisionList = new List<CollisionObject>(colliderDictionary.Values);
            collisionList.RemoveAll(collision => collision.IsConnected == false);
            return collisionList;
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
            if (colliderDictionary[snappingCollider].GroovePosition.GetHashCode() != grooveCollider.GetHashCode())
            {
                return;
            }

            colliderDictionary[snappingCollider].ResetObject();
        }

        public void OnBlockAttach(GameObject block)
        {
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

        public void OnBlockPulled()
        {
            foreach (CollisionObject collisionObject in colliderDictionary.Values)
            {
                collisionObject.ResetObject();
            }
        }

        public List<CollisionObject> GetCollisionObjectsForGameObject(GameObject gameObject)
        {
            List<CollisionObject> collisionList = new List<CollisionObject>(colliderDictionary.Values);
            collisionList.RemoveAll(block => block.CollidedBlock == null);
            collisionList.RemoveAll(block => block.CollidedBlock.GetHashCode() != gameObject.GetHashCode());
            return collisionList;
        }

        public void AcceptCollisionsAsConnected(bool shoudlAccept)
        {
            acceptNewCollisionsAsConnected = shoudlAccept;
        }
    }
}

