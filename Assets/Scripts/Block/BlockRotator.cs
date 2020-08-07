using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{


    public class BlockRotator : MonoBehaviour
    {
        
        public void RotateBlock(List<CollisionObject> currentCollisionObjects, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            
            MatchTargetBlockRotation(currentCollisionObjects[0], connectedOn);
           
            MatchTargetBlockDistance(currentCollisionObjects[0], connectedOn);
            

            MatchTargetBlockOffset(currentCollisionObjects[0], connectedOn);

            if(currentCollisionObjects.Count >= 2)
            {
                MatchPinRotation(currentCollisionObjects[0], currentCollisionObjects[1], connectedOn);
            }
            
        }

        private void MatchTargetBlockRotation(CollisionObject collision, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            switch (connectedOn)
            {
                case OTHER_BLOCK_IS_CONNECTED_ON.GROOVE:
                    gameObject.transform.rotation = Quaternion.LookRotation(collision.TapPosition.transform.up, -transform.forward);
                    break;

                case OTHER_BLOCK_IS_CONNECTED_ON.TAP:
                    gameObject.transform.rotation = Quaternion.LookRotation(collision.GroovePosition.transform.up, -transform.forward);
                    break;
            }

            
            gameObject.transform.Rotate(Vector3.right, 90f);

            //Debug.Log("1 wenn beide Blöcke in die selbe Richtugn zeigen: " + Vector3.Dot(collision.CollidedBlock.transform.up, gameObject.transform.up));
        }

        
        private void MatchTargetBlockDistance(CollisionObject collision, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            Plane planeBlock = new Plane();
            float distance = 0;

            switch (connectedOn)
            {
                case OTHER_BLOCK_IS_CONNECTED_ON.GROOVE:
                    planeBlock = new Plane(transform.TransformDirection(collision.GroovePosition.transform.up), transform.TransformPoint(collision.GroovePosition.transform.position));
                    distance = planeBlock.GetDistanceToPoint(transform.TransformPoint(collision.TapPosition.transform.position));
                    break;

                case OTHER_BLOCK_IS_CONNECTED_ON.TAP:
                    planeBlock = new Plane(transform.TransformDirection(collision.TapPosition.transform.up), transform.TransformPoint(collision.TapPosition.transform.position));
                    distance = planeBlock.GetDistanceToPoint(transform.TransformPoint(collision.GroovePosition.transform.position));
                    break;
            }

            //Debug.Log("Distanz vor translate: " + distance);
            transform.Translate(Vector3.up * distance, Space.Self);

            switch (connectedOn)
            {
                case OTHER_BLOCK_IS_CONNECTED_ON.GROOVE:
                    planeBlock = new Plane(collision.GroovePosition.transform.up, collision.GroovePosition.transform.position);
                    distance = planeBlock.GetDistanceToPoint(collision.TapPosition.transform.position);
                    //Debug.Log("Neue Distanz: " + distance);
                    break;

                case OTHER_BLOCK_IS_CONNECTED_ON.TAP:
                    planeBlock = new Plane(collision.TapPosition.transform.up, collision.TapPosition.transform.position);
                    distance = planeBlock.GetDistanceToPoint(collision.GroovePosition.transform.position);
                    //Debug.Log("Neue Distanz: " + distance);
                    break;
            }
        }


        private void MatchTargetBlockOffset(CollisionObject collision, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            switch (connectedOn)
            {
                case OTHER_BLOCK_IS_CONNECTED_ON.GROOVE:
                    {
                        Vector3 tapColliderCenterLocal = transform.InverseTransformPoint(collision.TapPosition.transform.position);
                        Vector3 grooveColliderCenterLocal = transform.InverseTransformPoint(collision.GroovePosition.transform.position);
                        Vector3 centerOffset = tapColliderCenterLocal - grooveColliderCenterLocal;
                        //Debug.Log("Offset: " + centerOffset.ToString("F5"));
                        transform.Translate(Vector3.right * centerOffset.x, Space.Self);
                        transform.Translate(Vector3.forward * centerOffset.z, Space.Self);
                        break;
                    }

                case OTHER_BLOCK_IS_CONNECTED_ON.TAP:
                    {
                        Vector3 grooveColliderCenterLocal = transform.InverseTransformPoint(collision.GroovePosition.transform.position);
                        Vector3 tapColliderCenterLocal = transform.InverseTransformPoint(collision.TapPosition.transform.position);
                        Vector3 centerOffset = grooveColliderCenterLocal - tapColliderCenterLocal;
                        transform.Translate(Vector3.right * centerOffset.x, Space.Self);
                        transform.Translate(Vector3.forward * centerOffset.z, Space.Self);
                        break;
                    }
            }
            //Debug.Log("Offset: " + (collision.TapPosition.transform.position - collision.GroovePosition.transform.position).ToString("F8"));
        }

        private void MatchPinRotation(CollisionObject matchedPin, CollisionObject secoundPin, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            if(connectedOn == OTHER_BLOCK_IS_CONNECTED_ON.GROOVE)
            {
                float angleRotation = GetAngleBetweenPins(matchedPin, secoundPin, connectedOn);
                transform.RotateAround(matchedPin.TapPosition.transform.position, matchedPin.TapPosition.transform.up, angleRotation);

                float newAngleRotation = GetAngleBetweenPins(matchedPin, secoundPin, connectedOn);
                if (newAngleRotation > 0.00001)
                {
                    transform.RotateAround(matchedPin.TapPosition.transform.position, matchedPin.TapPosition.transform.up, -newAngleRotation);
                }
            }
            else
            {
                float angleRotation = GetAngleBetweenPins(matchedPin, secoundPin, connectedOn);
                transform.RotateAround(matchedPin.GroovePosition.transform.position, matchedPin.GroovePosition.transform.position, angleRotation);

                float newAngleRotation = GetAngleBetweenPins(matchedPin, secoundPin, connectedOn);
                if (newAngleRotation > 0.00001)
                {
                    transform.RotateAround(matchedPin.GroovePosition.transform.position, matchedPin.GroovePosition.transform.position, -newAngleRotation);
                }
            }

            //Debug.Log("Zweiter Pin Offset: " + (secoundPin.GroovePosition.transform.position - secoundPin.TapPosition.transform.position).ToString("F8"));
            
        }


        private float GetAngleBetweenPins(CollisionObject matchedPin, CollisionObject secoundPin, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            float angleRotation = 0;
            if (connectedOn == OTHER_BLOCK_IS_CONNECTED_ON.GROOVE)
            {
                Vector3 intersectionPointTap = transform.InverseTransformPoint(matchedPin.TapPosition.transform.position);
                Vector3 tapColliderCenter = transform.InverseTransformPoint(secoundPin.TapPosition.transform.position);
                Vector3 grooveColliderCenter = transform.InverseTransformPoint(secoundPin.GroovePosition.transform.position);

                Vector3 vectorIntersectToTap = intersectionPointTap - tapColliderCenter;
                Vector3 vectorIntersectionToGroove = intersectionPointTap - grooveColliderCenter;

                vectorIntersectToTap = Vector3.ProjectOnPlane(vectorIntersectToTap, Vector3.up);
                vectorIntersectionToGroove = Vector3.ProjectOnPlane(vectorIntersectionToGroove, Vector3.up);

                angleRotation = Vector3.Angle(vectorIntersectionToGroove, vectorIntersectToTap);
                //Debug.Log("Angle Rotation: " + angleRotation);
            }
            else
            {
                Vector3 intersectionPointGroove = transform.InverseTransformPoint(matchedPin.GroovePosition.transform.position);
                Vector3 groovePoint = transform.InverseTransformPoint(secoundPin.GroovePosition.transform.position);
                Vector3 tapPoint = transform.InverseTransformPoint(secoundPin.TapPosition.transform.position);

                Vector3 vectorIntersectToGroove = intersectionPointGroove - groovePoint;
                Vector3 vectorIntersectionToTap = intersectionPointGroove - tapPoint;

                vectorIntersectToGroove = Vector3.ProjectOnPlane(vectorIntersectToGroove, Vector3.up);
                vectorIntersectionToTap = Vector3.ProjectOnPlane(vectorIntersectionToTap, Vector3.up);

                angleRotation = Vector3.Angle(vectorIntersectionToTap, vectorIntersectToGroove);
                //Debug.Log("Angle Rotation: " + angleRotation);
            }

            return angleRotation;

        }

        // Mimics Debug.DrawLine, drawing a plane containing the 3 provided worldspace points,
        // with the visualization centered on the centroid of the triangle they form.
        public static void DrawPlane(Vector3 a, Vector3 b, Vector3 c, float size,
            Color color, float duration = 0f, bool depthTest = true)
        {

            var plane = new Plane(a, b, c);
            var centroid = (a + b + c) / 3f;

            DrawPlaneAtPoint(plane, centroid, size, color, duration, depthTest);
        }

        // Draws the portion of the plane closest to the provided point, 
        // with an altitude line colour-coding whether the point is in front (cyan)
        // or behind (red) the provided plane.
        public static void DrawPlaneNearPoint(Plane plane, Vector3 point, float size, Color color, float duration = 0f, bool depthTest = true)
        {
            var closest = plane.ClosestPointOnPlane(point);
            Color side = plane.GetSide(point) ? Color.cyan : Color.red;
            Debug.DrawLine(point, closest, side, duration, depthTest);

            DrawPlaneAtPoint(plane, closest, size, color, duration, depthTest);
        }

        // Non-public method to do the heavy lifting of drawing the grid of a given plane segment.
        static void DrawPlaneAtPoint(Plane plane, Vector3 center, float size, Color color, float duration, bool depthTest)
        {
            var basis = Quaternion.LookRotation(plane.normal);
            var scale = Vector3.one * size / 10f;

            var right = Vector3.Scale(basis * Vector3.right, scale);
            var up = Vector3.Scale(basis * Vector3.up, scale);

            for (int i = -5; i <= 5; i++)
            {
                Debug.DrawLine(center + right * i - up * 5, center + right * i + up * 5, color, duration, depthTest);
                Debug.DrawLine(center + up * i - right * 5, center + up * i + right * 5, color, duration, depthTest);
            }
        }
    }
}
