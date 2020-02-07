using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem
{
    public class SavePrefabController : MonoBehaviour
    {
        public GameObject KeyBoard;
        public BlockMarker BlockMarker;
        public BlockGenerator BlockGenerator;
        public SaveGameManager SaveGameManager;
        public GameObject ListButtonPrecurser;
        public GameObject ButtonListContent;
        public Text DisplayedFile;
        public List<Button> buttons;
        public Hand HandToPrefabAttach;

        private SaveFilePath currentlyChoosenFile;

        public SaveFilePath CurrentlyChoosenFile
        {
            get
            {
                return currentlyChoosenFile;
            }
            set
            {
                currentlyChoosenFile = value;
                if (value == null)
                {
                    DisplayedFile.text = "";
                }
                else
                {
                    DisplayedFile.text = value.FileName + value.Extension;
                }

            }
        }

        private readonly string extension = ".structure";
        private string newFileName;

        void Start()
        {
            string[] filePaths = Directory.GetFiles(Application.persistentDataPath);
            foreach (string filePath in filePaths)
            {
                if (File.Exists(filePath) && Path.GetExtension(filePath).Equals(extension))
                {
                    GameObject newButton = Instantiate(ListButtonPrecurser) as GameObject;
                    newButton.GetComponentInChildren<Text>().text = Path.GetFileName(filePath);
                    newButton.transform.SetParent(ButtonListContent.transform, false);
                    newButton.GetComponent<Button>().onClick.AddListener(() => SetCurrentFile(new SaveFilePath(filePath)));
                    buttons.Add(newButton.GetComponent<Button>());
                }
            }
            KeyBoard.SetActive(false);
        }

        public void SetCurrentFile(SaveFilePath saveFilePath)
        {
            CurrentlyChoosenFile = saveFilePath;
        }

        void Update()
        {

        }

        public void KeyBoardListner(string keyBoardPress)
        {
            newFileName += keyBoardPress;
            DisplayedFile.text += keyBoardPress;
        }

        public void NewSave()
        {
            KeyBoard.SetActive(true);
            newFileName = "";
            DisplayedFile.text = "";
            DisableButtons();
        }

        public void ChancelNewSaveCreation()
        {
            KeyBoard.SetActive(false);
            newFileName = "";
            SetCurrentFile(null);
            EnableButtons();
        }

        public void CreateNewSaveFile()
        {
            if (newFileName.Equals(""))
            {
                return;
            }
           
            KeyBoard.SetActive(false);

            SaveFilePath saveFilePath = new SaveFilePath(newFileName, extension);

            SaveGameManager.SaveGame(CreateSaveGameObject(), saveFilePath.FilePath);


            GameObject newButton = Instantiate(ListButtonPrecurser) as GameObject;
            newButton.GetComponentInChildren<Text>().text = newFileName + extension;
            newButton.transform.SetParent(ButtonListContent.transform, false);
            newButton.GetComponent<Button>().onClick.AddListener(() => SetCurrentFile(saveFilePath));
            SetCurrentFile(saveFilePath);
            buttons.Add(newButton.GetComponent<Button>());
            EnableButtons();
        }

        public void OverrideSaveFile()
        {
            if (CurrentlyChoosenFile == null)
            {
                return;
            }
            SaveGameManager.SaveGame(CreateSaveGameObject(), CurrentlyChoosenFile.FilePath);
        }

        public void LoadSavedPrefab()
        {
            if(CurrentlyChoosenFile != null)
            {
                SaveGameManager.LoadPrefabFromFile(CurrentlyChoosenFile.FilePath);
            }
        }


        public SaveGame CreateSaveGameObject()
        {
            SaveGame newSaveGame = new SaveGame();
            List<GameObject> blocks = SearchBiggestConnectedStructure(BlockMarker.markedBlocks.Distinct().ToList());

            foreach (GameObject block in blocks)
            {
                if (block.GetComponent<BlockCommunication>().IsIndirectlyAttachedToFloor())
                {
                    newSaveGame.blockSaves.Add(new BlockSave(block));
                }
            }
            return newSaveGame;
        }

        public void DisableButtons()
        {
            buttons.ForEach(button => button.interactable = false);
        }

        public void EnableButtons()
        {
            buttons.ForEach(button => button.interactable = true);
        }

        public List<GameObject> SearchBiggestConnectedStructure(List<GameObject> markedBlocks)
        {
            if(markedBlocks.Count <= 1)
            {
                return markedBlocks;
            }

            Dictionary<GameObject, int> numberBlockConnections = new Dictionary<GameObject, int>();
            markedBlocks.ForEach(block => numberBlockConnections.Add(block, 0));

            foreach (GameObject block in markedBlocks)
            {
                foreach(GameObject otherBlock in markedBlocks)
                {
                    if(block.GetHashCode() != otherBlock.GetHashCode() && block.GetComponent<BlockCommunication>().IsIndirectlyAttachedToBlockMarked(otherBlock))
                    {
                        numberBlockConnections[block]++;
                    }
                }

                if(numberBlockConnections[block] == markedBlocks.Count - 1)
                {
                    return markedBlocks;
                }
            }

            int highestNumberConnections = 0;
            foreach(GameObject block in markedBlocks)
            {
                if(numberBlockConnections[block] > highestNumberConnections)
                {
                    highestNumberConnections = numberBlockConnections[block];
                }
            }

            List<GameObject> biggestStructure = new List<GameObject>();
            foreach(GameObject block in markedBlocks)
            {
                if(numberBlockConnections[block] == highestNumberConnections)
                {
                    biggestStructure.Add(block);
                }
            }
            return biggestStructure;
        }
    }

    public class SaveFilePath
    {
        public string FilePath;
        public string Extension;
        public string FileName;

        public SaveFilePath(string filePath)
        {
            FilePath = filePath;
            Extension = Path.GetExtension(filePath);
            FileName = Path.GetFileNameWithoutExtension(filePath);
        }

        public SaveFilePath(string fileName, string extension)
        {
            Extension = extension;
            FileName = fileName;
            FilePath = Application.persistentDataPath +"/" + fileName + Extension;
            Debug.Log(FilePath);
        }
    }
}

