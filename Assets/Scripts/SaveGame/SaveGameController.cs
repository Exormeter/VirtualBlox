using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem
{
    public class SaveGameController : MonoBehaviour
    {
        public BlockGenerator BlockGenerator;
        public BlockManager BlockManager;
        public SaveGameManager SaveGameManager;
        public GameObject ListButtonPrecurser;
        public GameObject ButtonListContent;
        public GameObject KeyBoard;
        public Text DisplayedFile;
        public List<Button> buttons;

        private SaveFilePath currentlyChoosenFile;
        private bool wasInitialized = false;
        public SaveFilePath CurrentlyChoosenFile
        {
            get
            {
                return currentlyChoosenFile;
            }
            set
            {
                currentlyChoosenFile = value;
                if(value == null)
                {
                    DisplayedFile.text = "";
                }
                else
                {
                    DisplayedFile.text = value.FileName + value.Extension;
                }
                
            }
        }

        private string newFileName = "";
        private readonly string extension = ".save";

        void Start()
        {
            if (!wasInitialized)
            {
                Initialize();
            }
        }

        private void OnEnable()
        {
            if (!wasInitialized)
            {
                Initialize();
            }
        }

        private void Initialize()
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
            wasInitialized = true;
        }

        public void SetCurrentFile(SaveFilePath saveFilePath)
        {
            CurrentlyChoosenFile = saveFilePath;
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
            EnableButtons();
        }

        public void OverrideSaveFile()
        {
            if(CurrentlyChoosenFile == null)
            {
                return;
            }
            SaveGameManager.SaveGame(CreateSaveGameObject(), CurrentlyChoosenFile.FilePath);
        }

        public void ResetGame()
        {
            BlockManager.RemoveAllBlocks();
        }

        public void DisableButtons()
        {
            buttons.ForEach(button =>
            {
                if (button != null)
                {
                    button.interactable = false;
                }
            });
                
            
        }

        public void EnableButtons()
        {
            buttons.ForEach(button =>
            {
                if (button != null)
                {
                    button.interactable = true;
                }
            });
        }

        public void LoadSaveGame()
        {
            if(currentlyChoosenFile == null)
            {
                return;
            }
            SaveGameManager.LoadSceneFromFile(currentlyChoosenFile.FilePath);
        }

        public SaveGame CreateSaveGameObject()
        {
            SaveGame newSaveGame = new SaveGame();
            GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
            foreach (GameObject block in blocks)
            {
                if (block.GetComponent<BlockCommunication>().IsIndirectlyAttachedToFloor())
                {
                    newSaveGame.blockSaves.Add(new BlockSave(block));
                }
            }
            newSaveGame.historyObjects = BlockManager.blockPlacingHistory;
            return newSaveGame;
        }



    }

    
}