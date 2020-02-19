using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem
{
    public class CheckPlayerHeightLevel : MonoBehaviour
    {
        public GameObject ElevationIndicator;
        public GameObject WalkablePlatform;
        public Player Player;
        //public Text DebugText;
        public Vector3 boxCastDimensions;
        

        public SteamVR_Action_Boolean headsetOnHead = SteamVR_Input.GetBooleanAction("HeadsetOnHead");
        private Plane currentPlane;
        private Coroutine MovePlatformCoroutine;
        private Vector3 neutralPosition;
        private bool platformIsCurrentlyMoving = false;
        
        // Start is called before the first frame update
        void Start()
        {
            neutralPosition = WalkablePlatform.transform.position;
            currentPlane = new Plane(new Vector3(0, 1, 0), gameObject.transform.position);
        }

        // Update is called once per frame
        void Update()
        {
            UpdateCurrentPlane();
            UpdateElivationIndikator();
        }



        private void UpdateElivationIndikator()
        {
            Vector3 feetPosition = ApproximatelyFeetPositionXZ();
            feetPosition.y = GetPlaneHeightFromZero() + 0.001f;
            ElevationIndicator.transform.position = feetPosition;
            ElevationIndicator.transform.rotation = Quaternion.Euler(new Vector3(0,Player.hmdTransform.rotation.eulerAngles.y + 180,0));
        }

        private void UpdateCurrentPlane()
        {
            float newPlaneHeight = 0;
            Vector3 feetPosition = ApproximatelyFeetPositionXZ();
            RaycastHit blockHit;
            int layermaskBlockWalk = 1 << LayerMask.NameToLayer("WalkableBlock");
            int layermaskPlatfromWalk = 1 << LayerMask.NameToLayer("WalkablePlatform");

            if (Physics.BoxCast(feetPosition, boxCastDimensions, transform.TransformDirection(Vector3.down), out blockHit, Quaternion.identity, Mathf.Infinity, layermaskBlockWalk | layermaskPlatfromWalk))
            {
                newPlaneHeight = blockHit.point.y;
            }

            if(newPlaneHeight > (GetPlaneHeightFromZero() + BlockGeometryScript.BRICK_HEIGHT_NORMAL + BlockGeometryScript.BRICK_HEIGHT_FLAT))
            {
                return;
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
            Vector3 playerHeightPosition = gameObject.transform.position;
            Vector3 walkablePlatformPosition = WalkablePlatform.transform.position;

            walkablePlatformPosition.y = height;
            playerHeightPosition.y = height;

            gameObject.transform.position = playerHeightPosition;

            if (!platformIsCurrentlyMoving)
            {
                WalkablePlatform.transform.position = walkablePlatformPosition;
            }
            
            currentPlane = new Plane(new Vector3(0, 1, 0), gameObject.transform.position);
        }

        public void OnTeleport()
        {
            RaycastHit blockHit;

            Vector3 feetPosition = ApproximatelyFeetPositionXZ();

            if(Physics.Raycast(feetPosition, transform.TransformDirection(Vector3.down), out blockHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("WalkableBlock")))
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

        public void RaiseWalkablePlatform()
        {
            if (MovePlatformCoroutine != null)
            {
                StopCoroutine(MovePlatformCoroutine);
            }
            MovePlatformCoroutine = StartCoroutine(MovePlatform(0.5f));
            platformIsCurrentlyMoving = true;
        }

        public void LowerWalkablePlatform()
        {
            if (MovePlatformCoroutine != null)
            {
                StopCoroutine(MovePlatformCoroutine);
            }
            MovePlatformCoroutine = StartCoroutine(MovePlatform(-0.5f));
            platformIsCurrentlyMoving = true;
        }

        public void StopMovingPlatform()
        {
            if(MovePlatformCoroutine != null)
            {
                StopCoroutine(MovePlatformCoroutine);
            }
            platformIsCurrentlyMoving = false;
        }

        public IEnumerator MovePlatform(float direction)
        {
            for(; ; )
            {
                WalkablePlatform.transform.Translate(neutralPosition + (Vector3.up * Time.deltaTime * direction));
                yield return new WaitForEndOfFrame();
            }
        }
    }
}

