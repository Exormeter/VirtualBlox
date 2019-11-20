using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace Valve.VR.InteractionSystem
{
    public class BlockStructure
    {

        private readonly BlockPart[,] matrix;
        private BlockPart[,] croppedMatrix;
        public int Rows { get; }
        public int Cols { get; }
        public int RowsCropped { get; private set; }
        public int ColsCropped { get; private set; }
        public BlockStructure(int row, int col)
        {
            Rows = row;
            Cols = col;
            matrix = new BlockPart[row, col];
        }

        public void AddNode(BlockPart node, int row, int col)
        {
            matrix[row, col] = node;
        }

        public BlockPart[,] GetCroppedMatrix()
        {

            if (croppedMatrix != null)
            {
                return croppedMatrix;
            }

            int emptyRows = 0;
            int emptyCols = 0;

            for (int row = 0; row < Rows; row++)
            {
                if (!IsEmpty(GetRow(matrix, row)))
                {
                    emptyRows = row;
                    break;
                }
            }

            for (int col = 0; col < Cols; col++)
            {
                if (!IsEmpty(GetColumn(matrix, col)))
                {
                    emptyCols = col;
                    break;
                }
            }

            RowsCropped = Rows - emptyRows;
            ColsCropped = Cols - emptyCols;

            croppedMatrix = new BlockPart[RowsCropped, ColsCropped];

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    if (matrix[row, col] != null)
                    {
                        croppedMatrix[row - emptyRows, col - emptyCols] = new BlockPart(row - emptyRows, col - emptyCols);
                    }
                }
            }


            
            for (int row = 0; row < RowsCropped; row++)
            {
                if (IsEmpty(GetRow(croppedMatrix, row)))
                {
                    RowsCropped = row;
                    break;
                }
            }

            for (int col = 0; col < ColsCropped; col++)
            {
                if (IsEmpty(GetColumn(croppedMatrix, col)))
                {
                    ColsCropped = col;
                    break;
                }
            }

            
            return croppedMatrix;


        }

        private bool IsEmpty(BlockPart[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != null)
                {
                    return false;
                }
            }
            return true;
        }

        private BlockPart[] GetColumn(BlockPart[,] matrix, int colNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(0))
                    .Select(x => matrix[x, colNumber])
                    .ToArray();
        }

        private BlockPart[] GetRow(BlockPart[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }

        public bool HasNeighbour(int row, int col, DIRECTION direction)
        {
            if (row >= RowsCropped || col >= ColsCropped || row < 0 || col < 0)
            {
                return false;
            }

            switch (direction)
            {

                case DIRECTION.UP:
                    if (row - 1 >= 0 && croppedMatrix[row - 1, col] != null)
                    {
                        return true;
                    }
                    break;

                case DIRECTION.DOWN:
                    if (row + 1 < RowsCropped && croppedMatrix[row + 1, col] != null)
                    {
                        return true;
                    }
                    break;

                case DIRECTION.LEFT:
                    if (col - 1 >= 0 && croppedMatrix[row, col - 1] != null)
                    {
                        return true;
                    }
                    break;

                case DIRECTION.RIGHT:
                    if (col + 1 < ColsCropped && croppedMatrix[row, col + 1] != null)
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }

        public BlockPart this[int row, int col]
        {
            get => Get(row, col);
            set => AddNode(value, row, col);
        }

        private BlockPart Get(int row, int col)
        {
            if (croppedMatrix == null)
            {
                return GetCroppedMatrix()[row, col];
            }
            return croppedMatrix[row, col];
        }
    }
}

