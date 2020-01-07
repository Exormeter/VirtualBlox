using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem
{
    public class SaveGameManager : MonoBehaviour
    {
        public BlockGenerator BlockGenerator;
        public BlockManager BlockManager;
        public GameObject button;
        public GameObject ButtonListContent;
        public GameObject KeyBoard;
        public Text ChoosenFile;

        private string currentlyChoosenFile;
        private string newFileName;

        void Start()
        {
            string[] filePaths = Directory.GetFiles(Application.persistentDataPath);
            foreach(string filePath in filePaths)
            {
                if (File.Exists(filePath))
                {
                    GameObject newButton = Instantiate(button) as GameObject;
                    
                    newButton.GetComponentInChildren<Text>().text = Path.GetFileName(filePath);
                    newButton.transform.SetParent(ButtonListContent.transform, false);
                    newButton.GetComponent<Button>().onClick.AddListener(() => GetFileName(filePath));
                }
            }
            KeyBoard.SetActive(false);
            ChoosenFile.text = "Ausgewählte Datei: ";


        }

        public void GetFileName(string filePath)
        {
            Debug.Log(filePath);
            string fileName = Path.GetFileName(filePath);
            currentlyChoosenFile = filePath;
            ChoosenFile.text += fileName;
        }

        public void ButtonListen(string button)
        {
            newFileName += button;
            ChoosenFile.text += button;
        }

        public void LoadGame()
        {
            
            if (File.Exists(currentlyChoosenFile))
            {
                
                
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(currentlyChoosenFile, FileMode.Open);
                SaveGame save = (SaveGame)bf.Deserialize(file);
                file.Close();

                //Load Blocks into the Scene
                foreach(BlockSave blockSave in save.blockSaves)
                {
                    GameObject restoredBlock = BlockGenerator.GenerateBlock(blockSave.GetBlockStructure());
                    restoredBlock.transform.position = blockSave.position;
                    restoredBlock.transform.rotation = blockSave.rotation;
                    restoredBlock.GetComponent<BlockCommunication>().Guid = blockSave.guid;
                    restoredBlock.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                }

                //Connect blocks in the Scene
                foreach (BlockSave blockSave in save.blockSaves)
                {
                    GameObject block = BlockManager.GetBlockByGuid(blockSave.guid);
                    foreach(ConnectedBlockSerialized connection in blockSave.connectedBlocks)
                    {
                        block.GetComponent<BlockCommunication>().ConnectBlocks(block, BlockManager.GetBlockByGuid(connection.guid), connection.connectedPins, connection.connectedOn);
                    }
                }
            }

            else
            {
                Debug.Log("No game saved!");
            }
        }

        public void NewSave()
        {
            KeyBoard.SetActive(true);
            newFileName = "";
        }

        public void ChancelNewSaveCreation()
        {
            KeyBoard.SetActive(false);
        }

        public void CreateNewSaveFile()
        {
            KeyBoard.SetActive(false);

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
            FileStream file = File.Create(currentlyChoosenFile);
            bf.Serialize(file, save);
            file.Close();
        }
    }

    
}