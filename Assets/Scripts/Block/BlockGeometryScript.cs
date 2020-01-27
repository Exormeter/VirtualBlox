using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Valve.VR.InteractionSystem
{
    public class BlockGeometryScript : MonoBehaviour
    {

        //The height of a brick, can be flat (0.032) or normal (0.096)
        private float BRICK_HEIGHT;

        //The height of the pins on the upside of the bricks
        private const float BRICK_PIN_HEIGHT = 0.016f;

        //The height of the pins halfed
        private const float BRICK_PIN_HEIGHT_HALF = BRICK_PIN_HEIGHT / 2;

        //The thickness of the walls of the brick
        private const float BRICK_WALL_WIDTH = 0.008f;

        //The thickness of the brick halfed
        private const float BRICK_WALL_WIDTH_HALF = BRICK_WALL_WIDTH / 2;

        //The distance between to pins on the upside of a brick
        private const float BRICK_PIN_DISTANCE = 0.08f;

        //The lenght of a brick, meassure taken from a 1x1 brick
        private const float BRICK_LENGTH = 0.08f;

        //The diameter of a pin on the upside of a brick
        private const float BRICK_PIN_DIAMETER = 0.048f;

        //A container GameObject that contains all TapCollider as well as the
        //TapHandler
        public GameObject TapContainer;

        //A container GameObject that contains all GrooveCollider as well as the
        //GrooveHandler
        public GameObject GroovesContainer;

        //The mesh of the block
        private Mesh mesh;

        //The BlockStructure is containing the color, height and the contoures of the
        //block in form of a matrix
        public BlockStructure blockStructure;

        //A list that contains all wall colliders of the block
        private List<Collider> wallColliderList = new List<Collider>();

        [HideInInspector]
        public GameObject CornerTopA { get; private set; }
        [HideInInspector]
        public GameObject CornerTopB { get; private set; }
        [HideInInspector]
        public GameObject CornerTopC { get; private set; }
        [HideInInspector]
        public GameObject CornerTopD { get; private set; }
        [HideInInspector]
        public GameObject CornerBottomA { get; private set; }
        [HideInInspector]
        public GameObject CornerBottomB { get; private set; }
        [HideInInspector]
        public GameObject CornerBottomC { get; private set; }
        [HideInInspector]
        public GameObject CornerBottomD { get; private set; }

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
            if(GetComponents<Collider>().Length == 0)
            {
                AddWallCollider();
                AddPinTriggerCollider(BRICK_PIN_HEIGHT_HALF, TapContainer, "Tap");
                AddPinTriggerCollider(-BRICK_HEIGHT / 1.1f, GroovesContainer, "Groove");
                SetWallColliderTrigger(false);
            }
            
            AddCorners();
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

        private void AddCorners()
        {
            Vector3 size = mesh.bounds.size;
            Vector3 center = transform.TransformPoint(mesh.bounds.center);
            Vector3 extends = mesh.bounds.extents;

            Vector3 pointTopA = new Vector3(center.x + extends.x, center.y + extends.y - BRICK_PIN_HEIGHT, center.z + extends.z);
            Vector3 pointTopB = new Vector3(center.x + extends.x, center.y + extends.y - BRICK_PIN_HEIGHT, center.z - extends.z);
            Vector3 pointTopC = new Vector3(center.x - extends.x, center.y + extends.y - BRICK_PIN_HEIGHT, center.z - extends.z);
            Vector3 pointTopD = new Vector3(center.x - extends.x, center.y + extends.y - BRICK_PIN_HEIGHT, center.z + extends.z);

            Vector3 pointBottomA = new Vector3(center.x + extends.x, center.y - extends.y, center.z + extends.z);
            Vector3 pointBottomB = new Vector3(center.x + extends.x, center.y - extends.y, center.z - extends.z);
            Vector3 pointBottomC = new Vector3(center.x - extends.x, center.y - extends.y, center.z - extends.z);
            Vector3 pointBottomD = new Vector3(center.x - extends.x, center.y - extends.y, center.z + extends.z);

            GameObject cornerTopA = new GameObject("CornerTopA");
            cornerTopA.transform.position = pointTopA;
            GameObject cornerTopB = new GameObject("CornerTopB");
            cornerTopB.transform.position = pointTopB;
            GameObject cornerTopC = new GameObject("CornerTopC");
            cornerTopC.transform.position = pointTopC;
            GameObject cornerTopD = new GameObject("CornerTopD");
            cornerTopD.transform.position = pointTopD;
            GameObject cornerBottomA = new GameObject("CornerBottomA");
            cornerBottomA.transform.position = pointBottomA;
            GameObject cornerBottomB = new GameObject("CornerBottomB");
            cornerBottomB.transform.position = pointBottomB;
            GameObject cornerBottomC = new GameObject("CornerBottomC");
            cornerBottomC.transform.position = pointBottomC;
            GameObject cornerBottomD = new GameObject("CornerBottomD");
            cornerBottomD.transform.position = pointBottomD;

            cornerTopA.transform.SetParent(this.transform);
            cornerTopB.transform.SetParent(this.transform);
            cornerTopC.transform.SetParent(this.transform);
            cornerTopD.transform.SetParent(this.transform);
            cornerBottomA.transform.SetParent(this.transform);
            cornerBottomB.transform.SetParent(this.transform);
            cornerBottomC.transform.SetParent(this.transform);
            cornerBottomD.transform.SetParent(this.transform);

            this.CornerTopA = cornerTopA;
            this.CornerTopB = cornerTopB;
            this.CornerTopC = cornerTopC;
            this.CornerTopD = cornerTopD;
            this.CornerBottomA = cornerBottomA;
            this.CornerBottomB = cornerBottomB;
            this.CornerBottomC = cornerBottomC;
            this.CornerBottomD = cornerBottomD;
        }



        private void DrawBlockNormalsDebug()
        {
            Debug.DrawLine(CornerTopA.transform.position, CornerTopB.transform.position, Color.cyan);
            Debug.DrawLine(CornerTopB.transform.position, CornerTopC.transform.position, Color.cyan);
            Debug.DrawLine(CornerTopC.transform.position, CornerTopD.transform.position, Color.cyan);
            Debug.DrawLine(CornerTopD.transform.position, CornerTopA.transform.position, Color.cyan);
            Debug.DrawLine(CornerBottomA.transform.position, CornerBottomB.transform.position, Color.cyan);
            Debug.DrawLine(CornerBottomB.transform.position, CornerBottomC.transform.position, Color.cyan);
            Debug.DrawLine(CornerBottomC.transform.position, CornerBottomD.transform.position, Color.cyan);
            Debug.DrawLine(CornerBottomD.transform.position, CornerBottomA.transform.position, Color.cyan);
            Debug.DrawRay(CornerTopA.transform.position, GetBlockNormale(), Color.green);

        }

        /*
         *
         */
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

            //Seatch the blockStructure for walls and add them the the list. Search is carried out
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

        public BlockStructure GetStructure()
        {
            return blockStructure;
        }

        private void AddTopCollider()
        {
            Vector3 topSideSize = new Vector3(BRICK_LENGTH, BRICK_WALL_WIDTH, BRICK_LENGTH);
            float rowMiddlePoint = (float)(blockStructure.RowsCropped - 1) / 2;
            float colMiddlePoint = (float)(blockStructure.ColsCropped - 1) / 2;

            for (int row = 0; row < blockStructure.RowsCropped; row++)
            {
                for(int col = 0; col < blockStructure.ColsCropped; col++)
                {
                    if(blockStructure[row, col] != null)
                    {
                        float centerX = (rowMiddlePoint - row) * BRICK_LENGTH;
                        float centerY = GetCenterTop().y - (BRICK_WALL_WIDTH / 2);
                        float centerZ = (colMiddlePoint - col) * BRICK_LENGTH;
                        
                        Vector3 colliderCenter = new Vector3(centerX, centerY, centerZ);
                        AddBoxCollider(topSideSize, colliderCenter, false, transform.gameObject);
                    }
                }
            }
        }

        public void AddWallCollider(List<List<BlockPart>> allWallsInStructure)
        {
            foreach(List<BlockPart> wall in allWallsInStructure)
            {
                float wallLength = wall.Count * BRICK_LENGTH;
                float rowMiddlePoint = (float) (blockStructure.RowsCropped - 1) / 2;
                float colMiddlePoint = (float) (blockStructure.ColsCropped - 1) / 2;

                Vector3 centerMesh = GetComponent<MeshFilter>().mesh.bounds.center;
                float wallCloumnAverage = 0;
                float wallRowAverage = 0;
                wall.ForEach(blockPart => {
                    wallRowAverage += blockPart.Row;
                    wallCloumnAverage += blockPart.Col;
                });
                wallRowAverage /= wall.Count;
                wallCloumnAverage /= wall.Count;

                switch (wall[0].WallDirection)
                {
                    case DIRECTION.UP:
                        {
                            float centerColliderZ = (colMiddlePoint - wallCloumnAverage) * BRICK_LENGTH;
                            float centerColliderX = (rowMiddlePoint + 0.5f - wall[0].Row) * BRICK_LENGTH;

                            Vector3 size = new Vector3(BRICK_WALL_WIDTH, BRICK_HEIGHT, wallLength);
                            Vector3 centerCollider = new Vector3(centerColliderX - BRICK_WALL_WIDTH_HALF, centerMesh.y - BRICK_PIN_HEIGHT_HALF, centerColliderZ);

                            AddBoxCollider(size, centerCollider, false, transform.gameObject);
                            break;
                        }

                    case DIRECTION.DOWN:
                        {
                            float centerColliderZ = (colMiddlePoint - wallCloumnAverage) * BRICK_LENGTH;
                            float centerColliderX = (rowMiddlePoint - 0.5f - wall[0].Row) * BRICK_LENGTH;

                            Vector3 size = new Vector3(BRICK_WALL_WIDTH, BRICK_HEIGHT, wallLength);
                            Vector3 centerCollider = new Vector3(centerColliderX + BRICK_WALL_WIDTH_HALF, centerMesh.y - BRICK_PIN_HEIGHT_HALF, centerColliderZ);

                            AddBoxCollider(size, centerCollider, false, transform.gameObject);
                            break;
                        }
                        

                    case DIRECTION.LEFT:
                        {
                            float centerColliderX = (rowMiddlePoint - wallRowAverage) * BRICK_LENGTH;
                            float centerColliderZ = (colMiddlePoint + 0.5f - wall[0].Col) * BRICK_LENGTH;

                            Vector3 size = new Vector3(wallLength, BRICK_HEIGHT, BRICK_WALL_WIDTH);
                            Vector3 centerCollider = new Vector3(centerColliderX, centerMesh.y - BRICK_PIN_HEIGHT_HALF, centerColliderZ - BRICK_WALL_WIDTH_HALF);

                            AddBoxCollider(size, centerCollider, false, transform.gameObject);
                            break;
                        }
                            

                    case DIRECTION.RIGHT:
                        {
                            float centerColliderX = (rowMiddlePoint - wallRowAverage) * BRICK_LENGTH;
                            float centerColliderZ = (colMiddlePoint - 0.5f - wall[0].Col) * BRICK_LENGTH;

                            Vector3 size = new Vector3(wallLength, BRICK_HEIGHT, BRICK_WALL_WIDTH);
                            Vector3 centerCollider = new Vector3(centerColliderX, centerMesh.y - BRICK_PIN_HEIGHT_HALF, centerColliderZ + BRICK_WALL_WIDTH_HALF);

                            AddBoxCollider(size, centerCollider, false, transform.gameObject);
                            break;
                        }
                }
            }
        }

        private List<List<BlockPart>> SearchWallsInStructure(DIRECTION direction)
        {
            List<List<BlockPart>> allWallsInStructure = new List<List<BlockPart>>();
            for (int row = 0; row < blockStructure.RowsCropped; row++)
            {
                for (int col = 0; col < blockStructure.ColsCropped; col++)
                {
                    //Need to reset the visited Status
                    if (blockStructure[row, col] != null && !blockStructure[row, col].WasDirectionVisited(direction))
                    {
                        List<BlockPart> tempList = SearchWallsAtLocation(row, col, direction);
                        if(tempList != null && tempList.Count != 0)
                        {
                            allWallsInStructure.Add(tempList);
                        }
                        
                    }
                }
            }
            return allWallsInStructure;
        }

        private List<BlockPart> SearchWallsAtLocation(int row, int col, DIRECTION direction, List<BlockPart> wallInStructure = null)
        {
            
            if(row >= blockStructure.RowsCropped || col >= blockStructure.ColsCropped || row < 0 || col < 0)
            {
                return wallInStructure;
            }

            if (wallInStructure == null)
            {
                wallInStructure = new List<BlockPart>();
            }

            if (blockStructure[row, col] != null && !blockStructure.HasNeighbour(row, col, direction) )
            {
                blockStructure[row, col].DirectionVisited(direction);
                wallInStructure.Add(new BlockPart(row, col, direction));
                if(direction == DIRECTION.UP || direction == DIRECTION.DOWN)
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

        private void RemoveWallCollider()
        {
            for(int i = 0; i < wallColliderList.Count; i++)
            {
                Destroy(wallColliderList[i]);
            }
            wallColliderList.Clear();
        }

        private void AddWallCollider()
        {
            Vector3 size = mesh.bounds.size;
            Vector3 center = mesh.bounds.center;
            Vector3 extends = mesh.bounds.extents;
            //Collider Long Side
            Vector3 longSideSize = new Vector3(size.x, BRICK_HEIGHT, BRICK_WALL_WIDTH);
            Vector3 longSideCenterLeft = new Vector3(center.x, center.y - BRICK_PIN_HEIGHT_HALF, center.z + extends.z - BRICK_WALL_WIDTH_HALF);
            Vector3 longSideCenterRight = new Vector3(center.x, center.y - BRICK_PIN_HEIGHT_HALF, center.z - extends.z + BRICK_WALL_WIDTH_HALF);
            AddBoxCollider(longSideSize, longSideCenterLeft, false, this.gameObject);
            AddBoxCollider(longSideSize, longSideCenterRight, false, this.gameObject);


            //Collider Short Side
            Vector3 shortSide = new Vector3(BRICK_WALL_WIDTH, BRICK_HEIGHT, size.z);
            Vector3 shortSideCenterUp = new Vector3(center.x + extends.x - BRICK_WALL_WIDTH_HALF, center.y - BRICK_PIN_HEIGHT_HALF, center.z);
            Vector3 shortSideCenterDown = new Vector3(center.x - extends.x + BRICK_WALL_WIDTH_HALF, center.y - BRICK_PIN_HEIGHT_HALF, center.z);
            AddBoxCollider(shortSide, shortSideCenterUp, false, this.gameObject);
            AddBoxCollider(shortSide, shortSideCenterDown, false, this.gameObject);

            //Collider Top Side
            Vector3 topSideSize = new Vector3(size.x, BRICK_WALL_WIDTH, size.z);
            Vector3 topSideCenter = GetCenterTop();
            topSideCenter.y = topSideCenter.y - (BRICK_WALL_WIDTH / 2);
            AddBoxCollider(topSideSize, topSideCenter, false, this.gameObject);
        }

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
                    bool isTrigger = true;
                    if (tag.Equals("Tap"))
                    {
                        isTrigger = false;
                    }
                    AddGameObjectCollider(currentPinCenterPoint, tag, containerObject, isTrigger);
                    currentPinCenterPoint.z = currentPinCenterPoint.z - BRICK_PIN_DISTANCE;
                }
                currentPinCenterPoint.x = currentPinCenterPoint.x - BRICK_PIN_DISTANCE;
                currentPinCenterPoint.z = firstPinCenterPoint.z;

            }
        }

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

        private void AddGameObjectCollider(Vector3 position, String tag, GameObject container, bool isTrigger)
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

        private Collider AddBoxCollider(Vector3 size, Vector3 center, bool isTrigger, GameObject otherGameObject, PhysicMaterial material)
        {
            BoxCollider collider = otherGameObject.AddComponent<BoxCollider>();
            collider.size = size;
            collider.center = center;
            collider.isTrigger = isTrigger;
            collider.material = material;
            wallColliderList.Add(collider);
            return collider;
        }

        private Collider AddBoxCollider(Vector3 sizeCollider, Vector3 centerCollider, bool isTrigger, GameObject otherGameObject)
        {
            BoxCollider newCollider = otherGameObject.AddComponent<BoxCollider>();
            newCollider.size = sizeCollider;
            newCollider.center = centerCollider;
            newCollider.isTrigger = isTrigger;
            wallColliderList.Add(newCollider);
            return newCollider;
        }

        public Vector3 GetCenterTop()
        {
            Vector3 center = GetComponent<MeshFilter>().mesh.bounds.center;
            Vector3 extends = GetComponent<MeshFilter>().mesh.bounds.extents;
            center.y = center.y + extends.y - BRICK_PIN_HEIGHT;
            return center;

        }

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

        public GameObject GetRootGameObject()
        {
            return gameObject;
        }

        public Vector3 GetBlockNormale()
        {
            Vector3 AB = CornerTopA.transform.position - CornerTopB.transform.position;
            Vector3 AC = CornerTopC.transform.position - CornerTopB.transform.position;
            return Vector3.Cross(AC, AB);
        }

        public void SetWallColliderTrigger(bool trigger)
        {
            wallColliderList.ForEach(collder => collder.isTrigger = trigger);
        }
    }
}