using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDraw
{

    public class LDrawStud3 : LDrawAbstractGrooveConnection
    {

        public LDrawStud3(GameObject pos) : base(pos)
        {
            base.Name = "stud3";
            base.ConnectionType = LDRawConnectionType.GROOVE_CONNECTION;
        }

        public override void GenerateColliderPositions(GameObject block)
        {
            GameObject brickFace = block.transform.Find("Grooves").gameObject;
            List<LDrawAbstractBoxConnector> boxConnectionPoints = new List<LDrawAbstractBoxConnector>();

            foreach(LDrawAbstractConnectionPoint connectorPoint in base.otherConnectionsInBlock)
            {
                if (connectorPoint is LDrawAbstractBoxConnector)
                {
                    boxConnectionPoints.Add((LDrawAbstractBoxConnector)connectorPoint);
                }
            }

            //Moving Conector up to ensure that it inside the encasing box
            base.ConnectorPosition.transform.position = base.ConnectorPosition.transform.position + base.ConnectorPosition.transform.up * 0.01f;

            //4-4Disc for stud3
            /*if(base.ConnectorPosition.transform.childCount <= 2)
            {
                return null;
            }*/
            Vector3 positionInBox = base.ConnectorPosition.transform.Find("4-4disc").transform.position;

            float smallestVolume = float.MaxValue;
            //LDrawAbstractBoxConnector smallestBox = null;
            Bounds smallestBounds = new Bounds();

            foreach(LDrawAbstractBoxConnector boxConnector in boxConnectionPoints)
            {
                Bounds currentBoxBounds = boxConnector.ConnectorPosition.GetComponentInChildren<Renderer>().bounds;
                if (currentBoxBounds.Contains(positionInBox))
                {
                    float currentVolume = currentBoxBounds.size.x * currentBoxBounds.size.y * currentBoxBounds.size.z;
                    if (currentVolume < smallestVolume)
                    {
                        Debug.Log(currentBoxBounds.size.ToString("F5"));
                        smallestVolume = currentVolume;
                        smallestBounds = currentBoxBounds;
                    }
                }
            }

            //Moving the Connector back to original position for Groove Collider Positioning
            base.ConnectorPosition.transform.position = base.ConnectorPosition.transform.position - base.ConnectorPosition.transform.up * 0.01f;

            if (smallestVolume == float.MaxValue)
            {
                Debug.Log("No containing Box found");
                return;
            }

            if(smallestBounds.size.x < smallestBounds.size.z)
            {
                AddGroveCollider(brickFace, this, new Vector3(0, 0, GROOVE_OFFSET));
                AddGroveCollider(brickFace, this, new Vector3(0, 0, -GROOVE_OFFSET));
            }
            else
            {
                AddGroveCollider(brickFace, this, new Vector3(GROOVE_OFFSET, 0, 0));
                AddGroveCollider(brickFace, this, new Vector3(-GROOVE_OFFSET, 0, 0));
            }
        }
    }

}