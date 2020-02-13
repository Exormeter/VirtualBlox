using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem
{
    public class CheckPlayerHeight : MonoBehaviour
    {
        //public Transform HMDTransform;
        public GameObject ElevationIndicator;
        public Player Player;
        public Text DebugText;
        public Vector3 boxCastDimensions;

        public SteamVR_Action_Boolean headsetOnHead = SteamVR_Input.GetBooleanAction("HeadsetOnHead");
        public float playherHeight = 0;
        private Coroutine heightCheck;
        private Plane currentPlane;
        
        // Start is called before the first frame update
        void Start()
        {
            currentPlane = new Plane(new Vector3(0, 1, 0), gameObject.transform.position);
        }

        // Update is called once per frame
        void Update()
        {
            DebugText.text = "Größe: " + playherHeight;
            if (!(GetCurrentPlayerHeight() < playherHeight - 0.4f))
            {
                UpdateCurrentPlane();
                UpdateElivationIndikator();
            }
            
            if (headsetOnHead != null && headsetOnHead.GetStateDown(SteamVR_Input_Sources.Head))
            {
                Invoke("StartCheckPlayerHeightCoroutine", 5);
            }

            else if (headsetOnHead != null && headsetOnHead.GetStateUp(SteamVR_Input_Sources.Head))
            {
                playherHeight = 0;
                if(heightCheck != null)
                {
                    StopCoroutine(heightCheck);
                }
                
            }
        }

        private void StartCheckPlayerHeightCoroutine()
        {
            heightCheck = StartCoroutine(CheckPlayerHeightCoroutine());
        }

        IEnumerator CheckPlayerHeightCoroutine()
        {
            for (; ; )
            {
                if (GetCurrentPlayerHeight() > playherHeight)
                {
                    playherHeight = GetCurrentPlayerHeight();
                }
                yield return new WaitForSeconds(0.5f);
            }

        }

        //private void AddHeightToRingBuffer(float height)
        //{
            
        //    lastPositions[lastPositionIndex] = height;
        //    lastPositionIndex++;
        //    if (lastPositionIndex >= lastPositions.Length)
        //    {
        //        lastPositionIndex = 0;
        //    }
        //}

        //private float GetMostCommonHeight()
        //{
        //    Dictionary<float, int> heightCounters = new Dictionary<float, int>();
        //    foreach (float height in lastPositions)
        //    {
        //        if (heightCounters.ContainsKey(height))
        //        {
        //            heightCounters[height]++;
        //        }
        //        else
        //        {
        //            heightCounters.Add(height, 1);
        //        }
        //    }

        //    int heightCount = 0;
        //    float mostCommonHeight = -1;

        //    foreach (KeyValuePair<float, int> pair in heightCounters)
        //    {
        //        if (heightCount < pair.Value)
        //        {
        //            mostCommonHeight = pair.Key;
        //            heightCount = pair.Value;
        //        }
        //    }
        //    return mostCommonHeight;

        //}

        private void UpdateElivationIndikator()
        {
            Vector3 feetPosition = ApproximatelyFeetPositionXZ();
            feetPosition.y = GetPlaneHeightFromZero() + 0.001f;
            ElevationIndicator.transform.position = feetPosition;
        }

        private void UpdateCurrentPlane()
        {
            float newPlaneHeight = 0;
            Vector3 feetPosition = ApproximatelyFeetPositionXZ();
            RaycastHit blockHit;
            if (Physics.BoxCast(feetPosition, boxCastDimensions, transform.TransformDirection(Vector3.down), out blockHit, Quaternion.identity, Mathf.Infinity, 1 << LayerMask.NameToLayer("RaycastOnly")))
            {
                newPlaneHeight = blockHit.point.y;
            }

            if(newPlaneHeight > (GetPlaneHeightFromZero() + BlockGeometryScript.BRICK_HEIGHT_NORMAL + BlockGeometryScript.BRICK_HEIGHT_FLAT))
            {
                return;
            }

            else if(newPlaneHeight < (GetPlaneHeightFromZero() - BlockGeometryScript.BRICK_HEIGHT_NORMAL - BlockGeometryScript.BRICK_HEIGHT_FLAT))
            {
                StartCoroutine(SetPlaneFalling(0.3f, newPlaneHeight));
            }

            else
            {
                SetPlaneHeight(newPlaneHeight);
            }
            
        }

        public float GetCurrentPlayerHeight()
        {
            return currentPlane.GetDistanceToPoint(Player.hmdTransform.position);
        }

        public void SetPlaneHeight(float height)
        {
            Vector3 newHeightPosition = gameObject.transform.position;
            newHeightPosition.y = height;
            gameObject.transform.position = newHeightPosition;
            currentPlane = new Plane(new Vector3(0, 1, 0), gameObject.transform.position);
        }

        public void OnTeleport()
        {
            RaycastHit blockHit;

            Vector3 feetPosition = ApproximatelyFeetPositionXZ();

            if(Physics.Raycast(feetPosition, transform.TransformDirection(Vector3.down), out blockHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("RaycastOnly")))
            {
                SetPlaneHeight(blockHit.point.y);
            }
            
        }

        public Vector3 ApproximatelyFeetPositionXZ()
        {
            Vector3 bodyDirection = Player.bodyDirectionGuess;
            Vector3 bodyDirectionTangent = Vector3.Cross(Player.trackingOriginTransform.up, bodyDirection);
            Vector3 startForward = Player.feetPositionGuess + Player.trackingOriginTransform.up * Player.eyeHeight * 0.75f;
            Vector3 feetPosition = startForward + bodyDirection * -0.33f;

            return feetPosition;
        }

        private float GetPlaneHeightFromZero()
        {
            return System.Math.Abs(currentPlane.GetDistanceToPoint(new Vector3(0, 0, 0)));
        }

        private IEnumerator SetPlaneFalling(float fadeTime, float newPlaneHeight)
        {
            SteamVR_Fade.Start(Color.black, fadeTime, true);

            yield return new WaitForSeconds(0.5f);
            SetPlaneHeight(newPlaneHeight);

            SteamVR_Fade.Start(Color.clear, fadeTime, true);
        }
    }
}

