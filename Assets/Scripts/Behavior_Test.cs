using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class Behavior_Test : MonoBehaviour
    {

        public SteamVR_Action_Pose Pose;
        public SteamVR_Action_Boolean taste;
        public SteamVR_Input_Sources RightHand;
        
        // Start is called before the first frame update
        void Start()
        {
            //StartCoroutine(ReadPose());
        }

        //private IEnumerator ReadController()
        //{
        //for(; ; )
        //{
        //    Vector3 velocity;
        //    Vector3 angualrVelocity;

        //    pose.GetEstimatedPeakVelocities(out velocity, out angualrVelocity);

        //    if(pose.historyBuffer.GetAtIndex(0) != null)
        //    {
        //        Vector3 rotation = pose.historyBuffer.GetAtIndex(0).rotation.eulerAngles;
        //        Debug.Log(rotation.ToString("F5"));
        //    }

        //    yield return new WaitForSeconds(1);
        //}
        //}

        //private IEnumerator ReadPose()
        //{
        //    for (; ; )
        //    {
        //        bool showGUI = true;
        //        List<Quaternion> poseOverTime = new List<Quaternion>();
        //        for(int i = 0; i < 5; i++)
        //        {
        //            Pose.GetPoseAtTimeOffset(LeftHand, (float)i/5, out Vector3 position, out Quaternion rotation, out Vector3 velocity, out Vector3 angularVelocity);
        //            if (rotation.eulerAngles.x > 5 || rotation.eulerAngles.x < -5 || rotation.eulerAngles.z > 85 || rotation.eulerAngles.z < 75)
        //            {
        //                showGUI = false;
        //                break;
        //            }
        //        }
        //        //Vector3 averageRotation = new Vector3();
        //        //poseOverTime.ForEach(pose => averageRotation += pose.eulerAngles);
        //        //averageRotation.x = averageRotation.x / 100;
        //        //averageRotation.y = averageRotation.y / 100;
        //        //averageRotation.z = averageRotation.z / 100;
                

        //        if (showGUI)
        //        {
        //            Debug.Log("GUI Show");
        //        }
                
        //        yield return new WaitForSeconds(0.3f);
        //    }
        //}

        // Update is called once per frame
        void Update()
        {
            if (taste.GetStateDown(RightHand))
            {
                Debug.Log("Gedrückt");
            }
        }
    }
}

