using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{


    public class BlockGenerator : MonoBehaviour
    {

        public GameObject Block1x1;
        private float heigt = 0.04f;
        // Start is called before the first frame update
        void Start()
        {
            List<GameObject> objects = new List<GameObject>();
            Debug.Log(Block1x1.GetComponent<MeshFilter>().mesh.bounds.ToString("F5"));
            GameObject container = new GameObject();
            objects.Add(Instantiate(Block1x1, new Vector3(heigt, 0, heigt), Quaternion.identity, container.transform));
            objects.Add(Instantiate(Block1x1, new Vector3(-heigt, 0, heigt), Quaternion.identity, container.transform));
            objects.Add(Instantiate(Block1x1, new Vector3(heigt, 0, -heigt), Quaternion.identity, container.transform));
            objects.Add(Instantiate(Block1x1, new Vector3(-heigt, 0, -heigt), Quaternion.identity, container.transform));
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
                meshFilters[i].gameObject.SetActive(false);
            }

            GameObject combinedTile = new GameObject("Floor");
            combinedTile.tag = "Floor";
            combinedTile.AddComponent(typeof(MeshFilter));
            combinedTile.AddComponent(typeof(MeshRenderer));
            combinedTile.AddComponent(typeof(Rigidbody));
            combinedTile.GetComponent<Rigidbody>().isKinematic = true;


            combinedTile.GetComponent<MeshFilter>().mesh = new Mesh();
            combinedTile.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
            //combinedTile.GetComponent<MeshRenderer>().material = this.material;
            combinedTile.AddComponent(typeof(BlockGeometryScript));
            combinedTile.AddComponent(typeof(BlockCommunication));

            //Destroy(container);
            //combinedTile.transform.SetParent(this.transform);
            transform.gameObject.SetActive(true);

        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
