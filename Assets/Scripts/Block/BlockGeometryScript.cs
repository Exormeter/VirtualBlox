using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Valve.VR.InteractionSystem
{
    public class BlockGeometryScript : MonoBehaviour
    {

        /// <summary>The height of a brick, can be flat (0.032) or normal (0.096)</summary>
        private float BRICK_HEIGHT;

        public static float BRICK_HEIGHT_NORMAL = 0.096f;

        public static float BRICK_HEIGHT_FLAT = 0.032f;

        /// <summary>The height of the pins on the upside of the bricks</summary>
        private const float BRICK_PIN_HEIGHT = 0.016f;

        /// <summary>The height of the pins halfed</summary>
        private const float BRICK_PIN_HEIGHT_HALF = BRICK_PIN_HEIGHT / 2;

        /// <summary>The thickness of the walls of the brick</summary>
        private const float BRICK_WALL_WIDTH = 0.008f;

        /// <summary>The thickness of the brick halfed</summary>
        private const float BRICK_WALL_WIDTH_HALF = BRICK_WALL_WIDTH / 2;

        /// <summary>The distance between to pins on the upside of a brick</summary>
        private const float BRICK_PIN_DISTANCE = 0.08f;

        /// <summary>The lenght of a brick, meassure taken from a 1x1 brick</summary>
        private const float BRICK_LENGTH = 0.08f;

        /// <summary>The diameter of a pin on the upside of a brick</summary>
        private const float BRICK_PIN_DIAMETER = 0.048f;

        /// <summary>
        /// A container GameObject that contains all TapCollider as well as the
        /// TapHandler
        /// </summary>
        public GameObject TapContainer;

        /// <summary>
        /// A container GameObject that contains all GrooveCollider as well as the
        /// GrooveHandler
        /// </summary>
        public GameObject GroovesContainer;

        /// <summary>The mesh of the block</summary>
        private Mesh mesh;

        /// <summary>
        /// The BlockStructure is containing the color, height and the contoures of the
        /// block in form of a matrix
        /// </summary>
        public BlockStructure blockStructure;

        /// <summary>
        /// Contains the TopCollider, so that a serperate Raycast Layer can be set for them
        /// </summary>
        public GameObject TopColliderContainer;

        /// <summary>A list that contains all wall colliders of the block</summary>
        private List<Collider> wallColliderList = new List<Collider>();

        void Awake()
        {
            this.mesh = GetComponent<MeshFilter>().mesh;

            //Sets the height of the brick to the correct value
            if (mesh.bounds.extents.y < 0.047)
            {
                BRICK_HEIGHT = 0.032f;
            }
            else
            {
                BRICK_HEIGHT = 0.096f;
            }

            //Create the TapContainer and adds the TapHandler to it
            GameObject taps = new GameObject("Taps");
            taps.tag = "Tap";
            taps.AddComponent<TapHandler>();
            taps.transform.SetParent(this.transform);
            taps.transform.localPosition = new Vector3(0f, 0f, 0f);

            //Create the GrooveContainer and adds the GrooveHandler to it
            GameObject grooves = new GameObject("Grooves");
            grooves.tag = "Groove";
            grooves.AddComponent<GrooveHandler>();
            grooves.transform.SetParent(this.transform);
            grooves.transform.localPosition = new Vector3(0f, 0f, 0f);
            TapContainer = taps;
            GroovesContainer = grooves;

        }

        private void Start()
        {
            //Structure was not set, so try to calculate Walls
            if (GetComponents<Collider>().Length == 0)
            {
                AddWallCollider();
                AddPinTriggerCollider(BRICK_PIN_HEIGHT_HALF, TapContainer, "Tap");
                AddPinTriggerCollider(-BRICK_HEIGHT / 1.1f, GroovesContainer, "Groove");
                SetWallColliderTrigger(false);
            }
        }

        private void Update()
        {
            if (Input.anyKeyDown)
            {
                TapContainer.SetActive(true);
                GroovesContainer.SetActive(true);
                
                SetWallColliderTrigger(false);
            }
        }


        /// <summary>
        /// Sets the BlockStructure for the Block, recalculated the Groove and Taps and Collider
        /// of the walls
        /// </summary>
        /// <param name="structure">The BlockStructure to set</param>
        public void SetStructure(BlockStructure structure)
        {
            //Reset the BlockStructre in case it wasn't done to ensure no BlockPart is marked visited
            structure.ResetBlockParts();

            //Set the new BlockStructure
            this.blockStructure = structure;

            //Delete the WallCollider in case they were already set, walls need to be recalculated
            RemoveWallCollider();

            //List of BlockParts which make up a wall in the Structure, which are collected inside
            //a list. 
            List<List<BlockPart>> allWallsInStrucure = new List<List<BlockPart>>();

            //Search the blockStructure for walls and add them the the list. Search is carried out
            //for all four directions
            SearchWallsInStructure(DIRECTION.UP).ForEach(wall => allWallsInStrucure.Add(wall));
            SearchWallsInStructure(DIRECTION.DOWN).ForEach(wall => allWallsInStrucure.Add(wall));
            SearchWallsInStructure(DIRECTION.LEFT).ForEach(wall => allWallsInStrucure.Add(wall));
            SearchWallsInStructure(DIRECTION.RIGHT).ForEach(wall => allWallsInStrucure.Add(wall));

            //Convert the found walls to a Collider and add them to the block GameObject
            AddWallCollider(allWallsInStrucure);

            //Add the collider in the Top of the Brick
            AddTopCollider();

            //Add the new Tap and Groove Collider to the Block, as the changed as well with the
            //new structure
            AddPinTriggerColliderByStructure(BRICK_PIN_HEIGHT_HALF, TapContainer, "Tap");
            AddPinTriggerColliderByStructure(-BRICK_HEIGHT / 1.1f, GroovesContainer, "Groove");
            

        }

        /// <summary>
        /// Returnes the current BlockStructure
        /// </summary>
        /// <returns>The BlockSturcture of the Block</returns>
        public BlockStructure GetStructure()
        {
            return blockStructure;
        }

        /// <summary>
        /// Adds the top Collider to the Block, the Collider is calculated by the currently
        /// setted BlockStructure
        /// </summary>
        private void AddTopCollider()
        {
            //Adds a container GameObject for the TopCollider, so it can use a own Layer for Raycasting
            GameObject topColliderContainer = new GameObject("TopColliderContainer");
            topColliderContainer.tag = "TopColliderContainer";
            topColliderContainer.transform.SetParent(gameObject.transform);
            TopColliderContainer = topColliderContainer;

            //Defines the Collider size, based of the 1x1 Block and wall thickness
            Vector3 topSideSize = new Vector3(BRICK_LENGTH, BRICK_WALL_WIDTH, BRICK_LENGTH);

            //MiddlePoint row of the BlockStructure Matrix
            float rowMiddlePoint = (float)(blockStructure.RowsCropped - 1) / 2;

            //MiddlePoint column of the BlockStructure Matrix
            float colMiddlePoint = (float)(blockStructure.ColsCropped - 1) / 2;

            //Loop throught the cropped BlockStructure
            for (int row = 0; row < blockStructure.RowsCropped; row++)
            {
                for(int col = 0; col < blockStructure.ColsCropped; col++)
                {
                    //If a Part inside the Matrix is not null, set a new collider
                    if(blockStructure[row, col] != null)
                    {

                        float centerX = (rowMiddlePoint - row) * BRICK_LENGTH;
                        float centerY = GetCenterTop().y - (BRICK_WALL_WIDTH / 2);
                        float centerZ = (colMiddlePoint - col) * BRICK_LENGTH;

                        //Defines the center of the Collider
                        Vector3 colliderCenter = new Vector3(centerX, centerY, centerZ);

                        //Adda the Collider to the GameObject and to the wallCollider list for caching
                        wallColliderList.Add(AddBoxCollider(topSideSize, colliderCenter, false, topColliderContainer));
                    }
                }
            }
        }

        /// <summary>
        /// Adds the Collider for the walls of the Block. Every List with BlockParts inside the List is
        /// getting converted into a wall Collider
        /// </summary>
        /// <param name="allWallsInStructure"> Contains the walls to convert</param>
        public void AddWallCollider(List<List<BlockPart>> allWallsInStructure)
        {
            //Loop thought every wall
            foreach(List<BlockPart> wall in allWallsInStructure)
            {
                //Lenght of the wall, depents on the number of BlockParts time the length of a 1x1 brick
                float wallLength = wall.Count * BRICK_LENGTH;

                //MiddlePoint row of the BlockStructure Matrix
                float rowMiddlePoint = (float) (blockStructure.RowsCropped - 1) / 2;

                //MiddlePoint column of the BlockStructure Matrix
                float colMiddlePoint = (float) (blockStructure.ColsCropped - 1) / 2;


                //Gets the center of the mesh for positioning of the collider
                Vector3 centerMesh = GetComponent<MeshFilter>().mesh.bounds.center;

                //The middlePoint row for the wall, used to position the collider in the middle of the wall
                float wallColumnMidPoint = 0;

                //The middlePoint column for the wall, used to position the collider in the middle of the wall
                float wallRowMidPoint = 0;
                wall.ForEach(blockPart => {
                    wallRowMidPoint += blockPart.Row;
                    wallColumnMidPoint += blockPart.Col;
                });
                wallRowMidPoint /= wall.Count;
                wallColumnMidPoint /= wall.Count;


                switch (wall[0].WallDirection)
                {
                    case DIRECTION.UP:
                        {
                            float centerColliderZ = (colMiddlePoint - wallColumnMidPoint) * BRICK_LENGTH;
                            float centerColliderX = (rowMiddlePoint + 0.5f - wall[0].Row) * BRICK_LENGTH;

                            Vector3 size = new Vector3(BRICK_WALL_WIDTH, BRICK_HEIGHT, wallLength);
                            Vector3 centerCollider = new Vector3(centerColliderX - BRICK_WALL_WIDTH_HALF, centerMesh.y - BRICK_PIN_HEIGHT_HALF, centerColliderZ);

                            wallColliderList.Add(AddBoxCollider(size, centerCollider, false, transform.gameObject));
                            break;
                        }

                    case DIRECTION.DOWN:
                        {
                            float centerColliderZ = (colMiddlePoint - wallColumnMidPoint) * BRICK_LENGTH;
                            float centerColliderX = (rowMiddlePoint - 0.5f - wall[0].Row) * BRICK_LENGTH;

                            Vector3 size = new Vector3(BRICK_WALL_WIDTH, BRICK_HEIGHT, wallLength);
                            Vector3 centerCollider = new Vector3(centerColliderX + BRICK_WALL_WIDTH_HALF, centerMesh.y - BRICK_PIN_HEIGHT_HALF, centerColliderZ);

                            wallColliderList.Add(AddBoxCollider(size, centerCollider, false, transform.gameObject));
                            break;
                        }
                        

                    case DIRECTION.LEFT:
                        {
                            float centerColliderX = (rowMiddlePoint - wallRowMidPoint) * BRICK_LENGTH;
                            float centerColliderZ = (colMiddlePoint + 0.5f - wall[0].Col) * BRICK_LENGTH;

                            Vector3 size = new Vector3(wallLength, BRICK_HEIGHT, BRICK_WALL_WIDTH);
                            Vector3 centerCollider = new Vector3(centerColliderX, centerMesh.y - BRICK_PIN_HEIGHT_HALF, centerColliderZ - BRICK_WALL_WIDTH_HALF);

                            wallColliderList.Add(AddBoxCollider(size, centerCollider, false, transform.gameObject));
                            break;
                        }
                            

                    case DIRECTION.RIGHT:
                        {
                            float centerColliderX = (rowMiddlePoint - wallRowMidPoint) * BRICK_LENGTH;
                            float centerColliderZ = (colMiddlePoint - 0.5f - wall[0].Col) * BRICK_LENGTH;

                            Vector3 size = new Vector3(wallLength, BRICK_HEIGHT, BRICK_WALL_WIDTH);
                            Vector3 centerCollider = new Vector3(centerColliderX, centerMesh.y - BRICK_PIN_HEIGHT_HALF, centerColliderZ + BRICK_WALL_WIDTH_HALF);

                            wallColliderList.Add(AddBoxCollider(size, centerCollider, false, transform.gameObject));
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Searches the BockStructure for walls. A wall is defined in that it is missing a neighbor on
        /// any of it's edges. The method only searches in one direction at a time.
        /// </summary>
        /// <param name="direction">The desired search direction</param>
        /// <returns>The found walls as a List of BlockParts</returns>
        private List<List<BlockPart>> SearchWallsInStructure(DIRECTION direction)
        {
            //List with List of BlocksParts to return
            List<List<BlockPart>> allWallsInStructure = new List<List<BlockPart>>();

            //Loop thought the cropped BlockStructure Matrix
            for (int row = 0; row < blockStructure.RowsCropped; row++)
            {
                for (int col = 0; col < blockStructure.ColsCropped; col++)
                {
                    //Check if the currently searched Part is not null and wasn't already visited
                    if (blockStructure[row, col] != null && !blockStructure[row, col].WasDirectionVisited(direction))
                    {
                        //Searches for walls at the given Location recursive
                        List<BlockPart> tempList = SearchWallsAtLocation(row, col, direction);

                        //If the Search was successful, add the List to the List that will returned
                        if(tempList != null && tempList.Count != 0)
                        {
                            allWallsInStructure.Add(tempList);
                        }
                        
                    }
                }
            }
            return allWallsInStructure;
        }

        /// <summary>
        /// Recursivly checks a position if the Block has a neighbor in the searched direction. If it
        /// does not, the next Block is searched until the wall is completely found
        /// </summary>
        /// <param name="row">The row where to search</param>
        /// <param name="col">The column where to search</param>
        /// <param name="direction">The direction in which the current search is carried out</param>
        /// <param name="wallInStructure">The List of found BlockParts, can be null to begin a new search</param>
        /// <returns>List with BlockParts that make up a wall</returns>
        private List<BlockPart> SearchWallsAtLocation(int row, int col, DIRECTION direction, List<BlockPart> wallInStructure = null)
        {
            //If search is outside the matrix, return the List
            if(row >= blockStructure.RowsCropped || col >= blockStructure.ColsCropped || row < 0 || col < 0)
            {
                return wallInStructure;
            }

            //If List was null, create a new list to cache BlockParts
            if (wallInStructure == null)
            {
                wallInStructure = new List<BlockPart>();
            }

            //If BlockPart has no neighbor in searched direction, part of wall is found
            if (blockStructure[row, col] != null && !blockStructure.HasNeighbour(row, col, direction) )
            {
                //Remember that this part was search in the direction
                blockStructure[row, col].DirectionVisited(direction);

                //Add the found BlockPart to the List
                wallInStructure.Add(new BlockPart(row, col, direction));

                //Continue the search in row respectively column direction
                if (direction == DIRECTION.UP || direction == DIRECTION.DOWN)
                {
                    SearchWallsAtLocation(row, col + 1, direction, wallInStructure);
                }
                else
                {
                    SearchWallsAtLocation(row + 1, col, direction, wallInStructure);
                }
                
            }

            return wallInStructure;
        }

        /// <summary>
        /// Remove all wall Collider on the GameObject
        /// </summary>
        private void RemoveWallCollider()
        {
            for(int i = 0; i < wallColliderList.Count; i++)
            {
                Destroy(wallColliderList[i]);
            }
            wallColliderList.Clear();
        }

        /// <summary>
        /// Tries to calculates the walls of the block, only works for rectangle blocks. For
        /// non-rectangle blocks a BlockStructure is needed
        /// </summary>
        private void AddWallCollider()
        {
            Vector3 size = mesh.bounds.size;
            Vector3 center = mesh.bounds.center;
            Vector3 extends = mesh.bounds.extents;
            //Collider Long Side
            Vector3 longSideSize = new Vector3(size.x, BRICK_HEIGHT, BRICK_WALL_WIDTH);
            Vector3 longSideCenterLeft = new Vector3(center.x, center.y - BRICK_PIN_HEIGHT_HALF, center.z + extends.z - BRICK_WALL_WIDTH_HALF);
            Vector3 longSideCenterRight = new Vector3(center.x, center.y - BRICK_PIN_HEIGHT_HALF, center.z - extends.z + BRICK_WALL_WIDTH_HALF);
            wallColliderList.Add(AddBoxCollider(longSideSize, longSideCenterLeft, false, this.gameObject));
            wallColliderList.Add(AddBoxCollider(longSideSize, longSideCenterRight, false, this.gameObject));


            //Collider Short Side
            Vector3 shortSide = new Vector3(BRICK_WALL_WIDTH, BRICK_HEIGHT, size.z);
            Vector3 shortSideCenterUp = new Vector3(center.x + extends.x - BRICK_WALL_WIDTH_HALF, center.y - BRICK_PIN_HEIGHT_HALF, center.z);
            Vector3 shortSideCenterDown = new Vector3(center.x - extends.x + BRICK_WALL_WIDTH_HALF, center.y - BRICK_PIN_HEIGHT_HALF, center.z);
            wallColliderList.Add(AddBoxCollider(shortSide, shortSideCenterUp, false, this.gameObject));
            wallColliderList.Add(AddBoxCollider(shortSide, shortSideCenterDown, false, this.gameObject));

            //Collider Top Side
            Vector3 topSideSize = new Vector3(size.x, BRICK_WALL_WIDTH, size.z);
            Vector3 topSideCenter = GetCenterTop();
            topSideCenter.y = topSideCenter.y - (BRICK_WALL_WIDTH / 2);
            wallColliderList.Add(AddBoxCollider(topSideSize, topSideCenter, false, this.gameObject));
        }

        /// <summary>
        /// Adds the Collider to the Tap and Groove Container, only for use if no BlockStructure provided
        /// </summary>
        /// <param name="heightOffset">If no offset is provided, the collider are inline with bottom edge of the block</param>
        /// <param name="containerObject">The GameObject which the ColliderContainer GameObject is added to</param>
        /// <param name="tag">The Tag that will be added to the ColliderContainer</param>
        private void AddPinTriggerCollider(float heightOffset, GameObject containerObject, String tag)
        {
            Vector3 center = mesh.bounds.center;
            Vector3 extends = mesh.bounds.extents;
            Vector3 blockCorner = new Vector3(center.x + extends.x, center.y + extends.y - BRICK_PIN_HEIGHT, center.z + extends.z);
            Vector3 firstPinCenterPoint = new Vector3(blockCorner.x - (BRICK_PIN_DISTANCE / 2), blockCorner.y + heightOffset, blockCorner.z - (BRICK_PIN_DISTANCE / 2));

            Vector3 currentPinCenterPoint = firstPinCenterPoint;

            while (mesh.bounds.Contains(currentPinCenterPoint))
            {

                while (mesh.bounds.Contains(currentPinCenterPoint))
                {
                    //bool isTrigger = true;
                    //if (tag.Equals("Tap"))
                    //{
                    //    isTrigger = false;
                    //}
                    AddGameObjectCollider(currentPinCenterPoint, tag, containerObject, true);
                    currentPinCenterPoint.z = currentPinCenterPoint.z - BRICK_PIN_DISTANCE;
                }
                currentPinCenterPoint.x = currentPinCenterPoint.x - BRICK_PIN_DISTANCE;
                currentPinCenterPoint.z = firstPinCenterPoint.z;

            }
        }

        /// <summary>
        /// Adds the Collider to the Tap and Groove Container, for use with the BlockStructure
        /// </summary>
        /// <param name="heightOffset">If no offset is provided, the collider are inline with bottom edge of the block</param>
        /// <param name="containerObject">The GameObject which the ColliderContainer GameObject is added to</param>
        /// <param name="tag">The Tag that will be added to the ColliderContainer</param>
        private void AddPinTriggerColliderByStructure(float heightOffset, GameObject containerObject, String tag)
        {
            for (int index = 0; index < containerObject.transform.childCount; index++)
            {
                Destroy(containerObject.transform.GetChild(index).gameObject);
            }

            float rowMiddlePoint = (float)(blockStructure.RowsCropped - 1) / 2;
            float colMiddlePoint = (float)(blockStructure.ColsCropped - 1) / 2;

            for (int row = 0; row < blockStructure.RowsCropped; row++)
            {
                for (int col = 0; col < blockStructure.ColsCropped; col++)
                {
                    if(blockStructure[row, col] != null)
                    {
                        bool isTrigger = true;
                        //if (tag.Equals("Tap"))
                        //{
                        //    isTrigger = false;
                        //}

                        float centerX = (rowMiddlePoint - row) * BRICK_LENGTH;
                        float centerY = GetCenterTop().y + heightOffset;
                        float centerZ = (colMiddlePoint - col) * BRICK_LENGTH;

                        Vector3 currentPinCenterPoint = new Vector3(centerX, centerY, centerZ);
                        AddGameObjectCollider(currentPinCenterPoint, tag, containerObject, isTrigger);
                    }
                    
                }
            }
        }

        /// <summary>
        /// Adds a single new GameObject with a Collider as well as a GrooveCollider or TapCollider
        /// component to the Container
        /// </summary>
        /// <param name="position">The position of the new GameObject and in term the Collider</param>
        /// <param name="tag">Changes with offset and if a Tap or GrooveCollider Component is added</param>
        /// <param name="container">Where the SubContainer is added to</param>
        /// <param name="isTrigger">Should the Collider be a trigger</param>
        public static void AddGameObjectCollider(Vector3 position, String tag, GameObject container, bool isTrigger)
        {
            GameObject colliderObject = new GameObject("Collider");
            colliderObject.tag = tag;
            colliderObject.transform.SetParent(container.transform);
            colliderObject.transform.localPosition = position;

            switch (tag)
            {
                case "Groove":
                    colliderObject.AddComponent<GrooveCollider>();
                    AddBoxCollider(new Vector3(BRICK_PIN_DIAMETER / 2, BRICK_PIN_HEIGHT, BRICK_PIN_DIAMETER / 2), new Vector3(0, 0, 0), isTrigger, colliderObject);
                    break;

                case "Tap":
                    colliderObject.AddComponent<TapCollider>();
                    AddBoxCollider(new Vector3(BRICK_PIN_DIAMETER / 2, BRICK_PIN_HEIGHT, BRICK_PIN_DIAMETER / 2), new Vector3(0, 0, 0), isTrigger, colliderObject);
                    break;

                //case "TapCollider":
                //    AddBoxCollider(new Vector3(BRICK_PIN_DIAMETER, BRICK_PIN_HEIGHT, BRICK_PIN_DIAMETER), new Vector3(0, 0, 0), isTrigger, colliderObject, material);
                //    break;
            }
        }

        /// <summary>
        /// Adds a Collider Component to a GameObject with a specific physic material
        /// </summary>
        /// <param name="size">Size of the new Collider</param>
        /// <param name="center">Center of the new Collider</param>
        /// <param name="isTrigger">Should the Collider be a trigger</param>
        /// <param name="otherGameObject">Where the Collider is added to</param>
        /// <param name="material">The material of the collider</param>
        /// <returns>The newly added Collider</returns>
        public static Collider AddBoxCollider(Vector3 size, Vector3 center, bool isTrigger, GameObject otherGameObject, PhysicMaterial material)
        {
            BoxCollider newCollider = otherGameObject.AddComponent<BoxCollider>();
            newCollider.size = size;
            newCollider.center = center;
            newCollider.isTrigger = isTrigger;
            newCollider.material = material;
            return newCollider;
        }

        /// <summary>
        /// Adds a Collider Component to a GameObject
        /// </summary>
        /// <param name="sizeCollider">Size of the new Collider</param>
        /// <param name="centerCollider">Center of the new Collider</param>
        /// <param name="isTrigger">Should the Collider be a trigger</param>
        /// <param name="otherGameObject">Where the Collider is added to</param>
        /// <returns>The newly added Collider</returns>
        public static Collider AddBoxCollider(Vector3 sizeCollider, Vector3 centerCollider, bool isTrigger, GameObject otherGameObject)
        {
            BoxCollider newCollider = otherGameObject.AddComponent<BoxCollider>();
            newCollider.size = sizeCollider;
            newCollider.center = centerCollider;
            newCollider.isTrigger = isTrigger;
            return newCollider;
        }

        /// <summary>
        /// Gets the center of the Block as local position
        /// </summary>
        /// <returns>Center of Block local</returns>
        public Vector3 GetCenterTop()
        {
            Vector3 center = GetComponent<MeshFilter>().mesh.bounds.center;
            Vector3 extends = GetComponent<MeshFilter>().mesh.bounds.extents;
            center.y = center.y + extends.y - BRICK_PIN_HEIGHT;
            return center;

        }

        /// <summary>
        /// Gets the center of the Block as world position
        /// </summary>
        /// <returns>Center of Block world</returns>
        public Vector3 GetCenterTopWorld()
        {
            Vector3 center = mesh.bounds.center;
            Vector3 extends = mesh.bounds.extents;
            center.y = center.y + extends.y - BRICK_PIN_HEIGHT;
            return transform.TransformPoint(center);

        }

        public Vector3 GetCenterBottom()
        {
            Vector3 center = mesh.bounds.center;
            Vector3 extends = mesh.bounds.extents;
            center.y = center.y - extends.y;
            return center;
        }

        public void SetWallColliderTrigger(bool trigger)
        {
            wallColliderList.ForEach(collder => collder.isTrigger = trigger);
        }

        public void OnAttachToFloor()
        {
            TopColliderContainer.layer = 8;
        }

        public void OnAttachToHand()
        {
            TopColliderContainer.layer = 0;
        }
    }
}