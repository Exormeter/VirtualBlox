using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;


namespace Valve.VR.InteractionSystem
{
    public class BlockManager : MonoBehaviour
    {

        public List<BlockScript> currentBlocksInGame = new List<BlockScript>();
        // Start is called before the first frame update
        void Start()
        {
            
        }
 
        

        public void AddBlock(BlockScript blockScript)
        {
            currentBlocksInGame.Add(blockScript);
        }
    }
}


