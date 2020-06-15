using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem
{


    public class CreateFloor : MonoBehaviour
    {
        public GameObject floorTile;
        public Material material;
        public int tileCountX;
        public int tileCountY;

        private const int COMBINED_TILE = 10;
        private Vector3 boundExtend;
        private Vector3 startPosition;
        private Mesh combinedMash;
        // Start is called before the first frame update
        void Start()
        {
            Mesh mesh = floorTile.GetComponent<MeshFilter>().mesh;
            Bounds bounds = mesh.bounds;
            this.startPosition = this.transform.position;
            this.boundExtend = bounds.extents;




            for (int i = 0; i < tileCountX; i++)
            {
                for (int j = 0; j < tileCountY; j++)
                {
                    CreateCombinedTile(i, j);
                }
            }
        }

        private GameObject CreateCombinedTile(int row, int column)
        {
            GameObject container = new GameObject("CombinedTile");
            for (int x = row * 10; x < row * 10 + 10; x++)
            {
                for (int y = column * 10; y < column * 10 + 10; y++)
                {
                    CreateTile(container, x, y);
                }
            }
            CombineTileMeshes(container);
            return container;
        }

        private void CreateTile(GameObject container, int x, int y)
        {
            float xCoord = (this.startPosition.x + this.boundExtend.x * y * 2);
            float yCoord = this.startPosition.y;
            float zCoord = (this.startPosition.z + this.boundExtend.z * x * 2);

            Vector3 newPosition = new Vector3(xCoord, yCoord, zCoord);
            GameObject newTile = Instantiate(floorTile, (newPosition), new Quaternion(0, 0, 0, 0));
            newTile.transform.SetParent(container.transform);
        }

        private void CombineTileMeshes(GameObject container)
        {
            MeshFilter[] meshFilters = container.GetComponentsInChildren<MeshFilter>();

            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            for (int i = 0; i < meshFilters.Length; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                //meshFilters[i].gameObject.SetActive(false);
                Destroy(meshFilters[i].gameObject.GetComponent<MeshFilter>());
                Destroy(meshFilters[i].gameObject.GetComponent<MeshRenderer>());
            }

            GameObject combinedTile = new GameObject("Floor");
            combinedTile.tag = "Floor";
            combinedTile.AddComponent(typeof(MeshFilter));
            combinedTile.AddComponent(typeof(MeshRenderer));
            combinedTile.AddComponent(typeof(Rigidbody));
            combinedTile.GetComponent<Rigidbody>().isKinematic = true;


            combinedTile.GetComponent<MeshFilter>().mesh = new Mesh();
            combinedTile.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
            combinedTile.GetComponent<MeshRenderer>().material = this.material;
            combinedTile.AddComponent<BlockGeometryFloor>();
            combinedTile.AddComponent<BlockCommunication>();
            combinedTile.GetComponent<BlockCommunication>().frameUntilColliderReEvaluation = 2;
            combinedTile.GetComponent<BlockCommunication>().breakForcePerPin = 2;
            combinedTile.AddComponent<AttachFloorHandler>();
            combinedTile.AddComponent<AttachHandHandler>();
            combinedTile.isStatic = true;

            //FloorTiles schould get the same Guid on every startup
            GameObject[] floorTiles = GameObject.FindGameObjectsWithTag("Floor");
            combinedTile.GetComponent<BlockCommunication>().Guid = CreateNewGuid(floorTiles.Length);
            combinedTile.GetComponent<BlockGeometryFloor>().SetBlockWalkable(true);
            Destroy(container);
            transform.gameObject.SetActive(true);
        }

        private Guid CreateNewGuid(int length)
        {
            string baseString = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
            baseString = baseString + length;
            int paddingLeft = baseString.Length - 32;
            string GuidString = baseString.Remove(0, paddingLeft);
            return new Guid(GuidString);
        }
    }
}
