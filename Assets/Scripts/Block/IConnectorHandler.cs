using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public interface IConnectorHandler
    {
        List<CollisionObject> GetCollidingObjects();


        void OnBlockPulled();

        void AttachBlocks(GameObject block);

        void OnBlockDetach(GameObject block);

        void AcceptCollisionsAsConnected(bool shoudlAccept);

        List<CollisionObject> GetCollisionObjectsForGameObject(GameObject gameObject);
    }
}

