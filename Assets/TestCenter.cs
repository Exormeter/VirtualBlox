using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCenter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        Vector3 center = other.bounds.center - GetComponent<BoxCollider>().bounds.center;
        transform.Translate(center);
    }
}
