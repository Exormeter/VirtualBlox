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

        /// <summary>
        /// Point where to move the loaded prefab
        /// </summary>
        public GameObject PrefabAttachPoint;

        /// <summary>
        /// Rotation speed of the prefab
        /// </summary>
        public float RotationSpeed;

        public float BlockPlaceInterval;
        /// <summary>
        /// Currently loaded prefab
        /// </summary>
        private  GameObject currentParentBlock;

        /// <summary>
        /// Loads scene from a file
        /// </summary>
        /// <param name="choosenFilePath">The path to the scene file</param>
        public void LoadSceneFromFile(string choosenFilePath)
        {

            if (File.Exists(choosenFilePath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(choosenFilePath, FileMode.Open);
                SaveGame save = (SaveGame)bf.Deserialize(file);
                file.Close();

                if(save == null || save.blockSaves.Count == 0)
                {
                    return;
                }
                BlockManager.RemoveAllBlocks();

                //Load Blocks into the Scene
                StartCoroutine(LoadScene(save));
            }
        }

        private void Update()
        {
            if(PrefabAttachPoint.transform.childCount > 0)
            {
                currentParentBlock.transform.Rotate(Vector3.up * (RotationSpeed * Time.deltaTime));
            }
        }

        /// <summary>
        /// Loads the scene from a save file
        /// </summary>
        /// <param name="save">The save Object to load</param>
        /// <returns></returns>
        IEnumerator LoadScene(SaveGame save)
        {
            //Sort after time of block placement
            save.historyObjects.Sort();
            BlockManager.blockPlacingHistory = save.historyObjects;

            List<GameObject> floorBlocks = BlockManager.GetFloorBlocks();
            //Prepare Floor Blocks for connection
            foreach (GameObject floorBlock in floorBlocks)
            {
                floorBlock.GetComponentInChildren<TapHandler>().AcceptCollisionsAsConnected(true);
                floorBlock.GetComponentInChildren<GrooveHandler>().AcceptCollisionsAsConnected(true);
            }

            //Load block in sequen of first placement
            for (int i = 0; i < save.historyObjects.Count; i++)
            {
                //Skip if the currently loaded Block already exsist in scene
                if(!BlockManager.BlockExists(save.historyObjects[i].guid))
                {
                    //Load the Block into the scene
                    LoadBlock(save, save.historyObjects[i]);

                    //Load all Block that were placed together with currently loaded Block
                    while (i + 1 < save.historyObjects.Count && save.historyObjects[i].timeStamp == save.historyObjects[i + 1].timeStamp)
                    {
                        LoadBlock(save, save.historyObjects[i + 1]);
                        i++;
                    }
                }

                yield return new WaitForSeconds(BlockPlaceInterval);
            }

            //Connect the Blocks together
            ConnectBlocks(save);

            foreach (GameObject floorBlock in floorBlocks)
            {
                floorBlock.GetComponentInChildren<TapHandler>().AcceptCollisionsAsConnected(false);
                floorBlock.GetComponentInChildren<GrooveHandler>().AcceptCollisionsAsConnected(false);
            }
        }

        /// <summary>
        /// Stub Method to unpack the BlockSaves in SaveGame and connect them
        /// </summary>
        /// <param name="saveGame">The SaveGame to connect</param>
        public void ConnectBlocks(SaveGame saveGame)
        {
            foreach (BlockSave blockSave in saveGame.blockSaves)
            {
                ConnectBlocks(blockSave, saveGame);
            }
        }

        /// <summary>
        /// Connect all Blocks together, information for which Block to connect is in BlockSave, also resets AcceptCollisionsAsConnected
        /// to false again, as the physical connection makeing is finshed.
        /// </summary>
        /// <param name="blockSave">Holds the connections</param>
        /// <param name="saveGame">Holds guids</param>
        public void ConnectBlocks(BlockSave blockSave, SaveGame saveGame = null)
        {
            GameObject block = BlockManager.GetBlockByGuid(blockSave.guid);

            //Reset the Groove- and Tap Handler
            block.GetComponentInChildren<TapHandler>().AcceptCollisionsAsConnected(false);
            block.GetComponentInChildren<GrooveHandler>().AcceptCollisionsAsConnected(false);

            foreach (ConnectedBlockSerialized connection in blockSave.connectedBlocks)
            {
                //Only connect the Block if no SaveGame was provided or if the other Block to connect to is found in the SaveGame.
                //This is to prevent Prefabs from connecting to non-Prefab Blocks when loaded
                //Floorplates are not included in the Savegame, so they are check manually by the Guid
                if(saveGame == null || saveGame.GetBlockSaveByGuid(connection.guid) != null || connection.guid.ToString().StartsWith("aaaaaaaa"))
                {
                    block.GetComponent<BlockCommunication>().ConnectBlocks(block, BlockManager.GetBlockByGuid(connection.guid), connection.connectedPins, connection.connectedOn);
                }
                
            }
        }

        /// <summary>
        /// Loads a Prefab from a file
        /// </summary>
        /// <param name="choosenFilePath">The path to the scene file</param>
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

        /// <summary>
        /// Loads a Prefab from a save file
        /// </summary>
        /// <param name="saveGame">The save game to load</param>
        /// <returns></returns>
        IEnumerator LoadPrefab(SaveGame saveGame)
        {
            //Only one prefab should be loaded at a time that wasn't grabbed by the user
            if (PrefabAttachPoint.transform.childCount > 0)
            {
                Destroy(currentParentBlock);
            }

            //Loaded Block in the prefab
            List<GameObject> loadedBlocks = new List<GameObject>();

            //The prefab Blocks need new Guids so multile prefabs can be loaded into the scene
            Dictionary<Guid, Guid> originalToNewGuid = new Dictionary<Guid, Guid>();

            //Load all Blocks in prefab, move with offset form original placement, set Groove- Tap handler
            //to accept Collisions as connected so Blocks connect physical on contact
            foreach(BlockSave blockSave in saveGame.blockSaves)
            {
                GameObject loadedBlock = LoadBlockWithNewGuid(blockSave, new Vector3(3, 3, 3));
                originalToNewGuid.Add(blockSave.guid, loadedBlock.GetComponent<BlockCommunication>().Guid);
                loadedBlocks.Add(loadedBlock);
                yield return new WaitForSeconds(0.1f);
            }

            //Replace old guids with new ones
            saveGame.ReplaceGuids(originalToNewGuid);

            saveGame.RemoveFloorConnections();

            //Connect the Blocks
            ConnectBlocks(saveGame);

            //Set the prefab as the currently loaded one. Parent them so they can easly moved to the
            //AttachPoint and set them kinematic as they shouldn't interfere with the scene yet
            GameObject parentBlock = loadedBlocks[0];
            foreach (GameObject loadedBlock in loadedBlocks)
            {
                loadedBlock.GetComponentInChildren<TapHandler>().AcceptCollisionsAsConnected(false);
                loadedBlock.GetComponentInChildren<GrooveHandler>().AcceptCollisionsAsConnected(false);
                loadedBlock.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                loadedBlock.GetComponent<Rigidbody>().isKinematic = true;
                loadedBlock.transform.SetParent(parentBlock.transform);
            }
            parentBlock.transform.position = PrefabAttachPoint.transform.position;
            parentBlock.transform.SetParent(PrefabAttachPoint.transform);
            
            currentParentBlock = parentBlock;
        }
        
        /// <summary>
        /// Save a SaveGame Object to file
        /// </summary>
        /// <param name="save">The SaveGame to persist</param>
        /// <param name="filePath">The path where to save the file</param>
        public void SaveGame(SaveGame save, string filePath)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(filePath);
            bf.Serialize(file, save);
            file.Close();
        }

        /// <summary>
        /// Loads a BlockSave from a given HistoryObject and SaveGame
        /// </summary>
        /// <param name="save">The SaveGame containing the BlockSave</param>
        /// <param name="historyObject">The HistoryObject to load</param>
        public void LoadBlock(SaveGame save, HistoryObject historyObject)
        {
            BlockSave blockSave = save.GetBlockSaveByGuid(historyObject.guid);
            LoadBlock(blockSave);
        }

        /// <summary>
        /// Loads a Block from a BlockSave and freezes the Block, Block can be loaded with offset
        /// </summary>
        /// <param name="blockSave">The BlockSave to load</param>
        /// <param name="offset">Optionak offset</param>
        /// <returns></returns>
        public GameObject LoadBlock(BlockSave blockSave, Vector3 offset = new Vector3())
        {
            GameObject restoredBlock = BlockGenerator.GenerateBlock(blockSave.GetBlockStructure());
            restoredBlock.transform.position = blockSave.position + offset;
            restoredBlock.transform.rotation = blockSave.rotation;
            restoredBlock.GetComponent<BlockCommunication>().Guid = blockSave.guid;
            restoredBlock.GetComponent<BlockGeometryScript>().TopColliderContainer.layer = 8;
            restoredBlock.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            restoredBlock.GetComponentInChildren<TapHandler>().AcceptCollisionsAsConnected(true);
            restoredBlock.GetComponentInChildren<GrooveHandler>().AcceptCollisionsAsConnected(true);
            return restoredBlock;
        }

        /// <summary>
        /// Loaded a Block from an BlockSave, but with a new Guid. Block can be loaded with an offset. Used for loading prefabs
        /// </summary>
        /// <param name="blockSave">The BlockSave to load</param>
        /// <param name="offset">Optional offset</param>
        /// <returns></returns>
        public GameObject LoadBlockWithNewGuid(BlockSave blockSave, Vector3 offset = new Vector3())
        {
            GameObject restoredBlock = BlockGenerator.GenerateBlock(blockSave.GetBlockStructure());
            restoredBlock.transform.position = blockSave.position + offset;
            restoredBlock.transform.rotation = blockSave.rotation;
            restoredBlock.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            restoredBlock.GetComponentInChildren<TapHandler>().AcceptCollisionsAsConnected(true);
            restoredBlock.GetComponentInChildren<GrooveHandler>().AcceptCollisionsAsConnected(true);
            return restoredBlock;
        }
    }

}
