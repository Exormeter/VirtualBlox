using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{
    public class BlockManager : MonoBehaviour
    {

        private Dictionary<Guid, GameObject> exsitingBlocksInGame = new Dictionary<Guid, GameObject>();


        public GameObject GetBlockByGuid(Guid guid)
        {
            return exsitingBlocksInGame[guid];
        }

        public void AddBlock(Guid guid, GameObject block)
        {
            exsitingBlocksInGame.Add(guid, block);
        }

        public void ChangeGuid(Guid oldGuid, Guid newGuid, GameObject block)
        {
            exsitingBlocksInGame.Remove(oldGuid);
            exsitingBlocksInGame.Add(newGuid, block);
        }
        
    }
}


