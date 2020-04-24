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
            MatchPinRotation(currentCollisionObjects[0], currentCollisionObjects[1], connectedOn);
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
        }

        
        private void MatchTargetBlockDistance(CollisionObject collision, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            Plane planeBlock = new Plane();
            float distance = 0;

            switch (connectedOn)
            {
                case OTHER_BLOCK_IS_CONNECTED_ON.GROOVE:
                    planeBlock = new Plane(collision.GroovePosition.transform.up, transform.TransformPoint(collision.GroovePosition.transform.position));
                    distance = planeBlock.GetDistanceToPoint(transform.TransformPoint(collision.TapPosition.transform.position));
                    break;

                case OTHER_BLOCK_IS_CONNECTED_ON.TAP:
                    planeBlock = new Plane(collision.TapPosition.transform.up, transform.TransformPoint(collision.TapPosition.transform.position));
                    distance = planeBlock.GetDistanceToPoint(transform.TransformPoint(collision.GroovePosition.transform.position));
                    break;
            }
            transform.Translate(Vector3.up * distance, Space.Self);
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
                        Debug.Log("Offset: " + centerOffset.ToString("F5"));
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
        }

        private void MatchPinRotation(CollisionObject matchedPin, CollisionObject secoundPin, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            if(connectedOn == OTHER_BLOCK_IS_CONNECTED_ON.GROOVE)
            {
                float angleRotation = GetAngleBetweenPins(matchedPin, secoundPin, connectedOn);
                transform.RotateAround(matchedPin.TapPosition.transform.position, transform.up, angleRotation);

                float newAngleRotation = GetAngleBetweenPins(matchedPin, secoundPin, connectedOn);
                if (newAngleRotation > 0.00001)
                {
                    transform.RotateAround(matchedPin.TapPosition.transform.position, transform.up, -newAngleRotation);
                }
            }
            else
            {
                float angleRotation = GetAngleBetweenPins(matchedPin, secoundPin, connectedOn);
                transform.RotateAround(matchedPin.GroovePosition.transform.position, transform.up, angleRotation);

                float newAngleRotation = GetAngleBetweenPins(matchedPin, secoundPin, connectedOn);
                if (newAngleRotation > 0.00001)
                {
                    transform.RotateAround(matchedPin.GroovePosition.transform.position, transform.up, -newAngleRotation);
                }
            }
            
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
                Debug.Log("Angle Rotation: " + angleRotation); 
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
                Debug.Log("Angle Rotation: " + angleRotation);
            }

            return angleRotation;

        }
    }
}
