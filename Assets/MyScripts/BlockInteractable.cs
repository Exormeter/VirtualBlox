﻿using UnityEngine;
using System.Collections.Generic;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(Interactable))]
    [RequireComponent(typeof(BlockScript))]
    public class BlockInteractable : MonoBehaviour
    {
        public enum AttachMode
        {
            FixedJoint,
            Force,
        }

        public float attachForce = 800.0f;
        public float attachForceDamper = 25.0f;
        public float pullDistanceMaximum = 1.0f;


       
        public AttachMode attachMode = AttachMode.FixedJoint;

        [EnumFlags]
        public Hand.AttachmentFlags attachmentFlags = 0;

        private bool pullingStart = false;
        private Hand pullingHand = null;
        private GrabTypes pullingGrabType;
        private GrooveHandler grooveHandler;
        private float nextPulseTime = 0;

        private List<Hand> holdingHands = new List<Hand>();
        private List<Rigidbody> holdingBodies = new List<Rigidbody>();
        private List<Vector3> holdingPoints = new List<Vector3>();

        private List<Rigidbody> rigidBodies = new List<Rigidbody>();

        //-------------------------------------------------
        void Start()
        {
            GetComponentsInChildren<Rigidbody>(rigidBodies);
            grooveHandler = GetComponentInChildren<GrooveHandler>();
        }


        //-------------------------------------------------
        void Update()
        {
            for (int i = 0; i < holdingHands.Count; i++)
            {
                if (holdingHands[i].IsGrabEnding(this.gameObject))
                {
                    PhysicsDetach(holdingHands[i]);
                }
            }
        }


        //-------------------------------------------------
        private void OnHandHoverBegin(Hand hand)
        {
            if (holdingHands.IndexOf(hand) == -1)
            {
                if (hand.isActive)
                {
                    hand.TriggerHapticPulse(300);
                }
            }
        }


        //-------------------------------------------------
        private void OnHandHoverEnd(Hand hand)
        {
            if (holdingHands.IndexOf(hand) == -1)
            {
                if (hand.isActive)
                {
                    
                }
            }
        }


        //-------------------------------------------------
        private void HandHoverUpdate(Hand hand)
        {
            GrabTypes startingGrabType = hand.GetGrabStarting();

            if (startingGrabType != GrabTypes.None  && grooveHandler.IsSnapped())
            {
                pullingStart = true;
                pullingHand = hand;
                pullingGrabType = startingGrabType;
            }
            else if(startingGrabType != GrabTypes.None)
            {
                PhysicsAttach(hand, startingGrabType);
            }
        }

        private void blockPullUpdate()
        {

            //No Hand is pulling
            if(pullingHand == null)
            {
                return;
            }

            

            if (pullingStart)
            {

                //hand is pulling but has let gone of the grab button
                if (!pullingHand.IsGrabbingWithType(pullingGrabType))
                {
                    Debug.Log("Pulling Ended");
                    pullingStart = false;
                    pullingHand = null;
                    pullingGrabType = GrabTypes.None;
                    return;
                }

                float distanceHandToBlock = Vector3.Distance(pullingHand.transform.position, rigidBodies[0].worldCenterOfMass);
                LineRenderer lineRenderer = GetComponent<LineRenderer>();
                lineRenderer.SetPosition(0, pullingHand.transform.position);
                lineRenderer.SetPosition(1, pullingHand.transform.position - rigidBodies[0].worldCenterOfMass);
                lineRenderer.SetPosition(2, rigidBodies[0].worldCenterOfMass);

                if (Time.time > nextPulseTime)
                {
                    Debug.Log("Distance: " + distanceHandToBlock);
                    pullingHand.TriggerHapticPulse(0.1f, 20, distanceHandToBlock * 3);
                    nextPulseTime = Time.time + 0.1f;
                }
                
                if(distanceHandToBlock >= pullDistanceMaximum)
                {
                    Debug.Log("Attached to hand");
                    lineRenderer.SetPosition(0, Vector3.zero);
                    lineRenderer.SetPosition(1, Vector3.zero);
                    PhysicsAttach(pullingHand, pullingGrabType);
                    pullingStart = false;
                    pullingHand = null;
                    pullingGrabType = GrabTypes.None;
                }
            }
        }


        //-------------------------------------------------
        private void PhysicsAttach(Hand hand, GrabTypes startingGrabType)
        {
            PhysicsDetach(hand);

            Rigidbody holdingBody = null;
            Vector3 holdingPoint = Vector3.zero;

            // The hand should grab onto the nearest rigid body
            float closestDistance = float.MaxValue;
            for (int i = 0; i < rigidBodies.Count; i++)
            {
                float distance = Vector3.Distance(rigidBodies[i].worldCenterOfMass, hand.transform.position);
                if (distance < closestDistance)
                {
                    holdingBody = rigidBodies[i];
                    closestDistance = distance;
                }
            }

            // Couldn't grab onto a body
            if (holdingBody == null)
                return;

            // Create a fixed joint from the hand to the holding body
            if (attachMode == AttachMode.FixedJoint)
            {
                Rigidbody handRigidbody = Util.FindOrAddComponent<Rigidbody>(hand.gameObject);
                handRigidbody.isKinematic = true;

                FixedJoint handJoint = hand.gameObject.AddComponent<FixedJoint>();
                handJoint.connectedBody = holdingBody;
            }

            // Don't let the hand interact with other things while it's holding us
            hand.HoverLock(null);

            // Affix this point
            Vector3 offset = hand.transform.position - holdingBody.worldCenterOfMass;
            offset = Mathf.Min(offset.magnitude, 1.0f) * offset.normalized;
            holdingPoint = holdingBody.transform.InverseTransformPoint(holdingBody.worldCenterOfMass + offset);

            hand.AttachObject(this.gameObject, startingGrabType, attachmentFlags);

            // Update holding list
            holdingHands.Add(hand);
            holdingBodies.Add(holdingBody);
            holdingPoints.Add(holdingPoint);
        }


        //-------------------------------------------------
        private bool PhysicsDetach(Hand hand)
        {
            int i = holdingHands.IndexOf(hand);

            if (i != -1)
            {
                // Detach this object from the hand
                holdingHands[i].DetachObject(this.gameObject, false);

                // Allow the hand to do other things
                holdingHands[i].HoverUnlock(null);

                // Delete any existing joints from the hand
                if (attachMode == AttachMode.FixedJoint)
                {
                    Destroy(holdingHands[i].GetComponent<FixedJoint>());
                }

                Util.FastRemove(holdingHands, i);
                Util.FastRemove(holdingBodies, i);
                Util.FastRemove(holdingPoints, i);

                return true;
            }

            return false;
        }


        //-------------------------------------------------
        void FixedUpdate()
        {
            blockPullUpdate();
            if (attachMode == AttachMode.Force)
            {
                for (int i = 0; i < holdingHands.Count; i++)
                {
                    Vector3 targetPoint = holdingBodies[i].transform.TransformPoint(holdingPoints[i]);
                    Vector3 vdisplacement = holdingHands[i].transform.position - targetPoint;

                    holdingBodies[i].AddForceAtPosition(attachForce * vdisplacement, targetPoint, ForceMode.Acceleration);
                    holdingBodies[i].AddForceAtPosition(-attachForceDamper * holdingBodies[i].GetPointVelocity(targetPoint), targetPoint, ForceMode.Acceleration);
                }
            }
        }
    }
}

