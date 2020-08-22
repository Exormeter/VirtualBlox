using System;
using System.Collections.Generic;
using UnityEngine;

namespace LDraw
{

    public class LDrawStud2a : LDrawAbstractTapConnection
    {

        public LDrawStud2a(GameObject pos) : base(pos)
        {
            base.Name = "stud2a";
            base.ConnectionType = LDRawConnectionType.TAP_CONNECTION;
        }

        public override void GenerateColliderPositions(GameObject block)
        {
            GameObject brickFace = block.transform.Find("Taps").gameObject;

            if (BrickName.Equals("4070"))
            {
                AddTapCollider(brickFace, this);
            }

            else if (BrickName.Equals("2464"))
            {
                AddTapCollider(brickFace, this);
            }
            
            else
            {
                Vector3 localPosition = ConnectorPosition.transform.localPosition;
                if (ConnectorPosition.transform.localScale.y > 0)
                {
                    localPosition.y = 0;
                    ConnectorPosition.transform.localPosition = localPosition;
                }
                AddTapCollider(brickFace, this);
            }
        }
    }

}