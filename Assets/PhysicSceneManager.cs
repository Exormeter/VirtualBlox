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
        private Dictionary<Guid, GameObject> exsitingBlocks = new Dictionary<Guid, GameObject>();
        void Awake()
        {
            SceneManager.LoadSceneAsync(1, new LoadSceneParameters()
            {
                loadSceneMode = LoadSceneMode.Additive,
                localPhysicsMode = LocalPhysicsMode.Physics3D

            });
            physicsScene = SceneManager.GetSceneByBuildIndex(1).GetPhysicsScene();
        }

        public void AddGameObjectToScene(GameObject gameObject)
        {
            exsitingBlocks.Add(gameObject.GetComponent<BlockScript>().guid, gameObject);
            StripGameObject(gameObject);
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetSceneByBuildIndex(1));
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
                if (!(component is BlockScript) && !(component is Rigidbody) && !(component is MeshFilter) && !(component is MeshRenderer) && !(component is Transform))
                {
                    Destroy(component);
                }
            }
        }

        internal void Simulate()
        {
            physicsScene.Simulate(Time.fixedDeltaTime);
        }

        public bool AlreadyExisits(Guid guid)
        {
            return exsitingBlocks.ContainsKey(guid);
        }

        public void MatchBlock(GameObject realBlock, Guid guid)
        {
            GameObject simBlock = exsitingBlocks[guid];
            simBlock.GetComponent<Rigidbody>().isKinematic = false;
            simBlock.transform.SetPositionAndRotation(realBlock.transform.position, realBlock.transform.rotation);
            simBlock.GetComponent<Rigidbody>().isKinematic = true;
        }

        public void ConnectBlocks(GameObject gameObject, GameObject collidedBlock, int v)
        {
            
        }
    }

}
