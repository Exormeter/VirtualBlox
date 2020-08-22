using System;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace LDraw
{
    public abstract class LDrawAbstractBoxConnector: LDrawAbstractConnectionPoint
    {

        public LDrawAbstractBoxConnector(GameObject pos): base(pos)
        {
            base.ConnectionType = LDRawConnectionType.GROOVE_CONNECTION;
        }


        public abstract bool IsValideConnectionPoint();

        protected void AddGroveCollider(GameObject brickFace, LDrawAbstractConnectionPoint connectionPoint, Vector3 offset)
        {
            if (!IsValideConnectionPoint())
            {
                return;
            }
            GameObject newConnectionPoint = new GameObject("Collider");

            BlockGeometryScript.AddBoxCollider(new Vector3(0.1f, 0.1f, 0.1f), new Vector3(0, 0, 0), true, newConnectionPoint);
            newConnectionPoint.transform.SetParent(brickFace.transform);
            newConnectionPoint.transform.LookAt(connectionPoint.ConnectorPosition.transform.up);
            newConnectionPoint.transform.Rotate(90, 0, 0);
            Debug.Log(connectionPoint.ConnectorPosition.transform.GetChild(0).position);
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
    }
}

