using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LDraw
{
    public class LDrawStud6 : LDrawAbstractTapConnection
    {

        public LDrawStud6(GameObject pos) : base(pos)
        {
            base.Name = "stud6";
            base.ConnectionType = LDRawConnectionType.TAP_CONNECTION;
        }

        public override void GenerateColliderPositions(GameObject block)
        {
            GameObject brickFace = block.transform.Find("Taps").gameObject;
            AddTapCollider(brickFace, this);
        }
    }
}