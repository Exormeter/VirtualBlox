using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Valve.VR.InteractionSystem
{
    public class PhysicSceneManager : MonoBehaviour
    {
        private PhysicsScene physicsScene;
        private Dictionary<Guid, GameObject> exsitingBlocksInSim = new Dictionary<Guid, GameObject>();
        private Dictionary<Guid, GameObject> exsitingBlocksInGame = new Dictionary<Guid, GameObject>();
        private List<Rigidbody> BlockMovement = new List<Rigidbody>();
        public int physicSteps = 50;

        public int simBlocks;
        public int inGameBlocks;
        void Awake()
        {
            SceneManager.LoadSceneAsync(1, new LoadSceneParameters()
            {
                loadSceneMode = LoadSceneMode.Additive,
                localPhysicsMode = LocalPhysicsMode.Physics3D

            });
            physicsScene = SceneManager.GetSceneByBuildIndex(1).GetPhysicsScene();
        }

        private void Update()
        {
            simBlocks = exsitingBlocksInSim.Count;
            inGameBlocks = exsitingBlocksInGame.Count;
        }

        

        public void AddGameObjectToPhysicScene(GameObject gameObject)
        {
            exsitingBlocksInSim.Add(gameObject.GetComponent<BlockScriptSim>().guid, gameObject);
            StripGameObject(gameObject);
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetSceneByBuildIndex(1));
        }

        public void AddGameObjectRefInGame(GameObject gameObject)
        {
            exsitingBlocksInGame.Add(gameObject.GetComponent<BlockScript>().guid, gameObject);
        }

        private void StripGameObject(GameObject gameObject)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                Destroy(gameObject.transform.GetChild(i).gameObject);
            }
            BlockInteractable blockInteractable = gameObject.GetComponent<BlockInteractable>();
            if(blockInteractable != null)
            {
                Destroy(blockInteractable);
            }
            foreach (Component component in gameObject.GetComponents(typeof(Component)))
            {
                if (!(component is BlockScriptSim) && !(component is MeshFilter) && !(component is MeshRenderer) && !(component is Transform) && !(component is Collider) && !(component is Rigidbody))
                {
                    Destroy(component);
                }
            }
        }

        IEnumerator Simulate()
        {
            for(int i = 0; i <= physicSteps; i++)
            {
                physicsScene.Simulate(Time.fixedDeltaTime);
                
                yield return new WaitForFixedUpdate();
            }

        }

        public bool AlreadyExisits(Guid guid)
        {
            return exsitingBlocksInSim.ContainsKey(guid);
        }

        public void MatchBlock(GameObject realBlock, Guid guid)
        {
            GameObject simBlock = exsitingBlocksInSim[guid];
            simBlock.GetComponent<BlockScriptSim>().MatchTwinBlock(realBlock);
        }

        public void ConnectBlocks(Guid block, Guid collidedBlock, int jointStrength, OTHER_BLOCK_IS_CONNECTED_ON connectedOn)
        {
            GameObject simBlock = exsitingBlocksInSim[block];
            GameObject simCollidedBlock = exsitingBlocksInSim[collidedBlock];
            simBlock.GetComponent<BlockScriptSim>().AddTempConnection(simBlock, simCollidedBlock, jointStrength, connectedOn);
        }

        public void JointBreak(Guid block, Guid connectedBlock)
        {
            GameObject realBlock = exsitingBlocksInGame[block];
            realBlock.GetComponent<BlockScript>().RemoveJointViaSimulation(connectedBlock);
        }

        internal void StartSimulation()
        {
            StartCoroutine(Simulate());
        }
    }

}
