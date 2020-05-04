using UnityEngine;
using System;
using System.Collections.Generic;

namespace Valve.VR.InteractionSystem
{
    [Serializable]
    public abstract class BlockSave
    {
        public Guid guid;
        public List<ConnectedBlockSerialized> connectedBlocks;
        public SerializableVector3 position;
        public SerializableQuaternion rotation;
        public SerializableColor color;
        public int timeStamp;

        public BlockSave(GameObject block)
        {
            guid = block.GetComponent<BlockCommunication>().Guid;
            position = block.transform.position;
            rotation = block.transform.rotation;
            color = block.GetComponentInChildren<MeshRenderer>().material.color;
            timeStamp = GameObject.FindGameObjectWithTag("BlockManager").GetComponent<BlockManager>().GetTimeStampByGuid(guid);
            connectedBlocks = GetConnectedBlocks(block.GetComponent<BlockCommunication>());
        }

        private List<ConnectedBlockSerialized> GetConnectedBlocks(BlockCommunication blockCommunication)
        {
            List<ConnectedBlockSerialized> Guids = new List<ConnectedBlockSerialized>();
            foreach (BlockContainer container in blockCommunication.ConnectedBlocks)
            {
                Guids.Add(new ConnectedBlockSerialized(container));
            }
            return Guids;
        }

        public abstract BlockStructure GetBlockStructure();

        protected abstract void SetInstanceVariables(BlockStructure structure);
    }
}
