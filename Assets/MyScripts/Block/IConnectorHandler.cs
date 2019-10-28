using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public interface IConnectorHandler
    {
        List<CollisionObject> GetCollidingObjects();

        //void RegisterCollision(IConnectorCollider snappingCollider, GameObject tapCollider);
        

        //void UnregisterCollision(IConnectorCollider snappingCollider, GameObject tapCollider);

        void OnBlockPulled();

        void OnBlockAttach(GameObject block);

        void OnBlockDetach(GameObject block);
        
    }
}

