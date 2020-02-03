using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{
    public class TapCollider : MonoBehaviour, IConnectorCollider
    {
        private TapHandler tapHandler;
        // Start is called before the first frame update
        void Start()
        {
            tapHandler = GetComponentInParent<TapHandler>();
        }

        private void OnTriggerEnter(Collider grooveCollider)
        {
            if (grooveCollider.gameObject.tag != "Groove")
            {
                return;
            }

            tapHandler.RegisterCollision(this, grooveCollider.gameObject);
        }

        private void OnTriggerExit(Collider grooveCollider)
        {
            if (grooveCollider.gameObject.tag != "Groove")
            {
                return;
            }

            tapHandler.UnregisterCollision(this, grooveCollider.gameObject);
        }

    }
}

