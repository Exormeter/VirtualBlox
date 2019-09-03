using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    [RequireComponent(typeof(Interactable))]
    public class BlockScript : MonoBehaviour
    {
        private float BRICK_HEIGHT;
        private const float BRICK_PIN_HEIGHT = 0.016f;
        private const float BRICK_PIN_HEIGHT_HALF = BRICK_PIN_HEIGHT / 2;
        private const float BRICK_WALL_WIDTH = 0.01f;
        private const float BRICK_WALL_WIDTH_HALF = BRICK_WALL_WIDTH / 2;
        private const float BRICK_PIN_DISTANCE = 0.08f;
        private const float BRICK_PIN_DIAMETER = 0.048f;
        private GameObject grooves;

        private Mesh mesh;

        void Start()
        {
            this.mesh = GetComponent<MeshFilter>().mesh;

            if (mesh.bounds.extents.y < 0.047)
            {
                BRICK_HEIGHT = 0.032f;
            }
            else
            {
                BRICK_HEIGHT = 0.096f;
            }

            AddWallCollider();

            GameObject grooves = new GameObject("Grooves");
            this.grooves = grooves;
            GameObject taps = new GameObject("Taps");
            grooves.AddComponent<GrooveHandler>();
            grooves.transform.SetParent(this.transform);
            taps.transform.SetParent(this.transform);
            grooves.transform.localPosition = new Vector3(0f, 0f, 0f);
            taps.transform.localPosition = new Vector3(0f, 0f, 0f);
            AddPinTriggerCollider(BRICK_PIN_HEIGHT, taps, "Tap");
            AddPinTriggerCollider(-(BRICK_HEIGHT / 1.3f), grooves, "Groove");
        }


        // Update is called once per frame
        void Update()
        {

        }


        private void AddWallCollider()
        {
            Vector3 size = mesh.bounds.size;
            Vector3 center = mesh.bounds.center;
            Vector3 extends = mesh.bounds.extents;
            //Collider Long Side
            Vector3 longSideSize = new Vector3(size.x, BRICK_HEIGHT, BRICK_WALL_WIDTH);
            Vector3 longSideCenterLeft = new Vector3(center.x, center.y - BRICK_PIN_HEIGHT_HALF, center.z + extends.z - BRICK_WALL_WIDTH_HALF);
            Vector3 longSideCenterRight = new Vector3(center.x, center.y - BRICK_PIN_HEIGHT_HALF, center.z - extends.z + BRICK_WALL_WIDTH_HALF);
            AddBoxCollider(longSideSize, longSideCenterLeft, false, this.gameObject);
            AddBoxCollider(longSideSize, longSideCenterRight, false, this.gameObject);


            //Collider Short Side
            Vector3 shortSide = new Vector3(BRICK_WALL_WIDTH, BRICK_HEIGHT, size.z);
            Vector3 shortSideCenterUp = new Vector3(center.x + extends.x - BRICK_WALL_WIDTH_HALF, center.y - BRICK_PIN_HEIGHT_HALF, center.z);
            Vector3 shortSideCenterDown = new Vector3(center.x - extends.x + BRICK_WALL_WIDTH_HALF, center.y - BRICK_PIN_HEIGHT_HALF, center.z);
            AddBoxCollider(shortSide, shortSideCenterUp, false, this.gameObject);
            AddBoxCollider(shortSide, shortSideCenterDown, false, this.gameObject);

            //Collider Top Side
            Vector3 topSideSize = new Vector3(size.x, BRICK_WALL_WIDTH, size.z);
            Vector3 topSideCenter = GetCenterTop();
            topSideCenter.y = topSideCenter.y - (BRICK_WALL_WIDTH / 2);
            AddBoxCollider(topSideSize, topSideCenter, false, this.gameObject);
        }

        private void AddPinTriggerCollider(float heightOffset, GameObject containerObject, String tag)
        {
            Vector3 center = mesh.bounds.center;
            Vector3 extends = mesh.bounds.extents;
            Vector3 blockCorner = new Vector3(center.x + extends.x, center.y + extends.y - BRICK_PIN_HEIGHT, center.z + extends.z);
            Vector3 firstPinCenterPoint = new Vector3(blockCorner.x - (BRICK_PIN_DISTANCE / 2), blockCorner.y + heightOffset, blockCorner.z - (BRICK_PIN_DISTANCE / 2));
            //AddBoxCollider(new Vector3(0.01f, 0.01f, 0.01f), blockCorner, false, this.gameObject);

            Vector3 currentPinCenterPoint = firstPinCenterPoint;

            while (mesh.bounds.Contains(currentPinCenterPoint))
            {

                while (mesh.bounds.Contains(currentPinCenterPoint))
                {
                    AddGameObjectCollider(currentPinCenterPoint, tag, containerObject);
                    currentPinCenterPoint.z = currentPinCenterPoint.z - BRICK_PIN_DISTANCE;
                }
                currentPinCenterPoint.x = currentPinCenterPoint.x - BRICK_PIN_DISTANCE;
                currentPinCenterPoint.z = firstPinCenterPoint.z;

            }
        }

        private void AddGameObjectCollider(Vector3 position, String tag, GameObject container)
        {
            GameObject colliderObject = new GameObject("Collider");
            colliderObject.tag = tag;
            colliderObject.transform.SetParent(container.transform);
            colliderObject.transform.localPosition = new Vector3(0f, 0f, 0f);
            if (tag.Equals("Groove"))
            {
                colliderObject.AddComponent<SnappingCollider>();
            }
            AddBoxCollider(new Vector3(BRICK_PIN_DIAMETER / 2, BRICK_PIN_HEIGHT, BRICK_PIN_DIAMETER / 2), position, true, colliderObject);
        }

        private Collider AddBoxCollider(Vector3 size, Vector3 center, bool isTrigger, GameObject otherGameObject)
        {
            BoxCollider collider = otherGameObject.AddComponent<BoxCollider>();
            collider.size = size;
            collider.center = center;
            collider.isTrigger = isTrigger;
            return collider;
        }

        public Vector3 GetCenterTop()
        {
            Vector3 center = mesh.bounds.center;
            Vector3 extends = mesh.bounds.extents;
            center.y = center.y + extends.y - BRICK_PIN_HEIGHT;
            return center;

        }

        public Vector3 GetCenterBottom()
        {
            Vector3 center = mesh.bounds.center;
            Vector3 extends = mesh.bounds.extents;
            center.y = center.y - extends.y;
            return center;
        }

        public GameObject getRootGameObject()
        {
            return gameObject;
        }

        private void OnAttachedToHand(Hand hand)
        {
            this.grooves.GetComponent<GrooveHandler>().blockWasAttachedToHand(hand);
        }



        //-------------------------------------------------
        // Called when this GameObject is detached from the hand
        //-------------------------------------------------
        private void OnDetachedFromHand(Hand hand)
        {
            this.grooves.GetComponent<GrooveHandler>().blockWasDetachedFromHand(hand);
        }
    }
}