using UnityEngine;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
    public class LDrawBlockStructure : BlockStructure
    {
        public string LDrawPartName;
        
        public LDrawBlockStructure(string lDrawPartName, Color color) : base(color, new LDrawPartGeneration())
        {
            LDrawPartName = lDrawPartName;
            GenerationStrategie.SetLDrawStructure(this);
        }

        public override GameObject GenerateBlock()
        {
            return GenerationStrategie.GenerateGameObject();
        }
    }
}