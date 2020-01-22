using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
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

        private bool wasPulled = false;
        private Hand pullingHand = null;
        private GrabTypes pullingGrabType;
        private GrooveHandler grooveHandler;
        private LineRenderer lineRenderer;
        //private float nextPulseTime = 0;

        public List<Hand> holdingHands = new List<Hand>();
        private List<Rigidbody> holdingBodies = new List<Rigidbody>();
        private List<Vector3> holdingPoints = new List<Vector3>();

        private List<Rigidbody> rigidBodies = new List<Rigidbody>();
        private int frameUntilColliderReEvaluation = 3;

        //-------------------------------------------------
        void Start()
        {
            GetComponentsInChildren(rigidBodies);
            grooveHandler = GetComponentInChildren<GrooveHandler>();
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 3;
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
            
            if (startingGrabType != GrabTypes.None && GetComponent<BlockCommunication>().IsIndirectlyAttachedToFloor() && !GetComponent<Interactable>().isMarked)
            {
                pullingHand = hand;
                pullingGrabType = startingGrabType;
                Debug.Log("Attaching To Hand");
            }

            else if (startingGrabType != GrabTypes.None && GetComponent<BlockCommunication>().IsIndirectlyAttachedToFloor() && GetComponent<Interactable>().isMarked)
            {
                GameObject block = GameObject.FindGameObjectWithTag("BlockMarker").GetComponent<BlockMarker>().RebuildMarkedStructure(this.gameObject);
                //block.GetComponent<BlockInteractable>().PhysicsAttach(hand, GrabTypes.Grip);
                StartCoroutine(AttachNewBlockToHand(block, hand));
                Debug.Log("Attaching To Hand");
            }

            else if(startingGrabType != GrabTypes.None)
            {
                Debug.Log("Attaching To Hand");
                PhysicsAttach(hand, startingGrabType);
            }
        }

        private void BlockPullUpdate()
        {

            //No Hand is pulling
            if(pullingHand == null)
            {
                return;
            }

            //hand is pulling but has let gone of the grab button
            if (!pullingHand.IsGrabbingWithType(pullingGrabType))
            {
                Debug.Log("Pulling Ended");
                ResetPullingState();
                return;
            }

            float distanceHandToBlock = Vector3.Distance(pullingHand.transform.position, rigidBodies[0].worldCenterOfMass);
            RenderForceLine(pullingHand.transform.position, GetComponent<BlockGeometryScript>().GetCenterTopWorld());

            //if (Time.time > nextPulseTime)
            //{
            //    pullingHand.TriggerHapticPulse(0.1f, 20, distanceHandToBlock * 3);
            //    nextPulseTime = Time.time + 0.1f;
            //}
                
            if(distanceHandToBlock >= pullDistanceMaximum)
            {
                GetComponent<BlockCommunication>().AttemptToFreeBlock();
                wasPulled = true;
            }

            if (wasPulled)
            {
                Vector3 direction = (pullingHand.transform.position - transform.position).normalized;
                rigidBodies[0].MovePosition(transform.position + direction * 10f * Time.deltaTime);

                if (Vector3.Distance(pullingHand.transform.position, gameObject.transform.position) <= 0.1f)
                {
                    PhysicsAttach(pullingHand, pullingGrabType);
                    Debug.Log("Attached to hand");
                    ResetPullingState();
                }
            }
           
        }

        private void ResetPullingState()
        {
            wasPulled = false;
            pullingHand = null;
            pullingGrabType = GrabTypes.None;
            GetComponent<LineRenderer>().enabled = false;
        }


        //-------------------------------------------------
        public void PhysicsAttach(Hand hand, GrabTypes startingGrabType)
        {
            GetComponentsInChildren(rigidBodies);
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
                handJoint.enableCollision = true;
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
            BlockPullUpdate();
            if (attachMode == AttachMode.Force)
            {
                for (int i = 0; i < holdingHands.Count; i++)
                {
                    Vector3 targetPoint = holdingBodies[i].transform.TransformPoint(holdingPoints[i]);
                    Vector3 vdisplacement = holdingHands[i].transform.position - targetPoint;

                    holdingBodies[i].AddForceAtPosition(attachForce * vdisplacement, targetPoint, ForceMode.Acceleration);
                    holdingBodies[i].AddForceAtPosition(-attachForceDamper * holdingBodies[i].GetPointVelocity(targetPoint), targetPoint, ForceMode.Acceleration);

                    Quaternion rotationDelta = holdingHands[i].transform.rotation * Quaternion.Inverse(holdingBodies[i].transform.rotation);

                    float angle;
                    Vector3 axis;

                    rotationDelta.ToAngleAxis(out angle, out axis);

                    if (angle > 180)
                        angle -= 360;

                    if (angle != 0)
                    {
                        Vector3 angularTarget = angle * axis;
                        if (float.IsNaN(angularTarget.x) == false)
                        {
                            angularTarget = (angularTarget * 25) * Time.deltaTime;
                            angularTarget = Vector3.MoveTowards(holdingBodies[i].angularVelocity, angularTarget, 3000);
                            holdingBodies[i].angularVelocity = angularTarget;
                        }
                    }

                }
            }
        }

        void RenderForceLine(Vector3 start, Vector3 end)
        {
            lineRenderer.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            float distance = Vector3.Distance(start, end);
            float width = 0.1f / (distance * 10);
            curve.AddKey(0, 0.01f);
            curve.AddKey(0.5f, width);
            curve.AddKey(1, 0.01f);
            
            lineRenderer.widthCurve = curve;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, (start + end) / 2);
            lineRenderer.SetPosition(2, end);
        }

        private IEnumerator AttachNewBlockToHand(GameObject generatedBlock, Hand hand)
        {
            {
                for (int i = 0; i <= frameUntilColliderReEvaluation; i++)
                {
                    if (i == frameUntilColliderReEvaluation)
                    { 
                        generatedBlock.GetComponent<BlockInteractable>().PhysicsAttach(hand, GrabTypes.Grip);
                    }
                    yield return new WaitForFixedUpdate();
                }

            }
        }
    }
}

