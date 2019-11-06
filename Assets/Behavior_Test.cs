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

        }

        // Update is called once per frame
        void Update()
        {
            Vector3 velocity;
            Vector3 angualrVelocity;

            pose.GetEstimatedPeakVelocities(out velocity, out angualrVelocity);

            Debug.Log(velocity.ToString("F5"));
            Debug.Log(angualrVelocity.ToString("F5"));
        }
    }
}

