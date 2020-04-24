using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class BlockGeometryFloor: BlockGeometryScript
    {

        

        public BlockGeometryFloor()
        {
        }

        protected new void Awake()
        {
            this.mesh = GetComponent<MeshFilter>().mesh;
            //Create the TapContainer and adds the TapHandler to it
            GameObject taps = new GameObject("Taps");
            taps.tag = "Tap";
            taps.AddComponent<TapHandler>();
            taps.transform.SetParent(this.transform);
            taps.transform.localPosition = new Vector3(0f, 0f, 0f);

            //Create the GrooveContainer and adds the GrooveHandler to it
            GameObject grooves = new GameObject("Grooves");
            grooves.tag = "Groove";
            grooves.AddComponent<GrooveHandler>();
            grooves.transform.SetParent(this.transform);
            grooves.transform.localPosition = new Vector3(0f, 0f, 0f);
            TapContainer = taps;
            GroovesContainer = grooves;
        }

        protected new void Start()
        {
            AddWallCollider();
            AddPinTriggerCollider(0, TapContainer, "Tap");
            AddPinTriggerCollider(-(BRICK_HEIGHT_FLAT), GroovesContainer, "Groove");
            SetWallColliderTrigger(false);
        }

        /// <summary>
        /// Tries to calculates the walls of the block, only works for rectangle blocks. For
        /// non-rectangle blocks a BlockStructure is needed
        /// </summary>
        private void AddWallCollider()
        {
            Vector3 size = mesh.bounds.size;
            Vector3 center = mesh.bounds.center;
            Vector3 extends = mesh.bounds.extents;
            //Collider Long Side
            Vector3 longSideSize = new Vector3(size.x, BRICK_HEIGHT, BRICK_WALL_WIDTH);
            Vector3 longSideCenterLeft = new Vector3(center.x, center.y - BRICK_PIN_HEIGHT_HALF, center.z + extends.z - BRICK_WALL_WIDTH_HALF);
            Vector3 longSideCenterRight = new Vector3(center.x, center.y - BRICK_PIN_HEIGHT_HALF, center.z - extends.z + BRICK_WALL_WIDTH_HALF);
            wallColliderList.Add(AddBoxCollider(longSideSize, longSideCenterLeft, false, this.gameObject));
            wallColliderList.Add(AddBoxCollider(longSideSize, longSideCenterRight, false, this.gameObject));


            //Collider Short Side
            Vector3 shortSide = new Vector3(BRICK_WALL_WIDTH, BRICK_HEIGHT, size.z);
            Vector3 shortSideCenterUp = new Vector3(center.x + extends.x - BRICK_WALL_WIDTH_HALF, center.y - BRICK_PIN_HEIGHT_HALF, center.z);
            Vector3 shortSideCenterDown = new Vector3(center.x - extends.x + BRICK_WALL_WIDTH_HALF, center.y - BRICK_PIN_HEIGHT_HALF, center.z);
            wallColliderList.Add(AddBoxCollider(shortSide, shortSideCenterUp, false, this.gameObject));
            wallColliderList.Add(AddBoxCollider(shortSide, shortSideCenterDown, false, this.gameObject));

            //Collider Top Side
            Vector3 topSideSize = new Vector3(size.x, BRICK_WALL_WIDTH, size.z);
            Vector3 topSideCenter = GetCenterTop();
            topSideCenter.y = topSideCenter.y - (BRICK_WALL_WIDTH / 2);
            wallColliderList.Add(AddBoxCollider(topSideSize, topSideCenter, false, this.gameObject));
        }

        /// <summary>
        /// Adds the Collider to the Tap and Groove Container, only for use if no BlockStructure provided
        /// </summary>
        /// <param name="heightOffset">If no offset is provided, the collider are inline with bottom edge of the block</param>
        /// <param name="containerObject">The GameObject which the ColliderContainer GameObject is added to</param>
        /// <param name="tag">The Tag that will be added to the ColliderContainer</param>
        private void AddPinTriggerCollider(float heightOffset, GameObject containerObject, String tag)
        {
            Vector3 center = mesh.bounds.center;
            Vector3 extends = mesh.bounds.extents;
            Vector3 blockCorner = new Vector3(center.x + extends.x, center.y + extends.y - BRICK_PIN_HEIGHT, center.z + extends.z);
            Vector3 firstPinCenterPoint = new Vector3(blockCorner.x - (BRICK_PIN_DISTANCE / 2), blockCorner.y + heightOffset, blockCorner.z - (BRICK_PIN_DISTANCE / 2));

            Vector3 currentPinCenterPoint = firstPinCenterPoint;

            while (mesh.bounds.Contains(currentPinCenterPoint))
            {

                while (mesh.bounds.Contains(currentPinCenterPoint))
                {
                    AddGameObjectCollider(currentPinCenterPoint, tag, containerObject, true);
                    currentPinCenterPoint.z = currentPinCenterPoint.z - BRICK_PIN_DISTANCE;
                }
                currentPinCenterPoint.x = currentPinCenterPoint.x - BRICK_PIN_DISTANCE;
                currentPinCenterPoint.z = firstPinCenterPoint.z;

            }
        }
    }
}

