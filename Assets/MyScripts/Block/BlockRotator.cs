using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{


    public class BlockRotator : MonoBehaviour
    {
        private BlockGeometryScript blockGeometry;
        // Start is called before the first frame update
        void Start()
        {
            blockGeometry = GetComponent<BlockGeometryScript>();
        }

        public void RotateBlock(List<CollisionObject> currentCollisionObjects, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            //currentCollisionObjects.ForEach(block => Debug.DrawLine(block.GroovePosition.transform.position, new Vector3(1, 1, 1), Color.red, 90));
            //currentCollisionObjects.ForEach(block => Debug.DrawLine(block.TapPosition.transform.position, new Vector3(1, 1, 1), Color.blue, 90));
            
            MatchTargetBlockRotation(currentCollisionObjects[0]);
            MatchTargetBlockDistance(currentCollisionObjects[0], connectedOn);

            MatchTargetBlockOffset(currentCollisionObjects[0], connectedOn);
            MatchPinRotation(currentCollisionObjects[0], currentCollisionObjects[1], connectedOn);
        }

        private void MatchTargetBlockRotation(CollisionObject collision)
        {
            gameObject.transform.rotation = Quaternion.LookRotation(collision.CollidedBlock.transform.up, -transform.forward);
            gameObject.transform.Rotate(Vector3.right, 90f);
        }

        private void MatchTargetBlockDistance(CollisionObject collision, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            Plane planeOtherBlock = new Plane();
            float distance = 0;

            switch (connectedOn)
            {
                case OTHER_BLOCK_IS_CONNECTED_ON.GROOVE:
                    planeOtherBlock = new Plane(transform.TransformPoint(blockGeometry.CornerBottomA.transform.position),
                                                      transform.TransformPoint(blockGeometry.CornerBottomB.transform.position),
                                                      transform.TransformPoint(blockGeometry.CornerBottomC.transform.position));
                    distance = planeOtherBlock.GetDistanceToPoint(transform.TransformPoint(collision.CollidedBlock.GetComponent<BlockGeometryScript>().CornerTopA.transform.position));
                    break;

                case OTHER_BLOCK_IS_CONNECTED_ON.TAP:
                    planeOtherBlock = new Plane(transform.TransformPoint(blockGeometry.CornerTopA.transform.position),
                                                      transform.TransformPoint(blockGeometry.CornerTopB.transform.position),
                                                      transform.TransformPoint(blockGeometry.CornerTopC.transform.position));
                    distance = planeOtherBlock.GetDistanceToPoint(transform.TransformPoint(collision.CollidedBlock.GetComponent<BlockGeometryScript>().CornerBottomA.transform.position));
                    break;
            }
            transform.Translate(Vector3.up * distance, Space.Self);

            Debug.Log(Vector3.Dot(GetComponent<BlockGeometryScript>().GetBlockNormale(), collision.CollidedBlock.GetComponent<BlockGeometryScript>().GetBlockNormale())); 
        }

        private void MatchTargetBlockOffset(CollisionObject collision, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            if (connectedOn == OTHER_BLOCK_IS_CONNECTED_ON.GROOVE)
            {
                Vector3 tapColliderCenterLocal = transform.InverseTransformPoint(collision.TapPosition.transform.position);
                Vector3 grooveColliderCenterLocal = collision.GroovePosition.transform.localPosition;
                Vector3 centerOffset = tapColliderCenterLocal - grooveColliderCenterLocal;
                transform.Translate(Vector3.right * centerOffset.x, Space.Self);
                transform.Translate(Vector3.forward * centerOffset.z, Space.Self);
            }
            else
            {
                Vector3 grooveColliderCenterLocal = transform.InverseTransformPoint(collision.GroovePosition.transform.position);
                Vector3 tapColliderCenterLocal = collision.TapPosition.transform.localPosition;
                Vector3 centerOffset = grooveColliderCenterLocal - tapColliderCenterLocal;
                transform.Translate(Vector3.right * centerOffset.x, Space.Self);
                transform.Translate(Vector3.forward * centerOffset.z, Space.Self);
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

        //TODO: Find Metric for negative Angle
        private float GetAngleBetweenPins(CollisionObject matchedPin, CollisionObject secoundPin, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            float angleRotation = 0;
            if (connectedOn == OTHER_BLOCK_IS_CONNECTED_ON.GROOVE)
            {
                Vector3 intersectionPointTap = transform.InverseTransformPoint(matchedPin.TapPosition.transform.position);
                Vector3 tapColliderCenter = transform.InverseTransformPoint(secoundPin.TapPosition.transform.position);
                Vector3 grooveColliderCenter = secoundPin.GroovePosition.transform.localPosition;

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
                Vector3 tapPoint = secoundPin.TapPosition.transform.localPosition;

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
