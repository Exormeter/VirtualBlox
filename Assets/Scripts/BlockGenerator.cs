using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Valve.VR.InteractionSystem
{

    /// <summary>
    /// Generate Blocks
    /// </summary>
    public class BlockGenerator : MonoBehaviour
    {
        /// <summary>
        /// The Part of a Block to generate, normal height
        /// </summary>
        public GameObject Block1x1;

        /// <summary>
        /// The Part of a Block to generate, flat height
        /// </summary>
        public GameObject Block1x1Flat;

        /// <summary>
        /// Components of the PrecurserBlock are compied to the generated Blocks
        /// </summary>
        public GameObject PrecurserBlock;

        /// <summary>
        /// The materal for the Blocks
        /// </summary>
        public Material material;


        private Dictionary<BLOCKSIZE, GameObject> partSizes = new Dictionary<BLOCKSIZE, GameObject>();

        /// <summary>
        /// Lenght of a 1x1 Block
        /// </summary>
        private readonly float length = 0.08f;

        // Start is called before the first frame update
        void Start()
        {
            partSizes.Add(BLOCKSIZE.FLAT, Block1x1Flat);
            partSizes.Add(BLOCKSIZE.NORMAL, Block1x1);
        }

        /// <summary>
        /// Generates a new Block from a BlockStructure, places it at (0,2,0)
        /// </summary>
        /// <param name="structure">The Structure to generatoe</param>
        /// <returns>A new Block</returns>
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

        

        
        /// <summary>
        /// Combines the meshes inside a container into one single mesh
        /// </summary>
        /// <param name="container">The meshes to combine</param>
        /// <returns>A GameObject with a combined Mesh</returns>
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

        /// <summary>
        /// Copies the Components in the PrecurserBlock to the handed over Block
        /// </summary>
        /// <param name="combinedBlock">Block without Components</param>
        private void AddPrecurserComponents(GameObject combinedBlock)
        {
            Component[] components = PrecurserBlock.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                //Skip MeshRenderer, Transform and MeshFilter components
                if(components[i] is MeshRenderer || components[i] is Transform || components[i] is MeshFilter)
                {
                    continue;
                }
                CopyComponent(components[i], combinedBlock);
            }

        }

        /// <summary>
        /// Copies the components with all set variables to the destination GameObject
        /// </summary>
        /// <param name="original">The original component</param>
        /// <param name="destination">Destination GameObject</param>
        /// <returns></returns>
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

        public void AttachNewBlockToHand(GameObject generatedBlock, Hand hand)
        {
            StartCoroutine(AttachNewBlockToHandAfterTwoFrames(generatedBlock, hand));
        }

        public IEnumerator AttachNewBlockToHandAfterTwoFrames(GameObject generatedBlock, Hand hand)
        {
            {
                for (int i = 0; i <= 2; i++)
                {
                    if (i == 2)
                    {
                        generatedBlock.transform.position = hand.objectAttachmentPoint.transform.position;
                        generatedBlock.GetComponent<BlockInteractable>().PhysicsAttach(hand, GrabTypes.Grip);
                    }
                    yield return new WaitForFixedUpdate();
                }

            }
        }

    }

    
    /// <summary>
    /// Part of the BlockStructure Matrix
    /// </summary>
    public class BlockPart{

        /// <summary>
        /// Which Row is the BlockPart inside the Matrix
        /// </summary>
        public int Row;

        /// <summary>
        /// Which Column is the BlockPart inside the Matrix
        /// </summary>
        public int Col;

        /// <summary>
        /// Which direction has already been visited when searching for walls
        /// </summary>
        public HashSet<DIRECTION> visitedDirections = new HashSet<DIRECTION>();

        /// <summary>
        /// Which wall direction represents this BlockPart
        /// </summary>
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


        /// <summary>
        /// Resets the visited List, used when coping Block to reset the search
        /// </summary>
        public void ResetVisited()
        {
            visitedDirections.Clear();
        }

        /// <summary>
        /// Adds a direction to keep track of which direction was visited
        /// </summary>
        /// <param name="direction"></param>
        public void DirectionVisited(DIRECTION direction)
        {
            visitedDirections.Add(direction);
        }

        /// <summary>
        /// Was the direction already visited
        /// </summary>
        /// <param name="direction">The direction</param>
        /// <returns>True if the direction was visited</returns>
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
