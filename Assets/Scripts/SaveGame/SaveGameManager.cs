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

        
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void LoadGame(string choosenFilePath)
        {

            if (File.Exists(choosenFilePath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(choosenFilePath, FileMode.Open);
                SaveGame save = (SaveGame)bf.Deserialize(file);
                file.Close();

                BlockManager.RemoveAllBlocks();

                //Load Blocks into the Scene
                StartCoroutine(LoadBlocks(save));
            }
        }

        IEnumerator LoadBlocks(SaveGame save)
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

        public void ConnectBlocks(SaveGame save)
        {
            foreach (BlockSave blockSave in save.blockSaves)
            {
                ConnectBlocks(blockSave);
            }
        }

        public void ConnectBlocks(BlockSave blockSave)
        {
            GameObject block = BlockManager.GetBlockByGuid(blockSave.guid);
            foreach (ConnectedBlockSerialized connection in blockSave.connectedBlocks)
            {
                block.GetComponent<BlockCommunication>().ConnectBlocks(block, BlockManager.GetBlockByGuid(connection.guid), connection.connectedPins, connection.connectedOn);
            }
        }

        

        public void SaveGame(SaveGame save, string filePath)
        {
            //SaveGame save = CreateSaveGameObject();
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

        public void LoadBlock(BlockSave blockSave)
        {
            GameObject restoredBlock = BlockGenerator.GenerateBlock(blockSave.GetBlockStructure());
            restoredBlock.transform.position = blockSave.position;
            restoredBlock.transform.rotation = blockSave.rotation;
            restoredBlock.GetComponent<BlockCommunication>().Guid = blockSave.guid;
            restoredBlock.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
    }

}
