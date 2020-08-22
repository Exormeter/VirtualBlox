using System.Collections.Generic;
using UnityEngine;

namespace LDraw
{
    public class LDrawStud15 : LDrawAbstractTapConnection
    {

        public LDrawStud15(GameObject pos) : base(pos)
        {
            base.Name = "stud15";
            base.ConnectionType = LDRawConnectionType.TAP_CONNECTION;
        }

        public override void GenerateColliderPositions(GameObject block)
        {
            GameObject brickFace = block.transform.Find("Taps").gameObject;
            AddTapCollider(brickFace, this);
        }
    }
}