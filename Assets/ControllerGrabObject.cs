using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
public class ControllerGrabObject : MonoBehaviour
{

    public SteamVR_Input_Sources handType;
    public SteamVR_Behaviour_Pose controllerPose;
    public SteamVR_Action_Boolean grabAction;

    private GameObject collidingObject;
    private GameObject objectInHand;

    private void SetCollidingObject(Collider collider)
    {
        // 1
        if (collidingObject || !collider.GetComponent<Rigidbody>())
        {
            return;
        }
        collidingObject = collider.gameObject;
    }

    public void OnTriggerEnter(Collider otherCollider)
    {
        SetCollidingObject(otherCollider);
    }

    public void OnTriggerStay(Collider otherCollider)
    {
        SetCollidingObject(otherCollider);
    }
 
    public void OnTriggerExit(Collider otherCollider)
    {
        if (!collidingObject)
        {
            return;
        }

        collidingObject = null;
    }

    private void GrabObject()
    {
   
        objectInHand = collidingObject;
        collidingObject = null;
        
        var joint = AddFixedJoint();
        joint.connectedBody = objectInHand.GetComponent<Rigidbody>();
    }

    private void ReleaseObject()
    {
       
        if (GetComponent<FixedJoint>())
        {
           
            GetComponent<FixedJoint>().connectedBody = null;
            Destroy(GetComponent<FixedJoint>());
            
            objectInHand.GetComponent<Rigidbody>().velocity = controllerPose.GetVelocity();
            objectInHand.GetComponent<Rigidbody>().angularVelocity = controllerPose.GetAngularVelocity();

        }
        
        objectInHand = null;
    }

    private FixedJoint AddFixedJoint()
    {
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
        fx.breakForce = 20000;
        fx.breakTorque = 20000;
        return fx;
    }
    // Update is called once per frame
    void Update()
    {
        if (grabAction.GetLastStateDown(handType))
        {
            if (collidingObject)
            {
                GrabObject();
            }
        }

        // 2
        if (grabAction.GetLastStateUp(handType))
        {
            if (objectInHand)
            {
                ReleaseObject();
            }
        }
    }
}
