using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using System.Linq;

namespace Valve.VR.InteractionSystem
{


    public class BlockGenerator : MonoBehaviour
    {

        public GameObject Block1x1;
        public GameObject PrecurserBlock;
        public Material material;
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
            for (int r = 0; r < Rows; r++)
            {
                matrix.Add(new List<GameObject>());
                for (int c = 0; c < Columns; c++)
                {

                    GameObject toggle = Instantiate(Toggle, canvas.transform, true);
                    RectTransform rectTransfrom = toggle.GetComponent<RectTransform>();
                    rectTransfrom.localScale = new Vector3(1, 1, 1);
                    Vector3 anchorPosition = new Vector3(r * rectTransfrom.sizeDelta.x, -c * rectTransfrom.sizeDelta.y, 0);

                    toggle.GetComponent<RectTransform>().anchoredPosition = anchorPosition;
                    matrix[r].Add(toggle);
                }
            }

        }

        void Update()
        {
            if (spawnBlockAction.GetLastStateDown(leftHand) || spawnBlockAction.GetStateDown(righthand))
            {

                List<BlockStructure<GameObject>> blockStructures = FindStructures();
                foreach (BlockStructure<GameObject> blockStructure in blockStructures)
                {
                    GenerateBlock(blockStructure);
                }
            }
        }

        private void GenerateBlock(BlockStructure<GameObject> structure)
        {
            List<GameObject> objects = new List<GameObject>();
            GameObject container = new GameObject();
            for(int row = 0; row < structure.Rows; row++)
            {
                for (int col = 0; col < structure.Cols; col++)
                {
                    if(structure.GetCroppedMatrix()[row, col] != null)
                    {
                        GameObject blockPart = Instantiate(Block1x1, new Vector3(row * heigt, 0, col * heigt), Quaternion.identity, container.transform);
                        blockPart.SetActive(true);
                        objects.Add(blockPart);
                    }
                    
                }
            }
            CombineTileMeshes(container);
        }

        private List<BlockStructure<GameObject>> FindStructures()
        {
            List<BlockStructure<GameObject>> blockStructures = new List<BlockStructure<GameObject>>();
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    Toggle toggle = matrix[row][col].GetComponent<Toggle>();
                    if (toggle.isOn && !matrix[row][col].tag.Equals("Visited"))
                    {
                        BlockStructure<GameObject> structure = new BlockStructure<GameObject>(Rows, Columns);
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

        private void FindAdjacentNodes(BlockStructure<GameObject> structure, int row, int column)
        {
            if (row >= matrix.Count || column >= matrix[0].Count || row < 0 || column < 0 || matrix[row][column].tag.Equals("Visited"))
                return;

            matrix[row][column].tag = "Visited";

            if (matrix[row][column].GetComponent<Toggle>().isOn)
            {
                structure.AddNode(matrix[row][column], row, column);
                FindAdjacentNodes(structure, row, column - 1);
                FindAdjacentNodes(structure, row, column + 1);
                FindAdjacentNodes(structure, row - 1, column);
                FindAdjacentNodes(structure, row + 1, column);
            }
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

            GameObject combinedBlock = new GameObject("Block");
            combinedBlock.AddComponent(typeof(MeshFilter));
            combinedBlock.AddComponent(typeof(MeshRenderer));
            combinedBlock.GetComponent<MeshFilter>().mesh = new Mesh();
            combinedBlock.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
            Destroy(container);

            combinedBlock.transform.position = new Vector3(0, 2, 0);
            AddPrecurserComponents(combinedBlock);

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
    }

    public class BlockStructure<T>{

        private readonly T[,] matrix;
        private T [,] croppedMatrix;
        public int Rows { get; }
        public int Cols { get; }
        public BlockStructure(int row, int col)
        {
            Rows = row;
            Cols = col;
            matrix = new T[row, col];
        }

        public void AddNode(T node, int row, int col)
        {
            matrix[row, col] = node;
        }

        public T[, ] GetCroppedMatrix()
        {

            if(croppedMatrix != null)
            {
                return croppedMatrix;
            }

            int emptyRows = 0;
            int emptyCols = 0;

            for(int row = 0; row < Rows; row++)
            {
                if(!IsEmpty(GetRow(matrix, row)))
                {
                    emptyRows = row;
                    break;
                }
            }

            for (int col = 0; col < Cols; col++)
            {
                if (!IsEmpty(GetColumn(matrix, col)))
                {
                    emptyCols = col;
                    break;
                }
            }

            croppedMatrix = new T[Rows, Cols];
            for(int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    if(matrix[row, col] != null)
                    {
                        croppedMatrix[row - emptyRows, col - emptyCols] = matrix[row, col];
                    }
                }
            }
            return croppedMatrix;


        }

        private bool IsEmpty(T[] array)
        {
            for(int i = 0; i < array.Length; i++)
            {
                if(array[i] != null)
                {
                    return false;
                }
            }
            return true;
        }

        private T[] GetColumn(T[,] matrix, int columnNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(0))
                    .Select(x => matrix[x, columnNumber])
                    .ToArray();
        }

        private T[] GetRow(T[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }

    }
}
