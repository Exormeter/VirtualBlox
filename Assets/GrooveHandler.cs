using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrooveHandler : MonoBehaviour
{
    private Dictionary<SnappingCollider, DistanceVector> colliderDictionary = new Dictionary<SnappingCollider, DistanceVector>();
    private float lastResetTime;
    private float timeDifference = 2.0f;
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
            colliderDictionary.Add(snaps, new DistanceVector(new Vector3()));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void registerCollision(SnappingCollider snappingCollider, Collider tapCollider)
    {
        if (hasSnapped)
        {
            Vector3 snapColliderCenter2 = transform.TransformDirection(snappingCollider.GetComponent<BoxCollider>().bounds.center);
            Vector3 tapColliderCenter2 = transform.TransformDirection(tapCollider.bounds.center);
            Vector3 centerDistance2 = snapColliderCenter2 - tapColliderCenter2;
            Debug.Log("Distance after snap" + centerDistance2.ToString("F4"));
            return;
        }
        //Debug.Log(snappingCollider.GetComponent<BoxCollider>().bounds.center.ToString("F5"));
        Vector3 snapColliderCenter = transform.TransformDirection(snappingCollider.GetComponent<BoxCollider>().bounds.center);
        Vector3 tapColliderCenter = transform.TransformDirection(tapCollider.bounds.center);
        Vector3 centerDistance = snapColliderCenter - tapColliderCenter;
        DistanceVector distanceVector = colliderDictionary[snappingCollider];
        distanceVector.hasDistance = true;

        if (centerDistance != distanceVector.Distance)
        { 
            colliderDictionary[snappingCollider] = new DistanceVector(centerDistance);
            lastResetTime = Time.time;
            
        }
        if (Time.time - timeDifference >= lastResetTime)
        {
            hasSnapped = true;
            //snapBlockToRaster();
            Vector3 path = distanceVector.Distance;
            path.y = 0;
            Debug.Log(centerDistance.ToString("F4"));
            block.transform.position = block.transform.position - path;
            //DeDebug.Log(transform.TransformDirection(block.GetComponent<SnappingCollider>().transform.position));
            //Vector3 newDistance = transform.TransformDirection(snappingCollider.GetComponent<BoxCollider>().bounds.center) - transform.TransformDirection(tapCollider.bounds.center);
            //Debug.Log(newDistance.ToString("F7"));
        }


    }

    public void snapBlockToRaster()
    {
        Debug.Log("Snapped");
        
        foreach (DistanceVector centerDistance in colliderDictionary.Values)
        {
            if(centerDistance.hasDistance)
            {
                Vector3 path = centerDistance.Distance;
                path.y = 0;
                block.transform.position = block.transform.position - path;

                break;
            }
        }
    }
    
}

public class DistanceVector
{
    private Vector3 distance;
    public bool hasDistance = false;

    public Vector3 Distance
    {
        get
        {
            return distance;
        }

        set
        {
            distance = value;
        }
    }

    public DistanceVector(Vector3 distance) => this.distance = distance;
}
