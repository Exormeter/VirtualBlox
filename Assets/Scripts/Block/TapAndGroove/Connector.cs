using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class Connector : MonoBehaviour, IConnectorHandler
    {
        public bool acceptNewCollisionsAsConnected;
        protected Dictionary<IConnectorCollider, CollisionObject> colliderDictionary = new Dictionary<IConnectorCollider, CollisionObject>();
        //public int occupiedTaps = 0;
        //public List<GameObject> connectedGrooves;
        //public List<GameObject> connectedTaps;

        public virtual void Start()
        {

        }

        // Update is called once per frame
        /*public virtual void Update()
        {
            occupiedTaps = GetOccupiedCollider().Count;
            connectedGrooves = colliderDictionary.Values.ToList<CollisionObject>().ConvertAll(collisionObject => collisionObject.GroovePosition);
            connectedTaps = colliderDictionary.Values.ToList<CollisionObject>().ConvertAll(collisionObject => collisionObject.TapPosition);
        }*/

        public void AcceptCollisionsAsConnected(bool shouldAccept)
        {
            acceptNewCollisionsAsConnected = shouldAccept;
        }

        /// <summary>
        /// Get all CollisionObjects that are Occupied but not connected to a Block
        /// </summary>
        /// <returns>Not connected CollisionObjects</returns>
        public List<CollisionObject> GetCollidingObjects()
        {
            BlockCommunication blockCommunication = GetComponentInParent<BlockCommunication>();

            //fix any case where a connection wasn't caught
            foreach (CollisionObject collisionObject in colliderDictionary.Values)
            {
                //If the other Block is not connected according to the Tap- or GrooveHandler but is in the List of ConnectedBlock, fix the issue
                if (!collisionObject.IsConnected && collisionObject.CollidedBlock != null && blockCommunication.ConnectedBlocks.Exists(blockContainer => blockContainer.GetHashCode() == collisionObject.GetHashCode()))
                {
                    collisionObject.IsConnected = true;
                }
            }

            List<CollisionObject> collisionList = new List<CollisionObject>(colliderDictionary.Values);
            collisionList.RemoveAll(collision => collision.CollidedBlock == null || collision.IsConnected);
            return collisionList;
        }

        /// <summary>
        /// Returns all CollisionObjects where the CollidingBlock is the passed GameObject
        /// </summary>
        /// <param name="gameObject">The searched GameObject</param>
        /// <returns>List of CollisionObjects containing the passed GameObject</returns>
        public List<CollisionObject> GetCollisionObjectsForGameObject(GameObject gameObject)
        {
            List<CollisionObject> collisionList = new List<CollisionObject>(colliderDictionary.Values);

            //Remove all empty CollisionObjects 
            collisionList.RemoveAll(block => block.CollidedBlock == null);

            //Remove all CollisionObjects which don't contain the right collidedBlock
            collisionList.RemoveAll(block => block.CollidedBlock.GetHashCode() != gameObject.GetHashCode());
            return collisionList;
        }

        /// <summary>
        /// Called when an other Block is connected to this Block. Set all CollisionObjects with the other Block
        /// to connected
        /// </summary>
        /// <param name="block">The newly connected Block</param>
        public virtual void AttachBlocks(GameObject block)
        {
            foreach (CollisionObject collisionObject in colliderDictionary.Values)
            {
                
                if (collisionObject.CollidedBlock != null && collisionObject.CollidedBlock.GetHashCode() == block.GetHashCode())
                {
                    collisionObject.IsConnected = true;
                }
            }
        }

        /// <summary>
        /// Called when an other Block is deatched form this Block. Sets all CollisionObject with the other Block
        /// to empty
        /// </summary>
        /// <param name="block">The removed Block</param>
        public void OnBlockDetach(GameObject block)
        {
            foreach (CollisionObject collisionObject in colliderDictionary.Values)
            {
                if (collisionObject.CollidedBlock != null && collisionObject.CollidedBlock.GetHashCode() == block.GetHashCode())
                {
                    collisionObject.ResetObject();
                }
            }
        }

        /// <summary>
        /// Called when the Block is Pulled by a Hand. Resets ALL CollisionObjects
        /// </summary>
        public void OnBlockPulled()
        {
            foreach (CollisionObject collisionObject in colliderDictionary.Values)
            {
                collisionObject.ResetObject();
            }
        }

        /// <summary>
        /// Get all CollisionObjects that are curretly connected
        /// </summary>
        /// <returns>List of CollisionObjects that are connected</returns>
        public List<CollisionObject> GetOccupiedCollider()
        {
            List<CollisionObject> collisionList = new List<CollisionObject>(colliderDictionary.Values);
            collisionList.RemoveAll(collision => collision.IsConnected == false);
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
