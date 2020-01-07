using System;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{

    [Serializable]
    public class SaveGame
    {
        public List<BlockSave> blockSaves = new List<BlockSave>();
    }

    [Serializable]
    public class ConnectedBlockSerialized
    {
        public Guid guid;
        public int connectedPins;
        public OTHER_BLOCK_IS_CONNECTED_ON connectedOn;

        public ConnectedBlockSerialized(BlockContainer container)
        {
            guid = container.BlockCommunication.Guid;
            connectedPins = container.ConnectedPinCount;
            connectedOn = container.ConnectedOn;
        }
    }


    [Serializable]
    public class BlockSave
    {
        public Guid guid;
        public List<ConnectedBlockSerialized> connectedBlocks;
        public SerializableVector3 position;
        public SerializableQuaternion rotation;
        public SerializableColor color;
        public int Rows;
        public int Cols;
        public bool[,] matrix;
        public BLOCKSIZE blockSize;

        public BlockSave(GameObject block)
        {
            guid = block.GetComponent<BlockCommunication>().Guid;
            position = block.transform.position;
            rotation = block.transform.rotation;
            matrix = ConvertToBoolMatrix(block.GetComponent<BlockGeometryScript>().blockStructure);
            connectedBlocks = GetConnectedBlocks(block.GetComponent<BlockCommunication>());
            Rows = block.GetComponent<BlockGeometryScript>().blockStructure.RowsCropped;
            Cols = block.GetComponent<BlockGeometryScript>().blockStructure.ColsCropped;
            color = block.GetComponent<MeshRenderer>().material.color;
            blockSize = block.GetComponent<BlockGeometryScript>().blockStructure.BlockSize;
        }

        private List<ConnectedBlockSerialized> GetConnectedBlocks(BlockCommunication blockCommunication)
        {
            List<ConnectedBlockSerialized> Guids = new List<ConnectedBlockSerialized>();
            foreach(BlockContainer container in blockCommunication.ConnectedBlocks)
            {
                Guids.Add(new ConnectedBlockSerialized(container));
            }
            return Guids;
        }

        private bool[,] ConvertToBoolMatrix(BlockStructure blockStructure)
        {
            bool[,] convertedMatrix = new bool[blockStructure.RowsCropped, blockStructure.ColsCropped];
            for(int row = 0; row < blockStructure.RowsCropped; row++)
            {
                for (int col = 0; col < blockStructure.ColsCropped; col++)
                {
                    if(blockStructure[row, col] == null)
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

        public BlockStructure GetBlockStructure()
        {
            BlockStructure structure = new BlockStructure(Rows, Cols, blockSize, color, matrix);
            return structure;
        }
    }

}
