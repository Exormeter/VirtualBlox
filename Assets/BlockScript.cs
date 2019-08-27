using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockScript : MonoBehaviour
{
    private const float BRICK_HEIGHT = 0.096f;
    private const float BRICK_PIN_HEIGHT = 0.016f;
    private const float BRICK_WALL_WIDTH = 0.01f;
    private const float BRICK_PIN_DISTANCE = 0.08f;
    private const float BRICK_PIN_DIAMETER = 0.048f;

    private Mesh mesh;

    void Start()
    {
        this.mesh = GetComponent<MeshFilter>().mesh;
        AddWallCollider();

        GameObject grooves = new GameObject("Grooves");
        GameObject taps = new GameObject("Taps");
        grooves.transform.SetParent(this.transform);
        taps.transform.SetParent(this.transform);
        grooves.transform.localPosition = new Vector3(0f, 0f, 0f);
        taps.transform.localPosition = new Vector3(0f, 0f, 0f);
        AddPinTriggerCollider(BRICK_PIN_HEIGHT, taps, new SnappingCollider(), "Tap");
        AddPinTriggerCollider(-(BRICK_HEIGHT / 1.3f), grooves, null, "Groove");
    }


    // Update is called once per frame
    void Update()
    {
        
    }


    private void AddWallCollider()
    {
        Vector3 size = mesh.bounds.size;
        //Collider Long Side
        Vector3 longSideSize = new Vector3(size.x, BRICK_HEIGHT, BRICK_WALL_WIDTH);
        Vector3 longSideCenterLeft = new Vector3(0, 0 - (BRICK_PIN_HEIGHT / 2), (size.z / 2) - (BRICK_WALL_WIDTH / 2));
        Vector3 longSideCenterRight = new Vector3(0, 0 - (BRICK_PIN_HEIGHT / 2), ((size.z / 2) - (BRICK_WALL_WIDTH / 2)) * -1);
        AddBoxCollider(longSideSize, longSideCenterLeft, false, this.gameObject);
        AddBoxCollider(longSideSize, longSideCenterRight, false, this.gameObject);


        //Collider Short Side
        Vector3 shortSide = new Vector3(BRICK_WALL_WIDTH, BRICK_HEIGHT, size.z);
        Vector3 shortSideCenterUp = new Vector3((size.x / 2) - (BRICK_WALL_WIDTH / 2), 0 - (BRICK_PIN_HEIGHT / 2), 0);
        Vector3 shortSideCenterDown = new Vector3(((size.x / 2) - (BRICK_WALL_WIDTH / 2)) * -1, 0 - (BRICK_PIN_HEIGHT / 2), 0);
        AddBoxCollider(shortSide, shortSideCenterUp, false, this.gameObject);
        AddBoxCollider(shortSide, shortSideCenterDown, false, this.gameObject);

        //Collider Top Side
        Vector3 topSideSize = new Vector3(size.x, BRICK_WALL_WIDTH, size.z);
        Vector3 topSideCenter = GetCenterTop();
        topSideCenter.y = topSideCenter.y - (BRICK_WALL_WIDTH / 2);
        AddBoxCollider(topSideSize, topSideCenter, false, this.gameObject);
    }

    private void AddPinTriggerCollider(float heightOffset, GameObject containerObject, Component component, String tag)
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3 size = mesh.bounds.size;
        Vector3 center = mesh.bounds.center;
        Vector3 blockCenter = new Vector3(center.x, center.y - BRICK_PIN_HEIGHT, center.z);
        Vector3 blockCorner = blockCenter - mesh.bounds.min;

        Vector3 firstPinCenterPoint = new Vector3(blockCorner.x - (BRICK_PIN_DISTANCE / 2), blockCorner.y + heightOffset, blockCorner.z - (BRICK_PIN_DISTANCE / 2));
        Vector3 currentPinCenterPoint = firstPinCenterPoint;

        while (mesh.bounds.Contains(currentPinCenterPoint))
        {

            while (mesh.bounds.Contains(currentPinCenterPoint))
            {
                AddGameObjectCollider(currentPinCenterPoint, tag, component, containerObject);
                currentPinCenterPoint.z = currentPinCenterPoint.z - BRICK_PIN_DISTANCE;
            }
            currentPinCenterPoint.x = currentPinCenterPoint.x - BRICK_PIN_DISTANCE;
            currentPinCenterPoint.z = firstPinCenterPoint.z;

        }
    }

    private void AddGameObjectCollider(Vector3 position, String tag, Component component, GameObject container)
    {
        GameObject colliderObject = new GameObject("Collider");
        colliderObject.tag = tag;
        colliderObject.transform.SetParent(container.transform);
        colliderObject.transform.localPosition = new Vector3(0f, 0f, 0f);
        colliderObject.AddComponent<SnappingCollider>();
        AddBoxCollider(new Vector3(BRICK_PIN_DIAMETER, BRICK_PIN_HEIGHT, BRICK_PIN_DIAMETER), position, true, colliderObject);
    }

    private Collider AddBoxCollider(Vector3 size, Vector3 center, bool isTrigger, GameObject otherGameObject)
    {
        BoxCollider collider = otherGameObject.AddComponent<BoxCollider>();
        collider.size = size;
        collider.center = center;
        collider.isTrigger = isTrigger;
        return collider;
    }

    public Vector3 GetCenterTop()
    {
        Vector3 center = mesh.bounds.center;
        Vector3 extends = mesh.bounds.extents;
        center.y = center.y + extends.y - BRICK_PIN_HEIGHT;
        return center;

    }

    public Vector3 GetCenterBottom()
    {
        Vector3 center = mesh.bounds.center;
        Vector3 extends = mesh.bounds.extents;
        center.y = center.y - extends.y;
        return center;
    }

    public GameObject getRootGameObject()
    {
        return gameObject;
    }
}
