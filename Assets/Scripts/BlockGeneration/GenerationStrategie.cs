using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public interface IGenerationStrategie
    {
        GameObject GenerateGameObject();

        void SetLDrawStructure(LDrawBlockStructure lDrawBlockStructure);

        void SetCustomStructure(CustomBlockStructure customBlockStructure);
    }
}