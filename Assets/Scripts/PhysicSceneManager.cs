﻿//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//namespace Valve.VR.InteractionSystem
//{
//    public class PhysicSceneManager : MonoBehaviour
//    {
//        private PhysicsScene physicsScene;
//        private Dictionary<Guid, GameObject> exsitingBlocksInSim = new Dictionary<Guid, GameObject>();
//        private Dictionary<Guid, GameObject> exsitingBlocksInGame = new Dictionary<Guid, GameObject>();
//        //private List<Rigidbody> BlockMovement = new List<Rigidbody>();
//        public int physicSteps;
//        public bool disableRenderer;

//        public int simBlocks;
//        public int inGameBlocks;
//        void Awake()
//        {
//            SceneManager.LoadSceneAsync(1, new LoadSceneParameters()
//            {
//                loadSceneMode = LoadSceneMode.Additive,
//                localPhysicsMode = LocalPhysicsMode.Physics3D

//            });
//            physicsScene = SceneManager.GetSceneByBuildIndex(1).GetPhysicsScene();
//        }

//        private void Update()
//        {
//            simBlocks = exsitingBlocksInSim.Count;
//            inGameBlocks = exsitingBlocksInGame.Count;
//        }

        

//        public void AddGameObjectToPhysicScene(GameObject gameObject)
//        {
//            exsitingBlocksInSim.Add(gameObject.GetComponent<BlockScriptSim>().guid, gameObject);
//            StripGameObject(gameObject);
//            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetSceneByBuildIndex(1));
//        }

//        public void AddGameObjectRefInGame(GameObject gameObject)
//        {
//            exsitingBlocksInGame.Add(gameObject.GetComponent<BlockCommunication>().Guid, gameObject);
//        }

//        private void StripGameObject(GameObject gameObject)
//        {
//            for (int i = 0; i < gameObject.transform.childCount; i++)
//            {
//                Destroy(gameObject.transform.GetChild(i).gameObject);
//            }
//            BlockInteractable blockInteractable = gameObject.GetComponent<BlockInteractable>();
//            if(blockInteractable != null)
//            {
//                Destroy(blockInteractable);
//            }
//            foreach (Component component in gameObject.GetComponents(typeof(Component)))
//            {
//                if (!(component is BlockScriptSim) && !(component is MeshFilter) && !(component is MeshFilter) && !(component is Transform) && !(component is Collider) && !(component is Rigidbody) && !(component is BlockGeometryScript))
//                {
//                    Destroy(component);
//                }
//                if(component is MeshRenderer && disableRenderer)
//                {
//                    Destroy(component);
//                }
//            }
//        }

//        IEnumerator Simulate()
//        {
//            for(int i = 0; i <= physicSteps; i++)
//            {
//                physicsScene.Simulate(Time.fixedDeltaTime);
//                yield return new WaitForFixedUpdate();
//            }

//        }

//        public bool AlreadyExisits(Guid guid)
//        {
//            return exsitingBlocksInSim.ContainsKey(guid);
//        }

        
//        public GameObject GetRealBlockByGuid(Guid guid)
//        {
//            return exsitingBlocksInGame[guid];
//        }

//        public GameObject GetSimBlockByGuid(Guid guid)
//        {
//            return exsitingBlocksInSim[guid];
//        }

//        public void JointBreak(Guid block, Guid connectedBlock)
//        {
//            GameObject realBlock = exsitingBlocksInGame[block];
//            realBlock.GetComponent<BlockCommunication>().RemoveJointViaSimulation(connectedBlock);
//        }

//        internal void StartSimulation()
//        {
//            StartCoroutine(Simulate());
//        }
//    }

//}
