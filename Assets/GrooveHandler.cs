﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrooveHandler : MonoBehaviour
{

    private const float ZERO = 0f;
    private const float NINETY = 90f;
    private const float ONE_EIGHTY = 180f;
    private const float TWO_SEVENTY = 270f;

    private Dictionary<SnappingCollider, CollisionObject> colliderDictionary = new Dictionary<SnappingCollider, CollisionObject>();
    private float lastResetTime;
    private float timeDifference = 2.0f;
    public bool hasRotated = false;
    public bool hasSnapped = false;
    private GameObject block;

    // Start is called before the first frame update
    void Start()
    {
        block = transform.root.gameObject;
        int i = 0;
        foreach(SnappingCollider snaps in GetComponentsInChildren<SnappingCollider>())
        {
            if (i != 0)
            {
                snaps.gameObject.SetActive(false);
            }
            i++;
            colliderDictionary.Add(snaps, new CollisionObject(snaps.GetComponent<BoxCollider>()));
        }
    }

    
    void LateUpdate()
    {
        if (hasSnapped)
        {
            Rigidbody body = block.GetComponent<Rigidbody>();
            body.isKinematic = false;
        }

        if (hasRotated && !hasSnapped)
        {
            SnappingCollider snappingCollider = null;
            foreach(SnappingCollider snap in colliderDictionary.Keys)
            {
                snappingCollider = snap;
                break;
            }
            CollisionObject collisionObject = colliderDictionary[snappingCollider];
            //Vector3 centerDistance = getCenterOffset(snappingCollider.GetComponent<BoxCollider>().bounds.center, tapCollider.bounds.center);
            Vector3 centerDistance = collisionObject.getDistanceInWorldSpace(transform);
            Vector3 currentBlockPosition = block.transform.position;
            centerDistance.y = 0;
            switch (Mathf.Floor(block.transform.rotation.eulerAngles.y))
            {
                case ZERO:

                    currentBlockPosition = currentBlockPosition - centerDistance;

                    //centerDistance = getCenterOffset(snappingCollider.GetComponent<BoxCollider>().bounds.center, tapCollider.bounds.center);
                    centerDistance = collisionObject.getDistanceInWorldSpace(transform);
                    Debug.Log("Distance after snap 0" + centerDistance.ToString("F4"));
                    break;

                case NINETY:
                    
                    currentBlockPosition.x = currentBlockPosition.x + centerDistance.z;
                    currentBlockPosition.z = currentBlockPosition.z - centerDistance.x;

                    //centerDistance = getCenterOffset(snappingCollider.GetComponent<BoxCollider>().bounds.center, tapCollider.bounds.center);
                    centerDistance = collisionObject.getDistanceInWorldSpace(transform);
                    Debug.Log("Distance after snap 90" + centerDistance.ToString("F4"));
                    break;

                case ONE_EIGHTY:

                    currentBlockPosition = currentBlockPosition + centerDistance;


                    //centerDistance = getCenterOffset(snappingCollider.GetComponent<BoxCollider>().bounds.center, tapCollider.bounds.center);
                    centerDistance = collisionObject.getDistanceInWorldSpace(transform);
                    Debug.Log("Distance after snap 180" + centerDistance.ToString("F4"));
                    break;

                case TWO_SEVENTY:
                    currentBlockPosition.x = currentBlockPosition.x - centerDistance.z;
                    currentBlockPosition.z = currentBlockPosition.z + centerDistance.x;
                    break;

            }
            block.transform.localPosition = currentBlockPosition;
            if(centerDistance.x == 0 && centerDistance.z == 0)
            {
                hasSnapped = true;
            }
            
        }

       

    }

    public void registerCollision(SnappingCollider snappingCollider, Collider tapCollider)
    {
        if (hasRotated)
            return;


        CollisionObject collisionObject = colliderDictionary[snappingCollider];
        Vector3 newCenterDistance = getCenterOffset(collisionObject.GrooveCollider.bounds.center, tapCollider.bounds.center);
        Vector3 oldCenterDistance = collisionObject.getDistanceInWorldSpace(transform);
        collisionObject.TapCollider = tapCollider;

        if (newCenterDistance != oldCenterDistance)
        { 
            lastResetTime = Time.time;
            
        }
        if (Time.time - timeDifference >= lastResetTime)
        {
            hasRotated = true;
            Vector3 correctedRotation = correctRotation(block.transform.rotation.eulerAngles);
            Rigidbody body = block.GetComponent<Rigidbody>();
            body.isKinematic = true;
            block.transform.rotation = Quaternion.Euler(correctedRotation);
        }
    }

    public void unregisterCollision(SnappingCollider snappingCollider, Collider tapCollider)
    {
        CollisionObject collisionObject = colliderDictionary[snappingCollider];
        collisionObject.TapCollider = null;
    }

    private Vector3 correctRotation(Vector3 rotation)
    {
        Vector3 correctedRotation = new Vector3(0f, 0f, 0f);
        if(rotation.y <= 45 || rotation.y > 315)
        {
            correctedRotation.y = ZERO;
        }

        else if(rotation.y > 45 && rotation.y <= 135)
        {
            correctedRotation.y = NINETY;
        }
        else if(rotation.y > 135 && rotation.y < 225)
        {
            correctedRotation.y = ONE_EIGHTY;
        }
        else
        {
            correctedRotation.y = TWO_SEVENTY;
        }
        return correctedRotation;
    }

    private Vector3 getCenterOffset(Vector3 center, Vector3 otherCenter)
    {
        Vector3 centerWorld = transform.TransformDirection(center);
        Vector3 otherCenterWorld = transform.TransformDirection(otherCenter);
        return centerWorld - otherCenterWorld; 
    }
    
}

public class CollisionObject
{
    
    private Collider tapCollider;
    public Collider TapCollider
    {
        get
        {
            return tapCollider;
        }
        set
        {
            if(value == null)
            {
                hasDistance = false;
            }
            else
            {
                tapCollider = value;
                hasDistance = true;
            }
            
        }
            
    }

    private BoxCollider grooveCollider;
    public BoxCollider GrooveCollider
    {
        get
        {
            return grooveCollider;
        }
    }
    public bool hasDistance = false;

    public CollisionObject(BoxCollider grooveCollider)
    {
        this.grooveCollider = grooveCollider;
    }

    public Vector3 getDistanceInWorldSpace(Transform transform)
    {
        if(tapCollider == null)
        {
            return new Vector3();
        }
        Vector3 centerWorld = transform.TransformDirection(grooveCollider.bounds.center);
        Vector3 otherCenterWorld = transform.TransformDirection(tapCollider.bounds.center);
        return centerWorld - otherCenterWorld;
    }
}