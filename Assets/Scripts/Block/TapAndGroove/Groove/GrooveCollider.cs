using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{


    public class GrooveCollider : MonoBehaviour, IConnectorCollider
    {
        private GrooveHandler grooveHandler;
        
        void Start()
        {
            grooveHandler = GetComponentInParent<GrooveHandler>();
        }

        private void OnTriggerEnter(Collider tapCollider)
        {
            if (tapCollider.gameObject.tag != "Tap")
            {
                return;
            }

            grooveHandler.RegisterCollision(this, tapCollider.gameObject);
        }

        private void OnTriggerExit(Collider tapCollider)
        {
            if (tapCollider.gameObject.tag != "Tap")
            {
                return;
            }
            
            grooveHandler.UnregisterCollision(this, tapCollider.gameObject);
        }
    }
}