using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDraw{

    public class LDrawStud : LDrawAbstractTapConnection
    {
        

        public LDrawStud(GameObject pos) : base(pos)
        {
            base.Name = "stud";
            base.ConnectionType = LDRawConnectionType.TAP_CONNECTION;
        }

        public override List<Vector3> GenerateColliderPositions(GameObject brickFace)
        {
            AddTapCollider(brickFace, this);
            return null;
        }
    }

}

