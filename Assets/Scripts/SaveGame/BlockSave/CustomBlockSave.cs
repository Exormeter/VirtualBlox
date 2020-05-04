using UnityEngine;
using System;

namespace Valve.VR.InteractionSystem
{
    [Serializable]
    public class CustomBlockSave : BlockSave
    {

        public int Rows;
        public int Cols;
        public bool[,] matrix;
        public BLOCKSIZE blockSize;

        public CustomBlockSave(GameObject block) : base(block)
        {
            SetInstanceVariables(block.GetComponent<BlockGeometryScript>().BlockStructure);
        }

        private bool[,] ConvertToBoolMatrix(CustomBlockStructure blockStructure)
        {
            bool[,] convertedMatrix = new bool[blockStructure.RowsCropped, blockStructure.ColsCropped];
            for (int row = 0; row < blockStructure.RowsCropped; row++)
            {
                for (int col = 0; col < blockStructure.ColsCropped; col++)
                {
                    if (blockStructure[row, col] == null)
                    {
                        convertedMatrix[row, col] = false;
                    }
                    else
                    {
                        convertedMatrix[row, col] = true;
                    }
                }
            }
            return convertedMatrix;
        }

        public override BlockStructure GetBlockStructure()
        {
            return new CustomBlockStructure(Rows, Cols, blockSize, color, matrix);
        }

        protected override void SetInstanceVariables(BlockStructure structure)
        {
            CustomBlockStructure customBlockStructure = (CustomBlockStructure)structure;
            matrix = ConvertToBoolMatrix(customBlockStructure);
            Rows = customBlockStructure.RowsCropped;
            Cols = customBlockStructure.ColsCropped;
            blockSize = customBlockStructure.BlockSize;
        }
    }
}