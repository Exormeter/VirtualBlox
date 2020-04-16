using UnityEngine;

namespace LDraw
{
    
    public class LDrawConnectionPoint
    {
        public GameObject ConnectorPosition;
        public LDRawConnectionType ConnectionType;
        public string Name;

        public LDrawConnectionPoint(GameObject connectorPosition, LDRawConnectionType connectionType, string name)
        {
            ConnectorPosition = connectorPosition;
            ConnectionType = connectionType;
            Name = name;
        }
    }
   

}