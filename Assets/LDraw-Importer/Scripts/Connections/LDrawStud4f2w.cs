using System.Collections.Generic;
using UnityEngine;

namespace LDraw
{
    public class LDrawStud4f2w : LDrawAbstractGrooveConnection
    {
        public LDrawStud4f2w(GameObject pos): base(pos)
        {
            base.Name = "stud4f2w";
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





