using System;
using System.Collections;
using System.Collections.Generic;
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
        public List<Button> buttons;

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
                    buttons.Add(newButton.GetComponent<Button>());
                }
            }
            KeyBoard.SetActive(false);
        }

        public void GetFileName(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            currentlyChoosenFile = filePath;
            ChoosenFile.text = fileName;
        }

        public void ButtonListen(string keyBoardPress)
        {
            newFileName += keyBoardPress;
            ChoosenFile.text += keyBoardPress;
        }

        public void LoadGame()
        {
            
            if (File.Exists(currentlyChoosenFile))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(currentlyChoosenFile, FileMode.Open);
                SaveGame save = (SaveGame)bf.Deserialize(file);
                file.Close();

                BlockManager.RemoveAllBlocks();

                //Load Blocks into the Scene
                StartCoroutine(LoadBlocks(save));
            }
        }

        IEnumerator LoadBlocks(SaveGame save)
        {
            foreach (BlockSave blockSave in save.blockSaves)
            {
                GameObject restoredBlock = BlockGenerator.GenerateBlock(blockSave.GetBlockStructure());
                restoredBlock.transform.position = blockSave.position;
                restoredBlock.transform.rotation = blockSave.rotation;
                restoredBlock.GetComponent<BlockCommunication>().Guid = blockSave.guid;
                restoredBlock.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                yield return new WaitForSeconds(0.5f);
            }
            ConnectedBlocks(save);
        }

        public void ConnectedBlocks(SaveGame save)
        {
            foreach (BlockSave blockSave in save.blockSaves)
            {
                GameObject block = BlockManager.GetBlockByGuid(blockSave.guid);
                foreach (ConnectedBlockSerialized connection in blockSave.connectedBlocks)
                {
                    block.GetComponent<BlockCommunication>().ConnectBlocks(block, BlockManager.GetBlockByGuid(connection.guid), connection.connectedPins, connection.connectedOn);
                }
            }
        }

        public void NewSave()
        {
            KeyBoard.SetActive(true);
            newFileName = "";
            ChoosenFile.text = "";
            DisableButtons();
        }

        public void ChancelNewSaveCreation()
        {
            KeyBoard.SetActive(false);
            newFileName = "";
            EnableButtons();
        }

        public void CreateNewSaveFile()
        {
            KeyBoard.SetActive(false);
            SaveGame(newFileName);

            GameObject newButton = Instantiate(button) as GameObject;
            newButton.GetComponentInChildren<Text>().text = newFileName + ".save";
            newButton.transform.SetParent(ButtonListContent.transform, false);
            newButton.GetComponent<Button>().onClick.AddListener(() => GetFileName(Application.persistentDataPath + "/" + newFileName + ".save"));
            currentlyChoosenFile = Application.persistentDataPath + "/" + newFileName + ".save";
            ChoosenFile.text += ".save";
            EnableButtons();
        }

        public void DisableButtons()
        {
            buttons.ForEach(button => button.interactable = false);
        }

        public void EnableButtons()
        {
            buttons.ForEach(button => button.interactable = true);
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
            newSaveGame.historyObjects = BlockManager.blockPlacingHistory;
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

        public void SaveGame(string fileName)
        {
            SaveGame save = CreateSaveGameObject();
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + "/" + fileName + ".save");
            bf.Serialize(file, save);
            file.Close();
        }
    }

    
}