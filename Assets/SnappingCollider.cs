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
        if (tapCollider.gameObject.tag == "Groove")
        {
            Debug.Log("HitGroove");
            //colliderList.AddLast(grooveCollider);
        }
    }

    private void OnTriggerExit(Collider tapCollider)
    {
        if (!(tapCollider.gameObject.tag == "Tap"))
        {
            return;
        }

        grooveHandler.unregisterCollision(this, tapCollider);
    }

    private void OnTriggerStay(Collider tapCollider)
    {

        if(!(tapCollider.gameObject.tag == "Tap"))
        {
            return;
        }

        grooveHandler.registerCollision(this, tapCollider);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
}