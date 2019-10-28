using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class MaterialHandler : MonoBehaviour
{
    private Material standardMaterial;
    public Material whileHoldingMaterial;
    private MeshRenderer meshRenderer;
    // Start is called before the first frame update
    void Start()
    {
        standardMaterial = transform.GetComponent<MeshRenderer>().material;
        meshRenderer = transform.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    
    void OnAttachedToHand(Hand hand)
    {
        meshRenderer.material = whileHoldingMaterial;
    }

    void OnDetachedFromHand(Hand hand)
    {
        meshRenderer.material = standardMaterial;
    }

    void OnBlockAttach()
    {
        if (GetComponent<BlockScript>().IsIndirectlyAttachedToHand())
        {
            meshRenderer.material = whileHoldingMaterial;
        }
    }

    void OnIndirectDetachedFromHand()
    {
        meshRenderer.material = standardMaterial;
    }

    void OnIndirectAttachedtoHand()
    {
        meshRenderer.material = whileHoldingMaterial;
    }

    void RemovedConnection()
    {
        if (!GetComponent<BlockScript>().IsIndirectlyAttachedToHand())
        {
            meshRenderer.material = standardMaterial;
        }

    }
}
