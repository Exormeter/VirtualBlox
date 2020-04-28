using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace LDraw
{
    public abstract class LDrawAbstractGrooveConnection : LDrawAbstractConnectionPoint
    {
        public LDrawAbstractGrooveConnection(GameObject pos) : base(pos)
        {

        }

        protected void AddGroveCollider(GameObject brickFace, LDrawAbstractConnectionPoint connectionPoint, Vector3 offset)
        {

            GameObject newConnectionPoint = new GameObject("Collider");

            BlockGeometryScript.AddBoxCollider(new Vector3(0.1f, 0.1f, 0.1f), new Vector3(0, 0, 0), true, newConnectionPoint);
            newConnectionPoint.transform.SetParent(brickFace.transform);
            newConnectionPoint.transform.LookAt(connectionPoint.ConnectorPosition.transform.up);
            newConnectionPoint.transform.Rotate(90, 0, 0);
            newConnectionPoint.transform.position = connectionPoint.ConnectorPosition.transform.GetChild(0).position;
            newConnectionPoint.transform.position += offset;

            for (int i = 0; i < brickFace.transform.childCount; i++)
            {
                GameObject siblingConnectionPoint = brickFace.transform.GetChild(i).gameObject;
                if (siblingConnectionPoint.transform.position == newConnectionPoint.transform.position &&
                    siblingConnectionPoint.GetHashCode() != newConnectionPoint.GetHashCode())
                {
                    UnityEngine.Object.DestroyImmediate(newConnectionPoint);
                    return;
                }
            }

            newConnectionPoint.AddComponent<GrooveCollider>();
        }

        //public abstract List<Vector3> GenerateColliderPositions(GameObject brickFace, List<LDrawAbstractBoxConnector> listBoxes);
    }
}

