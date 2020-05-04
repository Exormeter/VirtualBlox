using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{
    public class BlockManager : MonoBehaviour
    {
        /// <summary>
        /// Dictonary of all Blocks in Game as Value and identifiable by the Guid
        /// </summary>
        private Dictionary<Guid, GameObject> exsistingBlocksInGame = new Dictionary<Guid, GameObject>();

        /// <summary>
        /// Stack of Removed Blocks, the list can represent a Structure or a single Block
        /// </summary>
        private Stack<List<BlockSave>> RemovedBlockStack = new Stack<List<BlockSave>>();

        /// <summary>
        /// A History of which Block was placed last, History Object contains a Guid and a Timestamp
        /// </summary>
        [HideInInspector]
        public List<HistoryObject> blockPlacingHistory = new List<HistoryObject>();

        //public SteamVR_Input_Sources handInput = SteamVR_Input_Sources.Any;
        //public SteamVR_Action_Vector2 historyJump;
        //public SteamVR_Action_Boolean confirmAction;
        public InputManager MenuManager;
        public SaveGameManager SaveGameManager;


        public void Start()
        {
            
        }

        public bool BlockExists(Guid guid)
        {
            return exsistingBlocksInGame.ContainsKey(guid);
        }

        //private void ManipulateHistory(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
        //{
        //    if (MenuManager.CurrentMenuState == MenuState.BOTH_CLOSED && confirmAction.GetStateDown(fromSource))
        //    {
        //        if (fromAction.GetAxis(handInput).x < -0.7f)
        //        {
        //            RemoveLastPlacedBlock();
        //        }
        //        else if (fromAction.GetAxis(handInput).x > 0.7f)
        //        {
        //            RecoverLastRemovedBlock();
        //        }
        //    }
        //}

        /// <summary>
        /// Removes the newest placed Block or Structure from the scene and saves it in the RemovedBlockStack
        /// </summary>
        public void RemoveLastPlacedBlock(HANDSIDE handSide)
        {
            //Sort after Timestamp
            blockPlacingHistory.Sort();

            //Set index to the last place of the HistoryList
            int index = blockPlacingHistory.Count - 1;

            //No item inside the HistoryList
            if (index < 0)
                return;

            //Get the newest TimeStamp from the HistoryList
            int lastTimeStamp = blockPlacingHistory[index].timeStamp;

            //List to save HistoryOjects that were placed at the same time as the last HistoryObject in HistoryList
            List<HistoryObject> lastHistoryObjects = new List<HistoryObject>();

            //Search HistoryList for all Objects with same TimeStamp
            while(index >= 0 && blockPlacingHistory[index].timeStamp == lastTimeStamp)
            {
                lastHistoryObjects.Add(blockPlacingHistory[index]);
                index--;
            }

            //Get the actual Block by their Guid, save their values und Remove them form the Scene
            List<BlockSave> savedBlocks = new List<BlockSave>();
            foreach(HistoryObject historyObject in lastHistoryObjects)
            {
                GameObject block = GetBlockByGuid(historyObject.guid);
                Debug.Log(block.GetComponent<BlockGeometryScript>().BlockIdentifier.ToString());
                switch (block.GetComponent<BlockGeometryScript>().BlockIdentifier)
                {
                    case BlockIdentifier.BLOCK_CUSTOM:
                        savedBlocks.Add(new CustomBlockSave(block));
                        break;

                    case BlockIdentifier.BLOCK_LDRAW:
                        savedBlocks.Add(new LDrawBlockSave(block));
                        break;
                }
                RemoveBlock(historyObject.guid);
            }

            //Add the saved Block values to the Stack
            RemovedBlockStack.Push(savedBlocks);
        }

        /// <summary>
        /// Recover the newest Block inside the RemovedBlockStack
        /// </summary>
        public void RecoverLastRemovedBlock(HANDSIDE handSide)
        {

            //Stack is empty, can't restore nonexsiting Blocks
            if (RemovedBlockStack.Count == 0)
                return;

            //Get the first Block or Structure from the Stack
            List<BlockSave> blocksToRestore = RemovedBlockStack.Pop();

            //Restore the Block or Blocks by loading the BlockSave and add them back to the History
            foreach(BlockSave blockSave in blocksToRestore)
            {
                SaveGameManager.LoadBlock(blockSave);
                blockPlacingHistory.Add(new HistoryObject(blockSave.guid, blockSave.timeStamp));
            }



            //Connect the Blocks back
            //TODO: Reset the AcceptCollisionAsConnection to false again
            foreach (BlockSave blockSave in blocksToRestore)
            {
                SaveGameManager.ConnectBlocks(blockSave);
            }
        }

        /// <summary>
        /// Clears the History
        /// </summary>
        public void ResetHistoryStack()
        {
            RemovedBlockStack.Clear();
        }


        /// <summary>
        /// Gets a Block by Guid
        /// </summary>
        /// <param name="guid">Guid of the Block</param>
        /// <returns>The Block if found, otherwise null</returns>
        public GameObject GetBlockByGuid(Guid guid)
        {
            return exsistingBlocksInGame[guid];
        }

        /// <summary>
        /// Adds a Block to the exsitingBlock Dictionary
        /// </summary>
        /// <param name="guid">The guid of the Block</param>
        /// <param name="block">The GameObject to add</param>
        public void AddBlock(Guid guid, GameObject block)
        {
            exsistingBlocksInGame.Add(guid, block);
        }

        /// <summary>
        /// Removes a Block by guid from the list and the Scene, clearing everyting
        /// </summary>
        /// <param name="guid"></param>
        public void RemoveBlock(Guid guid)
        {
            BlockCommunication blockCommunication = GetBlockByGuid(guid).GetComponent<BlockCommunication>();

            //Clear the connection to other Blocks
            blockCommunication.RemoveAllBlockConnections();

            //Destroies the GameObject
            Destroy(GetBlockByGuid(guid));

            //Removes the Dictonary Entry
            exsistingBlocksInGame.Remove(guid);

            //Removes the History Entry
            RemoveEntryFromHistory(guid);
            
        }

        /// <summary>
        /// Changes the Guid of an exsiting Block to a new one
        /// </summary>
        /// <param name="oldGuid">The old original guid</param>
        /// <param name="newGuid">The new guid</param>
        /// <param name="block">The Block to change</param>
        public void ChangeGuid(Guid oldGuid, Guid newGuid, GameObject block)
        {
            exsistingBlocksInGame.Remove(oldGuid);
            exsistingBlocksInGame.Add(newGuid, block);
        }

        /// <summary>
        /// Removes all Blocks from the Scene except the Floor Plates
        /// </summary>
        public void RemoveAllBlocks()
        {
            //Save the Guids to remove
            List<Guid> entriesToRemove = new List<Guid>();

            //Enumerate over the exsiting Block Dictornary
            IDictionaryEnumerator blockEnumerator = exsistingBlocksInGame.GetEnumerator();
            while (blockEnumerator.MoveNext())
            {
                GameObject block = (GameObject)blockEnumerator.Value;

                //Normal Block, Destroy Block and add to removal list
                if ( block.tag != "Floor")
                {
                    Destroy(block);
                    entriesToRemove.Add((Guid)blockEnumerator.Key);
                }

                //Floor Plate, clear all connections to Blocks on Floor
                else if(block.tag == "Floor")
                {
                    block.GetComponent<BlockCommunication>().ClearConnectedBlocks();
                }
            }

            //Remove the exsiting Block Dictornary entries
            entriesToRemove.ForEach(key => exsistingBlocksInGame.Remove(key));
            blockPlacingHistory.Clear();
        }

        /// <summary>
        /// Adds a HistoryObject to the History if no Object exsists for the Guid
        /// </summary>
        /// <param name="historyObject">HistoryObject to add</param>
        public void AddHistoryEntry(HistoryObject historyObject)
        {
            if(!blockPlacingHistory.Exists(historyObjectTemp => historyObject.guid == historyObjectTemp.guid))
            {
                blockPlacingHistory.Add(historyObject);
            }
                
        }

        /// <summary>
        /// Removes a Block from the History
        /// </summary>
        /// <param name="guid">Guid to remove from History</param>
        public void RemoveEntryFromHistory(Guid guid)
        {
            blockPlacingHistory.RemoveAll(historyObject => guid == historyObject.guid);
        }

        /// <summary>
        /// Gets a TimeStamp by Guid in History
        /// </summary>
        /// <param name="guid">The Guid to search for</param>
        /// <returns>The searched timestamp or 0 if not found</returns>
        public int GetTimeStampByGuid(Guid guid)
        {
            return blockPlacingHistory.Find(historyObject => historyObject.guid == guid).timeStamp;
        }

        public List<GameObject> GetFloorBlocks()
        {
            List<GameObject> floorBlock = new List<GameObject>();
            foreach(KeyValuePair<Guid, GameObject> entry in exsistingBlocksInGame)
            {
                if (entry.Key.ToString().StartsWith("aaaaaaaa"))
                {
                    floorBlock.Add(entry.Value);
                }
            }
            return floorBlock;
        }
    }

    /// <summary>
    /// A HistoryObject containing a Guid and a TimeStimp, sortable by TimeStamp
    /// </summary>
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


