using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class KeyBoardController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject FirstRow;
    public GameObject SecondRow;
    public GameObject ThirdRow;
    public GameObject ForthRow;


    [Serializable]
    public class KeyBoardEvent : UnityEvent<string> { }

    public KeyBoardEvent keyboardPressed = new KeyBoardEvent();
    void Start()
    {
        AddListner(FirstRow);
        AddListner(SecondRow);
        AddListner(ThirdRow);
        AddListner(ForthRow);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AddListner(GameObject row)
    {
        for(int i = 0; i < row.transform.childCount; i++)
        {
            Button button = row.transform.GetChild(i).GetComponent<Button>();
            button.onClick.AddListener(() => KeyBordPress(button.name.ToLower()));
        }
    }

    private void KeyBordPress(string buttonPress)
    {
        keyboardPressed.Invoke(buttonPress);
    }
}
