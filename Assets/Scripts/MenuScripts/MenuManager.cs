using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    public class MenuManager : MonoBehaviour
    {

        public SteamVR_Action_Pose Pose;

        public SteamVR_Input_Sources RightHandInput;
        public SteamVR_Input_Sources LeftHandInput;

        public SteamVR_Input_Sources handInput;
        public SteamVR_Action_Vector2 TouchPadPosition;
        public SteamVR_Action_Boolean TouchPadButton;

        private HANDSIDE startedPulling = HANDSIDE.HAND_NONE;
        private Vector3 startPullPosition;


        [System.Serializable]
        public class MenuEvent: UnityEvent<HANDSIDE> { }

        [SerializeField]
        public MenuEvent OnPoseOpenMenuLeft = new MenuEvent();

        [SerializeField]
        public MenuEvent OnPoseOpenMenuRight = new MenuEvent();

        [SerializeField]
        public MenuEvent OnPoseCloseMenuLeft = new MenuEvent();

        [SerializeField]
        public MenuEvent OnPoseCloseMenuRight = new MenuEvent();

        [SerializeField]
        public MenuEvent OnStartMarkerPull = new MenuEvent();

        [SerializeField]
        public MenuEvent OnEndMarkerPull = new MenuEvent();

        [SerializeField]
        public MenuEvent OnMarkerPulling = new MenuEvent();

        private MenuState CurrentMenuState = MenuState.BOTH_CLOSED;
        

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(ReadPose());

            TouchPadButton.AddOnStateDownListener(LeftPressMarkerDown, LeftHandInput);
            TouchPadButton.AddOnStateUpListener(LeftPressMarkerUp, LeftHandInput);

            TouchPadButton.AddOnStateDownListener(RightPressMarkerDown, RightHandInput);
            TouchPadButton.AddOnStateUpListener(RightPressMarkerUp, RightHandInput);
        }

        // Update is called once per frame
        void Update()
        {
            
            if (startedPulling != HANDSIDE.HAND_NONE)
            {
                OnMarkerPulling.Invoke(startedPulling);
            }
        }

        private void LeftPressMarkerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            if(CurrentMenuState == MenuState.BOTH_CLOSED)
            {
                OnStartMarkerPull.Invoke(HANDSIDE.HAND_LEFT);
                startedPulling = HANDSIDE.HAND_LEFT;
                CurrentMenuState = MenuState.DONT_OPEN;
            }
            
        }

        private void RightPressMarkerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            if (CurrentMenuState == MenuState.BOTH_CLOSED)
            {
                OnStartMarkerPull.Invoke(HANDSIDE.HAND_RIGHT);
                startedPulling = HANDSIDE.HAND_RIGHT;
                CurrentMenuState = MenuState.DONT_OPEN;
            }

        }

        private void LeftPressMarkerUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            OnEndMarkerPull.Invoke(HANDSIDE.HAND_LEFT);
            startedPulling = HANDSIDE.HAND_NONE;
            CurrentMenuState = MenuState.BOTH_CLOSED;
        }

        private void RightPressMarkerUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            OnEndMarkerPull.Invoke(HANDSIDE.HAND_RIGHT);
            startedPulling = HANDSIDE.HAND_NONE;
            CurrentMenuState = MenuState.BOTH_CLOSED;
        }



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
            return true;
        }


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
