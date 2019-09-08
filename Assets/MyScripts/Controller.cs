using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Controller : MonoBehaviour
{
    public Controller otherController;
    public SteamVR_Input_Sources handType;

    public SteamVR_Behaviour_Pose trackedObject;

    public SteamVR_Action_Boolean grabPinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");

    public SteamVR_Action_Boolean grabGripAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");

    public SteamVR_Action_Vibration hapticAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");

    public SteamVR_Action_Boolean uiInteractAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI");

    public bool useHoverSphere = true;
    public Transform hoverSphereTransform;
    public float hoverSphereRadius = 0.05f;
    public bool useControllerHoverComponent = true;
    public string controllerHoverComponent = "tip";
    public float controllerHoverRadius = 0.075f;
    public Transform objectAttachmentPoint;

    private AttachedObject attachedObject;

    public struct AttachedObject
    {
        public GameObject attachedObject;
        public Valve.VR.InteractionSystem.Interactable interactable;
        public Rigidbody attachedRigidbody;
        public CollisionDetectionMode collisionDetectionMode;
        public bool attachedRigidbodyWasKinematic;
        public bool attachedRigidbodyUsedGravity;
        public GameObject originalParent;
        public bool isParentedToHand;
        public Valve.VR.InteractionSystem.GrabTypes grabbedWithType;
        public Valve.VR.InteractionSystem.Hand.AttachmentFlags attachmentFlags;
        public Vector3 initialPositionalOffset;
        public Quaternion initialRotationalOffset;
        public Transform attachedOffsetTransform;
        public Transform handAttachmentPointTransform;
        public Vector3 easeSourcePosition;
        public Quaternion easeSourceRotation;
        public float attachTime;

        public bool HasAttachFlag(Valve.VR.InteractionSystem.Hand.AttachmentFlags flag)
        {
            return (attachmentFlags & flag) == flag;
        }
    }

    protected Valve.VR.InteractionSystem.RenderModel mainRenderModel;
    protected Valve.VR.InteractionSystem.RenderModel hoverhighlightRenderModel;

    private int prevOverlappingColliders = 0;
    private const int ColliderArraySize = 16;
    private Collider[] overlappingColliders;

    public LayerMask hoverLayerMask = -1;
    public float hoverUpdateInterval = 0.1f;

    public bool isActive
    {
        get
        {
            if (trackedObject != null)
                return trackedObject.isActive;

            return this.gameObject.activeInHierarchy;
        }
    }

    public bool isPoseValid
    {
        get
        {
            return trackedObject.isValid;
        }
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected virtual void OnDrawGizmos()
    {
        if (useHoverSphere && hoverSphereTransform != null)
        {
            Gizmos.color = Color.green;
            float scaledHoverRadius = hoverSphereRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(hoverSphereTransform));
            Gizmos.DrawWireSphere(hoverSphereTransform.position, scaledHoverRadius / 2);
        }
    }

    private Valve.VR.InteractionSystem.Interactable _hoveringInteractable;

    public Valve.VR.InteractionSystem.Interactable hoveringInteractable
    {
        get { return _hoveringInteractable; }
        set
        {
            if (_hoveringInteractable != value)
            {

                //Hover End Event für altes Hover Objektes
                if (_hoveringInteractable != null)
                {
                    _hoveringInteractable.SendMessage("OnHandHoverEnd", this, SendMessageOptions.DontRequireReceiver);

                    //Note: The _hoveringInteractable can change after sending the OnHandHoverEnd message so we need to check it again before broadcasting this message
                    if (_hoveringInteractable != null)
                    {
                        this.BroadcastMessage("OnParentHandHoverEnd", _hoveringInteractable, SendMessageOptions.DontRequireReceiver); // let objects attached to the hand know that a hover has ended
                    }
                }

                //Setzen des neuen Hover Objektes
                _hoveringInteractable = value;

                //Event für neues Hover Objekt
                if (_hoveringInteractable != null)
                {
                    _hoveringInteractable.SendMessage("OnHandHoverBegin", this, SendMessageOptions.DontRequireReceiver);

                    //Note: The _hoveringInteractable can change after sending the OnHandHoverBegin message so we need to check it again before broadcasting this message
                    if (_hoveringInteractable != null)
                    {
                        this.BroadcastMessage("OnParentHandHoverBegin", _hoveringInteractable, SendMessageOptions.DontRequireReceiver); // let objects attached to the hand know that a hover has begun
                    }
                }
            }
        }
    }

    //-------------------------------------------------
    // Active GameObject attached to this Hand
    //-------------------------------------------------
    public GameObject currentAttachedObject
    {
        get
        {
            return attachedObject.attachedObject;
        }
    }

    public AttachedObject? currentAttachedObjectInfo
    {
        get
        {
            return attachedObject;
        }
    }


    //-------------------------------------------------
    // Show and Hide the Controller
    //-------------------------------------------------
    public void ShowController(bool permanent = false)
    {
        if (mainRenderModel != null)
            mainRenderModel.SetControllerVisibility(true, permanent);

        if (hoverhighlightRenderModel != null)
            hoverhighlightRenderModel.SetControllerVisibility(true, permanent);
    }

    public void HideController(bool permanent = false)
    {
        if (mainRenderModel != null)
            mainRenderModel.SetControllerVisibility(false, permanent);

        if (hoverhighlightRenderModel != null)
            hoverhighlightRenderModel.SetControllerVisibility(false, permanent);
    }

    public void Show()
    {
        SetVisibility(true);
    }

    public void Hide()
    {
        SetVisibility(false);
    }

    public void SetVisibility(bool visible)
    {
        if (mainRenderModel != null)
            mainRenderModel.SetVisibility(visible);
    }

    //-------------------------------------------------
    // Attach a GameObject to this GameObject
    //
    // objectToAttach - The GameObject to attach
    // flags - The flags to use for attaching the object
    // attachmentPoint - Name of the GameObject in the hierarchy of this Hand which should act as the attachment point for this GameObject
    //-------------------------------------------------
    public void AttachObject(GameObject objectToAttach, GrabTypes grabbedWithType, AttachmentFlags flags = defaultAttachmentFlags, Transform attachmentOffset = null)
    {
        AttachedObject attachedObject = new AttachedObject();
        attachedObject.attachmentFlags = flags;
        attachedObject.attachedOffsetTransform = attachmentOffset;
        attachedObject.attachTime = Time.time;

        if (flags == 0)
        {
            flags = defaultAttachmentFlags;
        }

        //Detach the object if it is already attached so that it can get re-attached at the top of the stack
        if (ObjectIsAttached(objectToAttach))
            DetachObject(objectToAttach);

        //Detach from the other hand if requested
        if (attachedObject.HasAttachFlag(AttachmentFlags.DetachFromOtherHand))
        {
            if (otherController != null)
                otherController.DetachObject(objectToAttach);
        }


        if (currentAttachedObject)
        {
            currentAttachedObject.SendMessage("OnHandFocusLost", this, SendMessageOptions.DontRequireReceiver);
        }

        attachedObject.attachedObject = objectToAttach;
        attachedObject.interactable = objectToAttach.GetComponent<Interactable>();
        attachedObject.handAttachmentPointTransform = this.transform;


        //Check Interactable Options
        if (attachedObject.interactable != null)
        {
            if (attachedObject.interactable.attachEaseIn)
            {
                attachedObject.easeSourcePosition = attachedObject.attachedObject.transform.position;
                attachedObject.easeSourceRotation = attachedObject.attachedObject.transform.rotation;
                attachedObject.interactable.snapAttachEaseInCompleted = false;
            }

            if (attachedObject.interactable.useHandObjectAttachmentPoint)
                attachedObject.handAttachmentPointTransform = objectAttachmentPoint;

            if (attachedObject.interactable.hideHandOnAttach)
                Hide();

            if (attachedObject.interactable.hideControllerOnAttach && mainRenderModel != null && mainRenderModel.displayControllerByDefault)
                HideController();

        }

        //Check if objectToAttach has a parent to remember it in the attachedObject struct
        attachedObject.originalParent = objectToAttach.transform.parent != null ? objectToAttach.transform.parent.gameObject : null;

        attachedObject.attachedRigidbody = objectToAttach.GetComponent<Rigidbody>();


        if (attachedObject.attachedRigidbody != null)
        {
            if (attachedObject.interactable.attachedToHand != null) //already attached to another hand
            {
                //if it was attached to another hand, get the flags from that hand

                for (int attachedIndex = 0; attachedIndex < attachedObject.interactable.attachedToHand.attachedObjects.Count; attachedIndex++)
                {
                    AttachedObject attachedObjectInList = attachedObject.interactable.attachedToHand.attachedObjects[attachedIndex];
                    if (attachedObjectInList.interactable == attachedObject.interactable)
                    {
                        attachedObject.attachedRigidbodyWasKinematic = attachedObjectInList.attachedRigidbodyWasKinematic;
                        attachedObject.attachedRigidbodyUsedGravity = attachedObjectInList.attachedRigidbodyUsedGravity;
                        attachedObject.originalParent = attachedObjectInList.originalParent;
                    }
                }
            }
            else
            {
                attachedObject.attachedRigidbodyWasKinematic = attachedObject.attachedRigidbody.isKinematic;
                attachedObject.attachedRigidbodyUsedGravity = attachedObject.attachedRigidbody.useGravity;
            }
        }

        attachedObject.grabbedWithType = grabbedWithType;


        //Paranting to the Controller
        if (attachedObject.HasAttachFlag(AttachmentFlags.ParentToHand))
        { 
            objectToAttach.transform.parent = this.transform;
            attachedObject.isParentedToHand = true;
        }

        else
        {
            attachedObject.isParentedToHand = false;
        }


        //Snap the Object to the specified attachment point
        if (attachedObject.HasAttachFlag(AttachmentFlags.SnapOnAttach))
        {
            
            if (attachmentOffset != null)
            {
                //offset the object from the hand by the positional and rotational difference between the offset transform and the attached object
                Quaternion rotDiff = Quaternion.Inverse(attachmentOffset.transform.rotation) * objectToAttach.transform.rotation;
                objectToAttach.transform.rotation = attachedObject.handAttachmentPointTransform.rotation * rotDiff;

                Vector3 posDiff = objectToAttach.transform.position - attachmentOffset.transform.position;
                objectToAttach.transform.position = attachedObject.handAttachmentPointTransform.position + posDiff;
            }
            else
            {
                //snap the object to the center of the attach point
                objectToAttach.transform.rotation = attachedObject.handAttachmentPointTransform.rotation;
                objectToAttach.transform.position = attachedObject.handAttachmentPointTransform.position;
            }

            Transform followPoint = objectToAttach.transform;

            attachedObject.initialPositionalOffset = attachedObject.handAttachmentPointTransform.InverseTransformPoint(followPoint.position);
            attachedObject.initialRotationalOffset = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * followPoint.rotation;
           
        }

        else
        {
            if (attachmentOffset != null)
            {
                //get the initial positional and rotational offsets between the hand and the offset transform
                Quaternion rotDiff = Quaternion.Inverse(attachmentOffset.transform.rotation) * objectToAttach.transform.rotation;
                Quaternion targetRotation = attachedObject.handAttachmentPointTransform.rotation * rotDiff;
                Quaternion rotationPositionBy = targetRotation * Quaternion.Inverse(objectToAttach.transform.rotation);

                Vector3 posDiff = (rotationPositionBy * objectToAttach.transform.position) - (rotationPositionBy * attachmentOffset.transform.position);

                attachedObject.initialPositionalOffset = attachedObject.handAttachmentPointTransform.InverseTransformPoint(attachedObject.handAttachmentPointTransform.position + posDiff);
                attachedObject.initialRotationalOffset = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * (attachedObject.handAttachmentPointTransform.rotation * rotDiff);
            }
            else
            {
                attachedObject.initialPositionalOffset = attachedObject.handAttachmentPointTransform.InverseTransformPoint(objectToAttach.transform.position);
                attachedObject.initialRotationalOffset = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * objectToAttach.transform.rotation;
            }
            
        }


        //Turn off/on Kinematic
        if (attachedObject.HasAttachFlag(AttachmentFlags.TurnOnKinematic))
        {
            if (attachedObject.attachedRigidbody != null)
            {
                attachedObject.collisionDetectionMode = attachedObject.attachedRigidbody.collisionDetectionMode;
                if (attachedObject.collisionDetectionMode == CollisionDetectionMode.Continuous)
                    attachedObject.attachedRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

                attachedObject.attachedRigidbody.isKinematic = true;
            }
        }

        //Turn off/on Gravity
        if (attachedObject.HasAttachFlag(AttachmentFlags.TurnOffGravity))
        {
            if (attachedObject.attachedRigidbody != null)
            {
                attachedObject.attachedRigidbody.useGravity = false;
            }
        }


        //
        if (attachedObject.interactable != null && attachedObject.interactable.attachEaseIn)
        {
            attachedObject.attachedObject.transform.position = attachedObject.easeSourcePosition;
            attachedObject.attachedObject.transform.rotation = attachedObject.easeSourceRotation;
        }

        //attachedObjects.Add(attachedObject);

        UpdateHovering();

        //Send Message to Skript in Object that it was attached
        objectToAttach.SendMessage("OnAttachedToHand", this, SendMessageOptions.DontRequireReceiver);
    }

    //-------------------------------------------------
    // Detach this GameObject from the attached object stack of this Hand
    //
    // objectToDetach - The GameObject to detach from this Hand
    //-------------------------------------------------
    public void DetachObject(GameObject objectToDetach, bool restoreOriginalParent = true)
    {
        if (attachedObject != null)
        {

            GameObject prevTopObject = currentAttachedObject;


            if (attachedObject.interactable != null)
            {
                if (attachedObject.interactable.hideHandOnAttach)
                    Show();

                if (attachedObject.interactable.hideControllerOnAttach && mainRenderModel != null && mainRenderModel.displayControllerByDefault)
                    ShowController();
            }

            Transform parentTransform = null;
            if (attachedObject.isParentedToHand)
            {
                if (restoreOriginalParent && (attachedObject.originalParent != null))
                {
                    parentTransform = attachedObject.originalParent.transform;
                }

                if (attachedObject.attachedObject != null)
                {
                    attachedObject.attachedObject.transform.parent = parentTransform;
                }
            }

            if (attachedObject.HasAttachFlag(Valve.VR.InteractionSystem.Hand.AttachmentFlags.TurnOnKinematic))
            {
                if (attachedObject.attachedRigidbody != null)
                {
                    attachedObject.attachedRigidbody.isKinematic = attachedObject.attachedRigidbodyWasKinematic;
                    attachedObject.attachedRigidbody.collisionDetectionMode = attachedObject.collisionDetectionMode;
                }
            }

            if (attachedObject.HasAttachFlag(Valve.VR.InteractionSystem.Hand.AttachmentFlags.TurnOffGravity))
            {
                if (attachedObject.attachedObject != null)
                {
                    if (attachedObject.attachedRigidbody != null)
                        attachedObject.attachedRigidbody.useGravity = attachedObject.attachedRigidbodyUsedGravity;
                }
            }


            if (attachedObject.attachedObject != null)
            {
                if (attachedObject.interactable == null || (attachedObject.interactable != null && attachedObject.interactable.isDestroying == false))
                    attachedObject.attachedObject.SetActive(true);

                attachedObject.attachedObject.SendMessage("OnDetachedFromHand", this, SendMessageOptions.DontRequireReceiver);
            }

            GameObject newTopObject = currentAttachedObject;


            //Give focus to the top most object on the stack if it changed
            if (newTopObject != null && newTopObject != prevTopObject)
            {
                newTopObject.SetActive(true);
                newTopObject.SendMessage("OnHandFocusAcquired", this, SendMessageOptions.DontRequireReceiver);
            }
        }


        if (mainRenderModel != null)
            mainRenderModel.MatchHandToTransform(mainRenderModel.transform);
        if (hoverhighlightRenderModel != null)
            hoverhighlightRenderModel.MatchHandToTransform(hoverhighlightRenderModel.transform);
    }

    protected virtual void UpdateHovering()
    { 
        if (applicationLostFocusObject.activeSelf)
            return;

        float closestDistance = float.MaxValue;
        Valve.VR.InteractionSystem.Interactable closestInteractable = null;

        if (useHoverSphere)
        {
            float scaledHoverRadius = hoverSphereRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(hoverSphereTransform));
            CheckHoveringForTransform(hoverSphereTransform.position, scaledHoverRadius, ref closestDistance, ref closestInteractable, Color.green);
        }

        if (useControllerHoverComponent && mainRenderModel != null && mainRenderModel.IsControllerVisibile())
        {
            float scaledHoverRadius = controllerHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(this.transform));
            CheckHoveringForTransform(mainRenderModel.GetControllerPosition(controllerHoverComponent), scaledHoverRadius / 2f, ref closestDistance, ref closestInteractable, Color.blue);
        }

        // Hover on this one
        hoveringInteractable = closestInteractable;
    }

    protected virtual bool CheckHoveringForTransform(Vector3 hoverPosition, float hoverRadius, ref float closestDistance, ref Valve.VR.InteractionSystem.Interactable closestInteractable, Color debugColor)
    {
        bool foundCloser = false;

        // null out old vals
        for (int i = 0; i < overlappingColliders.Length; ++i)
        {
            overlappingColliders[i] = null;
        }

        int numColliding = Physics.OverlapSphereNonAlloc(hoverPosition, hoverRadius, overlappingColliders, hoverLayerMask.value);

        if (numColliding == ColliderArraySize)
            Debug.LogWarning("<b>[SteamVR Interaction]</b> This hand is overlapping the max number of colliders: " + ColliderArraySize + ". Some collisions may be missed. Increase ColliderArraySize on Hand.cs");

        // DebugVar
        int iActualColliderCount = 0;

        // Pick the closest hovering
        for (int colliderIndex = 0; colliderIndex < overlappingColliders.Length; colliderIndex++)
        {
            Collider collider = overlappingColliders[colliderIndex];

            if (collider == null)
                continue;

            Valve.VR.InteractionSystem.Interactable contacting = collider.GetComponentInParent<Valve.VR.InteractionSystem.Interactable>();

            // Yeah, it's null, skip
            if (contacting == null)
                continue;

            // Ignore this collider for hovering
            Valve.VR.InteractionSystem.IgnoreHovering ignore = collider.GetComponent<Valve.VR.InteractionSystem.IgnoreHovering>();
            if (ignore != null)
            {
                if (ignore.onlyIgnoreHand == null || ignore.onlyIgnoreHand == this)
                {
                    continue;
                }
            }

            // Can't hover over the object if it's attached
            bool hoveringOverAttached = false;
            
            if (attachedObject.attachedObject == contacting.gameObject)
            {
                hoveringOverAttached = true;
                break;
            }
            if (hoveringOverAttached)
                continue;

            // Occupied by another hand, so we can't touch it
            if (otherController && otherController.hoveringInteractable == contacting)
                continue;

            // Best candidate so far...
            float distance = Vector3.Distance(contacting.transform.position, hoverPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestInteractable = contacting;
                foundCloser = true;
            }
            iActualColliderCount++;
        }

        if (iActualColliderCount > 0 && iActualColliderCount != prevOverlappingColliders)
        {
            prevOverlappingColliders = iActualColliderCount;
        }

        return foundCloser;
    }

    //-------------------------------------------------
    // Get the world velocity of the VR Hand.
    //-------------------------------------------------
    public Vector3 GetTrackedObjectVelocity(float timeOffset = 0)
    {
        if (trackedObject == null)
        {
            Vector3 velocityTarget, angularTarget;
            GetUpdatedAttachedVelocities(currentAttachedObjectInfo.Value, out velocityTarget, out angularTarget);
            return velocityTarget;
        }

        if (isActive)
        {
            if (timeOffset == 0)
                return Valve.VR.InteractionSystem.Player.instance.trackingOriginTransform.TransformVector(trackedObject.GetVelocity());
            else
            {
                Vector3 velocity;
                Vector3 angularVelocity;

                trackedObject.GetVelocitiesAtTimeOffset(timeOffset, out velocity, out angularVelocity);
                return Valve.VR.InteractionSystem.Player.instance.trackingOriginTransform.TransformVector(velocity);
            }
        }

        return Vector3.zero;
    }


    //-------------------------------------------------
    // Get the world space angular velocity of the VR Hand.
    //-------------------------------------------------
    public Vector3 GetTrackedObjectAngularVelocity(float timeOffset = 0)
    {
        if (trackedObject == null)
        {
            Vector3 velocityTarget, angularTarget;
            GetUpdatedAttachedVelocities(currentAttachedObjectInfo.Value, out velocityTarget, out angularTarget);
            return angularTarget;
        }

        if (isActive)
        {
            if (timeOffset == 0)
                return Valve.VR.InteractionSystem.Player.instance.trackingOriginTransform.TransformDirection(trackedObject.GetAngularVelocity());
            else
            {
                Vector3 velocity;
                Vector3 angularVelocity;

                trackedObject.GetVelocitiesAtTimeOffset(timeOffset, out velocity, out angularVelocity);
                return Valve.VR.InteractionSystem.Player.instance.trackingOriginTransform.TransformDirection(angularVelocity);
            }
        }

        return Vector3.zero;
    }

    public void GetEstimatedPeakVelocities(out Vector3 velocity, out Vector3 angularVelocity)
    {
        trackedObject.GetEstimatedPeakVelocities(out velocity, out angularVelocity);
        velocity = Valve.VR.InteractionSystem.Player.instance.trackingOriginTransform.TransformVector(velocity);
        angularVelocity = Valve.VR.InteractionSystem.Player.instance.trackingOriginTransform.TransformDirection(angularVelocity);
    }

    protected const float MaxVelocityChange = 10f;
    protected const float VelocityMagic = 6000f;
    protected const float AngularVelocityMagic = 50f;
    protected const float MaxAngularVelocityChange = 20f;

    protected void UpdateAttachedVelocity(AttachedObject attachedObjectInfo)
    {
        Vector3 velocityTarget, angularTarget;
        bool success = GetUpdatedAttachedVelocities(attachedObjectInfo, out velocityTarget, out angularTarget);
        if (success)
        {
            float scale = SteamVR_Utils.GetLossyScale(currentAttachedObjectInfo.Value.handAttachmentPointTransform);
            float maxAngularVelocityChange = MaxAngularVelocityChange * scale;
            float maxVelocityChange = MaxVelocityChange * scale;

            attachedObjectInfo.attachedRigidbody.velocity = Vector3.MoveTowards(attachedObjectInfo.attachedRigidbody.velocity, velocityTarget, maxVelocityChange);
            attachedObjectInfo.attachedRigidbody.angularVelocity = Vector3.MoveTowards(attachedObjectInfo.attachedRigidbody.angularVelocity, angularTarget, maxAngularVelocityChange);
        }
    }

    /// <summary>
    /// Snap an attached object to its target position and rotation. Good for error correction.
    /// </summary>
    public void ResetAttachedTransform(AttachedObject attachedObject)
    {
        attachedObject.attachedObject.transform.position = TargetItemPosition(attachedObject);
        attachedObject.attachedObject.transform.rotation = TargetItemRotation(attachedObject);
    }

    protected Vector3 TargetItemPosition(AttachedObject attachedObject)
    {
        if (attachedObject.interactable != null && attachedObject.interactable.skeletonPoser != null && HasSkeleton())
        {
            Vector3 tp = attachedObject.handAttachmentPointTransform.InverseTransformPoint(transform.TransformPoint(attachedObject.interactable.skeletonPoser.GetBlendedPose(skeleton).position));
            //tp.x *= -1;
            return currentAttachedObjectInfo.Value.handAttachmentPointTransform.TransformPoint(tp);
        }
        else
        {
            return currentAttachedObjectInfo.Value.handAttachmentPointTransform.TransformPoint(attachedObject.initialPositionalOffset);
        }
    }

    protected Quaternion TargetItemRotation(AttachedObject attachedObject)
    {
        if (attachedObject.interactable != null && attachedObject.interactable.skeletonPoser != null && HasSkeleton())
        {
            Quaternion tr = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * (transform.rotation * attachedObject.interactable.skeletonPoser.GetBlendedPose(skeleton).rotation);
            return currentAttachedObjectInfo.Value.handAttachmentPointTransform.rotation * tr;
        }
        else
        {
            return currentAttachedObjectInfo.Value.handAttachmentPointTransform.rotation * attachedObject.initialRotationalOffset;
        }
    }

    protected bool GetUpdatedAttachedVelocities(AttachedObject attachedObjectInfo, out Vector3 velocityTarget, out Vector3 angularTarget)
    {
        bool realNumbers = false;


        float velocityMagic = VelocityMagic;
        float angularVelocityMagic = AngularVelocityMagic;

        Vector3 targetItemPosition = TargetItemPosition(attachedObjectInfo);
        Vector3 positionDelta = (targetItemPosition - attachedObjectInfo.attachedRigidbody.position);
        velocityTarget = (positionDelta * velocityMagic * Time.deltaTime);

        if (float.IsNaN(velocityTarget.x) == false && float.IsInfinity(velocityTarget.x) == false)
        {

            realNumbers = true;
        }
        else
            velocityTarget = Vector3.zero;


        Quaternion targetItemRotation = TargetItemRotation(attachedObjectInfo);
        Quaternion rotationDelta = targetItemRotation * Quaternion.Inverse(attachedObjectInfo.attachedObject.transform.rotation);


        float angle;
        Vector3 axis;
        rotationDelta.ToAngleAxis(out angle, out axis);

        if (angle > 180)
            angle -= 360;

        if (angle != 0 && float.IsNaN(axis.x) == false && float.IsInfinity(axis.x) == false)
        {
            angularTarget = angle * axis * angularVelocityMagic * Time.deltaTime;
            realNumbers &= true;
        }
        else
            angularTarget = Vector3.zero;

        return realNumbers;
    }

    protected virtual void Awake()
    {
        inputFocusAction = SteamVR_Events.InputFocusAction(OnInputFocus);

        if (hoverSphereTransform == null)
            hoverSphereTransform = this.transform;

        if (objectAttachmentPoint == null)
            objectAttachmentPoint = this.transform;

        applicationLostFocusObject = new GameObject("_application_lost_focus");
        applicationLostFocusObject.transform.parent = transform;
        applicationLostFocusObject.SetActive(false);

        if (trackedObject == null)
        {
            trackedObject = this.gameObject.GetComponent<SteamVR_Behaviour_Pose>();

            if (trackedObject != null)
                trackedObject.onTransformUpdatedEvent += OnTransformUpdated;
        }
    }

    protected virtual void OnDestroy()
    {
        if (trackedObject != null)
        {
            trackedObject.onTransformUpdatedEvent -= OnTransformUpdated;
        }
    }

    protected virtual void OnTransformUpdated(SteamVR_Behaviour_Pose updatedPose, SteamVR_Input_Sources updatedSource)
    {
        HandFollowUpdate();
    }

    //-------------------------------------------------
    protected virtual IEnumerator Start()
    {
        // save off player instance
        playerInstance = Player.instance;
        if (!playerInstance)
        {
            Debug.LogError("<b>[SteamVR Interaction]</b> No player instance found in Hand Start()");
        }

        // allocate array for colliders
        overlappingColliders = new Collider[ColliderArraySize];

        // We are a "no SteamVR fallback hand" if we have this camera set
        // we'll use the right mouse to look around and left mouse to interact
        // - don't need to find the device
        if (noSteamVRFallbackCamera)
        {
            yield break;
        }

        //Debug.Log( "<b>[SteamVR Interaction]</b> Hand - initializing connection routine" );

        while (true)
        {
            if (isPoseValid)
            {
                InitController();
                break;
            }

            yield return null;
        }
    }



}
