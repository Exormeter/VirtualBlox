using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDraw
{
    public class LDrawStud4 : LDrawAbstractGrooveConnection
    {
        protected new float GROOVE_OFFSET = 0.1f;

        public LDrawStud4(GameObject pos) : base(pos)
        {
            base.Name = "stud4";
            base.ConnectionType = LDRawConnectionType.GROOVE_CONNECTION;
        }


        public override List<Vector3> GenerateColliderPositions(GameObject brickFace)
        {
            AddGroveCollider(brickFace, this, new Vector3(GROOVE_OFFSET, 0, GROOVE_OFFSET));
            AddGroveCollider(brickFace, this, new Vector3(GROOVE_OFFSET, 0, -GROOVE_OFFSET));
            AddGroveCollider(brickFace, this, new Vector3(-GROOVE_OFFSET, 0, GROOVE_OFFSET));
            AddGroveCollider(brickFace, this, new Vector3(-GROOVE_OFFSET, 0, -GROOVE_OFFSET));

            return null;
        }
    }
}