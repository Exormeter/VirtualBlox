using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{
    public class BlockSpawn : MonoBehaviour
    {

        public SteamVR_Input_Sources leftHand;
        public SteamVR_Input_Sources righthand;
        public SteamVR_Action_Boolean spawnBlockAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SpawnBlock");
        public PhysicSceneManager manager;

        public GameObject spawnAble;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame     
        void Update()
        {
            if (spawnBlockAction.GetLastStateDown(leftHand) || spawnBlockAction.GetStateDown(righthand)) 
            {
                //GameObject block = Instantiate(spawnAble, new Vector3(0.2f, 0.7f, 0.3f), new Quaternion(0,0,0,0));
                //block.SetActive(true);
                manager.Simulate();
            }
        }
    }

}
