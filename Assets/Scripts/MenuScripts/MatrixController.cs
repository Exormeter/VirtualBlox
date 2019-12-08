using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Valve.VR.InteractionSystem
{
    [RequireComponent(typeof(DragHandler))]
    public class MatrixController : MonoBehaviour
    {
        public int Rows;
        public int Columns;
        public GameObject Toggle;
       

        private List<List<GameObject>> matrix = new List<List<GameObject>>();
        //private Canvas canvas;
        // Start is called before the first frame update
        void Start()
        {
            PopulateMatrix();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void PopulateMatrix()
        {
            for (int r = 0; r < Rows; r++)
            {
                matrix.Add(new List<GameObject>());
                for (int c = 0; c < Columns; c++)
                {
                    GameObject toggle = InstantiateToggle(r, c);
                    matrix[r].Add(toggle);
                }
            }
        }

        private GameObject InstantiateToggle(int row, int col)
        {
            GameObject toggle = Instantiate(Toggle, Vector3.zero, Quaternion.identity, gameObject.transform);
            RectTransform rectTransfrom = toggle.GetComponent<RectTransform>();
            rectTransfrom.localScale = new Vector3(2, 2, 1);
            rectTransfrom.localPosition = Vector3.zero;
            rectTransfrom.localRotation = Quaternion.identity;

            Vector3 anchorPosition = new Vector3(col * rectTransfrom.sizeDelta.x, -row * rectTransfrom.sizeDelta.y, 0);

            toggle.GetComponent<RectTransform>().anchoredPosition = anchorPosition;
            return toggle;
        }

        public void RemoveRow()
        {
            matrix[Rows].ForEach(gameObject => gameObject.SetActive(false));
            Rows--;
        }

        public void AddRow()
        {
            Rows++;
            if(matrix.Count >= Rows)
            {
                matrix[Rows].ForEach(gameObject => gameObject.SetActive(true));
            }
            else
            {
                List<GameObject> newRow = new List<GameObject>();
                for (int col = 0; col > Columns; col++)
                {
                    GameObject toggle = InstantiateToggle(Rows, col);
                    newRow.Add(toggle);
                }
                matrix.Add(newRow);
            }

        }

        public void RemoveCol()
        {
            matrix.ForEach(rows => rows[Columns].SetActive(false));
            Columns--;
        }

        public void AddCol()
        {
            Columns++;
            if(matrix[0].Count >= Columns)
            {
                for(int row = 0; row < Rows; row++)
                {
                    matrix[row][Columns].SetActive(true);
                }
            }
            else
            {
                for (int row = 0; row < Rows; row++)
                {
                    GameObject toggle = InstantiateToggle(row, Columns);
                    matrix[row].Add(toggle);
                }
            }
        }

       
        public List<BlockStructure> GetStructures()
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
    }
}