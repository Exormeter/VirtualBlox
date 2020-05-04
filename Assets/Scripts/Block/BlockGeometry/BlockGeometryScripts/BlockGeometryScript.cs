using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Valve.VR.InteractionSystem
{
    public abstract class BlockGeometryScript : MonoBehaviour
    {

        

        public static float BRICK_HEIGHT_NORMAL = 0.096f;

        public static float BRICK_HEIGHT_FLAT = 0.032f;

        /// <summary>The height of the pins on the upside of the bricks</summary>
        protected const float BRICK_PIN_HEIGHT = 0.016f;

        /// <summary>The height of the pins halfed</summary>
        protected const float BRICK_PIN_HEIGHT_HALF = BRICK_PIN_HEIGHT / 2;

        /// <summary>The thickness of the walls of the brick</summary>
        protected const float BRICK_WALL_WIDTH = 0.008f;

        /// <summary>The thickness of the brick halfed</summary>
        protected const float BRICK_WALL_WIDTH_HALF = BRICK_WALL_WIDTH / 2;

        /// <summary>The distance between to pins on the upside of a brick</summary>
        protected const float BRICK_PIN_DISTANCE = 0.08f;

        /// <summary>The lenght of a brick, meassure taken from a 1x1 brick</summary>
        protected const float BRICK_LENGTH = 0.08f;

        /// <summary>The diameter of a pin on the upside of a brick</summary>
        protected const float BRICK_PIN_DIAMETER = 0.048f;

        /// <summary>The mesh of the block</summary>
        protected Mesh mesh;

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

        /// <summary>
        /// The BlockStructure is containing the color, height and the contoures of the
        /// block in form of a matrix
        /// </summary>
        public BlockStructure BlockStructure;

        /// <summary>
        /// What kind of Block is the GameObject, Custom or LDraw
        /// </summary>
        public BlockIdentifier BlockIdentifier;

        /// <summary>A list that contains all wall colliders of the block</summary>
        protected List<Collider> wallColliderList = new List<Collider>();

        /// <summary>
        /// The Layer which is set to be walkable
        /// </summary>
        protected const int WALKABLE_LAYER = 8;

        protected void Awake()
        {
        
        }

        protected void Start()
        {

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
                    AddBoxCollider(new Vector3(BRICK_PIN_DIAMETER / 2, BRICK_PIN_HEIGHT, BRICK_PIN_DIAMETER / 2), new Vector3(0, 0.01f, 0), isTrigger, colliderObject);
                    break;
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

        /// <summary>
        /// Sets the Walls of the Block to be able to pass through or be solid
        /// </summary>
        /// <param name="trigger"></param>
        public void SetWallColliderTrigger(bool trigger)
        {
            wallColliderList.ForEach(collder => collder.isTrigger = trigger);
        }

        /// <summary>
        /// Returnes the current BlockStructure
        /// </summary>
        /// <returns>The BlockSturcture of the Block</returns>
        public BlockStructure GetStructure()
        {
            return BlockStructure;
        }

        /// <summary>
        /// Sets if the Block should be walkable or not
        /// </summary>
        /// <param name="walkable">Is the Block walkable</param>
        public abstract void SetBlockWalkable(bool walkable);

    }

    public enum BlockIdentifier
    {
        BLOCK_FLOOR,
        BLOCK_CUSTOM,
        BLOCK_LDRAW
    }
}