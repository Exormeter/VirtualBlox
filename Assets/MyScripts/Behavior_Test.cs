using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class Behavior_Test : MonoBehaviour
    {

        public SteamVR_Behaviour_Pose pose;
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(ReadController());
        }

        private IEnumerator ReadController()
        {
            for(; ; )
            {
                Vector3 velocity;
                Vector3 angualrVelocity;

                pose.GetEstimatedPeakVelocities(out velocity, out angualrVelocity);

                if(pose.historyBuffer.GetAtIndex(0) != null)
                {
                    Vector3 rotation = pose.historyBuffer.GetAtIndex(0).rotation.eulerAngles;
                    Debug.Log(rotation.ToString("F5"));
                }
                
                yield return new WaitForSeconds(1);
            }
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}

