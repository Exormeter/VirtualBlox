using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDraw
{

    public class LDrawStud4a : LDrawAbstractGrooveConnection
    {

        public LDrawStud4a(GameObject pos) : base(pos)
        {
            base.Name = "stud4a";
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