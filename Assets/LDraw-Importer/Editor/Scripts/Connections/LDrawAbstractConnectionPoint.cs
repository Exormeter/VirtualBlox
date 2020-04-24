using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace LDraw
{
    
    public abstract class LDrawAbstractConnectionPoint
    {
        public GameObject ConnectorPosition;
        public LDRawConnectionType ConnectionType;
        public string Name;
        public string LDrawName;
        public string BrickName;
        protected const float GROOVE_OFFSET = 0.1f;
        protected List<LDrawAbstractConnectionPoint> otherConnectionsInBlock;


        public LDrawAbstractConnectionPoint(GameObject connectorPosition)
        {
            ConnectorPosition = connectorPosition;
            BrickName = connectorPosition.transform.parent.name;
            LDrawName = connectorPosition.name;
        }

        public abstract List<Vector3> GenerateColliderPositions(GameObject container);

        public void AddConnectionPoint(List<LDrawAbstractConnectionPoint> connectionPoints)
        {
            otherConnectionsInBlock = new List<LDrawAbstractConnectionPoint>(connectionPoints);

        }

    }
   

}