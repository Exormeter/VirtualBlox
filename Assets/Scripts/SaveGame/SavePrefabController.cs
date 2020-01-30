using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            SaveGameManager.SaveGame(CreateSaveGameObject(), newFileName);

            GameObject newButton = Instantiate(ListButtonPrecurser) as GameObject;
            newButton.GetComponentInChildren<Text>().text = newFileName + extension;
            newButton.transform.SetParent(ButtonListContent.transform, false);
            newButton.GetComponent<Button>().onClick.AddListener(() => SetCurrentFile(new SaveFilePath(newFileName, extension)));
            SetCurrentFile(new SaveFilePath(newFileName, extension));
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


        public SaveGame CreateSaveGameObject()
        {
            SaveGame newSaveGame = new SaveGame();
            List<GameObject> blocks = BlockMarker.markedBlocks;
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
            foreach (GameObject block in markedBlocks)
            {

            }

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
            FilePath = Application.persistentDataPath + fileName + Extension;
            Debug.Log(FilePath);
        }
    }
}

