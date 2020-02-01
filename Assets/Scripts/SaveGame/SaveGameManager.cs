using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{

    public class SaveGameManager : MonoBehaviour
    {
        public BlockManager BlockManager;
        public BlockGenerator BlockGenerator;

        public void LoadSceneFromFile(string choosenFilePath)
        {

            if (File.Exists(choosenFilePath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(choosenFilePath, FileMode.Open);
                SaveGame save = (SaveGame)bf.Deserialize(file);
                file.Close();

                BlockManager.RemoveAllBlocks();

                //Load Blocks into the Scene
                StartCoroutine(LoadScene(save));
            }
        }

        IEnumerator LoadScene(SaveGame save)
        {
            save.historyObjects.Sort();
            BlockManager.blockPlacingHistory = save.historyObjects;
            for (int i = 0; i < save.historyObjects.Count; i++)
            {
                LoadBlock(save, save.historyObjects[i]);

                //Load all Block that were placed together
                while (i + 1 < save.historyObjects.Count && save.historyObjects[i].timeStamp == save.historyObjects[i + 1].timeStamp)
                {
                    LoadBlock(save, save.historyObjects[i + 1]);
                    i++;
                }

                yield return new WaitForSeconds(0.5f);
            }
            ConnectBlocks(save);
        }

        public void ConnectBlocks(SaveGame saveGame)
        {
            foreach (BlockSave blockSave in saveGame.blockSaves)
            {
                ConnectBlocks(blockSave, saveGame);
            }
        }

        public void ConnectBlocks(BlockSave blockSave, SaveGame saveGame = null)
        {
            GameObject block = BlockManager.GetBlockByGuid(blockSave.guid);
            foreach (ConnectedBlockSerialized connection in blockSave.connectedBlocks)
            {
                block.GetComponentInChildren<TapHandler>().AcceptCollisionsAsConnected(false);
                block.GetComponentInChildren<GrooveHandler>().AcceptCollisionsAsConnected(false);

                if(saveGame == null || saveGame.GetBlockSaveByGuid(connection.guid) != null)
                {
                    block.GetComponent<BlockCommunication>().ConnectBlocks(block, BlockManager.GetBlockByGuid(connection.guid), connection.connectedPins, connection.connectedOn);
                }
                
            }
        }

        public void LoadPrefabFromFile(string choosenFilePath)
        {
            if (File.Exists(choosenFilePath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(choosenFilePath, FileMode.Open);
                SaveGame save = (SaveGame)bf.Deserialize(file);
                file.Close();

                //Load Blocks into the Scene
                StartCoroutine(LoadPrefab(save));
            }
        }

        IEnumerator LoadPrefab(SaveGame saveGame)
        {
            List<GameObject> loadedBlocks = new List<GameObject>();

            Dictionary<Guid, Guid> originalToNewGuid = new Dictionary<Guid, Guid>();

            foreach(BlockSave blockSave in saveGame.blockSaves)
            {
                GameObject loadedBlock = LoadBlockWithNewGuid(blockSave, new Vector3(0, 3, 0));
                originalToNewGuid.Add(blockSave.guid, loadedBlock.GetComponent<BlockCommunication>().Guid);
                loadedBlock.GetComponentInChildren<TapHandler>().AcceptCollisionsAsConnected(true);
                loadedBlock.GetComponentInChildren<GrooveHandler>().AcceptCollisionsAsConnected(true);
                loadedBlocks.Add(loadedBlock);
                yield return new WaitForSeconds(0.1f);
            }

            saveGame.ReplaceGuids(originalToNewGuid);

            ConnectBlocks(saveGame);
            foreach (GameObject loadedBlock in loadedBlocks)
            {
                loadedBlock.GetComponentInChildren<TapHandler>().AcceptCollisionsAsConnected(false);
                loadedBlock.GetComponentInChildren<GrooveHandler>().AcceptCollisionsAsConnected(false);
                //loadedBlock.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            }
        }
        

        public void SaveGame(SaveGame save, string filePath)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(filePath);
            bf.Serialize(file, save);
            file.Close();
        }

        public void LoadBlock(SaveGame save, HistoryObject historyObject)
        {
            BlockSave blockSave = save.GetBlockSaveByGuid(historyObject.guid);
            LoadBlock(blockSave);
        }

        public GameObject LoadBlock(BlockSave blockSave, Vector3 offset = new Vector3())
        {
            GameObject restoredBlock = BlockGenerator.GenerateBlock(blockSave.GetBlockStructure());
            restoredBlock.transform.position = blockSave.position + offset;
            restoredBlock.transform.rotation = blockSave.rotation;
            restoredBlock.GetComponent<BlockCommunication>().Guid = blockSave.guid;
            restoredBlock.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            return restoredBlock;
        }

        public GameObject LoadBlockWithNewGuid(BlockSave blockSave, Vector3 offset = new Vector3())
        {
            GameObject restoredBlock = BlockGenerator.GenerateBlock(blockSave.GetBlockStructure());
            restoredBlock.transform.position = blockSave.position + offset;
            restoredBlock.transform.rotation = blockSave.rotation;
            restoredBlock.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            return restoredBlock;
        }
    }

}
