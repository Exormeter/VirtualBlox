using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
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
        public Canvas canvas;
        public GameObject Toggle;
        public SteamVR_Input_Sources leftHand;
        public SteamVR_Input_Sources righthand;
        public SteamVR_Action_Boolean spawnBlockAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SpawnBlock");
        public int Rows;
        public int Columns;

        private List<List<GameObject>> matrix = new List<List<GameObject>>();
        private GameObject currentlyChoosenBlock;
        private bool currentlyChoosenFlat = true;
        private readonly float length = 0.08f;

        // Start is called before the first frame update
        void Start()
        {
            currentlyChoosenBlock = Block1x1Flat;
            for (int r = 0; r < Rows; r++)
            {
                matrix.Add(new List<GameObject>());
                for (int c = 0; c < Columns; c++)
                {

                    GameObject toggle = Instantiate(Toggle, canvas.transform, true);
                    RectTransform rectTransfrom = toggle.GetComponent<RectTransform>();
                    rectTransfrom.localScale = new Vector3(2, 2, 1);

                    Vector3 anchorPosition = new Vector3(c * rectTransfrom.sizeDelta.x, -r * rectTransfrom.sizeDelta.y, 0);

                    toggle.GetComponent<RectTransform>().anchoredPosition = anchorPosition;
                    matrix[r].Add(toggle);
                }
            }

        }

        void Update()
        {
            if (spawnBlockAction.GetLastStateDown(leftHand) || spawnBlockAction.GetStateDown(righthand))
            {

                List<BlockStructure> blockStructures = FindStructures();
                foreach (BlockStructure blockStructure in blockStructures)
                {
                    GenerateBlock(blockStructure);
                }
            }
            if (Input.GetKeyUp("space"))
            {
                Debug.Log("test");
                List<BlockStructure> blockStructures = FindStructures();
                foreach (BlockStructure blockStructure in blockStructures)
                {
                    GenerateBlock(blockStructure);
                }
                
            }
        }

        private void GenerateBlock(BlockStructure structure)
        {
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
                        GameObject blockPart = Instantiate(currentlyChoosenBlock, partPosition, Quaternion.identity, container.transform);
                        blockPart.SetActive(true);
                    }

                }
            }
            GameObject newBlock = CombineTileMeshes(container);
            newBlock.AddComponent<BlockGeometryScript>().SetStructure(structure);
            newBlock.transform.position = new Vector3(0, 2, 0);
            AddPrecurserComponents(newBlock);
            
        }

        private List<BlockStructure> FindStructures()
        {
            List<BlockStructure> blockStructures = new List<BlockStructure>();
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    Toggle toggle = matrix[row][col].GetComponent<Toggle>();
                    if (toggle.isOn && !matrix[row][col].tag.Equals("Visited"))
                    {
                        BlockStructure structure = new BlockStructure(Rows, Columns);
                        FindAdjacentNodes(structure, row, col);
                        blockStructures.Add(structure);
                    }
                }
            }
            ResetMatrix();
            return blockStructures;
        }

        private void ResetMatrix()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    Toggle toggle = matrix[row][col].GetComponent<Toggle>();
                    matrix[row][col].tag = "Untagged";
                }
            }
        }

        private void FindAdjacentNodes(BlockStructure structure, int row, int column)
        {
            if (row >= matrix.Count || column >= matrix[0].Count || row < 0 || column < 0 || matrix[row][column].tag.Equals("Visited"))
                return;

            matrix[row][column].tag = "Visited";

            if (matrix[row][column].GetComponent<Toggle>().isOn)
            {
                structure.AddNode(new BlockPart(row, column), row, column);
                FindAdjacentNodes(structure, row, column - 1);
                FindAdjacentNodes(structure, row, column + 1);
                FindAdjacentNodes(structure, row - 1, column);
                FindAdjacentNodes(structure, row + 1, column);
            }
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

        public void ChangeBlockSize()
        {
            if (currentlyChoosenFlat)
            {
                currentlyChoosenBlock = Block1x1;
            }
            else
            {
                currentlyChoosenBlock = Block1x1Flat;
            }
            currentlyChoosenFlat = !currentlyChoosenFlat;
                
        }
    }

    

    public class BlockPart{

        public int Row;
        public int Col;

        public HashSet<DIRECTION> visitedDirections = new HashSet<DIRECTION>();
        public DIRECTION WallDirection;

        public Vector3 partCenter;

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

}
