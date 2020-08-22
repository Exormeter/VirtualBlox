using System;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class BlockGeometryCustom : BlockGeometryScript
    {
        /// <summary>The height of a brick, can be flat (0.032) or normal (0.096)</summary>
        protected float BRICK_HEIGHT;
        /// <summary>

        /// <summary>Only for inclass use *HACK*</summary>
        private CustomBlockStructure customBlockStructure;


        /// <summary>
        /// Contains the TopCollider, so that a serperate Raycast Layer can be set for them
        /// </summary>
        private GameObject TopColliderContainer;

        protected new void Awake()
        {
            this.mesh = GetComponent<MeshFilter>().mesh;
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

            BlockIdentifier = BlockIdentifier.BLOCK_CUSTOM;
        }

        

        /// <summary>
        /// Sets the BlockStructure for the Block, recalculated the Groove and Taps and Collider
        /// of the walls
        /// </summary>
        /// <param name="structure">The BlockStructure to set</param>
        public void SetCustomStructure(CustomBlockStructure structure)
        {
            //Reset the BlockStructre in case it wasn't done to ensure no BlockPart is marked visited
            structure.ResetBlockParts();

            //Set the new BlockStructure
            BlockStructure = structure;

            customBlockStructure = structure;

            if(customBlockStructure.BlockSize == BLOCKSIZE.FLAT)
            {
                BRICK_HEIGHT = BlockGeometryScript.BRICK_HEIGHT_FLAT;
            }
            else if(customBlockStructure.BlockSize == BLOCKSIZE.NORMAL)
            {
                BRICK_HEIGHT = BlockGeometryScript.BRICK_HEIGHT_NORMAL;
            }

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
            AddPinTriggerColliderByStructure(0, TapContainer, "Tap");
            AddPinTriggerColliderByStructure(-BRICK_HEIGHT, GroovesContainer, "Groove");
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
            float rowMiddlePoint = (float)(customBlockStructure.RowsCropped - 1) / 2;

            //MiddlePoint column of the BlockStructure Matrix
            float colMiddlePoint = (float)(customBlockStructure.ColsCropped - 1) / 2;

            //Loop throught the cropped BlockStructure
            for (int row = 0; row < customBlockStructure.RowsCropped; row++)
            {
                for (int col = 0; col < customBlockStructure.ColsCropped; col++)
                {
                    //If a Part inside the Matrix is not null, set a new collider
                    if (customBlockStructure[row, col] != null)
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
            foreach (List<BlockPart> wall in allWallsInStructure)
            {
                //Lenght of the wall, depents on the number of BlockParts time the length of a 1x1 brick
                float wallLength = wall.Count * BRICK_LENGTH;

                //MiddlePoint row of the BlockStructure Matrix
                float rowMiddlePoint = (float)(customBlockStructure.RowsCropped - 1) / 2;

                //MiddlePoint column of the BlockStructure Matrix
                float colMiddlePoint = (float)(customBlockStructure.ColsCropped - 1) / 2;


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
            for (int row = 0; row < customBlockStructure.RowsCropped; row++)
            {
                for (int col = 0; col < customBlockStructure.ColsCropped; col++)
                {
                    //Check if the currently searched Part is not null and wasn't already visited
                    if (customBlockStructure[row, col] != null && !customBlockStructure[row, col].WasDirectionVisited(direction))
                    {
                        //Searches for walls at the given Location recursive
                        List<BlockPart> tempList = SearchWallsAtLocation(row, col, direction);

                        //If the Search was successful, add the List to the List that will returned
                        if (tempList != null && tempList.Count != 0)
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
            if (row >= customBlockStructure.RowsCropped || col >= customBlockStructure.ColsCropped || row < 0 || col < 0)
            {
                return wallInStructure;
            }

            //If List was null, create a new list to cache BlockParts
            if (wallInStructure == null)
            {
                wallInStructure = new List<BlockPart>();
            }

            //If BlockPart has no neighbor in searched direction, part of wall is found
            if (customBlockStructure[row, col] != null && !customBlockStructure.HasNeighbour(row, col, direction))
            {
                //Remember that this part was search in the direction
                customBlockStructure[row, col].DirectionVisited(direction);

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

            float rowMiddlePoint = (float)(customBlockStructure.RowsCropped - 1) / 2;
            float colMiddlePoint = (float)(customBlockStructure.ColsCropped - 1) / 2;

            for (int row = 0; row < customBlockStructure.RowsCropped; row++)
            {
                for (int col = 0; col < customBlockStructure.ColsCropped; col++)
                {
                    if (customBlockStructure[row, col] != null)
                    {
                        bool isTrigger = true;

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
        /// Remove all wall Collider on the GameObject
        /// </summary>
        private void RemoveWallCollider()
        {
            for (int i = 0; i < wallColliderList.Count; i++)
            {
                Destroy(wallColliderList[i]);
            }
            wallColliderList.Clear();
        }

        public override void SetBlockWalkable(bool walkable)
        {
            //Walkable Blocks are not supported at the moment
            /*if (walkable)
            {
                gameObject.layer = WALKABLE_LAYER;
            }
            else
            {
                gameObject.layer = 0;
            }*/
        }
    }
}

