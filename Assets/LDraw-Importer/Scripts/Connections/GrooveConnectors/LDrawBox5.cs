using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDraw
{

    public class LDrawBox5 : LDrawAbstractBoxConnector
    {

        public LDrawBox5(GameObject pos) : base(pos)
        {
            base.Name = "box5";
        }


        public override void GenerateColliderPositions(GameObject block)
        {
            GameObject brickFace = block.transform.Find("Grooves").gameObject;
            AddGroveCollider(brickFace, this, new Vector3(0, 0, 0));
        }

        public override bool IsValideConnectionPoint()
        {
            return (LDrawConnectionFactory.IsBetween(6, 8, ConnectorPosition.transform.localScale.x) &&
                LDrawConnectionFactory.IsBetween(6, 8, ConnectorPosition.transform.localScale.z));
        }
    }
}