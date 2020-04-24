using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDraw
{

    public class LDraw4070 : LDrawAbstractConnectionPoint
    {

        public LDraw4070(GameObject pos) : base(pos)
        {
            base.Name = "4070";
            base.ConnectionType = LDRawConnectionType.SPECIAL;
        }


        public override List<Vector3> GenerateColliderPositions(GameObject brickFace)
        {
            //AddGroveCollider(brickFace, this, new Vector3(GROOVE_OFFSET, 0, GROOVE_OFFSET));
            //AddGroveCollider(brickFace, this, new Vector3(GROOVE_OFFSET, 0, -GROOVE_OFFSET));
     
            return null;
        }
    }
}