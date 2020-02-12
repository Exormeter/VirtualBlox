using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    public class InputManager : MonoBehaviour
    {
        /// <summary>
        /// Read the Controller Pose (Rotation and Positon) in
        /// </summary>
        public SteamVR_Action_Pose Pose;

        /// <summary>
        /// Inputs from the right Controller
        /// </summary>
        public SteamVR_Input_Sources RightHandInput;

        /// <summary>
        ///Input form the left Controller 
        /// </summary>
        public SteamVR_Input_Sources LeftHandInput;

        /// <summary>
        /// Thump position on the TouchPad
        /// </summary>
        public SteamVR_Action_Vector2 TouchPadPosition;

        /// <summary>
        /// Was the Touchpad pressed in
        /// </summary>
        public SteamVR_Action_Boolean TouchPadButton;

        /// <summary>
        /// Was the GripButton pressed
        /// </summary>
        public SteamVR_Action_Boolean GripButton;

        /// <summary>
        /// Left Hand Script
        /// </summary>
        public Hand leftHand;

        /// <summary>
        /// Right Hand Script
        /// </summary>
        public Hand rightHand;

        /// <summary>
        /// Which Hand side initiated the pulling action
        /// </summary>
        private HANDSIDE startedPulling = HANDSIDE.HAND_NONE;

        /// <summary>
        /// The position of the pull start
        /// </summary>
        private Vector3 startPullPosition;

        /// <summary>
        /// Markes the left side of the TouchPad
        /// </summary>
        private readonly float leftArraowActivationThreshold = -0.7f;

        /// <summary>
        /// Markes the right side of the TouchPad
        /// </summary>
        private readonly float rightArraowActivationThreshold = 0.7f;


        [System.Serializable]
        public class MarkerAddEvent: UnityEvent<Interactable> { }

        [System.Serializable]
        public class MenuEvent: UnityEvent<HANDSIDE> { }

        /// <summary>
        /// Event when the left Menu was opened
        /// </summary>
        [SerializeField]
        public MenuEvent OnPoseOpenMenuLeft = new MenuEvent();

        /// <summary>
        /// Event when the right Menu was opened
        /// </summary>
        [SerializeField]
        public MenuEvent OnPoseOpenMenuRight = new MenuEvent();

        /// <summary>
        /// Event when the left Menu was closed
        /// </summary>
        [SerializeField]
        public MenuEvent OnPoseCloseMenuLeft = new MenuEvent();

        /// <summary>
        /// Event when right Menu was closed
        /// </summary>
        [SerializeField]
        public MenuEvent OnPoseCloseMenuRight = new MenuEvent();

        /// <summary>
        /// Event when the Marker started pulling
        /// </summary>
        [SerializeField]
        public MenuEvent OnStartMarkerPull = new MenuEvent();

        /// <summary>
        /// Event when the Marker stopped pulling
        /// </summary>
        [SerializeField]
        public MenuEvent OnEndMarkerPull = new MenuEvent();

        /// <summary>
        /// Event when the Marker is pulling
        /// </summary>
        [SerializeField]
        public MenuEvent OnMarkerPulling = new MenuEvent();

        /// <summary>
        /// Event when the left side of the TouchPad was pressed
        /// </summary>
        [SerializeField]
        public MenuEvent OnLeftArrowClick = new MenuEvent();

        /// <summary>
        /// Event when the right side of the TouchPad was pressed
        /// </summary>
        [SerializeField]
        public MenuEvent OnRightArrowClick = new MenuEvent();

        /// <summary>
        /// Event when a Block should be marked
        /// </summary>
        [SerializeField]
        public MarkerAddEvent OnMarkBlock = new MarkerAddEvent();

        /// <summary>
        /// Event when Teleport Pointer should appear
        /// </summary>
        [SerializeField]
        public MenuEvent OnTeleportDown = new MenuEvent();

        /// <summary>
        /// Event when Teleport should happen
        /// </summary>
        [SerializeField]
        public MenuEvent OnTeleportUp = new MenuEvent();

        /// <summary>
        /// Current State of the on Controller Menu
        /// </summary>
        private MenuState CurrentMenuState = MenuState.BOTH_CLOSED;
        
        void Start()
        {
            StartCoroutine(ReadPose());

            TouchPadButton.AddOnStateDownListener(LeftTouchPadDown, LeftHandInput);
            TouchPadButton.AddOnStateUpListener(LeftTouchPadUp, LeftHandInput);

            TouchPadButton.AddOnStateDownListener(RightTouchPadDown, RightHandInput);
            TouchPadButton.AddOnStateUpListener(RightTouchPadUp, RightHandInput);

            GripButton.AddOnStateDownListener(PushGripButtonLeft, LeftHandInput);
            GripButton.AddOnStateDownListener(PushGripButtonRight, RightHandInput);


        }

        

        void Update()
        {
            //If startedPulling is set, the marker is currently pulled
            if (startedPulling != HANDSIDE.HAND_NONE)
            {
                OnMarkerPulling.Invoke(startedPulling);
            }
        }

        /// <summary>
        /// Callback for when Left TouchPad was clicked, check what section of TouchPad was pressed
        /// </summary>
        /// <param name="fromAction"></param>
        /// <param name="fromSource"></param>
        private void LeftTouchPadDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            //Return if Menu is open or Marker is currently pulled
            if (CurrentMenuState != MenuState.BOTH_CLOSED || startedPulling != HANDSIDE.HAND_NONE)
                return;

            //Check if Marker TouchPad position was touched
            else if(TouchPadPosition.GetLastAxis(fromSource).y < 0 && TouchPadPosition.GetLastAxis(fromSource).x > leftArraowActivationThreshold && TouchPadPosition.GetLastAxis(fromSource).x < rightArraowActivationThreshold)
            {
                OnStartMarkerPull.Invoke(HANDSIDE.HAND_LEFT);
                startedPulling = HANDSIDE.HAND_LEFT;
                CurrentMenuState = MenuState.DONT_OPEN;
            }

            //Check if Teleport TouchPad position was touched
            else if(TouchPadPosition.GetLastAxis(fromSource).y > 0 && TouchPadPosition.GetLastAxis(fromSource).x > leftArraowActivationThreshold && TouchPadPosition.GetLastAxis(fromSource).x < rightArraowActivationThreshold)
            {
                OnTeleportDown.Invoke(HANDSIDE.HAND_LEFT);
            }
        }

        /// <summary>
        /// Callback for when Right TouchPad was clicked, check if the Marker section of TouchPad was pressed
        /// </summary>
        /// <param name="fromAction"></param>
        /// <param name="fromSource"></param>
        private void RightTouchPadDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            //Return if Menu is open or Marker is currently pulled
            if (CurrentMenuState != MenuState.BOTH_CLOSED || startedPulling != HANDSIDE.HAND_NONE)
                return;

            //Check if Marker TouchPad position was touched
            else if (TouchPadPosition.GetLastAxis(fromSource).y < 0 && TouchPadPosition.GetLastAxis(fromSource).x > leftArraowActivationThreshold && TouchPadPosition.GetLastAxis(fromSource).x < rightArraowActivationThreshold)
            {
                OnStartMarkerPull.Invoke(HANDSIDE.HAND_RIGHT);
                startedPulling = HANDSIDE.HAND_RIGHT;
                CurrentMenuState = MenuState.DONT_OPEN;
            }

            //Check if Teleport TouchPad position was touched
            else if (TouchPadPosition.GetLastAxis(fromSource).y > 0 && TouchPadPosition.GetLastAxis(fromSource).x > leftArraowActivationThreshold && TouchPadPosition.GetLastAxis(fromSource).x < rightArraowActivationThreshold)
            {
                OnTeleportDown.Invoke(HANDSIDE.HAND_RIGHT);
            }
        }

        /// <summary>
        /// Callback for when left TouchPad is up
        /// </summary>
        /// <param name="fromAction"></param>
        /// <param name="fromSource"></param>
        private void LeftTouchPadUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {

            //Check if Teleport TouchPad position was touched
            if (TouchPadPosition.GetLastAxis(fromSource).y > 0 && TouchPadPosition.GetLastAxis(fromSource).x > leftArraowActivationThreshold && TouchPadPosition.GetLastAxis(fromSource).x < rightArraowActivationThreshold)
            {
                OnTeleportUp.Invoke(HANDSIDE.HAND_LEFT);
            }

            //Leftside of TouchPad was clicked
            if (TouchPadPosition.GetLastAxis(fromSource).x < leftArraowActivationThreshold)
            {
                OnLeftArrowClick.Invoke(HANDSIDE.HAND_LEFT);
            }

            //Rightside of the TouchPad was clicked
            else if(TouchPadPosition.GetLastAxis(fromSource).x > rightArraowActivationThreshold)
            {
                OnRightArrowClick.Invoke(HANDSIDE.HAND_LEFT);
            }

            //Stop the pulling of marker
            else if(startedPulling == HANDSIDE.HAND_LEFT)
            {
                OnEndMarkerPull.Invoke(HANDSIDE.HAND_LEFT);
                startedPulling = HANDSIDE.HAND_NONE;
                CurrentMenuState = MenuState.BOTH_CLOSED;
            }
        }

        /// <summary>
        /// Callback for when right TouchPad is up
        /// </summary>
        /// <param name="fromAction"></param>
        /// <param name="fromSource"></param>
        private void RightTouchPadUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            //Check if Teleport TouchPad position was touched
            if (TouchPadPosition.GetLastAxis(fromSource).y > 0 && TouchPadPosition.GetLastAxis(fromSource).x > leftArraowActivationThreshold && TouchPadPosition.GetLastAxis(fromSource).x < rightArraowActivationThreshold)
            {
                OnTeleportUp.Invoke(HANDSIDE.HAND_RIGHT);
            }

            //Leftside of TouchPad was clicked
            if (TouchPadPosition.GetLastAxis(fromSource).x < leftArraowActivationThreshold)
            {
                OnLeftArrowClick.Invoke(HANDSIDE.HAND_RIGHT);
            }

            //Rightside of the TouchPad was clicked
            else if (TouchPadPosition.GetLastAxis(fromSource).x > rightArraowActivationThreshold)
            {
                OnRightArrowClick.Invoke(HANDSIDE.HAND_RIGHT);
            }

            //Stop the pulling of marker
            else if (startedPulling == HANDSIDE.HAND_RIGHT)
            {
                OnEndMarkerPull.Invoke(HANDSIDE.HAND_RIGHT);
                startedPulling = HANDSIDE.HAND_NONE;
                CurrentMenuState = MenuState.BOTH_CLOSED;
            }
            
        }

        /// <summary>
        /// Callback when the GripButton is up
        /// </summary>
        /// <param name="fromAction"></param>
        /// <param name="fromSource"></param>
        private void PushGripButtonLeft(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            //Mark the currently hovered Block
            if(leftHand.hoveringInteractable != null)
            {
                OnMarkBlock.Invoke(leftHand.hoveringInteractable);
            }
        }

        /// <summary>
        /// Callback when the GripButton is up
        /// </summary>
        /// <param name="fromAction"></param>
        /// <param name="fromSource"></param>
        private void PushGripButtonRight(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            //Mark the currently hovered Block
            if (rightHand.hoveringInteractable != null)
            {
                OnMarkBlock.Invoke(rightHand.hoveringInteractable);
            }
        }

        /// <summary>
        /// Reads the pose of the Controller three times a second and checks if the Menu should be shown
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReadPose()
        {
            for (; ; )
            {
                switch (CurrentMenuState)
                {
                    case MenuState.BOTH_CLOSED:

                        if (ShouldShowMenu(LeftHandInput, 345, 10, 265, 295))
                        {
                            OnPoseOpenMenuLeft.Invoke(HANDSIDE.HAND_LEFT);
                            CurrentMenuState = MenuState.LEFT_OPEN;
                        }

                        else if (ShouldShowMenu(RightHandInput, 345, 10, 60, 95))
                        {
                            OnPoseOpenMenuRight.Invoke(HANDSIDE.HAND_RIGHT);
                            CurrentMenuState = MenuState.RIGHT_OPEN;
                        }
                        break;

                    case MenuState.LEFT_OPEN:

                        if (ShouldCloseMenu(LeftHandInput, 15, 340, 310, 260))
                        {
                            OnPoseCloseMenuLeft.Invoke(HANDSIDE.HAND_LEFT);
                            CurrentMenuState = MenuState.BOTH_CLOSED;
                        }
                        break;

                    case MenuState.RIGHT_OPEN:
                        if (ShouldCloseMenu(RightHandInput, 15, 340, 100, 60))
                        {
                            OnPoseCloseMenuRight.Invoke(HANDSIDE.HAND_RIGHT);
                            CurrentMenuState = MenuState.BOTH_CLOSED;
                        }
                        break;
                }
                
                yield return new WaitForSeconds(0.3f);
            }
        }

        /// <summary>
        /// Should the Menu on the given side open. Pose must been between cetain values for a certain amount of time for the menu to open.
        /// Should the minimum value be bigger than the maximum, the spectrum throu the zero degree point is used.
        /// </summary>
        /// <param name="hand">The hand to check</param>
        /// <param name="minX">The minimum X rotation the controller must be in for the menu to open</param>
        /// <param name="maxX">The maximum X rotation the controller must be in for the menu to open</param>
        /// <param name="minZ">The minimum Z rotation the controller must be in for the menu to open</param>
        /// <param name="maxZ">The maximum Z rotation the controller must be in for the menu to open</param>
        /// <returns>True is the menu should open</returns>
        private bool ShouldShowMenu(SteamVR_Input_Sources hand, float minX, float maxX, float minZ, float maxZ)
        {
            for (int i = 0; i < 5; i++)
            {

                Pose.GetPoseAtTimeOffset(hand, (float)i / 5, out Vector3 position, out Quaternion rotation, out Vector3 velocity, out Vector3 angularVelocity);

                if (!IsBetween(minX, maxX, rotation.eulerAngles.x) || !IsBetween(minZ, maxZ, rotation.eulerAngles.z))
                {
                    return false;
                }
            }

            if(rightHand.currentAttachedObject != null || rightHand.hoveringInteractable != null || leftHand.currentAttachedObject != null || leftHand.hoveringInteractable != null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Should the Menu on the given side close. Pose must been between cetain values for a certain amount of time for the menu to open.
        /// Should the minimum value be bigger than the maximum, the spectrum throu the zero degree point is used.
        /// </summary>
        /// <param name="hand">The hand to check</param>
        /// <param name="minX">The minimum X rotation the controller must be in for the menu to open</param>
        /// <param name="maxX">The maximum X rotation the controller must be in for the menu to open</param>
        /// <param name="minZ">The minimum Z rotation the controller must be in for the menu to open</param>
        /// <param name="maxZ">The maximum Z rotation the controller must be in for the menu to open</param>
        /// <returns>True is the menu should close</returns>
        private bool ShouldCloseMenu(SteamVR_Input_Sources hand, float minX, float maxX, float minZ, float maxZ)
        {
            for (int i = 0; i < 5; i++)
            {

                Pose.GetPoseAtTimeOffset(hand, (float)i / 5, out Vector3 position, out Quaternion rotation, out Vector3 velocity, out Vector3 angularVelocity);

                if (!IsBetween(minX, maxX, rotation.eulerAngles.x) || !IsBetween(minZ, maxZ, rotation.eulerAngles.z))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Helper method to determen if a angle is between to values
        /// </summary>
        /// <param name="min">Lower limit</param>
        /// <param name="max">Upper limit</param>
        /// <param name="number">The angle to check</param>
        /// <returns>True if the value is between the tow limits</returns>
        private bool IsBetween(float min, float max, float number)
        {
            if (min > max)
            {
                bool result = IsBetween(min, 360, number) || IsBetween(0, max, number);
                return result;
            }


            if (number > min && number < max)
            {
                return true;
            }
            return false;
        }

    }

    public enum MenuState
    {
        BOTH_CLOSED,
        LEFT_OPEN,
        LEFT_CLOSED,
        RIGHT_OPEN,
        RIGHT_CLOSED,
        DONT_OPEN
    }
}
