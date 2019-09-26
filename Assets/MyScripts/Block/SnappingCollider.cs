using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{


    public class SnappingCollider : MonoBehaviour
{
    public bool debug = false;
    private GrooveHandler grooveHandler;
    void Start()
    {
        grooveHandler = GetComponentInParent<GrooveHandler>();
    }

    private void OnTriggerEnter(Collider tapCollider)
    {
        if (!(tapCollider.gameObject.tag == "Tap"))
        {
            return;
        }

        grooveHandler.RegisterCollision(this, tapCollider.gameObject);
    }

    private void OnTriggerExit(Collider tapCollider)
    {
        if (!(tapCollider.gameObject.tag == "Tap"))
        {
            return;
        }

        grooveHandler.UnregisterCollision(this);
    }

    private void OnTriggerStay(Collider tapCollider)
    {

        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
}