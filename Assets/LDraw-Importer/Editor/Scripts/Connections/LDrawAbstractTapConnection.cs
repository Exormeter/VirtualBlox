using System;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace LDraw
{
    public abstract class LDrawAbstractTapConnection: LDrawAbstractConnectionPoint
    {


        public LDrawAbstractTapConnection(GameObject pos): base(pos)
        {
            
        }

        protected void AddTapCollider(GameObject brickFace, LDrawAbstractConnectionPoint connectionPoint)
        {
            GameObject nwConnectionPoint = new GameObject("Collider");
            nwConnectionPoint.AddComponent<TapCollider>();

            BlockGeometryScript.AddBoxCollider(new Vector3(0.1f, 0.1f, 0.1f), new Vector3(0, 0.01f, 0), true, nwConnectionPoint);
            nwConnectionPoint.transform.SetParent(brickFace.transform);
            nwConnectionPoint.transform.LookAt(-connectionPoint.ConnectorPosition.transform.up);
            nwConnectionPoint.transform.Rotate(90, 0, 0);
            nwConnectionPoint.transform.position = connectionPoint.ConnectorPosition.transform.position;
        }
    }   
}

