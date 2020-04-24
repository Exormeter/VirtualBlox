using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDraw
{

    public class LDrawStud2 : LDrawAbstractTapConnection
    {

        public LDrawStud2(GameObject pos) : base(pos)
        {
            base.Name = "stud2";
            base.ConnectionType = LDRawConnectionType.TAP_CONNECTION;
        }

        public override List<Vector3> GenerateColliderPositions(GameObject brickFace)
        {
            AddTapCollider(brickFace, this);
            return null;
        }
    }

}
