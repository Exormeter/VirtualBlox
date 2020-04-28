using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PartContainerController : MonoBehaviour
{
    public List<Toggle> Toggles;
    public List<GameObject> Containers;

    private void Awake()
    {
        Toggles = new List<Toggle>(GetComponentsInChildren<Toggle>());

        Containers = new List<GameObject>();

        for(int i = 0; i < transform.childCount; i++)
        {
            Containers.Add(transform.GetChild(i).gameObject);
        }
    }

    public void AddTogglesToGroup(ToggleGroup toggleGroup)
    {
        foreach(Toggle toggle in Toggles)
        {
            toggle.group = toggleGroup;
        }
    }
}
