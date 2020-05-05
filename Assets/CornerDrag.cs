using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class CornerDrag : MonoBehaviour
    {
        public BoxDrawer BoxDrawer;

        public List<GameObject> xAxisPlane = new List<GameObject>();
        public List<GameObject> yAxisPlane = new List<GameObject>();
        public List<GameObject> zAxisPlane = new List<GameObject>();

        private bool ShouldUpdateBox = false;
        void Start()
        {
            BoxDrawer = GetComponentInParent<BoxDrawer>();
        }

        // Update is called once per frame
        void Update()
        {
            if (ShouldUpdateBox)
            {
                BoxDrawer.EditCube(this);
            }
        }

        public void OnAttachedToHand(Hand hand)
        {
            ShouldUpdateBox = true;
        }

        public void OnDetachedFromHand(Hand hand)
        {
            ShouldUpdateBox = false;
        }

        public void SetDependingSides(List<GameObject> x, List<GameObject> y, List<GameObject> z)
        {
            xAxisPlane = x;
            yAxisPlane = y;
            zAxisPlane = z;
        }

        public void UpdateCorners()
        {
            foreach(GameObject cornerGameObject in xAxisPlane)
            {
                Vector3 newPosition = cornerGameObject.transform.position;
                newPosition.x = gameObject.transform.position.x;
                cornerGameObject.transform.position = newPosition;
            }

            foreach (GameObject cornerGameObject in yAxisPlane)
            {
                Vector3 newPosition = cornerGameObject.transform.position;
                newPosition.y = gameObject.transform.position.y;
                cornerGameObject.transform.position = newPosition;
            }

            foreach (GameObject cornerGameObject in zAxisPlane)
            {
                Vector3 newPosition = cornerGameObject.transform.position;
                newPosition.z = gameObject.transform.position.z;
                cornerGameObject.transform.position = newPosition;
            }
        }
    }
}