using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDraw
{

    public class LDrawStud10 : LDrawAbstractTapConnection
    {


        public LDrawStud10(GameObject pos) : base(pos)
        {
            base.Name = "stud10";
            base.ConnectionType = LDRawConnectionType.TAP_CONNECTION;
        }

        public override void GenerateColliderPositions(GameObject block)
        {
            GameObject brickFace = block.transform.Find("Taps").gameObject;
            AddTapCollider(brickFace, this);
        }
    }

}