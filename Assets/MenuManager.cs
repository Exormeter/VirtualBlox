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
        public class PoseEvent: UnityEvent<SteamVR_Input_Sources> { }

        [SerializeField]
        public PoseEvent OnPoseOpenMenuLeft = new PoseEvent();

        [SerializeField]
        public PoseEvent OnPoseOpenMenuRight = new PoseEvent();

        [SerializeField]
        public PoseEvent OnPoseCloseMenuLeft = new PoseEvent();

        [SerializeField]
        public PoseEvent OnPoseCloseMenuRight = new PoseEvent();

        private bool menuOpen = false;
        

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
                

                if (ShouldShowMenu(LeftHandInput) && !menuOpen)
                {
                    OnPoseOpenMenuLeft.Invoke(LeftHandInput);
                }

                else if (ShouldShowMenu(RightHandInput) && !menuOpen)
                {
                    OnPoseOpenMenuRight.Invoke(RightHandInput);
                }

                else if(ShouldCloseMenu(LeftHandInput) && menuOpen)
                {
                    OnPoseCloseMenuLeft.Invoke(LeftHandInput);
                }

                else if (ShouldCloseMenu(RightHandInput) && menuOpen)
                {
                    OnPoseCloseMenuRight.Invoke(RightHandInput);
                }


                yield return new WaitForSeconds(0.3f);
            }
        }


        private bool ShouldShowMenu(SteamVR_Input_Sources hand)
        {
            for (int i = 0; i < 5; i++)
            {

                Pose.GetPoseAtTimeOffset(LeftHandInput, (float)i / 5, out Vector3 position, out Quaternion rotation, out Vector3 velocity, out Vector3 angularVelocity);

                if (!IsBetween(355, 5, rotation.eulerAngles.x) || !IsBetween(73, 85, rotation.eulerAngles.z))
                {
                    return false;
                }
            }
            return true;
        }

        private bool ShouldCloseMenu(SteamVR_Input_Sources hand)
        {
            for (int i = 0; i < 5; i++)
            {

                Pose.GetPoseAtTimeOffset(LeftHandInput, (float)i / 5, out Vector3 position, out Quaternion rotation, out Vector3 velocity, out Vector3 angularVelocity);

                if (!IsBetween(355, 5, rotation.eulerAngles.x) || !IsBetween(73, 85, rotation.eulerAngles.z))
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
}
