﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace Valve.VR.InteractionSystem
{
    /// <summary>
    /// Represents a Block as a Structure, can be converted to a real Block by the BlockGenerator
    /// </summary>
    public class BlockStructure
    {
        /// <summary>
        /// Uncropped raw BlockPart Matrix, could contain empty leading Rows or Columns. Is generated by the User
        /// via the Block create Menu
        /// </summary>
        private readonly BlockPart[,] matrix;

        /// <summary>
        /// Cropped BlockPart Matrix, dosn't contain empty Rows or columns
        /// </summary>
        private BlockPart[,] croppedMatrix;

        /// <summary>
        /// Rows of the raw Martix
        /// </summary>
        public int Rows { get; }

        /// <summary>
        /// Columns of the raw Matrix
        /// </summary>
        public int Cols { get; }

        /// <summary>
        /// Rows of the cropped Matrix
        /// </summary>
        public int RowsCropped { get; private set; }

        /// <summary>
        /// Columns of the cropped Matrix
        /// </summary>
        public int ColsCropped { get; private set; }

        /// <summary>
        /// Height of the Block, can be FLAT or NORMAL
        /// </summary>
        public BLOCKSIZE BlockSize { get; set; }

        /// <summary>
        /// Color of the Block
        /// </summary>
        public Color BlockColor { get; set; }

        public BlockStructure(int row, int col, BLOCKSIZE size, Color color)
        {
            Rows = row;
            Cols = col;
            matrix = new BlockPart[row, col];
            BlockSize = size;
            BlockColor = color;
        }

        public BlockStructure(int row, int col, BLOCKSIZE size, Color color, bool[,] serializedMatrix)
        {
            Rows = row;
            Cols = col;
            matrix = DeserializeMatrix(serializedMatrix);
            BlockSize = size;
            BlockColor = color;
        }

        public BlockStructure(int rows, int columns, bool fill = false)
        {
            Rows = rows;
            Cols = columns;
            matrix = new BlockPart[rows, columns];
            if (fill)
            {
                FillComplete();
            }
        }

        /// <summary>
        /// Deserializes a Matrix from a serialized Matrix, used when BlockStructure is saved to a file.
        /// </summary>
        /// <param name="serializedMatrix">A Matrix of bools</param>
        /// <returns>A BlockPart Matrix</returns>
        private BlockPart[,] DeserializeMatrix(bool[,] serializedMatrix)
        {
            BlockPart[,] newMatrix = new BlockPart[Rows, Cols];
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    if(serializedMatrix[row, col])
                    {
                        newMatrix[row, col] = new BlockPart(row, col);
                    }
                }
            }
            return newMatrix;
        }

        /// <summary>
        /// Adds a BlockPart to the Matrix
        /// </summary>
        /// <param name="node">The Part to add</param>
        /// <param name="row">The Row in the Matrix</param>
        /// <param name="col">The Column in the Matrix</param>
        public void AddNode(BlockPart node, int row, int col)
        {
            matrix[row, col] = node;
        }

        /// <summary>
        /// Fills the Matrix complete with BlockParts
        /// </summary>
        public void FillComplete()
        {
            for(int row = 0; row < Rows; row++)
            {
                for(int col = 0; col < Cols; col++)
                {
                    matrix[row, col] = new BlockPart(row, col);
                }
            }
        }

        /// <summary>
        /// Crops the Matrix and returnes the cropped Matrix. If a cropped Matrix already exsists, it is
        /// returned imidiatly
        /// </summary>
        /// <returns>A cropped Matrix</returns>
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


        /// <summary>
        /// Checks if a Array of BlockParts is empty
        /// </summary>
        /// <param name="array">The array to check</param>
        /// <returns>True if array is empty, as in contains null in every spot</returns>
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

        /// <summary>
        /// Gets a certain Column from a Matrix
        /// </summary>
        /// <param name="matrixToSearch">Matrix to get the Colum from</param>
        /// <param name="colNumber">The wanted Column number</param>
        /// <returns>Array that represents the Column</returns>
        private BlockPart[] GetColumn(BlockPart[,] matrixToSearch, int colNumber)
        {
            return Enumerable.Range(0, matrixToSearch.GetLength(0))
                    .Select(x => matrix[x, colNumber])
                    .ToArray();
        }

        /// <summary>
        /// Gets a certain Row from a Matrix
        /// </summary>
        /// <param name="matrixToSearch">Matrix to get the Row from</param>
        /// <param name="rowNumber">The wanted Row number</param>
        /// <returns>Array that represents the Row</returns>
        private BlockPart[] GetRow(BlockPart[,] matrixToSearch, int rowNumber)
        {
            return Enumerable.Range(0, matrixToSearch.GetLength(1))
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }

        /// <summary>
        /// Checks if a BlockPart has a Neighbor in a certain direction or if the place is null
        /// </summary>
        /// <param name="row">The Row to check</param>
        /// <param name="col">The Column to check</param>
        /// <param name="direction">The direction to check</param>
        /// <returns>True if BlockPart has a Neighbor</returns>
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

        /// <summary>
        /// Overload the [,] Operator to get BlockParts on certain positin
        /// </summary>
        /// <param name="row">Row of the desired BlockPart</param>
        /// <param name="col">Column of the desited BlockPart</param>
        /// <returns>The disired BlockPart</returns>
        public BlockPart this[int row, int col]
        {
            get => Get(row, col);
            set => AddNode(value, row, col);
        }

        /// <summary>
        /// Helper Method for the [] overload
        /// </summary>
        /// <param name="row">Row of the desired BlockPart</param>
        /// <param name="col">Column of the desited BlockPart</param>
        /// <returns>The disired BlockPart</returns>
        private BlockPart Get(int row, int col)
        {
            if (croppedMatrix == null)
            {
                return GetCroppedMatrix()[row, col];
            }
            return croppedMatrix[row, col];
        }

        /// <summary>
        /// Resets all BlockParts in the Matrix to the non visitd Status
        /// </summary>
        public void ResetBlockParts()
        {
            for (int row = 0; row < RowsCropped; row++)
            {
                for (int col = 0; col < ColsCropped; col++)
                {
                    croppedMatrix[row, col].ResetVisited();
                }
            }
        }
    }
}

