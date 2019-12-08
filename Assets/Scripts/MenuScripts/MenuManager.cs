using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    public class MenuManager : MonoBehaviour
    {

        public SteamVR_Action_Pose Pose;

        public SteamVR_Input_Sources RightHandInput;
        public SteamVR_Input_Sources LeftHandInput;


        [System.Serializable]
        public class PoseEvent: UnityEvent<HANDSIDE> { }

        [SerializeField]
        public PoseEvent OnPoseOpenMenuLeft = new PoseEvent();

        [SerializeField]
        public PoseEvent OnPoseOpenMenuRight = new PoseEvent();

        [SerializeField]
        public PoseEvent OnPoseCloseMenuLeft = new PoseEvent();

        [SerializeField]
        public PoseEvent OnPoseCloseMenuRight = new PoseEvent();

        private MenuState currentMenuState = MenuState.BOTH_CLOSED;
        

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(ReadPose());
        }

        // Update is called once per frame
        void Update()
        {

        }

        

        private IEnumerator ReadPose()
        {
            for (; ; )
            {


                switch (currentMenuState)
                {
                    case MenuState.BOTH_CLOSED:

                        if (ShouldShowMenu(LeftHandInput, 345, 10, 265, 295))
                        {
                            OnPoseOpenMenuLeft.Invoke(HANDSIDE.HAND_LEFT);
                            currentMenuState = MenuState.LEFT_OPEN;
                        }

                        else if (ShouldShowMenu(RightHandInput, 345, 10, 60, 95))
                        {
                            OnPoseOpenMenuRight.Invoke(HANDSIDE.HAND_RIGHT);
                            currentMenuState = MenuState.RIGHT_OPEN;
                        }
                        break;

                    case MenuState.LEFT_OPEN:

                        if (ShouldCloseMenu(LeftHandInput, 15, 340, 310, 260))
                        {
                            OnPoseCloseMenuLeft.Invoke(HANDSIDE.HAND_LEFT);
                            currentMenuState = MenuState.BOTH_CLOSED;
                        }
                        break;

                    case MenuState.RIGHT_OPEN:
                        if (ShouldCloseMenu(RightHandInput, 15, 340, 100, 60))
                        {
                            OnPoseCloseMenuRight.Invoke(HANDSIDE.HAND_RIGHT);
                            currentMenuState = MenuState.BOTH_CLOSED;
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
        RIGHT_CLOSED
    }
}
