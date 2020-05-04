using System;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{

    [Serializable]
    public class SaveGame
    {
        public List<BlockSave> blockSaves = new List<BlockSave>();
        public List<HistoryObject> historyObjects = new List<HistoryObject>();

        public BlockSave GetBlockSaveByGuid(Guid guid)
        {
            return blockSaves.Find(blockSave => blockSave.guid == guid);
        }

        public void ReplaceGuids(Dictionary<Guid, Guid> originalToNewGuid)
        {
            foreach(BlockSave blockSave in blockSaves)
            {
                blockSave.guid = originalToNewGuid[blockSave.guid];
                foreach(ConnectedBlockSerialized connectedBlockSerialized in blockSave.connectedBlocks)
                {
                    if (originalToNewGuid.ContainsKey(connectedBlockSerialized.guid))
                    {
                        connectedBlockSerialized.guid = originalToNewGuid[connectedBlockSerialized.guid];
                    }
                }
            }
        }

        internal void RemoveFloorConnections()
        {
            foreach(BlockSave blockSave in blockSaves)
            {
                blockSave.connectedBlocks.RemoveAll(connectedBlockSerialized => connectedBlockSerialized.guid.ToString().StartsWith("aaaaaaaa"));
            }
        }
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
}
