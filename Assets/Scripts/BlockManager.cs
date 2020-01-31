using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{
    public class BlockManager : MonoBehaviour
    {

        private Dictionary<Guid, GameObject> exsistingBlocksInGame = new Dictionary<Guid, GameObject>();
        private Stack<List<BlockSave>> RemovedBlockStack = new Stack<List<BlockSave>>();

        [HideInInspector]
        public List<HistoryObject> blockPlacingHistory = new List<HistoryObject>();

        public SteamVR_Input_Sources handInput = SteamVR_Input_Sources.Any;
        public SteamVR_Action_Vector2 historyJump;
        public SteamVR_Action_Boolean confirmAction;
        public MenuManager MenuManager;
        public SaveGameManager SaveGameManager;


        public void Start()
        {
            historyJump.AddOnChangeListener(ManipulateHistory, handInput);
        }

        private void ManipulateHistory(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
        {
            //Debug.Log("TouchPad touch" + fromAction.GetAxis(fromSource));
            //if (MenuManager.CurrentMenuState == MenuState.BOTH_CLOSED && confirmAction.GetStateDown(fromSource))
            //{
            //    if (fromAction.GetAxis(handInput).x < -0.7f)
            //    {
            //        RemoveLastPlacedBlock();
            //    }
            //    else if(fromAction.GetAxis(handInput).x > 0.7f)
            //    {
            //        RecoverLastRemovedBlock();
            //    }
            //}
        }

        private void RemoveLastPlacedBlock()
        {
            blockPlacingHistory.Sort();
            int index = blockPlacingHistory.Count - 1;

            if (index < 0)
                return;

            int lastTimeStamp = blockPlacingHistory[index].timeStamp;
            List<HistoryObject> lastHistoryObjects = new List<HistoryObject>();
            
            while(index >= 0 && blockPlacingHistory[index].timeStamp == lastTimeStamp)
            {
                lastHistoryObjects.Add(blockPlacingHistory[index]);
                index--;
            }

            List<BlockSave> savedBlocks = new List<BlockSave>();
            foreach(HistoryObject historyObject in lastHistoryObjects)
            {
                savedBlocks.Add(new BlockSave(GetBlockByGuid(historyObject.guid)));
                RemoveBlock(historyObject.guid);
            }
            RemovedBlockStack.Push(savedBlocks);
        }

        private void RecoverLastRemovedBlock()
        {
            if (RemovedBlockStack.Count == 0)
                return;

            List<BlockSave> blocksToRestore = RemovedBlockStack.Pop();
            foreach(BlockSave blockSave in blocksToRestore)
            {
                SaveGameManager.LoadBlock(blockSave);
                blockPlacingHistory.Add(new HistoryObject(blockSave.guid, blockSave.timeStamp));
            }
            
            //TODO: Reset the AcceptCollisionAsConnection to false again
            foreach (BlockSave blockSave in blocksToRestore)
            {
                SaveGameManager.ConnectBlocks(blockSave);
            }
        }

        public void ResetHistoryStack()
        {
            RemovedBlockStack.Clear();
        }



        public GameObject GetBlockByGuid(Guid guid)
        {
            return exsistingBlocksInGame[guid];
        }

        public void AddBlock(Guid guid, GameObject block)
        {
            exsistingBlocksInGame.Add(guid, block);
        }

        public void RemoveBlock(Guid guid)
        {
            BlockCommunication blockCommunication = GetBlockByGuid(guid).GetComponent<BlockCommunication>();
            blockCommunication.RemoveAllBlockConnections();
            Destroy(GetBlockByGuid(guid));
            exsistingBlocksInGame.Remove(guid);
            RemoveEntryFromHistory(guid);
            
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

        public int GetTimeStampByGuid(Guid guid)
        {
            return blockPlacingHistory.Find(historyObject => historyObject.guid == guid).timeStamp;
        }
    }

    [Serializable]
    public class HistoryObject: IComparable<HistoryObject>
    {
        public Guid guid;
        public int timeStamp;

        public HistoryObject(Guid guid,int timeStamp)
        {
            this.guid = guid;
            this.timeStamp = timeStamp;
        }

        public int CompareTo(HistoryObject other)
        {
            if(other == null)
            {
                return 1;
            }
            else if(other.timeStamp > timeStamp)
            {
                return -1;
            }
            return 1;
        }
    }
}


