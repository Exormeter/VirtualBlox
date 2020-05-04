using UnityEngine;
using System;

namespace Valve.VR.InteractionSystem
{
    [Serializable]
    public class LDrawBlockSave : BlockSave
    {
        public string LDrawPartName;

        public LDrawBlockSave(GameObject block) : base(block)
        {
            SetInstanceVariables(block.GetComponent<BlockGeometryScript>().BlockStructure);
        }

        public override BlockStructure GetBlockStructure()
        {
            return new LDrawBlockStructure(LDrawPartName, base.color);
        }

        protected override void SetInstanceVariables(BlockStructure structure)
        {
            LDrawBlockStructure lDrawBlockStructure = (LDrawBlockStructure)structure;
            LDrawPartName = lDrawBlockStructure.LDrawPartName;
        }
    }
}

