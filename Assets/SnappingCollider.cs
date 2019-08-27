using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnappingCollider : MonoBehaviour
{
    private LinkedList<Collider> colliderList = new LinkedList<Collider>();
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider grooveCollider)
    {
        if (grooveCollider.gameObject.tag == "Groove")
        {
            Debug.Log("HitGroove");
            //colliderList.AddLast(grooveCollider);
        }
    }

    private void OnTriggerExit(Collider grooveCollider)
    {
        
    }

    private void OnTriggerStay(Collider grooveCollider)
    { 
        BlockScript blockScriptTapBlock = grooveCollider.gameObject.gameObject.GetComponent<BlockScript>();
        Vector3 centerGrooveCollider = grooveCollider.bounds.center;
        Vector3 centerTapCollider = gameObject.GetComponent<BoxCollider>().bounds.center;
        Vector3 snapPath = centerGrooveCollider - centerTapCollider;
        GameObject snapedBlock = blockScriptTapBlock.gameObject;
        snapedBlock.transform.position = snapedBlock.transform.position - snapPath;


    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
