using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class MaterialHandler : MonoBehaviour
{
    public Material HoldingMaterial;
    private Material standardMaterial;
    private Color blockColor;
    private Material whileHoldingMaterial;
    private MeshRenderer meshRenderer;
     
    void Start()
    {
        whileHoldingMaterial = new Material(HoldingMaterial);
        standardMaterial = transform.GetComponent<MeshRenderer>().material;
        meshRenderer = transform.GetComponent<MeshRenderer>();
        blockColor = standardMaterial.color;
        blockColor.a = 1;
        whileHoldingMaterial.color = blockColor;
    }


    void OnAttachedToHand(Hand hand)
    {
        meshRenderer.material = whileHoldingMaterial;
    }

    void OnDetachedFromHand(Hand hand)
    {
        if (!GetComponent<BlockCommunication>().IsIndirectlyAttachedToHand())
        {
            meshRenderer.material = standardMaterial;
        }
    }

    void OnBlockAttach()
    {
        if (GetComponent<BlockCommunication>().IsIndirectlyAttachedToHand())
        {
            meshRenderer.material = whileHoldingMaterial;
        }
    }

    void OnIndirectDetachedFromHand()
    {
        if (!GetComponent<BlockCommunication>().IsIndirectlyAttachedToHand())
        {
            meshRenderer.material = standardMaterial;
        }
    }

    void OnIndirectAttachedtoHand()
    {
        meshRenderer.material = whileHoldingMaterial;
    }

    void RemovedConnection()
    {
        if (!GetComponent<BlockCommunication>().IsIndirectlyAttachedToHand())
        {
            meshRenderer.material = standardMaterial;
        }

    }
}
