using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{


    public class BlockGenerator : MonoBehaviour
    {

        public GameObject Block1x1;
        public Canvas canvas;
        public GameObject Toggle;
        public SteamVR_Input_Sources leftHand;
        public SteamVR_Input_Sources righthand;
        public SteamVR_Action_Boolean spawnBlockAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SpawnBlock");
        private List<List<GameObject>> matrix = new List<List<GameObject>>();
        public int Rows;
        public int Columns;
        private readonly float heigt = 0.08f;

        // Start is called before the first frame update
        void Start()
        {
            for(int r = 0; r < Rows; r++)
            {
                matrix.Add(new List<GameObject>());
                for(int c = 0; c < Columns; c++)
                {
                    
                    GameObject toggle = Instantiate(Toggle, canvas.transform, true);
                    RectTransform rectTransfrom = toggle.GetComponent<RectTransform>();
                    rectTransfrom.localScale = new Vector3(1, 1, 1);
                    Vector3 anchorPosition = new Vector3(r * rectTransfrom.sizeDelta.x, - c * rectTransfrom.sizeDelta.y, 0);

                    toggle.GetComponent<RectTransform>().anchoredPosition = anchorPosition;
                    matrix[r].Add(toggle);
                }
            }
            
        }

        void Update()
        {
            if (spawnBlockAction.GetLastStateDown(leftHand) || spawnBlockAction.GetStateDown(righthand))
            {
                //GameObject block = Instantiate(spawnAble, transform.position, new Quaternion(0, 0, 0, 0));
                //block.SetActive(true);
                GenerateBlock();
            }
        }

        private void GenerateBlock()
        {
            List<GameObject> objects = new List<GameObject>();
            GameObject container = new GameObject();
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    Toggle toggle = matrix[r][c].GetComponent<Toggle>();
                    if (toggle.isOn)
                    {
                        GameObject blockPart = Instantiate(Block1x1, new Vector3(r * heigt, 0, c * heigt), Quaternion.identity, container.transform);
                        blockPart.SetActive(true);
                        objects.Add(blockPart);
                    }
                }
            }
            CombineTileMeshes(container);
        }

        private void CombineTileMeshes(GameObject container)
        {
            MeshFilter[] meshFilters = container.GetComponentsInChildren<MeshFilter>();

            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            for (int i = 0; i < meshFilters.Length; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            }

            GameObject combinedTile = new GameObject("Block");
            combinedTile.AddComponent(typeof(MeshFilter));
            combinedTile.AddComponent(typeof(MeshRenderer));
            combinedTile.AddComponent(typeof(Rigidbody));
            combinedTile.GetComponent<Rigidbody>().isKinematic = true;


            combinedTile.GetComponent<MeshFilter>().mesh = new Mesh();
            combinedTile.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
            //combinedTile.GetComponent<MeshRenderer>().material = this.material;
            combinedTile.AddComponent(typeof(BlockGeometryScript));
            combinedTile.AddComponent(typeof(BlockCommunication));
            Destroy(container); 
        }

    }
}
