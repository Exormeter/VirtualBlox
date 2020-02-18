using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevealWalkableArea : MonoBehaviour
{
    // Start is called before the first frame update
    public Material revealMaterial;
    public Transform HMDTransform;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 HMD_XY_Position = HMDTransform.position;
        HMD_XY_Position.y = gameObject.transform.position.y + 1;
        revealMaterial.SetVector("_LightPosition", HMD_XY_Position);
    }
}
