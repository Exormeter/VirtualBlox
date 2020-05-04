using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class CustomBlockGeneration : IGenerationStrategie
    {
        /// <summary>
        /// Lenght of a 1x1 Block
        /// </summary>
        private readonly float length = 0.08f;

        private CustomBlockStructure structure;
        private BlockIdentifier blockIdentifier = BlockIdentifier.BLOCK_CUSTOM;

        public CustomBlockGeneration()
        {
            
        }

        public GameObject GenerateGameObject()
        {
            GameObject container = new GameObject();
            structure.GetCroppedMatrix();
            float rowMiddlePoint = (float)(structure.RowsCropped - 1) / 2;
            float colMiddlePoint = (float)(structure.ColsCropped - 1) / 2;
            for (int row = 0; row < structure.RowsCropped; row++)
            {
                for (int col = 0; col < structure.ColsCropped; col++)
                {
                    if (structure[row, col] != null)
                    {

                        Vector3 partPosition = new Vector3((rowMiddlePoint - row) * length, 0, (colMiddlePoint - col) * length);
                        GameObject blockPart = Object.Instantiate(BlockGenerator.PartSizes[structure.BlockSize], partPosition, Quaternion.identity, container.transform);
                        blockPart.SetActive(true);
                    }

                }
            }
            GameObject newBlock = BlockGenerator.CombineTileMeshes(container);
            newBlock.AddComponent<BlockGeometryCustom>();
            newBlock.GetComponent<BlockGeometryCustom>().SetCustomStructure(structure);
            newBlock.GetComponent<BlockGeometryCustom>().BlockIdentifier = blockIdentifier;
            return newBlock;
        }

        public void SetCustomStructure(CustomBlockStructure customBlockStructure)
        {
            structure = customBlockStructure;
        }

        public void SetLDrawStructure(LDrawBlockStructure lDrawBlockStructure)
        {
            throw new System.NotImplementedException();
        }
    }
}