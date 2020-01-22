using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Valve.VR.InteractionSystem
{


    public class BlockGenerator : MonoBehaviour
    {

        public GameObject Block1x1;
        public GameObject Block1x1Flat;
        public GameObject PrecurserBlock;
        public Material material;


        private Dictionary<BLOCKSIZE, GameObject> partSizes = new Dictionary<BLOCKSIZE, GameObject>();
        private readonly float length = 0.08f;

        // Start is called before the first frame update
        void Start()
        {
            partSizes.Add(BLOCKSIZE.FLAT, Block1x1Flat);
            partSizes.Add(BLOCKSIZE.NORMAL, Block1x1);
        }

        public GameObject GenerateBlock(BlockStructure structure)
        {

            Material blockMaterial = new Material(material);
            blockMaterial.color = structure.BlockColor;
            GameObject container = new GameObject();
            structure.GetCroppedMatrix();
            float rowMiddlePoint = (float) (structure.RowsCropped - 1) / 2;
            float colMiddlePoint = (float) (structure.ColsCropped - 1) / 2;
            for (int row = 0; row < structure.RowsCropped; row++)
            {
                for (int col = 0; col < structure.ColsCropped; col++)
                {
                    if (structure[row, col] != null)
                    {

                        Vector3 partPosition = new Vector3((rowMiddlePoint - row) * length, 0, (colMiddlePoint - col) * length);
                        GameObject blockPart = Instantiate(partSizes[structure.BlockSize], partPosition, Quaternion.identity, container.transform);
                        blockPart.SetActive(true);
                    }

                }
            }
            GameObject newBlock = CombineTileMeshes(container);
            newBlock.AddComponent<BlockGeometryScript>().SetStructure(structure);
            newBlock.GetComponent<MeshRenderer>().material = blockMaterial;
            newBlock.transform.position = new Vector3(0, 2, 0);
            newBlock.tag = "Block";
            AddPrecurserComponents(newBlock);
            return newBlock;
        }

        

        

        private GameObject CombineTileMeshes(GameObject container)
        {
            MeshFilter[] meshFilters = container.GetComponentsInChildren<MeshFilter>();

            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            for (int i = 0; i < meshFilters.Length; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            }

            GameObject combinedBlock = new GameObject("Block");
            combinedBlock.AddComponent(typeof(MeshFilter));
            combinedBlock.AddComponent(typeof(MeshRenderer));
            combinedBlock.GetComponent<MeshFilter>().mesh = new Mesh();
            combinedBlock.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);

            Destroy(container);
            return combinedBlock;

        }

        private void AddPrecurserComponents(GameObject combinedBlock)
        {
            Component[] components = PrecurserBlock.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if(!(components[i] is MeshRenderer) && !(components[i] is Transform) && (components[i] is MeshFilter))
                CopyComponent(components[i], combinedBlock);
            }

        }

        private Component CopyComponent(Component original, GameObject destination)
        {
            Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            // Copied fields can be restricted with BindingFlags
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));

            }
            return copy;
        }

    }

    

    public class BlockPart{

        public int Row;
        public int Col;

        public HashSet<DIRECTION> visitedDirections = new HashSet<DIRECTION>();
        public DIRECTION WallDirection;

        public BlockPart(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public BlockPart(int row, int col, DIRECTION wallDirection)
        {
            WallDirection = wallDirection;
            Row = row;
            Col = col;
        }

        public void ResetVisited()
        {
            visitedDirections.Clear();
        }

        public void DirectionVisited(DIRECTION direction)
        {
            visitedDirections.Add(direction);
        }

        public bool WasDirectionVisited(DIRECTION direction)
        {
            return visitedDirections.Contains(direction);
        }
    }

    public enum DIRECTION
    {
        LEFT,
        RIGHT,
        UP,
        DOWN
    }

    public enum BLOCKSIZE
    {
        NORMAL,
        FLAT
    }

    public enum BLOCKCOLOR
    {
        RED,
        GREEN,
        BLUE,
        YELLOW,
        ORANGE
    }

}
