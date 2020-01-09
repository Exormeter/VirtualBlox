using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{
    public class BlockManager : MonoBehaviour
    {

        private Dictionary<Guid, GameObject> exsistingBlocksInGame = new Dictionary<Guid, GameObject>();
        public List<HistoryObject> blockPlacingHistory = new List<HistoryObject>();


        public GameObject GetBlockByGuid(Guid guid)
        {
            return exsistingBlocksInGame[guid];
        }

        public void AddBlock(Guid guid, GameObject block)
        {
            exsistingBlocksInGame.Add(guid, block);
        }

        public void ChangeGuid(Guid oldGuid, Guid newGuid, GameObject block)
        {
            exsistingBlocksInGame.Remove(oldGuid);
            exsistingBlocksInGame.Add(newGuid, block);
        }

        public void RemoveAllBlocks()
        {
            List<Guid> entriesToRemove = new List<Guid>();
            IDictionaryEnumerator blockEnumerator = exsistingBlocksInGame.GetEnumerator();
            while (blockEnumerator.MoveNext())
            {
                GameObject block = (GameObject)blockEnumerator.Value;
                if ( block.tag != "Floor")
                {
                    Destroy(block);
                    entriesToRemove.Add((Guid)blockEnumerator.Key);
                }
                else if(block.tag == "Floor")
                {
                    block.GetComponent<BlockCommunication>().ClearConnectedBlocks();
                }
            }
            entriesToRemove.ForEach(key => exsistingBlocksInGame.Remove(key));
        }

        public void AddHistoryEntry(HistoryObject historyObject)
        {
            blockPlacingHistory.Add(historyObject);
        }

        public void RemoveEntryFromHistory(Guid guid)
        {
            blockPlacingHistory.RemoveAll(historyObject => guid == historyObject.guid);
        }
    }

    [Serializable]
    public class HistoryObject
    {
        public Guid guid;
        public int timeStamp;

        public HistoryObject(Guid guid,int timeStamp)
        {
            this.guid = guid;
            this.timeStamp = timeStamp;
        }
    }
}


