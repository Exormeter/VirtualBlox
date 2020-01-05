using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class SaveGameManager : MonoBehaviour
    {
        public BlockGenerator BlockGenerator;
        public BlockManager BlockManager;
        
        void Start()
        {

        }

        
        void Update()
        {

        }

        public void LoadGame()
        {
            
            if (File.Exists(Application.persistentDataPath + "/gamesave.save"))
            {
                
                
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/gamesave.save", FileMode.Open);
                SaveGame save = (SaveGame)bf.Deserialize(file);
                file.Close();

                //Load Blocks into the Scene
                foreach(BlockSave blockSave in save.blockSaves)
                {
                    GameObject restoredBlock = BlockGenerator.GenerateBlock(blockSave.GetBlockStructure());
                    restoredBlock.transform.position = blockSave.position;
                    restoredBlock.transform.rotation = blockSave.rotation;
                    restoredBlock.GetComponent<BlockCommunication>().Guid = blockSave.guid;
                }

                //Connect blocks in the Scene
                foreach (BlockSave blockSave in save.blockSaves)
                {
                    GameObject block = BlockManager.GetBlockByGuid(blockSave.guid);
                    foreach(Guid guid in blockSave.connectedBlocks)
                    {
                        block.GetComponent<BlockCommunication>().ConnectBlocks(block, BlockManager.GetBlockByGuid(guid), 12, OTHER_BLOCK_IS_CONNECTED_ON.GROOVE);
                    }
                }
            }

            else
            {
                Debug.Log("No game saved!");
            }
        }

        public SaveGame CreateSaveGameObject()
        {
            SaveGame newSaveGame = new SaveGame();
            GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
            foreach(GameObject block in blocks)
            {
                if (block.GetComponent<BlockCommunication>().IsIndirectlyAttachedToFloor())
                {
                    newSaveGame.blockSaves.Add(new BlockSave(block));
                }
            }
            return newSaveGame;
        }

        public void SaveGame()
        {
            SaveGame save = CreateSaveGameObject();
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + "/gamesave.save");
            bf.Serialize(file, save);
            file.Close();
        }
    }

    
}