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


        public SteamVR_Action_Boolean headsetOnHead = SteamVR_Input.GetBooleanAction("HeadsetOnHead");
        public float playherHeight = 0;
        private float[] lastPositions = new float[60];
        private int lastPositionIndex = 0;
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
            UpdateElivationIndikator();
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

        private void AddHeightToRingBuffer(float height)
        {
            
            lastPositions[lastPositionIndex] = height;
            lastPositionIndex++;
            if (lastPositionIndex >= lastPositions.Length)
            {
                lastPositionIndex = 0;
            }
        }

        private float GetMostCommonHeight()
        {
            Dictionary<float, int> heightCounters = new Dictionary<float, int>();
            foreach (float height in lastPositions)
            {
                if (heightCounters.ContainsKey(height))
                {
                    heightCounters[height]++;
                }
                else
                {
                    heightCounters.Add(height, 1);
                }
            }

            int heightCount = 0;
            float mostCommonHeight = -1;

            foreach (KeyValuePair<float, int> pair in heightCounters)
            {
                if (heightCount < pair.Value)
                {
                    mostCommonHeight = pair.Key;
                    heightCount = pair.Value;
                }
            }
            return mostCommonHeight;

        }

        private void UpdateElivationIndikator()
        {
            if(GetCurrentPlayerHeight() < playherHeight - 0.4f)
            {
                return;
            }

            //Only RaycastLayer
            RaycastHit blockHit;

            Vector3 bodyDirection = Player.bodyDirectionGuess;
            Vector3 bodyDirectionTangent = Vector3.Cross(Player.trackingOriginTransform.up, bodyDirection);
            Vector3 startForward = Player.feetPositionGuess + Player.trackingOriginTransform.up * Player.eyeHeight * 0.75f;
            Vector3 endForward = startForward + bodyDirection * -0.33f;

            if (Physics.Raycast(endForward, transform.TransformDirection(Vector3.down), out blockHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("RaycastOnly")))
            {

                endForward.y = blockHit.point.y + 0.001f;
                ElevationIndicator.transform.position = endForward;
                //ElevationIndicator.transform.Translate(Vector3.up * blockHit.point.y, Space.World);
                AddHeightToRingBuffer(blockHit.collider.transform.position.y);
            }
            else
            {
                AddHeightToRingBuffer(0);
            }
            SetPlaneHeight(GetMostCommonHeight());
            
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

            Vector3 bodyDirection = Player.bodyDirectionGuess;
            Vector3 bodyDirectionTangent = Vector3.Cross(Player.trackingOriginTransform.up, bodyDirection);
            Vector3 startForward = Player.feetPositionGuess + Player.trackingOriginTransform.up * Player.eyeHeight * 0.75f;
            Vector3 endForward = startForward + bodyDirection * -0.33f;

            Physics.Raycast(endForward, transform.TransformDirection(Vector3.down), out blockHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("RaycastOnly"));
            for(int index = 0; index < lastPositions.Length; index++)
            {
                lastPositions[index] = blockHit.point.y;
            }
        }
    }
}

