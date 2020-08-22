using System.Collections.Generic;
using UnityEngine;

namespace LDraw
{
    public class LDrawBox3 : LDrawAbstractBoxConnector
    {
        public LDrawBox3(GameObject pos) : base(pos)
        {
            base.Name = "box3";
        }


        public override void GenerateColliderPositions(GameObject block)
        {

            GameObject brickFace = block.transform.Find("Grooves").gameObject;
            //Needs to be modified as box3 origin is in the middle of the mesh, other boxes are on the edge
            //Moves the box to be flush with edge of brick

            //TODO: Rotation der Box einbeziehen
            ConnectorPosition.transform.localPosition = new Vector3(ConnectorPosition.transform.localPosition.x, ConnectorPosition.transform.localPosition.y + ConnectorPosition.transform.localScale.x, ConnectorPosition.transform.localPosition.z + (ConnectorPosition.transform.localScale.x / 2 ));

            AddGroveCollider(brickFace, this, new Vector3(0, 0, 0));
        }

        public override bool IsValideConnectionPoint()
        {
            return (LDrawConnectionFactory.IsBetween(11, 13, ConnectorPosition.transform.localScale.y) &&
                LDrawConnectionFactory.IsBetween(6, 8, ConnectorPosition.transform.localScale.z));
        }
    }
}