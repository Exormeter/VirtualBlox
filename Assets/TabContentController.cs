using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class TabContentController : MonoBehaviour
{
    
    public GameObject Content;
    void Start()
    {
        
    }

    public void DeactivateContent()
    {
        Content.SetActive(false);

    }

    public void ActivateContent()
    {
        Content.SetActive(true);
    }

    /// <summary>
    /// Saves the last open tab state in the ContentController
    /// </summary>
    public void CloseMenu()
    {
        Content.GetComponent<ContentController>().WasLastActive = Content.activeSelf;
        Content.SetActive(false);
    }
}
