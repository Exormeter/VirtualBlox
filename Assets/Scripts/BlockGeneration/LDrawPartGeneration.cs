using UnityEngine;
using System.Collections;
using LDraw;

namespace Valve.VR.InteractionSystem
{
    public class LDrawPartGeneration : IGenerationStrategie
    {
        private LDrawModelConverter lDrawModelConverter = new LDrawModelConverter();
        private BlockIdentifier blockIdentifier = BlockIdentifier.BLOCK_LDRAW;
        private LDrawBlockStructure structure;
        public LDrawPartGeneration()
        {
            
        }
        public GameObject GenerateGameObject()
        {
            LDrawModel model = LDrawModel.Create(structure.LDrawPartName, LDrawConfig.Instance.GetSerializedPart(structure.LDrawPartName));
            GameObject newBlock = lDrawModelConverter.ConvertLDrawModel(model);
            newBlock.AddComponent<BlockGeometryLDrawPart>();
            newBlock.GetComponent<BlockGeometryLDrawPart>().SetWallCollider();
            newBlock.GetComponent<BlockGeometryLDrawPart>().BlockIdentifier = blockIdentifier;
            newBlock.GetComponent<BlockGeometryLDrawPart>().BlockStructure = structure;
            return newBlock;
        }

        public void SetCustomStructure(CustomBlockStructure customBlockStructure)
        {
            
        }

        public void SetLDrawStructure(LDrawBlockStructure lDrawBlockStructure)
        {
            structure = lDrawBlockStructure;
        }
    }
}